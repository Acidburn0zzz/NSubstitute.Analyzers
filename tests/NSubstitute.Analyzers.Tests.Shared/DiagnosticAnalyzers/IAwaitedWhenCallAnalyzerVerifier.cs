using System.Threading.Tasks;

namespace NSubstitute.Analyzers.Tests.Shared.DiagnosticAnalyzers
{
    public interface IAwaitedWhenCallAnalyzerVerifier
    {
        Task ReportsDiagnostics_WhenAsyncMethod_IsAwaited(string method);

        Task ReportsNoDiagnostics_WhenAsyncMethod_IsNotAwaited(string method);

        Task ReportsNoDiagnostics_WhenUnfortunatelyNamedAsyncMethod_IsAwaited(string method);
    }
}