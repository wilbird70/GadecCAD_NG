'Gadec Engineerings Software (c) 2022
Imports System.Windows.Forms
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports GadecCAD_NG.Extensions

''' <summary>
''' Provides a method that allows the user to draw a wiringduct.
''' <para>Note: Wiringducts are used in interface cabinets.</para>
''' </summary>
Public Class WiringDuctMethod

    ''' <summary>
    ''' Allows the user to draw a wiringduct.
    ''' <para>Note: Wiringducts are used in interface cabinets.</para>
    ''' </summary>
    ''' <param name="document">The present document.</param>
    Public Shared Sub Draw(document As Document)
        Dim db = document.Database
        Dim ed = document.Editor
        Dim wiringDuctData = DataSetHelper.LoadFromXml("{Support}\SetStandards.xml".Compose).GetTable("WiringDucts", "Name")
        Dim items = wiringDuctData.GetStringsFromColumn("Name")

        Dim currentSelection = Registerizer.UserSetting("WiringDuct")
        Dim dialog = New ListBoxDialog("SelectDuct".Translate, items, currentSelection)
        If Not dialog.DialogResult = DialogResult.OK Then Exit Sub

        Registerizer.UserSetting("WiringDuct", items(dialog.GetSelectedIndex))
        Dim row = wiringDuctData.Rows.Find(items(dialog.GetSelectedIndex))
        Do
            Dim vertices = New Point2dCollection
            Dim rotation = 0.0
            Dim textPoint As Point3d
            Using sysVar = New SysVarHandler(document)
                Dim promptPointOptions = New PromptPointOptions("Sel First Point:".Translate) With {.AllowNone = True}
                sysVar.Set("OSMODE", 512)
                sysVar.Set("ORTHOMODE", 1)
                Dim selectPointResult = ed.GetPoint(promptPointOptions)
                If Not selectPointResult.Status = PromptStatus.OK Then Exit Do

                Dim startPoint = selectPointResult.Value
                promptPointOptions.Message = "Sel Next Point:".Translate
                promptPointOptions.UseBasePoint = True
                promptPointOptions.BasePoint = startPoint
                sysVar.Set("OSMODE", 128)
                sysVar.Set("ORTHOMODE", 1)
                selectPointResult = ed.GetPoint(promptPointOptions)
                If Not selectPointResult.Status = PromptStatus.OK Then Continue Do

                Dim endPoint = selectPointResult.Value
                Dim ductWidth = row.GetString("Width").ToInteger
                Select Case New Line(startPoint, endPoint).Alignment
                    Case LineAlignment.horizontal
                        vertices.Add(New Point2d(startPoint.X, startPoint.Y - (ductWidth / 2)))
                        vertices.Add(New Point2d(startPoint.X, startPoint.Y + (ductWidth / 2)))
                        vertices.Add(New Point2d(endPoint.X, endPoint.Y + (ductWidth / 2)))
                        vertices.Add(New Point2d(endPoint.X, endPoint.Y - (ductWidth / 2)))
                        textPoint = New Point3d((startPoint.X + endPoint.X) / 2, startPoint.Y, startPoint.Z)
                    Case LineAlignment.vertical
                        vertices.Add(New Point2d(startPoint.X - (ductWidth / 2), startPoint.Y))
                        vertices.Add(New Point2d(startPoint.X + (ductWidth / 2), startPoint.Y))
                        vertices.Add(New Point2d(endPoint.X + (ductWidth / 2), endPoint.Y))
                        vertices.Add(New Point2d(endPoint.X - (ductWidth / 2), endPoint.Y))
                        textPoint = New Point3d(startPoint.X, (startPoint.Y + endPoint.Y) / 2, startPoint.Z)
                        rotation = 0.5 * Math.PI
                End Select
            End Using
            If vertices.Count = 0 Then Continue Do

            Dim ductIds = New ObjectIdCollection
            Dim polyLine = PolylineHelper.Create(vertices, True)
            Dim layerId = LayerHelper.GetLayerIdFromType(db, "Line")
            ductIds.Add(EntityHelper.Add(document, polyLine, layerId))
            Dim dbText = New DBText
            With dbText
                .TextString = "{0}x{1}".Compose(row.GetString("Width"), row.GetString("Height"))
                .Height = SysVarHandler.GetVar("DIMSCALE") * 1.8
                .Justify = AttachmentPoint.MiddleMid
                .Rotation = rotation
                .AlignmentPoint = textPoint
            End With
            layerId = LayerHelper.GetLayerIdFromType(db, "Hatch")
            ductIds.Add(EntityHelper.Add(document, dbText, layerId))
            Dim hatch = New Hatch
            hatch.SetHatchPattern(HatchPatternType.PreDefined, row.GetString("HatchPattern"))
            hatch.PatternScale = row.GetString("PatternScale").ToDouble
            hatch.PatternAngle = row.GetString("PatternAngle").ToDouble
            hatch.HatchStyle = HatchStyle.Normal
            hatch.AppendLoop(HatchLoopTypes.Outermost, New ObjectIdCollection({ductIds(0)}))
            hatch.AppendLoop(HatchLoopTypes.TextIsland, New ObjectIdCollection({ductIds(1)}))
            hatch.EvaluateHatch(True)
            ductIds.Add(EntityHelper.Add(document, hatch, layerId))
            GroupHelper.Create(db, ductIds)
        Loop
    End Sub

End Class
