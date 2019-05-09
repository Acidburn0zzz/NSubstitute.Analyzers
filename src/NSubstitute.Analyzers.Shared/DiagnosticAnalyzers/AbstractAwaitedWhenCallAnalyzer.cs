using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSubstitute.Analyzers.Shared.DiagnosticAnalyzers
{
    internal abstract class AbstractAwaitedWhenCallAnalyzer<TSyntaxKind, TInvocationExpressionSyntax> : AbstractDiagnosticAnalyzer
        where TInvocationExpressionSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        private static readonly Version MinimumSupportedVersion = new Version(4, 1, 0);
        private static readonly ImmutableHashSet<string> MethodNames = ImmutableHashSet.Create(
            MetadataNames.NSubstituteWhenMethod,
            MetadataNames.NSubstituteWhenForAnyArgsMethod);

        protected AbstractAwaitedWhenCallAnalyzer(IDiagnosticDescriptorsProvider diagnosticDescriptorsProvider)
            : base(diagnosticDescriptorsProvider)
        {
            SupportedDiagnostics = ImmutableArray.Create(diagnosticDescriptorsProvider.AwaitedWhenCall);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        protected abstract TSyntaxKind InvocationExpressionKind { get; }

        protected abstract IReadOnlyList<SyntaxNode> GetArgumentExpressions(TInvocationExpressionSyntax invocationExpressionSyntax);

        protected override void InitializeAnalyzer(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(analysisContext =>
            {
                var supportsAsyncWhenCalls = analysisContext.Compilation
                    .ReferencedAssemblyNames.Any(assembly =>
                        assembly.Name == MetadataNames.NSubstituteAssemblyName &&
                        assembly.Version >= MinimumSupportedVersion);

                if (supportsAsyncWhenCalls)
                {
                    analysisContext.RegisterSyntaxNodeAction(AnalyzeInvocation, InvocationExpressionKind);
                }
            });
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext syntaxNodeContext)
        {
            var invocationExpression = (TInvocationExpressionSyntax)syntaxNodeContext.Node;
            var methodSymbolInfo = syntaxNodeContext.SemanticModel.GetSymbolInfo(invocationExpression);

            if (methodSymbolInfo.Symbol?.Kind != SymbolKind.Method)
            {
                return;
            }

            var methodSymbol = (IMethodSymbol)methodSymbolInfo.Symbol;
            if (methodSymbol == null)
            {
                return;
            }

            if (IsWhenLikeMethod(syntaxNodeContext, invocationExpression, methodSymbol.Name) == false)
            {
                return;
            }

            var argumentExpressions = GetArgumentExpressions(invocationExpression);

            if (argumentExpressions == null)
            {
                return;
            }

            var argumentExpression = methodSymbol.ReducedFrom == null
                ? argumentExpressions.Skip(1).First()
                : argumentExpressions.First();

            if (syntaxNodeContext.SemanticModel.GetSymbolInfo(argumentExpression).Symbol is IMethodSymbol argumentMethodSymbol)
            {
                if (argumentMethodSymbol.IsAsync)
                {
                    syntaxNodeContext.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptorsProvider.AwaitedWhenCall,
                        argumentExpression.GetLocation()));
                }
            }
        }

        private bool IsWhenLikeMethod(SyntaxNodeAnalysisContext syntaxNodeContext, SyntaxNode syntax, string memberName)
        {
            if (MethodNames.Contains(memberName) == false)
            {
                return false;
            }

            var symbol = syntaxNodeContext.SemanticModel.GetSymbolInfo(syntax);

            return symbol.Symbol?.ContainingAssembly?.Name.Equals(MetadataNames.NSubstituteAssemblyName, StringComparison.OrdinalIgnoreCase) == true &&
                   symbol.Symbol?.ContainingType?.ToString().Equals(MetadataNames.NSubstituteSubstituteExtensionsFullTypeName, StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}