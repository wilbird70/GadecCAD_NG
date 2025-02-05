'Gadec Engineerings Software (c) 2022
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module DoubleExtensions

        ''' <summary>
        ''' Determine if the number is equal to the other within the tolerance related to the active snapunit.
        ''' </summary>
        ''' <param name="eDouble"></param>
        ''' <param name="compareTo">The number to compare with.</param>
        ''' <returns>True is equal within tolerance.</returns>
        <Extension()>
        Public Function IsEqual(ByVal eDouble As Double, compareTo As Double) As Boolean
            Dim snapunit = CDbl(SysVarHandler.GetVar("SNAPUNIT").X)
            Dim tolerance = snapunit / 50
            Return eDouble < (compareTo + tolerance) And eDouble > (compareTo - tolerance)
        End Function

        ''' <summary>
        ''' Gets a <see cref="Between"/> enum value depending on where the value is relative to the start and end values.
        ''' </summary>
        ''' <param name="eDouble"></param>
        ''' <param name="startValue">The start value.</param>
        ''' <param name="endValue">The end value.</param>
        ''' <returns>A <see cref="Between"/> value.</returns>
        <Extension()>
        Public Function GetPostionBetween(ByVal eDouble As Double, startValue As Double, endValue As Double) As Between
            Select Case True
                Case eDouble.IsEqual(startValue) : Return Between.onStartPoint
                Case eDouble.IsEqual(endValue) : Return Between.onEndPoint
                Case eDouble.IsEqual((startValue + endValue) / 2) : Return Between.onMidPoint
                Case eDouble - startValue < endValue - eDouble : Return Between.onFirstHalf
                Case Else : Return Between.onSecondHalf
            End Select
        End Function

    End Module

End Namespace
