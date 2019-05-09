using System.Threading.Tasks;
using NSubstitute.Analyzers.Tests.Shared.Extensibility;

namespace NSubstitute.Analyzers.Tests.VisualBasic.DiagnosticAnalyzersTests.AwaitedWhenCallAnalyzerTests
{
    [CombinatoryData("When", "WhenForAnyArgs")]
    public class WhenAsExtensionMethodTests : AwaitedWhenCallAnalyzerVerifier
    {
        public override async Task ReportsDiagnostics_WhenAsyncMethod_IsAwaited(string method)
        {
            var source = $@"Imports System.Threading.Tasks
Imports NSubstitute

Namespace MyNamespace
    Public Class Foo
        Public Overridable Async Function Bar() As Task(Of Integer)
            Await Task.FromResult(1)
        End Function
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Foo)()
            substitute.{method}([|Async Function(callInfo) Await callInfo.Bar()|])
        End Sub
    End Class
End Namespace";

            await VerifyDiagnostic(source, AwaitedWhenCallDescriptor);
        }

        public override async Task ReportsNoDiagnostics_WhenAsyncMethod_IsNotAwaited(string method)
        {
            var source = $@"Imports System.Threading.Tasks
Imports NSubstitute

Namespace MyNamespace
    Public Class Foo
        Public Overridable Async Function Bar() As Task(Of Integer)
            Await Task.FromResult(1)
        End Function
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Foo)()
            substitute.{method}(Function(callInfo) callInfo.Bar())
        End Sub
    End Class
End Namespace";

            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostics_WhenUnfortunatelyNamedAsyncMethod_IsAwaited(string method)
        {
            var source = $@"Imports System.Threading.Tasks
Imports NSubstitute
Imports System.Runtime.CompilerServices

Namespace MyNamespace
    Public Class Foo
        Public Overridable Async Function Bar() As Task(Of Integer)
            Await Task.FromResult(1)
        End Function
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim substitute = NSubstitute.Substitute.[For](Of Foo)()
            substitute.{method}(Async Function(callInfo) Await callInfo.Bar(), 1)
        End Sub
    End Class

    Module SubstituteExtensions
        <Extension()>
        Function [When](Of T)(ByVal substitute As T, ByVal substituteCall As System.Action(Of T), ByVal x As Integer) As T
            Return Nothing
        End Function

        <Extension()>
        Function WhenForAnyArgs(Of T)(ByVal substitute As T, ByVal substituteCall As System.Action(Of T), ByVal x As Integer) As T
            Return Nothing
        End Function
    End Module
End Namespace";

            await VerifyNoDiagnostic(source);
        }
    }
}