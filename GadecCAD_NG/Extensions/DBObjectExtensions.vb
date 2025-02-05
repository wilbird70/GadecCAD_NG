'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module DBObjectExtensions

        ''' <summary>
        ''' Gets the last part of the type-description of the <see cref="DBObject"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The partly type-description</returns>
        <Extension()>
        Public Function GetDBObjectType(ByVal eDBObject As DBObject) As String
            Try
                Dim type = eDBObject.GetType.ToString
                Return type.MidString(InStrRev(type, ".") + 1)
            Catch ex As System.Exception
                Return ""
            End Try
        End Function

        ''' <summary>
        ''' Casts the <see cref="DBObject"/> to a <see cref="BlockReference"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The <see cref="BlockReference"/> or Nothing if cast fails.</returns>
        <Extension()>
        Public Function CastAsBlockReference(ByVal eDBObject As DBObject) As BlockReference
            Try
                Return DirectCast(eDBObject, BlockReference)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Casts the <see cref="DBObject"/> to a <see cref="AttributeReference"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The <see cref="AttributeReference"/> or Nothing if cast fails.</returns>
        <Extension()>
        Public Function CastAsAttributeReference(ByVal eDBObject As DBObject) As AttributeReference
            Try
                Return DirectCast(eDBObject, AttributeReference)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Casts the <see cref="DBObject"/> to a <see cref="AttributeDefinition"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The <see cref="AttributeDefinition"/> or Nothing if cast fails.</returns>
        <Extension()>
        Public Function CastAsAttributeDefinition(ByVal eDBObject As DBObject) As AttributeDefinition
            Try
                Return DirectCast(eDBObject, AttributeDefinition)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Casts the <see cref="DBObject"/> to a <see cref="DBText"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The <see cref="DBText"/> or Nothing if cast fails.</returns>
        <Extension()>
        Public Function CastAsDBText(ByVal eDBObject As DBObject) As DBText
            Try
                Return DirectCast(eDBObject, DBText)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Casts the <see cref="DBObject"/> to a <see cref="Line"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The <see cref="Line"/> or Nothing if cast fails.</returns>
        <Extension()>
        Public Function CastAsLine(ByVal eDBObject As DBObject) As Line
            Try
                Return DirectCast(eDBObject, Line)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Casts the <see cref="DBObject"/> to a <see cref="Polyline"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The <see cref="Polyline"/> or Nothing if cast fails.</returns>
        <Extension()>
        Public Function CastAsPolyline(ByVal eDBObject As DBObject) As Polyline
            Try
                Return DirectCast(eDBObject, Polyline)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Casts the <see cref="DBObject"/> to a <see cref="Circle"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The <see cref="Circle"/> or Nothing if cast fails.</returns>
        <Extension()>
        Public Function CastAsCircle(ByVal eDBObject As DBObject) As Circle
            Try
                Return DirectCast(eDBObject, Circle)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Casts the <see cref="DBObject"/> to a <see cref="Ellipse"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The <see cref="Ellipse"/> or Nothing if cast fails.</returns>
        <Extension()>
        Public Function CastAsEllipse(ByVal eDBObject As DBObject) As Ellipse
            Try
                Return DirectCast(eDBObject, Ellipse)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Casts the <see cref="DBObject"/> to a <see cref="Dimension"/>.
        ''' </summary>
        ''' <param name="eDBObject"></param>
        ''' <returns>The <see cref="Dimension"/> or Nothing if cast fails.</returns>
        <Extension()>
        Public Function CastAsDimension(ByVal eDBObject As DBObject) As Dimension
            Try
                Return DirectCast(eDBObject, Dimension)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

    End Module

End Namespace