'Gadec Engineerings Software (c) 2022
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Colors
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides methods that are no longer used, but not yet discarded.
''' </summary>
Public Class NotUsed

    ''' <summary>
    ''' Gets an abbreviated textstring from the original, if the length of the text with the specified font exceeds the maximum length.
    ''' </summary>
    ''' <param name="text">The textstring.</param>
    ''' <param name="font">The used font.</param>
    ''' <param name="maxLength">The maximum length of text.</param>
    ''' <returns></returns>
    Public Shared Function MaximizeLengthOfText(text As String, font As Drawing.Font, maxLength As Integer) As String
        Dim output = text
        If System.Windows.Forms.TextRenderer.MeasureText(output, font).Width > maxLength Then
            Do While System.Windows.Forms.TextRenderer.MeasureText(output, font).Width > maxLength - 20
                output = output.EraseEnd(1)
            Loop
            output &= "..."
        End If
        Return output
    End Function

    ''' <summary>
    ''' Gets the current version of AutoCAD.
    ''' </summary>
    ''' <returns>The official version name.</returns>
    Public Shared Function AutoCADVersion() As String
        Dim output = ""
        Select Case Left(Application.GetSystemVariable("acadver"), 4)
            Case "24.2" : output = "AutoCAD 2023"
            Case "24.1" : output = "AutoCAD 2022"
            Case "24.0" : output = "AutoCAD 2021"
            Case "23.1" : output = "AutoCAD 2020"
            Case "23.0" : output = "AutoCAD 2019"
            Case "22.0" : output = "AutoCAD 2018"
            Case "21.0" : output = "AutoCAD 2017"
            Case "20.1" : output = "AutoCAD 2016"
            Case "20.0" : output = "AutoCAD 2015"
            Case "19.1" : output = "AutoCAD 2014"
            Case "19.0" : output = "AutoCAD 2013"
            Case "18.2" : output = "AutoCAD 2012"
            Case "18.1" : output = "AutoCAD 2011"
            Case "18.0" : output = "AutoCAD 2010"
            Case "17.2" : output = "AutoCAD 2009"
            Case "17.1" : output = "AutoCAD 2008"
            Case "17.0" : output = "AutoCAD 2007"
            Case "16.2" : output = "AutoCAD 2006"
            Case "16.1" : output = "AutoCAD 2005"
            Case "16.0" : output = "AutoCAD 2004"
            Case "15.6" : output = "AutoCAD 2002"
            Case "15.1" : output = "AutoCAD 2000i"
            Case "15.0" : output = "AutoCAD 2000"
        End Select
        Return output
    End Function

    ''' <summary>
    ''' Gets the name of the specified layer.
    ''' </summary>
    ''' <param name="database">The present drawing.</param>
    ''' <param name="layerId">The objectid of the layer.</param>
    ''' <returns>Layername.</returns>
    Public Shared Function GetLayerName(database As Database, layerId As ObjectId) As String
        Dim layNam = ""
        Using tr = database.TransactionManager.StartTransaction
            Dim ltr = tr.GetLayerTableRecord(layerId)
            layNam = ltr.Name
        End Using
        Return layNam
    End Function

    ''' <summary>
    ''' Renames a layout.
    ''' <para>Note: Skipped if the current layoutname is Model or the new name already existed.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    ''' <param name="oldLayoutName">The current layoutname.</param>
    ''' <param name="newLayoutName">The new layoutname.</param>
    ''' <returns>True if layout is renamed.</returns>
    Public Shared Function RenameLayout(document As Document, oldLayoutName As String, newLayoutName As String) As Boolean
        Dim db = document.Database
        Dim ed = document.Editor
        Dim output = False
        If oldLayoutName <> newLayoutName AndAlso "MODEL" <> oldLayoutName.ToUpper() AndAlso "MODEL" <> newLayoutName.ToUpper() Then
            Using tr = db.TransactionManager.StartOpenCloseTransaction()
                Dim layoutDictionary = tr.GetDBDictionary(db.LayoutDictionaryId)
                If layoutDictionary.Contains(oldLayoutName) AndAlso Not layoutDictionary.Contains(newLayoutName) Then output = True
                tr.Commit()
            End Using
        End If
        If output Then
            Dim layoutMgr = LayoutManager.Current
            layoutMgr.RenameLayout(oldLayoutName, newLayoutName)
            ed.Regen()
        End If
        Return output
    End Function

    ''' <summary>
    ''' Creates a table-style.
    ''' </summary>
    ''' <param name="document"></param>
    ''' <returns>The <see cref="ObjectId"/> of the tablestyle.</returns>
    Public Shared Function CreateSomeTableStyle(document As Document) As ObjectId
        Dim db = document.Database
        Dim output = ObjectId.Null
        Dim styleName = "Gadec Table Style"
        Using document.LockDocument
            Using tr = db.TransactionManager.StartTransaction()
                Dim tableStyles = tr.GetDBDictionary(db.TableStyleDictionaryId)
                Select Case tableStyles.Contains(styleName)
                    Case True : output = tableStyles.GetAt(styleName)
                    Case Else
                        Dim allRowTypes = CInt(RowType.HeaderRow Or RowType.TitleRow Or RowType.DataRow)
                        Dim allGridLines = CInt(GridLineType.AllGridLines)
                        Dim tableStyle As New TableStyle()
                        With tableStyle
                            .SetMargin(CellMargins.Left, 2, "Title")
                            .SetMargin(CellMargins.Top, 1.5, "Title")
                            .SetMargin(CellMargins.Left, 1, "Data")
                            .SetMargin(CellMargins.Top, 0.7, "Data")
                            .SetTextStyle(TextStyleHelper.GetTextStyleId(db, "ISO_94"), allRowTypes)
                            .SetTextHeight(0.1, allRowTypes)
                            .SetAlignment(CellAlignment.TopLeft, allRowTypes)
                            .SetGridColor(Color.FromColorIndex(ColorMethod.ByAci, 1), allGridLines, allRowTypes)
                        End With
                        tableStyles.UpgradeOpen()
                        output = tableStyles.SetAt(styleName, tableStyle)
                        tr.AddNewlyCreatedDBObject(tableStyle, True)
                        tableStyles.DowngradeOpen()
                End Select
                tr.Commit()
            End Using
        End Using
        If output = ObjectId.Null Then output = db.Tablestyle
        Return output
    End Function

    ''' <summary>
    ''' For converting MX-Graph symbols to winguard symbols.
    ''' <para>Note: This code needs to be thought through again.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ReplaceBlocksTgx2Wng(document As Document)
        'schaal voor de nieuwe symbolen vragen
        Dim db = document.Database
        Dim scale = DesignMethods.SetDrawingScale(document)
        Dim scale3D = New Scale3d(scale)

        'Starten
        Dim oldAtt = New List(Of ObjectId)
        SysVarHandler.SetVar(document, "ATTMODE", 1)
        Dim oldNames = "VLAM;APM800;BEAM;CIM800;FLITSLICHT;RIM800;MIM800;LUTO;STANDBEWAKING;SPRINK;ALGEMEEN;FLITLICHT;FLITLICHTEX".Cut
        Using document.LockDocument
            Using tr = db.TransactionManager.StartTransaction
                Dim bt = tr.GetBlockTable(db.BlockTableId, OpenMode.ForRead)
                For Each n In oldNames
                    If bt.Has(n) Then
                        Dim btr = tr.GetBlockTableRecord(bt(n), OpenMode.ForWrite)
                        If Not btr.IsDynamicBlock Then btr.Name &= "TGX"
                    End If
                Next

                'replaceList opzet voor Antonius Ziekenhuis
                Dim x = "APM800TGX=APM800;BEAMTGX=BDM800;CIM800TGX=CIM800;FBANTGX=ALG_SW;FCPSTGX=MCP820;FHRORTGX=801H;FHTGX=801H;FLITSLICHTTGX=ALG_FL"
                x &= ";FPCTGX=801PC;FPHTGX=801PH;FPTGX=813P;RIM800TGX=RIM800;VESDATGX=VLC800;VLAMTGX=801F"
                'Replacelist aanvullingen voor Abbott Healthcare Products B.V.
                x &= ";ALGEMEENTGX=CIM800\TECH;DRUKTGX=CIM800\DRUK;FBANEXTGX=ALG_SWex;FCPEXSTGX=CP840ex;FHROREXTGX=801Hex;FLITLICHTTGX=ALG_FL"
                x &= ";FLITLICHTEXTGX=ALG_FLex;FPH-BPTGX=801PH\BP;FPH-OVTGX=801PH\OV;FPHEXTGX=801PHex;FPH-OVEXTGX=801PHex\OV;FPSTGX=801PS"
                x &= ";KLEPTGX=CIM800\KLEP;LUTOTGX=ALG_RNB;MIM800TGX=MIM800;SOLTGX=SNM800\SLND;SPRINKTGX=CIM800\FLOW;STANDBEWAKINGTGX=CIM800\STND"
                x &= ";VESDATGX1=VLC800"

                Dim replaceList = x.Cut.ToIniDictionary

                'Importeren en herdefiniëren van benodigde symbolen
                Using import = New DefinitionsImporter("{Resources}\SymbolsTgx2Wng.dwg".Compose)
                    '////////// Deze functie bestaat niet meer. Moet alle definities uit de bron importeren en een lijst van bloknamen teruggeven.
                    'Dim blkDefNms = import.ImportDefinitions(db)
                    '////////// Deze functie is tegenwoordig private in de ReferenceMethods class.
                    'ReferenceMethods.RedefineReferences(document, blkDefNms)

                    'Seleceteren van alle blokken in modelspace
                    Dim blkRefIDs = SelectHelper.GetAllReferencesInModelspace(document)
                    Dim textsToChange = New Dictionary(Of ObjectId, String)

                    For Each id As ObjectId In blkRefIDs
                        Dim blkRef = tr.GetBlockReference(id, OpenMode.ForWrite)
                        For Each a As ObjectId In blkRef.AttributeCollection
                            oldAtt.Add(a)
                        Next
                        Dim blkRefbtr = If(blkRef.IsDynamicBlock, blkRef.DynamicBlockTableRecord, blkRef.BlockTableRecord)

                        Dim btr = tr.GetBlockTableRecord(blkRefbtr, OpenMode.ForRead)
                        If replaceList.ContainsKey(btr.Name) Then
                            Dim newBlkNam = replaceList(btr.Name).Cut("\").ElementAt(0)
                            If bt.Has(newBlkNam) Then
                                blkRef.ScaleFactors = scale3D
                                blkRef.BlockTableRecord = bt(newBlkNam)
                                '////////// Deze functie is tegenwoordig private in de ReferenceMethods class.
                                'ReferenceMethods.ReplaceAttributes(tr, blkRef, document.Database)

                                Dim row = ReferenceHelper.GetReferenceData(db, id)
                                If NotNothing(row) Then
                                    Dim pnr = "P{0}".Compose("00{0}".Compose(row.GetString("PANEL")).RightString(2))
                                    Dim lnr = row.GetString("POINT").LeftString(1)
                                    Dim anr = "00{0}".Compose(row.GetString("POINT").MidString(2)).RightString(3)
                                    textsToChange.Add(row.GetObjectId("PNR"), pnr)
                                    textsToChange.Add(row.GetObjectId("LS"), lnr)
                                    textsToChange.Add(row.GetObjectId("ADR"), anr)
                                    textsToChange.Add(row.GetObjectId("TYPE"), replaceList(btr.Name).Replace("\", "_"))
                                    AttributeHelper.SetVisibility(db, row.GetObjectId("PANEL"), False)
                                    AttributeHelper.SetVisibility(db, row.GetObjectId("POINT"), False)
                                End If
                            End If
                        End If
                    Next
                    TextHelper.ChangeTextStrings(document, textsToChange)
                End Using
                For Each id In oldAtt
                    Dim attRef = tr.GetAttributeReference(id, OpenMode.ForWrite)
                    attRef.Erase()
                Next
                tr.Commit()
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' For converting old symbols to winguard symbols.
    ''' <para>Note: This code especially made for the Claus centrale te Maasbracht.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub ReplaceBlocksOld2Wng(document As Document)
        'schaal voor de nieuwe symbolen vragen
        Dim db = document.Database
        Dim scale = DesignMethods.SetDrawingScale(document)
        Dim scale3D = New Scale3d(scale)

        'Importeren en herdefiniëren van benodigde symbolen
        Dim x = "VA110=830PH;VA201=CP820;VB201=LPSY800-R;VB202=LPSY800-R;VB203=LPSY800-R;VB204=LPSY800-R;VB303=LPAV800-R\LPAV800-R_F"
        x &= ";MXdev060=LPSB3000;VA111=830PC;VA301=CIM800;VA302=CIM800;VA305=CIM800;;VA308=CIM800;VA622=CIM800;VA624=RIM800"
        Dim replaceList = x.Cut.ToIniDictionary
        For Each key In replaceList.Keys
            Using import = New DefinitionsImporter("{Resources}\DC_MZX_Symbols.dwg".Compose)
                Dim blkDefNms = import.ImportNestedDefinitions(document, "_" & replaceList(key).Cut("\").ElementAt(0))
                '////////// Deze functie is tegenwoordig private in de blockhandler class.
                ReferenceMethods.RedefineReferences(document, blkDefNms)
            End Using
        Next

        'Starten
        Dim oldAtt = New List(Of ObjectId)
        SysVarHandler.SetVar(document, "ATTMODE", 1)
        Using document.LockDocument
            Using tr = db.TransactionManager.StartTransaction
                Dim bt = tr.GetBlockTable(db.BlockTableId, OpenMode.ForRead)

                'Seleceteren van alle blokken in modelspace
                Dim blkRefIDs = SelectHelper.GetAllReferencesInModelspace(document)
                Dim blkRefTbl = ReferenceHelper.GetReferenceData(db, blkRefIDs)
                Dim textsToChange = New Dictionary(Of ObjectId, String)

                For Each id As ObjectId In blkRefIDs
                    Dim row = blkRefTbl.Rows.Find(id)
                    If row.GetString("HARDWARE").StartsWith("P") Then

                        Dim blkRef = tr.GetBlockReference(id, OpenMode.ForWrite)
                        For Each a As ObjectId In blkRef.AttributeCollection
                            oldAtt.Add(a)
                        Next
                        Dim blkRefbtr = If(blkRef.IsDynamicBlock, blkRef.DynamicBlockTableRecord, blkRef.BlockTableRecord)

                        Dim btr = tr.GetBlockTableRecord(blkRefbtr, OpenMode.ForRead)
                        If replaceList.ContainsKey(btr.Name) Then
                            Dim newBlkNam = replaceList(btr.Name).Cut("\").ElementAt(0)
                            If bt.Has(newBlkNam) Then
                                blkRef.ScaleFactors = scale3D
                                blkRef.BlockTableRecord = bt(newBlkNam)
                                '////////// Deze functie is tegenwoordig private in de ReferenceMethods class.
                                ReferenceMethods.ReplaceAttributes(tr, blkRef, document.Database)
                                Dim pnr = row.GetString("HARDWARE").LeftString(3)
                                Dim lnr = row.GetString("HARDWARE").MidString(5, 1)
                                Dim anr = row.GetString("HARDWARE").MidString(6)
                                Dim kks = row.GetString("TEKST").MidString(6)

                                Dim newRow = ReferenceHelper.GetReferenceData(db, id)
                                If NotNothing(newRow) Then
                                    textsToChange.Add(newRow.GetObjectId("PNR"), pnr)
                                    textsToChange.Add(newRow.GetObjectId("LS"), lnr)
                                    textsToChange.Add(newRow.GetObjectId("ADR"), anr)
                                    textsToChange.Add(newRow.GetObjectId("KKS"), kks)
                                End If
                                If btr.Name = "VB303" Then
                                    Dim extraBlkRefId = ReferenceHelper.InsertReference(document, db.CurrentSpaceId, db.Clayer, bt("LPAV800-R_F"), blkRef.Position)
                                    Dim extraBlkRef = tr.GetBlockReference(extraBlkRefId)
                                    extraBlkRef.TransformBy(Matrix3d.Scaling(scale, blkRef.Position))
                                    Dim extraRow = ReferenceHelper.GetReferenceData(db, extraBlkRef.ObjectId)
                                    If NotNothing(extraRow) Then
                                        textsToChange.Add(extraRow.GetObjectId("PNR"), pnr)
                                        textsToChange.Add(extraRow.GetObjectId("LS"), lnr)
                                        textsToChange.Add(extraRow.GetObjectId("ADR"), anr)
                                        textsToChange.Add(extraRow.GetObjectId("KKS"), kks)
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
                TextHelper.ChangeTextStrings(document, textsToChange)
                For Each id In oldAtt
                    Dim attRef = tr.GetAttributeReference(id, OpenMode.ForWrite)
                    Select Case attRef.Tag = "TYPE"
                        Case True : attRef.Visible = False
                        Case Else : attRef.Erase()
                    End Select
                Next
                tr.Commit()
            End Using
        End Using
    End Sub

End Class
