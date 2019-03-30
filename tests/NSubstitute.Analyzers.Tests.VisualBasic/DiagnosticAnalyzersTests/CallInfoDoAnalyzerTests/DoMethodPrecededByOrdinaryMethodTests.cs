using System.Threading.Tasks;
using NSubstitute.Analyzers.Tests.Shared.Extensibility;

namespace NSubstitute.Analyzers.Tests.VisualBasic.DiagnosticAnalyzersTests.CallInfoDoAnalyzerTests
{
    [CombinatoryData("SubstituteExtensions.When", "SubstituteExtensions.WhenForAnyArgs")]
    public class DoMethodPrecededByOrdinaryMethodTests : CallInfoDoAnalyzerVerifier
    {
        public override async Task ReportsNoDiagnostics_WhenSubstituteMethodCannotBeInferred(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer) As Integer
        ReadOnly Property Barr As Integer
        Default  ReadOnly Property Item(ByVal x As Integer) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            Dim returnValue = {method}(mock, Function(substitute) 
                                                Dim x = {call}
                                           End Function)

            returnValue.Do(Function(callInfo)
                                {argAccess}
                           End Function)
        End Sub
    End Class
End Namespace";

            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsDiagnostic_WhenAccessingArgumentOutOfBounds(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer) As Integer
        ReadOnly Property Barr As Integer
        Default ReadOnly Property Item(ByVal x As Integer) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyDiagnostic(source, CallInfoArgumentOutOfRangeDescriptor, "There is no argument at position 1");
        }

        public override async Task ReportsNoDiagnostic_WhenAccessingArgumentOutOfBound_AndPositionIsNotLiteralExpression(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer, ByVal y As Integer) As Integer
        ReadOnly Property Barr As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal y As Integer) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace";
            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostic_WhenAccessingArgumentWithinBounds(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer, ByVal y As Integer) As Integer
        ReadOnly Property Barr As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal y As Integer) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostic_WhenManuallyCasting_ToSupportedType(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer, ByVal y As Bar) As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal y As Bar) As Integer
    End Interface

    Public Class BarBase
    End Class

    Public Class Bar
        Inherits BarBase
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace";

            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsDiagnostic_WhenManuallyCasting_ToUnsupportedType(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer, ByVal y As Double) As Integer
        Function Foo(ByVal x As Integer, ByVal bar As FooBar) As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal y As Double) As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal bar As FooBar) As Integer
    End Interface

    Public Class Bar
    End Class

    Public Class FooBar
        Inherits Bar
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace
";

            await VerifyDiagnostic(source, CallInfoCouldNotConvertParameterAtPositionDescriptor, "Couldn't convert parameter at position 1 to type MyNamespace.Bar.");
        }

        public override async Task ReportsNoDiagnostic_WhenCasting_WithArgAt_ToSupportedType(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer, ByVal y As Bar) As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal y As Bar) As Integer
    End Interface

    Public Class BarBase
    End Class

    Public Class Bar
        Inherits BarBase
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace";

            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsDiagnostic_WhenCasting_WithArgAt_ToUnsupportedType(string method, string call, string argAccess, string message)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer, ByVal y As Double) As Integer
        Function Foo(ByVal x As Integer, ByVal bar As FooBar) As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal y As Double) As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal bar As FooBar) As Integer
    End Interface

    Public Class Bar
    End Class

    Public Class FooBar
        Inherits Bar
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyDiagnostic(source, CallInfoCouldNotConvertParameterAtPositionDescriptor, message);
        }

        public override async Task ReportsNoDiagnostic_WhenCastingElementsFromArgTypes(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Bar) As Integer
        Default ReadOnly Property Item(ByVal x As Bar) As Integer
    End Interface

    Public Class Bar
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostic_WhenAssigningValueToNotRefNorOutArgumentViaIndirectCall(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Bar) As Integer
        Default ReadOnly Property Item(ByVal x As Bar) As Integer
    End Interface

    Public Class Bar
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsDiagnostic_WhenAccessingArgumentByTypeNotInInvocation(string method, string call, string argAccess, string message)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer) As Integer
        ReadOnly Property Barr As Integer
        Default ReadOnly Property Item(ByVal x As Integer) As Integer

    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace
