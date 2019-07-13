﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute.Analyzers.Shared.Extensions;

namespace NSubstitute.Analyzers.Shared.DiagnosticAnalyzers
{
    internal abstract class AbstractNonSubstitutableMemberWhenAnalyzer<TSyntaxKind, TInvocationExpressionSyntax, TMemberAccessExpressionSyntax> : AbstractDiagnosticAnalyzer
        where TInvocationExpressionSyntax : SyntaxNode
        where TMemberAccessExpressionSyntax : SyntaxNode
        where TSyntaxKind : struct, Enum
    {
        private readonly ISubstitutionNodeFinder<TInvocationExpressionSyntax> _substitutionNodeFinder;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        private readonly Action<SyntaxNodeAnalysisContext> _analyzeInvocationAction;

        private static readonly ImmutableHashSet<string> MethodNames = ImmutableHashSet.Create(
            MetadataNames.NSubstituteWhenMethod,
            MetadataNames.NSubstituteWhenForAnyArgsMethod);

        protected abstract TSyntaxKind InvocationExpressionKind { get; }

        protected AbstractNonSubstitutableMemberWhenAnalyzer(IDiagnosticDescriptorsProvider diagnosticDescriptorsProvider, ISubstitutionNodeFinder<TInvocationExpressionSyntax> substitutionNodeFinder)
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

            if (IsWhenLikeMethod(methodSymbol) == false)
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
                            GetSubstitutionNodeActualLocation(analysedSyntax, symbolInfo),
                            symbolInfo.Symbol.Name);

                        syntaxNodeContext.ReportDiagnostic(diagnostic);
                    }

                    if (canBeSetuped && symbolInfo.Symbol.MemberVisibleToProxyGenerator() == false)
                    {
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptorsProvider.InternalSetupSpecification,
                            GetSubstitutionNodeActualLocation(analysedSyntax, symbolInfo),
                            symbolInfo.Symbol.Name);

                        syntaxNodeContext.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private bool IsWhenLikeMethod(ISymbol symbol)
        {
            if (MethodNames.Contains(symbol.Name) == false)
            {
                return false;
            }

            return symbol.ContainingAssembly?.Name.Equals(MetadataNames.NSubstituteAssemblyName, StringComparison.OrdinalIgnoreCase) == true &&
                   symbol.ContainingType?.ToString().Equals(MetadataNames.NSubstituteSubstituteExtensionsFullTypeName, StringComparison.OrdinalIgnoreCase) == true;
        }

        private static Location GetSubstitutionNodeActualLocation(SyntaxNode analysedSyntax, SymbolInfo symbolInfo)
        {
            return analysedSyntax.GetSubstitutionNodeActualLocation<TMemberAccessExpressionSyntax>(symbolInfo.Symbol);
        }
    }
}