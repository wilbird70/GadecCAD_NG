Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TextBalloon
    Inherits Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TextBalloon))
        Me.cDescr1 = New Label()
        Me.cDescr2 = New Label()
        Me.cDescr3 = New Label()
        Me.cClient1 = New Label()
        Me.cClient2 = New Label()
        Me.cClient3 = New Label()
        Me.cClient4 = New Label()
        Me.cLastRev_Char = New Label()
        Me.cLastRev_Date = New Label()
        Me.cLastRev_Descr = New Label()
        Me.cProject = New Label()
        Me.cDossier = New Label()
        Me.cScale = New Label()
        Me.cDescr4 = New Label()
        Me.cFilename = New Label()
        Me.BackPictureBox = New PictureBox()
        CType(Me.BackPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'cDescr1
        '
        resources.ApplyResources(Me.cDescr1, "cDescr1")
        Me.cDescr1.BackColor = System.Drawing.Color.Black
        Me.cDescr1.ForeColor = System.Drawing.Color.White
        Me.cDescr1.Name = "cDescr1"
        '
        'cDescr2
        '
        resources.ApplyResources(Me.cDescr2, "cDescr2")
        Me.cDescr2.BackColor = System.Drawing.Color.Black
        Me.cDescr2.ForeColor = System.Drawing.Color.White
        Me.cDescr2.Name = "cDescr2"
        '
        'cDescr3
        '
        resources.ApplyResources(Me.cDescr3, "cDescr3")
        Me.cDescr3.BackColor = System.Drawing.Color.Black
        Me.cDescr3.ForeColor = System.Drawing.Color.White
        Me.cDescr3.Name = "cDescr3"
        '
        'cClient1
        '
        resources.ApplyResources(Me.cClient1, "cClient1")
        Me.cClient1.BackColor = System.Drawing.Color.Black
        Me.cClient1.ForeColor = System.Drawing.Color.White
        Me.cClient1.Name = "cClient1"
        '
        'cClient2
        '
        resources.ApplyResources(Me.cClient2, "cClient2")
        Me.cClient2.BackColor = System.Drawing.Color.Black
        Me.cClient2.ForeColor = System.Drawing.Color.White
        Me.cClient2.Name = "cClient2"
        '
        'cClient3
        '
        resources.ApplyResources(Me.cClient3, "cClient3")
        Me.cClient3.BackColor = System.Drawing.Color.Black
        Me.cClient3.ForeColor = System.Drawing.Color.White
        Me.cClient3.Name = "cClient3"
        '
        'cClient4
        '
        resources.ApplyResources(Me.cClient4, "cClient4")
        Me.cClient4.BackColor = System.Drawing.Color.Black
        Me.cClient4.ForeColor = System.Drawing.Color.White
        Me.cClient4.Name = "cClient4"
        '
        'cLastRev_Char
        '
        resources.ApplyResources(Me.cLastRev_Char, "cLastRev_Char")
        Me.cLastRev_Char.BackColor = System.Drawing.Color.Black
        Me.cLastRev_Char.ForeColor = System.Drawing.Color.White
        Me.cLastRev_Char.Name = "cLastRev_Char"
        '
        'cLastRev_Date
        '
        resources.ApplyResources(Me.cLastRev_Date, "cLastRev_Date")
        Me.cLastRev_Date.BackColor = System.Drawing.Color.Black
        Me.cLastRev_Date.ForeColor = System.Drawing.Color.White
        Me.cLastRev_Date.Name = "cLastRev_Date"
        '
        'cLastRev_Descr
        '
        resources.ApplyResources(Me.cLastRev_Descr, "cLastRev_Descr")
        Me.cLastRev_Descr.BackColor = System.Drawing.Color.Black
        Me.cLastRev_Descr.ForeColor = System.Drawing.Color.White
        Me.cLastRev_Descr.Name = "cLastRev_Descr"
        '
        'cProject
        '
        resources.ApplyResources(Me.cProject, "cProject")
        Me.cProject.BackColor = System.Drawing.Color.Black
        Me.cProject.ForeColor = System.Drawing.Color.White
        Me.cProject.Name = "cProject"
        '
        'cDossier
        '
        resources.ApplyResources(Me.cDossier, "cDossier")
        Me.cDossier.BackColor = System.Drawing.Color.Black
        Me.cDossier.ForeColor = System.Drawing.Color.White
        Me.cDossier.Name = "cDossier"
        '
        'cScale
        '
        resources.ApplyResources(Me.cScale, "cScale")
        Me.cScale.BackColor = System.Drawing.Color.Black
        Me.cScale.ForeColor = System.Drawing.Color.White
        Me.cScale.Name = "cScale"
        '
        'cDescr4
        '
        resources.ApplyResources(Me.cDescr4, "cDescr4")
        Me.cDescr4.BackColor = System.Drawing.Color.Black
        Me.cDescr4.ForeColor = System.Drawing.Color.White
        Me.cDescr4.Name = "cDescr4"
        '
        'cFilename
        '
        resources.ApplyResources(Me.cFilename, "cFilename")
        Me.cFilename.BackColor = System.Drawing.Color.Black
        Me.cFilename.FlatStyle = FlatStyle.Flat
        Me.cFilename.ForeColor = System.Drawing.Color.White
        Me.cFilename.Name = "cFilename"
        '
        'BackPictureBox
        '
        Me.BackPictureBox.BackColor = System.Drawing.Color.Black
        resources.ApplyResources(Me.BackPictureBox, "BackPictureBox")
        Me.BackPictureBox.Name = "BackPictureBox"
        Me.BackPictureBox.TabStop = False
        '
        'TextBalloon
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.ControlBox = False
        Me.Controls.Add(Me.cFilename)
        Me.Controls.Add(Me.cDescr4)
        Me.Controls.Add(Me.cScale)
        Me.Controls.Add(Me.cDossier)
        Me.Controls.Add(Me.cProject)
        Me.Controls.Add(Me.cLastRev_Descr)
        Me.Controls.Add(Me.cLastRev_Date)
        Me.Controls.Add(Me.cLastRev_Char)
        Me.Controls.Add(Me.cClient4)
        Me.Controls.Add(Me.cClient3)
        Me.Controls.Add(Me.cClient2)
        Me.Controls.Add(Me.cClient1)
        Me.Controls.Add(Me.cDescr3)
        Me.Controls.Add(Me.cDescr2)
        Me.Controls.Add(Me.cDescr1)
        Me.Controls.Add(Me.BackPictureBox)
        Me.FormBorderStyle = FormBorderStyle.None
        Me.Name = "TextBalloon"
        Me.TopMost = True
        Me.TransparencyKey = System.Drawing.SystemColors.Control
        CType(Me.BackPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cDescr1 As Label
    Friend WithEvents cDescr2 As Label
    Friend WithEvents cDescr3 As Label
    Friend WithEvents cClient1 As Label
    Friend WithEvents cClient2 As Label
    Friend WithEvents cClient3 As Label
    Friend WithEvents cClient4 As Label
    Friend WithEvents cLastRev_Char As Label
    Friend WithEvents cLastRev_Date As Label
    Friend WithEvents cLastRev_Descr As Label
    Friend WithEvents cProject As Label
    Friend WithEvents cDossier As Label
    Friend WithEvents cScale As Label
    Friend WithEvents cDescr4 As Label
    Friend WithEvents cFilename As Label
    Friend WithEvents BackPictureBox As PictureBox
End Class
