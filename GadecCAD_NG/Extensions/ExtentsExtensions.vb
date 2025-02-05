'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module ExtentsExtensions

        ''' <summary>
        ''' Converts the <see cref="Extents3d"/> to a <see cref="Extents2d"/>.
        ''' </summary>
        ''' <param name="eExtents3d"></param>
        ''' <returns>The resulting <see cref="Extents2d"/>.</returns>
        <Extension()>
        Public Function ConvertTo2d(ByVal eExtents3d As Extents3d) As Extents2d
            Dim min2D = eExtents3d.MinPoint.Convert2d(New Plane)
            Dim max2D = eExtents3d.MaxPoint.Convert2d(New Plane)
            Return New Extents2d(min2D, max2D)
        End Function

        ''' <summary>
        ''' Gets the extents-points of the <see cref="Extents3d"/>.
        ''' </summary>
        ''' <param name="eExtents3d"></param>
        ''' <returns>A <see cref="Point3dCollection"/> with the four <see cref="Point3d"/>.</returns>
        <Extension()>
        Public Function GetSurroundingPoints(ByVal eExtents3d As Extents3d) As Point3dCollection
            Return New Point3dCollection From {
                eExtents3d.MinPoint,
                New Point3d(eExtents3d.MinPoint.X, eExtents3d.MaxPoint.Y, (eExtents3d.MaxPoint.Z - eExtents3d.MinPoint.Z) / 2),
                eExtents3d.MaxPoint,
                New Point3d(eExtents3d.MaxPoint.X, eExtents3d.MinPoint.Y, (eExtents3d.MaxPoint.Z - eExtents3d.MinPoint.Z) / 2)
            }
        End Function

        ''' <summary>
        ''' Gets the extents-points of the <see cref="Extents2d"/>.
        ''' </summary>
        ''' <param name="eExtents2d"></param>
        ''' <returns>A <see cref="Point2dCollection"/> with the four <see cref="Point2d"/>.</returns>
        <Extension()>
        Public Function GetSurroundingPoints(ByVal eExtents2d As Extents2d) As Point2dCollection
            Return New Point2dCollection From {
                eExtents2d.MinPoint,
                New Point2d(eExtents2d.MinPoint.X, eExtents2d.MaxPoint.Y),
                eExtents2d.MaxPoint,
                New Point2d(eExtents2d.MaxPoint.X, eExtents2d.MinPoint.Y)
            }
        End Function

    End Module

End Namespace