";

            await VerifyDiagnostic(source, CallInfoCouldNotFindArgumentToThisCallDescriptor, message);
        }

        public override async Task ReportsNoDiagnostic_WhenAccessingArgumentByTypeInInInvocation(string method, string call, string argAccess)
        {
            var source = $@"Imports System
Imports NSubstitute

Namespace MyNamespace
    Interface IFoo
        Function Bar(ByVal x As Integer) As Integer
        Function Bar(ByVal x As Foo) As Integer
        Default ReadOnly Property Item(ByVal x As Integer) As Integer
        Default ReadOnly Property Item(ByVal x As Foo) As Integer
    End Interface

    Public Class FooBase
    End Class

    Public Class Foo
        Inherits FooBase
    End Class

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of IFoo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace
";

            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsDiagnostic_WhenAccessingArgumentByTypeMultipleTimesInInvocation(string method, string call, string argAccess, string message)
        {
            var source = $@"Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer, ByVal y As Integer) As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal y As Integer) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               {argAccess}
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyDiagnostic(source, CallInfoMoreThanOneArgumentOfTypeDescriptor, message);
        }

        public override async Task ReportsNoDiagnostic_WhenAccessingArgumentByTypeMultipleDifferentTypesInInvocation(string method, string call)
        {
            var source = $@"Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer, ByVal y As Double) As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal y As Double) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               callInfo.Arg(Of Integer)()
                                           End Function)
        End Sub
    End Class
End Namespace
";

            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsDiagnostic_WhenAssigningValueToNotOutNorRefArgument(string method, string call)
        {
            var source = $@"Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByVal x As Integer, ByVal y As Double) As Integer
        Default ReadOnly Property Item(ByVal x As Integer, ByVal y As Double) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = {call}
                          End Function).Do(Function(callInfo)
                                               [|callInfo(1)|] = 1
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyDiagnostic(source, CallInfoArgumentIsNotOutOrRefDescriptor, "Could not set argument 1 (Double) as it is not an out or ref argument.");
        }

        public override async Task ReportsNoDiagnostic_WhenAssigningValueToRefArgument(string method)
        {
            var source = $@"Imports NSubstitute

Namespace MyNamespace
    Interface Foo
        Function Bar(ByRef x As Integer) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim value As Integer = 0
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = substitute.Bar(value)
                          End Function).Do(Function(callInfo)
                                               callInfo(0) = 1
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsNoDiagnostic_WhenAssigningValueToOutArgument(string method)
        {
            var source = $@"Imports NSubstitute
Imports System.Runtime.InteropServices

Namespace MyNamespace
    Interface Foo
        Function Bar(<Out> ByRef x As Integer) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim value As Integer = 0
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = substitute.Bar(value)
                          End Function).Do(Function(callInfo)
                                               callInfo(0) = 1
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyNoDiagnostic(source);
        }

        public override async Task ReportsDiagnostic_WhenAssigningValueToOutOfBoundsArgument(string method)
        {
            var source = $@"Imports NSubstitute
Imports System.Runtime.InteropServices

Namespace MyNamespace
    Interface Foo
        Function Bar(<Out> ByRef x As Integer) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim value As Integer = 0
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = substitute.Bar(value)
                          End Function).Do(Function(callInfo)
                                               [|callInfo(1)|] = 1
                                           End Function)
        End Sub
    End Class
End Namespace
";
            await VerifyDiagnostic(source, CallInfoArgumentOutOfRangeDescriptor, "There is no argument at position 1");
        }

        public override async Task ReportsDiagnostic_WhenAssigningType_NotAssignableTo_Argument(string method, string left, string right, string expectedMessage)
        {
            var source = $@"Imports NSubstitute
Imports System.Runtime.InteropServices
Imports System.Collections.Generic

Namespace MyNamespace
    Interface Foo
        Function Bar(<Out> ByRef x As {left}) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim value As {left} = Nothing
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = substitute.Bar(value)
                          End Function).Do(Function(callInfo)
                                               [|callInfo(0)|] = {right}
                                           End Function)
        End Sub
    End Class
End Namespace
";

            await VerifyDiagnostic(source, CallInfoArgumentSetWithIncompatibleValueDescriptor, expectedMessage);
        }

        public override async Task ReportsNoDiagnostic_WhenAssigningType_AssignableTo_Argument(string method, string left, string right)
        {
            var source = $@"Imports NSubstitute
Imports System.Runtime.InteropServices
Imports System.Collections.Generic

Namespace MyNamespace
    Interface Foo
        Function Bar(<Out> ByRef x As {left}) As Integer
    End Interface

    Public Class FooTests
        Public Sub Test()
            Dim value As {left} = Nothing
            Dim mock = NSubstitute.Substitute.[For](Of Foo)()
            {method}(mock, Function(substitute) 
                             Dim x = substitute.Bar(value)
                          End Function).Do(Function(callInfo)
                                               callInfo(0) = {right}
                                           End Function)
        End Sub
    End Class
End Namespace
";

            await VerifyNoDiagnostic(source);
        }
    }
}