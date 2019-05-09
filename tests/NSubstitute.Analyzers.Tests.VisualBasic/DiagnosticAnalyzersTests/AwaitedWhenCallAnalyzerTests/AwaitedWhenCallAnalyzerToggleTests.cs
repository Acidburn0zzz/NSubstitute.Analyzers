using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute.Analyzers.Tests.Shared.DiagnosticAnalyzers;
using NSubstitute.Analyzers.VisualBasic.DiagnosticAnalyzers;

namespace NSubstitute.Analyzers.Tests.VisualBasic.DiagnosticAnalyzersTests.AwaitedWhenCallAnalyzerTests
{
    public class AwaitedWhenCallAnalyzerToggleTests : AwaitedWhenCallAnalyzerToggleVerifier
    {
        protected override DiagnosticAnalyzer GetAwaitedWhenCallAnalyzer()
        {
            return new AwaitedWhenCallAnalyzer();
        }
    }
}