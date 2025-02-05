'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module TransactionExtensions

        ''' <summary>
        ''' Gets the <see cref="DBObject"/> with the specified <see cref="ObjectId"/> in read mode.
        ''' <para>Note: Extends the GetObject methode for <see cref="Transaction"/> for use without the need to specify the <see cref="OpenMode.ForRead"/>.</para>
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <returns>The <see cref="DBObject"/>.</returns>
        <Extension()>
        Public Function GetObject(ByVal eTransaction As Transaction, objectId As ObjectId) As DBObject
            Try
                Return eTransaction.GetObject(objectId, OpenMode.ForRead)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Determine if the specified <see cref="Entity"/> is in Model- or Paperspace.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="entity">The <see cref="Entity"/>.</param>
        ''' <returns>True if <see cref="Entity"/> is in Model- or Paperspace.</returns>
        <Extension()>
        Public Function EntityIsInModelOrPaperspace(ByVal eTransaction As Transaction, entity As Entity) As Boolean
            Dim btr = eTransaction.GetBlockTableRecord(entity.OwnerId)
            If IsNothing(btr) Then Return False

            Return btr.Name.ToLower = "*model_space" Or btr.Name.ToLower = "*paper_space"
        End Function

        'database

        ''' <summary>
        ''' Gets the <see cref="BlockTable"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <param name="openErased">Use True to get also an erased object.</param>
        ''' <returns>The <see cref="BlockTable"/>.</returns>
        <Extension()>
        Public Function GetBlockTable(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead, Optional openErased As Nullable(Of Boolean) = Nothing) As BlockTable
            Try
                Select Case IsNothing(openErased)
                    Case True : Return DirectCast(eTransaction.GetObject(objectId, openMode), BlockTable)
                    Case Else : Return DirectCast(eTransaction.GetObject(objectId, openMode, openErased), BlockTable)
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="BlockTableRecord"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <param name="openErased">Use True to get also an erased object.</param>
        ''' <returns>The <see cref="BlockTableRecord"/>.</returns>
        <Extension()>
        Public Function GetBlockTableRecord(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead, Optional openErased As Nullable(Of Boolean) = Nothing) As BlockTableRecord
            Try
                Dim dbObject As DBObject
                Select Case IsNothing(openErased)
                    Case True : dbObject = eTransaction.GetObject(objectId, openMode)
                    Case Else : dbObject = eTransaction.GetObject(objectId, openMode, openErased)
                End Select
                Select Case dbObject.GetDBObjectType = "BlockTableRecord"
                    Case True : Return DirectCast(dbObject, BlockTableRecord)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="LayerTable"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="LayerTable"/>.</returns>
        <Extension()>
        Public Function GetLayerTable(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As LayerTable
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), LayerTable)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="LayerTableRecord"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="LayerTableRecord"/>.</returns>
        <Extension()>
        Public Function GetLayerTableRecord(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As LayerTableRecord
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), LayerTableRecord)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="TextStyleTable"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="TextStyleTable"/>.</returns>
        <Extension()>
        Public Function GetTextStyleTable(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As TextStyleTable
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), TextStyleTable)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="TextStyleTableRecord"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="TextStyleTableRecord"/>.</returns>
        <Extension()>
        Public Function GetTextStyleTableRecord(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As TextStyleTableRecord
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), TextStyleTableRecord)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="LinetypeTable"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="LinetypeTable"/>.</returns>
        <Extension()>
        Public Function GetLinetypeTable(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As LinetypeTable
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), LinetypeTable)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Viewport"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Viewport"/>.</returns>
        <Extension()>
        Public Function GetViewport(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Viewport
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), Viewport)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        'named objects dictionary

        ''' <summary>
        ''' Gets the <see cref="Layout"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Layout"/>.</returns>
        <Extension()>
        Public Function GetLayout(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Layout
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), Layout)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Group"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Group"/>.</returns>
        <Extension()>
        Public Function GetGroup(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Group
            Try
                Dim dbObject = eTransaction.GetObject(objectId, openMode)
                Select Case dbObject.GetDBObjectType = "Group"
                    Case True : Return DirectCast(dbObject, Group)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="DrawOrderTable"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="DrawOrderTable"/>.</returns>
        <Extension()>
        Public Function GetDrawOrderTable(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As DrawOrderTable
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), DrawOrderTable)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="DBDictionary"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="DBDictionary"/>.</returns>
        <Extension()>
        Public Function GetDBDictionary(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As DBDictionary
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), DBDictionary)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Xrecord"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Xrecord"/>.</returns>
        <Extension()>
        Public Function GetXrecord(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Xrecord
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), Xrecord)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        'tekenobjecten

        ''' <summary>
        ''' Gets the <see cref="BlockReference"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="BlockReference"/>.</returns>
        <Extension()>
        Public Function GetBlockReference(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As BlockReference
            Try
                Dim dbObject = eTransaction.GetObject(objectId, openMode)
                Select Case dbObject.GetDBObjectType = "BlockReference"
                    Case True : Return DirectCast(dbObject, BlockReference)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="AttributeDefinition"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="AttributeDefinition"/>.</returns>
        <Extension()>
        Public Function GetAttributeDefinition(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As AttributeDefinition
            Try
                Dim dbObject = eTransaction.GetObject(objectId, openMode)
                Select Case dbObject.GetDBObjectType = "AttributeDefinition"
                    Case True : Return DirectCast(dbObject, AttributeDefinition)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="AttributeReference"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="AttributeReference"/>.</returns>
        <Extension()>
        Public Function GetAttributeReference(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As AttributeReference
            Try
                Dim dbObject = eTransaction.GetObject(objectId, openMode)
                Select Case dbObject.GetDBObjectType = "AttributeReference"
                    Case True : Return DirectCast(dbObject, AttributeReference)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Line"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Line"/>.</returns>
        <Extension()>
        Public Function GetLine(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Line
            Try
                Dim dbObject = eTransaction.GetObject(objectId, openMode)
                Select Case dbObject.GetDBObjectType = "Line"
                    Case True : Return DirectCast(dbObject, Line)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Polyline"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Polyline"/>.</returns>
        <Extension()>
        Public Function GetPolyline(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Polyline
            Try
                Dim dbObject = eTransaction.GetObject(objectId, openMode)
                Select Case dbObject.GetDBObjectType = "Polyline"
                    Case True : Return DirectCast(dbObject, Polyline)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Circle"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Circle"/>.</returns>
        <Extension()>
        Public Function GetCircle(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Circle
            Try
                Dim dbObject = eTransaction.GetObject(objectId, openMode)
                Select Case dbObject.GetDBObjectType = "Circle"
                    Case True : Return DirectCast(dbObject, Circle)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Ellipse"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Ellipse"/>.</returns>
        <Extension()>
        Public Function GetEllipse(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Ellipse
            Try
                Dim dbObject = eTransaction.GetObject(objectId, openMode)
                Select Case dbObject.GetDBObjectType = "Ellipse"
                    Case True : Return DirectCast(dbObject, Ellipse)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="DBText"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="DBText"/>.</returns>
        <Extension()>
        Public Function GetDBText(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As DBText
            Try
                Dim dbObject = eTransaction.GetObject(objectId, openMode)
                Select Case dbObject.GetDBObjectType = "DBText"
                    Case True : Return DirectCast(dbObject, DBText)
                    Case Else : Return Nothing
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        'tekenobjecten, speciaal

        ''' <summary>
        ''' Gets the <see cref="Entity"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <param name="forceOpenOnLockedLayer">Use True to get also an object on a locked layer.</param>
        ''' <returns>The <see cref="Entity"/>.</returns>
        <Extension()>
        Public Function GetEntity(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead, Optional forceOpenOnLockedLayer As Nullable(Of Boolean) = Nothing) As Entity
            Try
                Select Case IsNothing(forceOpenOnLockedLayer)
                    Case True : Return DirectCast(eTransaction.GetObject(objectId, openMode), Entity)
                    Case Else : Return DirectCast(eTransaction.GetObject(objectId, openMode, False, forceOpenOnLockedLayer), Entity)
                End Select
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Curve"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Curve"/>.</returns>
        <Extension()>
        Public Function GetCurve(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Curve
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), Curve)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Table"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Table"/>.</returns>
        <Extension()>
        Public Function GetTable(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Table
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), Table)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="MText"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="MText"/>.</returns>
        <Extension()>
        Public Function GetMText(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As MText
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), MText)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Gets the <see cref="Dimension"/> with the specified <see cref="ObjectId"/> in read mode unless otherwise specified.
        ''' </summary>
        ''' <param name="eTransaction"></param>
        ''' <param name="objectId">The <see cref="ObjectId"/>.</param>
        ''' <param name="openMode">To specify another <see cref="OpenMode"/>.</param>
        ''' <returns>The <see cref="Dimension"/>.</returns>
        <Extension()>
        Public Function GetDimension(ByVal eTransaction As Transaction, objectId As ObjectId, Optional openMode As OpenMode = OpenMode.ForRead) As Dimension
            Try
                Return DirectCast(eTransaction.GetObject(objectId, openMode), Dimension)
            Catch ex As System.Exception
                Return Nothing
            End Try
        End Function

    End Module

End Namespace
