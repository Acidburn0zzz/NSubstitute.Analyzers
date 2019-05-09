using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute.Analyzers.CSharp.DiagnosticAnalyzers;
using NSubstitute.Analyzers.Tests.Shared.DiagnosticAnalyzers;

namespace NSubstitute.Analyzers.Tests.CSharp.DiagnosticAnalyzerTests.AwaitedWhenCallAnalyzerTests
{
    public class AwaitedWhenCallAnalyzerToggleTests : AwaitedWhenCallAnalyzerToggleVerifier
    {
        protected override DiagnosticAnalyzer GetAwaitedWhenCallAnalyzer()
        {
            return new AwaitedWhenCallAnalyzer();
        }
    }
}