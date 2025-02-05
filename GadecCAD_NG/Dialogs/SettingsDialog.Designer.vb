Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SettingsDialog
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
        Me.ltResetFactorySettings = New Label()
        Me.ltOK = New Button()
        Me.ltCancel = New Button()
        Me.SuspendLayout()
        '
        'ltResetFactorySettings
        '
        Me.ltResetFactorySettings.AutoSize = True
        Me.ltResetFactorySettings.Location = New System.Drawing.Point(12, 9)
        Me.ltResetFactorySettings.Name = "ltResetFactorySettings"
        Me.ltResetFactorySettings.Size = New System.Drawing.Size(28, 13)
        Me.ltResetFactorySettings.TabIndex = 0
        Me.ltResetFactorySettings.Text = "XXX"
        '
        'ltOK
        '
        Me.ltOK.DialogResult = DialogResult.OK
        Me.ltOK.Location = New System.Drawing.Point(190, 139)
        Me.ltOK.Name = "ltOK"
        Me.ltOK.Size = New System.Drawing.Size(86, 23)
        Me.ltOK.TabIndex = 4
        Me.ltOK.Text = "XXX"
        Me.ltOK.UseVisualStyleBackColor = True
        '
        'ltCancel
        '
        Me.ltCancel.DialogResult = DialogResult.Cancel
        Me.ltCancel.Location = New System.Drawing.Point(282, 139)
        Me.ltCancel.Name = "ltCancel"
        Me.ltCancel.Size = New System.Drawing.Size(85, 23)
        Me.ltCancel.TabIndex = 5
        Me.ltCancel.Text = "XXX"
        Me.ltCancel.UseVisualStyleBackColor = True
        '
        'SettingsDialog
        '
        Me.AcceptButton = Me.ltOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.CancelButton = Me.ltCancel
        Me.ClientSize = New System.Drawing.Size(379, 174)
        Me.Controls.Add(Me.ltOK)
        Me.Controls.Add(Me.ltCancel)
        Me.Controls.Add(Me.ltResetFactorySettings)
        Me.Name = "SettingsDialog"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Text = "SettingsDialog"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ltResetFactorySettings As Label
    Friend WithEvents ltOK As Button
    Friend WithEvents ltCancel As Button
End Class
