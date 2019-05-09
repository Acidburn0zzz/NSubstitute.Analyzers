using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute.Analyzers.CSharp;
using NSubstitute.Analyzers.CSharp.DiagnosticAnalyzers;
using NSubstitute.Analyzers.Shared;
using NSubstitute.Analyzers.Tests.Shared.DiagnosticAnalyzers;
using NSubstitute.Analyzers.Tests.Shared.Extensibility;
using Xunit;

namespace NSubstitute.Analyzers.Tests.CSharp.DiagnosticAnalyzerTests.AwaitedWhenCallAnalyzerTests
{
    public abstract class AwaitedWhenCallAnalyzerVerifier : CSharpDiagnosticVerifier, IAwaitedWhenCallAnalyzerVerifier
    {
        protected DiagnosticDescriptor AwaitedWhenCallDescriptor { get; } = DiagnosticDescriptors<DiagnosticDescriptorsProvider>.AwaitedWhenCall;

        [CombinatoryTheory]
        [InlineData]
        public abstract Task ReportsDiagnostics_WhenAsyncMethod_IsAwaited(string method);

        [CombinatoryTheory]
        [InlineData]
        public abstract Task ReportsNoDiagnostics_WhenAsyncMethod_IsNotAwaited(string method);

        [CombinatoryTheory]
        [InlineData]
        public abstract Task ReportsNoDiagnostics_WhenUnfortunatelyNamedAsyncMethod_IsAwaited(string method);

        protected override DiagnosticAnalyzer GetDiagnosticAnalyzer()
        {
            return new AwaitedWhenCallAnalyzer();
        }
    }
}