'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.Geometry
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module Point3dExtensions

        ''' <summary>
        ''' Gets a <see cref="Point3d"/> at the specified distance at the specified angle from the original <see cref="Point3d"/>.
        ''' </summary>
        ''' <param name="ePoint3d"></param>
        ''' <param name="angle">The angle.</param>
        ''' <param name="distance">The distance.</param>
        ''' <returns>The resulting <see cref="Point3d"/>.</returns>
        <Extension()>
        Public Function GetPolarPoint(ByVal ePoint3d As Point3d, angle As Double, distance As Double) As Point3d
            Dim newX = ePoint3d.X + distance * Math.Cos(angle)
            Dim newY = ePoint3d.Y + distance * Math.Sin(angle)
            Return New Point3d(newX, newY, ePoint3d.Z)
        End Function

        ''' <summary>
        ''' Gets the <see cref="Point2d"/> portion (X and Y) of a <see cref="Point3d"/>.
        ''' </summary>
        ''' <param name="ePoint3d"></param>
        ''' <returns>The <see cref="Point2d"/>.</returns>
        <Extension()>
        Public Function GetPoint2d(ByVal ePoint3d As Point3d) As Point2d
            Return New Point2d(ePoint3d.X, ePoint3d.Y)
        End Function

    End Module

End Namespace
