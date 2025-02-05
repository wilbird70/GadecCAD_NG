'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module ToArrayExtensions

        ''' <summary>
        ''' Converts the collection to an array of <see cref="Document"/>.
        ''' </summary>
        ''' <param name="eDocumentCollection"></param>
        ''' <returns>The array of <see cref="Document"/>.</returns>
        <Extension()>
        Public Function ToArray(ByVal eDocumentCollection As DocumentCollection) As Document()
            Return eDocumentCollection.Cast(Of Document).ToArray
        End Function

        ''' <summary>
        ''' Converts the collection to an array of <see cref="ObjectId"/>.
        ''' </summary>
        ''' <param name="eObjectIdCollection"></param>
        ''' <returns>The array of <see cref="ObjectId"/>.</returns>
        <Extension()>
        Public Function ToArray(ByVal eObjectIdCollection As ObjectIdCollection) As ObjectId()
            Return eObjectIdCollection.Cast(Of ObjectId).ToArray
        End Function

        ''' <summary>
        ''' Converts the collection to an array of <see cref="ObjectId"/>.
        ''' </summary>
        ''' <param name="eAttributeCollection"></param>
        ''' <returns>The array of <see cref="ObjectId"/>.</returns>
        <Extension()>
        Public Function ToArray(ByVal eAttributeCollection As AttributeCollection) As ObjectId()
            Return eAttributeCollection.Cast(Of ObjectId).ToArray
        End Function

        ''' <summary>
        ''' Converts the collection inside the <see cref="BlockTableRecord"/> to an array of <see cref="ObjectId"/>.
        ''' </summary>
        ''' <param name="eBlockTableRecord"></param>
        ''' <returns>The array of <see cref="ObjectId"/>.</returns>
        <Extension()>
        Public Function ToArray(ByVal eBlockTableRecord As BlockTableRecord) As ObjectId()
            Return eBlockTableRecord.Cast(Of ObjectId).ToArray
        End Function

        ''' <summary>
        ''' Converts the collection to an array of <see cref="Entity"/>.
        ''' </summary>
        ''' <param name="eDBObjectCollection"></param>
        ''' <returns>The array of <see cref="Entity"/>.</returns>
        <Extension()>
        Public Function ToArray(ByVal eDBObjectCollection As DBObjectCollection) As Entity()
            Return eDBObjectCollection.Cast(Of Entity).ToArray
        End Function

        ''' <summary>
        ''' Converts the collection to an array of <see cref="DBDictionaryEntry"/>.
        ''' </summary>
        ''' <param name="eDBDictionary"></param>
        ''' <returns>The array of <see cref="DBDictionaryEntry"/>.</returns>
        <Extension()>
        Public Function ToArray(ByVal eDBDictionary As DBDictionary) As DBDictionaryEntry()
            Return eDBDictionary.Cast(Of DBDictionaryEntry).ToArray
        End Function

        ''' <summary>
        ''' Converts the collection to an array of <see cref="DynamicBlockReferenceProperty"/>.
        ''' </summary>
        ''' <param name="eDynamicBlockReferencePropertyCollection"></param>
        ''' <returns>The array of <see cref="DynamicBlockReferenceProperty"/>.</returns>
        <Extension()>
        Public Function ToArray(ByVal eDynamicBlockReferencePropertyCollection As DynamicBlockReferencePropertyCollection) As DynamicBlockReferenceProperty()
            Return eDynamicBlockReferencePropertyCollection.Cast(Of DynamicBlockReferenceProperty).ToArray
        End Function

    End Module

End Namespace
