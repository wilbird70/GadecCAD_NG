'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.DatabaseServices
Imports System.Runtime.CompilerServices

Namespace Extensions

    Public Module DatabaseExtensions

        ''' <summary>
        ''' Determine if the drawing is currently in ModelSpace.
        ''' </summary>
        ''' <param name="eDatabase"></param>
        ''' <returns>True if in ModelSpace.</returns>
        <Extension()>
        Public Function ModelSpaceIsCurrent(ByVal eDatabase As Database) As Boolean
            Return eDatabase.CurrentSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(eDatabase)
        End Function

    End Module

End Namespace
