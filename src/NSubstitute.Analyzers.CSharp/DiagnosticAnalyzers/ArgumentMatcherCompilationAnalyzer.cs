using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute.Analyzers.Shared;
using NSubstitute.Analyzers.Shared.DiagnosticAnalyzers;

namespace NSubstitute.Analyzers.CSharp.DiagnosticAnalyzers
{
    internal class ArgumentMatcherCompilationAnalyzer : AbstractArgumentMatcherCompilationAnalyzer<InvocationExpressionSyntax, MemberAccessExpressionSyntax>
    {
        private static ImmutableArray<ImmutableArray<int>> AncestorPaths { get; } = ImmutableArray.Create(
            ImmutableArray.Create(
                (int)SyntaxKind.Argument,
                (int)SyntaxKind.ArgumentList,
                (int)SyntaxKind.InvocationExpression),
            ImmutableArray.Create(
                (int)SyntaxKind.Argument,
                (int)SyntaxKind.BracketedArgumentList,
                (int)SyntaxKind.ElementAccessExpression));

        public ArgumentMatcherCompilationAnalyzer(ISubstitutionNodeFinder<InvocationExpressionSyntax> substitutionNodeFinder, IDiagnosticDescriptorsProvider diagnosticDescriptorsProvider)
            : base(substitutionNodeFinder, diagnosticDescriptorsProvider)
        {
        }

        protected override ImmutableArray<ImmutableArray<int>> PossibleAncestorPaths { get; } = AncestorPaths;

        protected override bool IsFollowedBySetupInvocation(SyntaxNodeAnalysisContext syntaxNodeContext, SyntaxNode invocationExpressionSyntax)
        {
            var parentNote = invocationExpressionSyntax.Parent;

            if (parentNote is MemberAccessExpressionSyntax)
            {
                var child = parentNote.ChildNodes().Except(new[] { invocationExpressionSyntax }).FirstOrDefault();

                return child != null && IsSetupLikeMethod(syntaxNodeContext.SemanticModel.GetSymbolInfo(child).Symbol);
            }

            if (parentNote is ArgumentSyntax)
            {
                var operation = syntaxNodeContext.SemanticModel.GetOperation(parentNote);
                return IsSetupLikeMethod(syntaxNodeContext.SemanticModel.GetSymbolInfo(operation.Parent.Syntax).Symbol);
            }

            return false;
        }
    }
}