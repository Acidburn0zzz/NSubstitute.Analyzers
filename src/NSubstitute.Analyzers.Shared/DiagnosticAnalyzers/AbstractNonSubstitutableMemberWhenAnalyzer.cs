﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute.Analyzers.Shared.Extensions;

namespace NSubstitute.Analyzers.Shared.DiagnosticAnalyzers
{
    internal abstract class AbstractNonSubstitutableMemberWhenAnalyzer<TSyntaxKind, TInvocationExpressionSyntax> : AbstractDiagnosticAnalyzer
        where TInvocationExpressionSyntax : SyntaxNode
        where TSyntaxKind : struct, Enum
    {
        private readonly ISubstitutionNodeFinder<TInvocationExpressionSyntax> _substitutionNodeFinder;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        private readonly Action<SyntaxNodeAnalysisContext> _analyzeInvocationAction;

        protected abstract TSyntaxKind InvocationExpressionKind { get; }

        protected AbstractNonSubstitutableMemberWhenAnalyzer(
            IDiagnosticDescriptorsProvider diagnosticDescriptorsProvider,
            ISubstitutionNodeFinder<TInvocationExpressionSyntax> substitutionNodeFinder)
            : base(diagnosticDescriptorsProvider)
        {
            _substitutionNodeFinder = substitutionNodeFinder;
            _analyzeInvocationAction = AnalyzeInvocation;
            SupportedDiagnostics = ImmutableArray.Create(
                DiagnosticDescriptorsProvider.NonVirtualWhenSetupSpecification,
                DiagnosticDescriptorsProvider.InternalSetupSpecification);
        }

        protected override void InitializeAnalyzer(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(_analyzeInvocationAction, InvocationExpressionKind);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext syntaxNodeContext)
        {
            var invocationExpression = (TInvocationExpressionSyntax)syntaxNodeContext.Node;
            var methodSymbolInfo = syntaxNodeContext.SemanticModel.GetSymbolInfo(invocationExpression);

            if (methodSymbolInfo.Symbol == null || methodSymbolInfo.Symbol.Kind != SymbolKind.Method)
            {
                return;
            }

            var methodSymbol = (IMethodSymbol)methodSymbolInfo.Symbol;

            if (methodSymbol.IsWhenLikeMethod() == false)
            {
                return;
            }

            var expressionsForAnalysys = _substitutionNodeFinder.FindForWhenExpression(syntaxNodeContext, invocationExpression, methodSymbol);
            foreach (var analysedSyntax in expressionsForAnalysys)
            {
                var symbolInfo = syntaxNodeContext.SemanticModel.GetSymbolInfo(analysedSyntax);
                if (symbolInfo.Symbol != null)
                {
                    var canBeSetuped = symbolInfo.Symbol.CanBeSetuped();
                    if (canBeSetuped == false)
                    {
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptorsProvider.NonVirtualWhenSetupSpecification,
                            analysedSyntax.GetLocation(),
                            symbolInfo.Symbol.Name);

                        syntaxNodeContext.ReportDiagnostic(diagnostic);
                    }

                    if (canBeSetuped && symbolInfo.Symbol.MemberVisibleToProxyGenerator() == false)
                    {
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptorsProvider.InternalSetupSpecification,
                            analysedSyntax.GetLocation(),
                            symbolInfo.Symbol.Name);

                        syntaxNodeContext.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}