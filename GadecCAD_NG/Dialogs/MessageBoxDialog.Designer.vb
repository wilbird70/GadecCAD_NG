Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MessageBoxDialog
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
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.OutputTextBox = New TextBox()
        Me.CaptionLabel = New Label()
        Me.Button2 = New Button()
        Me.ltCancel = New Button()
        Me.Button0 = New Button()
        Me.Button1 = New Button()
        Me.SuspendLayout()
        '
        'OutputTextBox
        '
        Me.OutputTextBox.Location = New System.Drawing.Point(12, 25)
        Me.OutputTextBox.Multiline = True
        Me.OutputTextBox.Name = "OutputTextBox"
        Me.OutputTextBox.ScrollBars = ScrollBars.Both
        Me.OutputTextBox.Size = New System.Drawing.Size(355, 108)
        Me.OutputTextBox.TabIndex = 0
        Me.OutputTextBox.WordWrap = False
        '
        'CaptionLabel
        '
        Me.CaptionLabel.AutoSize = True
        Me.CaptionLabel.Location = New System.Drawing.Point(12, 9)
        Me.CaptionLabel.Name = "CaptionLabel"
        Me.CaptionLabel.Size = New System.Drawing.Size(28, 13)
        Me.CaptionLabel.TabIndex = 12
        Me.CaptionLabel.Text = "XXX"
        '
        'Button2
        '
        Me.Button2.DialogResult = DialogResult.OK
        Me.Button2.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button2.Location = New System.Drawing.Point(192, 139)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(84, 23)
        Me.Button2.TabIndex = 13
        Me.Button2.Text = "XXX"
        Me.Button2.UseVisualStyleBackColor = True
        Me.Button2.Visible = False
        '
        'ltCancel
        '
        Me.ltCancel.DialogResult = DialogResult.Cancel
        Me.ltCancel.Location = New System.Drawing.Point(282, 139)
        Me.ltCancel.Name = "ltCancel"
        Me.ltCancel.Size = New System.Drawing.Size(85, 23)
        Me.ltCancel.TabIndex = 0
        Me.ltCancel.Text = "XXX"
        Me.ltCancel.UseVisualStyleBackColor = True
        '
        'Button0
        '
        Me.Button0.DialogResult = DialogResult.OK
        Me.Button0.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button0.Location = New System.Drawing.Point(12, 139)
        Me.Button0.Name = "Button0"
        Me.Button0.Size = New System.Drawing.Size(84, 23)
        Me.Button0.TabIndex = 15
        Me.Button0.Text = "XXX"
        Me.Button0.UseVisualStyleBackColor = True
        Me.Button0.Visible = False
        '
        'Button1
        '
        Me.Button1.DialogResult = DialogResult.OK
        Me.Button1.Font = New System.Drawing.Font("Arial Narrow", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button1.Location = New System.Drawing.Point(102, 139)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(84, 23)
        Me.Button1.TabIndex = 16
        Me.Button1.Text = "XXX"
        Me.Button1.UseVisualStyleBackColor = True
        Me.Button1.Visible = False
        '
        'MessageBoxDialog
        '
        Me.AcceptButton = Me.Button0
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.CancelButton = Me.ltCancel
        Me.ClientSize = New System.Drawing.Size(379, 174)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Button0)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.ltCancel)
        Me.Controls.Add(Me.OutputTextBox)
        Me.Controls.Add(Me.CaptionLabel)
        Me.Name = "MessageBoxDialog"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Text = "MessageBoxDialog"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents OutputTextBox As TextBox
    Friend WithEvents CaptionLabel As Label
    Friend WithEvents Button2 As Button
    Friend WithEvents ltCancel As Button
    Friend WithEvents Button0 As Button
    Friend WithEvents Button1 As Button
End Class
