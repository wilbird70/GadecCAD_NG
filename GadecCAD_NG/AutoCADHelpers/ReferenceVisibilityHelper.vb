'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods for blockreferences.
''' </summary>
Public Class ReferenceVisibilityHelper

    'subs

    ''' <summary>
    ''' Sets the visibility property of a blockreference to the specified value if it is an allowed value.
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="referenceId">The objectid of the blockreference.</param>
    ''' <param name="visibility">The new visibility value.</param>
    Public Shared Sub SetProperty(document As Document, referenceId As ObjectId, visibility As String)
        Using document.LockDocument
            SetProperty(document.Database, referenceId, visibility)
        End Using
    End Sub

    ''' <summary>
    ''' Sets the visibility property of a blockreference to the specified value if it is an allowed value.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceId">The objectid of the blockreference.</param>
    ''' <param name="visibility">The new visibility value.</param>
    Public Shared Sub SetProperty(database As Database, referenceId As ObjectId, visibility As String)
        Using tr = database.TransactionManager.StartTransaction
            Dim reference = tr.GetBlockReference(referenceId, OpenMode.ForWrite)
            SetProperty(reference, visibility)
            tr.Commit()
        End Using
    End Sub

    ''' <summary>
    ''' Sets the visibility property of a blockreference to the specified value if it is an allowed value.
    ''' </summary>
    ''' <param name="reference">The blockreference.</param>
    ''' <param name="visibility">The new visibility value.</param>
    Public Shared Sub SetProperty(reference As BlockReference, visibility As String)
        If Not reference.IsDynamicBlock Then Exit Sub

        Dim referenceProperties = reference.DynamicBlockReferencePropertyCollection
        For Each referenceProperty In referenceProperties.ToArray
            Dim x = referenceProperty.PropertyTypeCode
            If referenceProperty.PropertyName.ToLower.Contains("visibility") Then
                Dim visibilities = referenceProperty.GetAllowedValues().ToList
                If visibilities.Contains(visibility) Then referenceProperty.Value = visibility
            End If
        Next
    End Sub

    'functions

    ''' <summary>
    ''' Gets the value of the visibility property of a blockreference.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="referenceId">The objectid of the blockreference.</param>
    ''' <returns>The visibility value.</returns>
    Public Shared Function GetProperty(database As Database, referenceId As ObjectId) As String
        Using tr = database.TransactionManager.StartTransaction
            Dim reference = tr.GetBlockReference(referenceId)
            Return GetProperty(reference)
            tr.Commit()
        End Using
    End Function

    ''' <summary>
    ''' Gets the value of the visibility property of a blockreference.
    ''' </summary>
    ''' <param name="reference">The blockreference.</param>
    ''' <returns>The visibility value.</returns>
    Public Shared Function GetProperty(reference As BlockReference) As String
        If Not reference.IsDynamicBlock Then Return ""

        Dim referenceProperties = reference.DynamicBlockReferencePropertyCollection
        For Each referenceProperty In referenceProperties.ToArray
            If referenceProperty.PropertyName.ToLower.Contains("visibility") Then Return referenceProperty.Value
        Next
        Return ""
    End Function

End Class
