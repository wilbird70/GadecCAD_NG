'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module LineExtensions

        ''' <summary>
        ''' Gets a <see cref="LineAlignment"/> enum value depending on whether the line is horizontal, vertical or diagonal.
        ''' <para>Note: Horizontal or vertical within the tolerance related to the active snapunit.</para>
        ''' </summary>
        ''' <param name="eLine"></param>
        ''' <returns>A <see cref="LineAlignment"/> value.</returns>
        <Extension()>
        Public Function Alignment(ByVal eLine As Line) As LineAlignment
            If eLine.StartPoint.X.IsEqual(eLine.EndPoint.X) Then Return LineAlignment.vertical
            If eLine.StartPoint.Y.IsEqual(eLine.EndPoint.Y) Then Return LineAlignment.horizontal
            Return LineAlignment.diagonal
        End Function

        ''' <summary>
        ''' When the line is horizontal or vertical within the tolerance related to the active snapunit, then the start and end points will be aligned.
        ''' <para>Also the points will be swapped when endpoint has the lower values.</para>
        ''' </summary>
        ''' <param name="eLine"></param>
        <Extension()>
        Public Sub Sequence(ByRef eLine As Line)
            Select Case True
                Case eLine.StartPoint.X.IsEqual(eLine.EndPoint.X)
                    Dim valueX = Averaging(eLine.StartPoint.X, eLine.EndPoint.X) 'opheffen tolerantie
                    Dim valuesY = Ordering(eLine.StartPoint.Y, eLine.EndPoint.Y)
                    Dim valueZ = Averaging(eLine.StartPoint.Z, eLine.EndPoint.Z) 'opheffen verschil
                    eLine.StartPoint = New Point3d(valueX, valuesY.Lowest, valueZ)
                    eLine.EndPoint = New Point3d(valueX, valuesY.Highest, valueZ)
                Case eLine.StartPoint.Y.IsEqual(eLine.EndPoint.Y)
                    Dim valuesX = Ordering(eLine.StartPoint.X, eLine.EndPoint.X)
                    Dim valueY = Averaging(eLine.StartPoint.Y, eLine.EndPoint.Y) 'opheffen tolerantie
                    Dim valueZ = Averaging(eLine.StartPoint.Z, eLine.EndPoint.Z) 'opheffen verschil
                    eLine.StartPoint = New Point3d(valuesX.Lowest, valueY, valueZ)
                    eLine.EndPoint = New Point3d(valuesX.Highest, valueY, valueZ)
            End Select
        End Sub

        'private functions

        ''' <summary>
        ''' Orders the values in a <see cref="Tuple"/> with the lowest and highest value.
        ''' </summary>
        ''' <param name="first">The first value.</param>
        ''' <param name="second">The second value.</param>
        ''' <returns>A <see cref="Tuple"/> with lowest and highest value.</returns>
        Private Function Ordering(first As Double, second As Double) As (Lowest As Double, Highest As Double)
            Select Case first < second
                Case True : Return (first, second)
                Case Else : Return (second, first)
            End Select
        End Function

        ''' <summary>
        ''' Averages the two values.
        ''' </summary>
        ''' <param name="first">The first value.</param>
        ''' <param name="second">The second value.</param>
        ''' <returns>The average.</returns>
        Private Function Averaging(first As Double, second As Double) As Double
            Return (first + second) / 2
        End Function

    End Module

End Namespace
