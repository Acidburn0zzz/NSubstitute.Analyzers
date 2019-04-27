using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute.Analyzers.Shared.Extensions;

namespace NSubstitute.Analyzers.Shared.DiagnosticAnalyzers
{
    internal abstract class AbstractArgumentMatcherCompilationAnalyzer<TInvocationExpressionSyntax, TMemberAccessExpression>
        where TInvocationExpressionSyntax : SyntaxNode
        where TMemberAccessExpression : SyntaxNode
    {
        private readonly ISubstitutionNodeFinder<TInvocationExpressionSyntax> _substitutionNodeFinder;

        private readonly IDiagnosticDescriptorsProvider _diagnosticDescriptorsProvider;

        private HashSet<SyntaxNode> ReceivedInOrderNodes { get; } = new HashSet<SyntaxNode>();

        private HashSet<SyntaxNode> WhenNodes { get; } = new HashSet<SyntaxNode>();

        private Dictionary<SyntaxNode, List<SyntaxNode>> PotentialMissusedNodes { get; } = new Dictionary<SyntaxNode, List<SyntaxNode>>();

        protected AbstractArgumentMatcherCompilationAnalyzer(ISubstitutionNodeFinder<TInvocationExpressionSyntax> substitutionNodeFinder, IDiagnosticDescriptorsProvider diagnosticDescriptorsProvider)
        {
            _substitutionNodeFinder = substitutionNodeFinder;
            _diagnosticDescriptorsProvider = diagnosticDescriptorsProvider;
        }

        public void BeginAnalyzeArgMatchers(SyntaxNodeAnalysisContext syntaxNodeContext)
        {
            var invocationExpression = (TInvocationExpressionSyntax)syntaxNodeContext.Node;
            var methodSymbolInfo = syntaxNodeContext.SemanticModel.GetSymbolInfo(invocationExpression);

            if (methodSymbolInfo.Symbol?.Kind != SymbolKind.Method)
            {
                return;
            }

            var methodSymbol = (IMethodSymbol)methodSymbolInfo.Symbol;

            switch (methodSymbol)
            {
                case ISymbol symbol when symbol.IsArgLikeMethod():
                    BeginAnalyzeArgLikeMethod(syntaxNodeContext, invocationExpression);
                    break;
                case ISymbol symbol when symbol.IsWhenLikeMethod():
                    BeginAnalyzeWhenLikeMethod(syntaxNodeContext, invocationExpression);
                    break;
                case ISymbol symbol when symbol.IsReceivedInOrderMethod():
                    BeginAnalyzeReceivedInOrderMethod(syntaxNodeContext, invocationExpression);
                    break;
            }
        }

        public void FinishAnalyzeArgMatchers(CompilationAnalysisContext compilationAnalysisContext)
        {
            foreach (var potential in PotentialMissusedNodes)
            {
                if (WhenNodes.Contains(potential.Key) == false && ReceivedInOrderNodes.Contains(potential.Key) == false)
                {
                    foreach (var arg in potential.Value)
                    {
                        var diagnostic = Diagnostic.Create(
                            _diagnosticDescriptorsProvider.ArgumentMatcherUsedOutsideOfCall,
                            arg.GetLocation());

                        compilationAnalysisContext.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        protected bool IsSetupLikeMethod(ISymbol symbol)
        {
            return symbol.IsReturnLikeMethod() || symbol.IsThrowLikeMethod();
        }

        protected abstract SyntaxNode FindEnclosingExpression(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext, TInvocationExpressionSyntax invocationExpression);

        protected abstract bool IsFollowedBySetupInvocation(SyntaxNodeAnalysisContext syntaxNodeContext, SyntaxNode invocationExpressionSyntax);

        protected abstract bool IsPrecededByReceivedInvocation(SyntaxNodeAnalysisContext syntaxNodeContext, SyntaxNode invocationExpressionSyntax);

        private void BeginAnalyzeWhenLikeMethod(SyntaxNodeAnalysisContext syntaxNodeContext, TInvocationExpressionSyntax invocationExpression)
        {
            foreach (var syntaxNode in _substitutionNodeFinder.FindForWhenExpression(syntaxNodeContext, invocationExpression))
            {
                var symbol = syntaxNodeContext.SemanticModel.GetSymbolInfo(syntaxNode).Symbol;
                var actualNode = syntaxNode is TMemberAccessExpression && symbol is IMethodSymbol _
                    ? syntaxNode.Parent
                    : syntaxNode;

                WhenNodes.Add(actualNode);
            }
        }

        private void BeginAnalyzeReceivedInOrderMethod(SyntaxNodeAnalysisContext syntaxNodeContext, TInvocationExpressionSyntax invocationExpression)
        {
            foreach (var syntaxNode in _substitutionNodeFinder
                .FindForReceivedInOrderExpression(syntaxNodeContext, invocationExpression).ToList())
            {
                var symbol = syntaxNodeContext.SemanticModel.GetSymbolInfo(syntaxNode).Symbol;
                var actualNode = syntaxNode is TMemberAccessExpression && symbol is IMethodSymbol _
                    ? syntaxNode.Parent
                    : syntaxNode;

                ReceivedInOrderNodes.Add(actualNode);
            }
        }

        private void BeginAnalyzeArgLikeMethod(SyntaxNodeAnalysisContext syntaxNodeContext, TInvocationExpressionSyntax invocationExpression)
        {
            // find enclosing type
            var enclosingExpression = FindEnclosingExpression(syntaxNodeContext, invocationExpression);

            if (enclosingExpression != null &&
                IsFollowedBySetupInvocation(syntaxNodeContext, enclosingExpression) == false &&
                IsPrecededByReceivedInvocation(syntaxNodeContext, enclosingExpression) == false)
            {
                if (PotentialMissusedNodes.TryGetValue(enclosingExpression, out var nodes))
                {
                    nodes.Add(invocationExpression);
                }
                else
                {
                    PotentialMissusedNodes.Add(enclosingExpression, new List<SyntaxNode> { invocationExpression });
                }
            }
        }
    }
}