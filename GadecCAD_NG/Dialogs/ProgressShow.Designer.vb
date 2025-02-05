Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ProgressShow
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ProgressShow))
        Me.OutputProgressBar = New ProgressBar()
        Me.PromptLabel = New Label()
        Me.ValueLabel = New Label()
        Me.MaxLabel = New Label()
        Me.ltCancel = New Button()
        Me.SuspendLayout()
        '
        'OutputProgressBar
        '
        Me.OutputProgressBar.Location = New System.Drawing.Point(12, 37)
        Me.OutputProgressBar.Name = "OutputProgressBar"
        Me.OutputProgressBar.Size = New System.Drawing.Size(355, 18)
        Me.OutputProgressBar.TabIndex = 0
        '
        'PromptLabel
        '
        Me.PromptLabel.Font = New System.Drawing.Font("Arial Narrow", 8.5!)
        Me.PromptLabel.Location = New System.Drawing.Point(12, 15)
        Me.PromptLabel.Name = "PromptLabel"
        Me.PromptLabel.Size = New System.Drawing.Size(355, 19)
        Me.PromptLabel.TabIndex = 1
        Me.PromptLabel.Text = "XXX"
        '
        'ValueLabel
        '
        Me.ValueLabel.Font = New System.Drawing.Font("Arial Narrow", 8.5!)
        Me.ValueLabel.Location = New System.Drawing.Point(12, 58)
        Me.ValueLabel.Name = "ValueLabel"
        Me.ValueLabel.Size = New System.Drawing.Size(54, 19)
        Me.ValueLabel.TabIndex = 3
        Me.ValueLabel.Text = "0"
        '
        'MaxLabel
        '
        Me.MaxLabel.Font = New System.Drawing.Font("Arial Narrow", 8.5!)
        Me.MaxLabel.Location = New System.Drawing.Point(313, 58)
        Me.MaxLabel.Name = "MaxLabel"
        Me.MaxLabel.Size = New System.Drawing.Size(54, 19)
        Me.MaxLabel.TabIndex = 4
        Me.MaxLabel.Text = "100"
        Me.MaxLabel.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'ltCancel
        '
        Me.ltCancel.DialogResult = DialogResult.Cancel
        Me.ltCancel.Location = New System.Drawing.Point(282, 80)
        Me.ltCancel.Name = "ltCancel"
        Me.ltCancel.Size = New System.Drawing.Size(85, 23)
        Me.ltCancel.TabIndex = 10
        Me.ltCancel.Text = "XXX"
        Me.ltCancel.UseVisualStyleBackColor = True
        '
        'ProgressShow
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.CancelButton = Me.ltCancel
        Me.ClientSize = New System.Drawing.Size(379, 114)
        Me.Controls.Add(Me.ltCancel)
        Me.Controls.Add(Me.MaxLabel)
        Me.Controls.Add(Me.ValueLabel)
        Me.Controls.Add(Me.PromptLabel)
        Me.Controls.Add(Me.OutputProgressBar)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "ProgressShow"
        Me.Text = "XXX"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents OutputProgressBar As ProgressBar
    Friend WithEvents PromptLabel As Label
    Friend WithEvents ValueLabel As Label
    Friend WithEvents MaxLabel As Label
    Friend WithEvents ltCancel As Button
End Class
