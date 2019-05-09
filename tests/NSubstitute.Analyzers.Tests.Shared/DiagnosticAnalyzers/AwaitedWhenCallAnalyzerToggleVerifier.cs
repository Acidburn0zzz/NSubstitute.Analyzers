using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace NSubstitute.Analyzers.Tests.Shared.DiagnosticAnalyzers
{
    public abstract class AwaitedWhenCallAnalyzerToggleVerifier
    {
        public static IEnumerable<object[]> SupportedVersionsTestCases
        {
            get
            {
                yield return new object[] { new Version(4, 1, 0, 0) };
                yield return new object[] { new Version(4, 2, 0, 0) };
            }
        }

        public static IEnumerable<object[]> NotSupportedVersionsTestCases
        {
            get
            {
                yield return new object[] { new Version(4, 0, 0, 0) };
                yield return new object[] { new Version(3, 1, 0, 0) };
            }
        }

        [Theory]
        [MemberData(nameof(SupportedVersionsTestCases))]
        public void AnalyzerIsEnabled_WhenCompilationContains_SupportedNSubstituteVersion(Version version)
        {
            VerifyRegisteredNodeActions(version, 1);
        }

        [Theory]
        [MemberData(nameof(NotSupportedVersionsTestCases))]
        public void AnalyzerIsNotEnabled_WhenCompilationDoesNotContain_SupportedNSubstituteVersion(Version version)
        {
            VerifyRegisteredNodeActions(version, 0);
        }

        protected abstract DiagnosticAnalyzer GetAwaitedWhenCallAnalyzer();

        private void VerifyRegisteredNodeActions(Version substituteVersion, int expectedRegisteredNodeActions)
        {
            var analyzer = GetAwaitedWhenCallAnalyzer();
            var analysisContext = Substitute.For<AnalysisContext>();
            var compilation = Substitute.For<Compilation>(string.Empty, null, null, false, null);
            compilation.ReferencedAssemblyNames.Returns(new[]
            {
                new AssemblyIdentity("OtherAssembly", substituteVersion),
                new AssemblyIdentity("NSubstitute", substituteVersion)
            });

            var compilationStartAnalysisContext = new FakeCompilationStartAnalysisContext(compilation);

            analysisContext.When(context =>
                    context.RegisterCompilationStartAction(Arg.Any<Action<CompilationStartAnalysisContext>>()))
                .Do(callInfo =>
                    callInfo.Arg<Action<CompilationStartAnalysisContext>>()(compilationStartAnalysisContext));

            analyzer.Initialize(analysisContext);

            compilationStartAnalysisContext.RegisteredNodeActions.Should().Be(expectedRegisteredNodeActions);
        }

        private class FakeCompilationStartAnalysisContext : CompilationStartAnalysisContext
        {
            public int RegisteredNodeActions { get; private set; }

            public FakeCompilationStartAnalysisContext(Compilation compilation)
                : base(compilation, new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty), CancellationToken.None)
            {
            }

            public override void RegisterCompilationEndAction(Action<CompilationAnalysisContext> action)
            {
                RegisteredNodeActions++;
            }

            public override void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action)
            {
                RegisteredNodeActions++;
            }

            public override void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
            {
                RegisteredNodeActions++;
            }

            public override void RegisterCodeBlockStartAction<TLanguageKindEnum>(
                Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
            {
                RegisteredNodeActions++;
            }

            public override void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action)
            {
                RegisteredNodeActions++;
            }

            public override void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action)
            {
                RegisteredNodeActions++;
            }

            public override void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds)
            {
                RegisteredNodeActions++;
            }
        }
    }
}