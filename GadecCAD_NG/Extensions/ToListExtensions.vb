'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module ToListExtensions

        ''' <summary>
        ''' Converts the collection to a list of <see cref="Document"/>.
        ''' </summary>
        ''' <param name="eDocumentCollection"></param>
        ''' <returns>The list of <see cref="Document"/>.</returns>
        <Extension()>
        Public Function ToList(ByVal eDocumentCollection As DocumentCollection) As List(Of Document)
            Return eDocumentCollection.Cast(Of Document).ToList
        End Function

        ''' <summary>
        ''' Converts the collection to a list of <see cref="ObjectId"/>.
        ''' </summary>
        ''' <param name="eObjectIdCollection"></param>
        ''' <returns>The list of <see cref="ObjectId"/>.</returns>
        <Extension()>
        Public Function ToList(ByVal eObjectIdCollection As ObjectIdCollection) As List(Of ObjectId)
            Return eObjectIdCollection.Cast(Of ObjectId).ToList
        End Function

        ''' <summary>
        ''' Converts the collection to a list of <see cref="ObjectId"/>.
        ''' </summary>
        ''' <param name="eAttributeCollection"></param>
        ''' <returns>The list of <see cref="ObjectId"/>.</returns>
        <Extension()>
        Public Function ToList(ByVal eAttributeCollection As AttributeCollection) As List(Of ObjectId)
            Return eAttributeCollection.Cast(Of ObjectId).ToList
        End Function

        ''' <summary>
        ''' Converts the collection inside the <see cref="BlockTableRecord"/> to a list of <see cref="ObjectId"/>.
        ''' </summary>
        ''' <param name="eBlockTableRecord"></param>
        ''' <returns>The list of <see cref="ObjectId"/>.</returns>
        <Extension()>
        Public Function ToList(ByVal eBlockTableRecord As BlockTableRecord) As List(Of ObjectId)
            Return eBlockTableRecord.Cast(Of ObjectId).ToList
        End Function

        ''' <summary>
        ''' Converts the collection to a list of <see cref="Entity"/>.
        ''' </summary>
        ''' <param name="eDBObjectCollection"></param>
        ''' <returns>The list of <see cref="Entity"/>.</returns>
        <Extension()>
        Public Function ToList(ByVal eDBObjectCollection As DBObjectCollection) As List(Of Entity)
            Return eDBObjectCollection.Cast(Of Entity).ToList
        End Function

        ''' <summary>
        ''' Converts the collection to a list of <see cref="DBDictionaryEntry"/>.
        ''' </summary>
        ''' <param name="eDBDictionary"></param>
        ''' <returns>The list of <see cref="DBDictionaryEntry"/>.</returns>
        <Extension()>
        Public Function ToList(ByVal eDBDictionary As DBDictionary) As List(Of DBDictionaryEntry)
            Return eDBDictionary.Cast(Of DBDictionaryEntry).ToList
        End Function

        ''' <summary>
        ''' Converts the collection to a list of <see cref="DynamicBlockReferenceProperty"/>.
        ''' </summary>
        ''' <param name="eDynamicBlockReferencePropertyCollection"></param>
        ''' <returns>The list of <see cref="DynamicBlockReferenceProperty"/>.</returns>
        <Extension()>
        Public Function ToList(ByVal eDynamicBlockReferencePropertyCollection As DynamicBlockReferencePropertyCollection) As List(Of DynamicBlockReferenceProperty)
            Return eDynamicBlockReferencePropertyCollection.Cast(Of DynamicBlockReferenceProperty).ToList
        End Function

    End Module

End Namespace
