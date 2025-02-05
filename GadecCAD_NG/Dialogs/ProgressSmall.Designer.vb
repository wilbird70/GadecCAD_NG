<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ProgressSmall
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.OutputProgessBar = New System.Windows.Forms.ProgressBar()
        Me.PromptLabel = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'OutputProgessBar
        '
        Me.OutputProgessBar.Location = New System.Drawing.Point(12, 32)
        Me.OutputProgessBar.Maximum = 2000000000
        Me.OutputProgessBar.Name = "OutputProgessBar"
        Me.OutputProgessBar.Size = New System.Drawing.Size(300, 10)
        Me.OutputProgessBar.TabIndex = 34
        '
        'PromptLabel
        '
        Me.PromptLabel.Font = New System.Drawing.Font("Arial Narrow", 8.5!)
        Me.PromptLabel.Location = New System.Drawing.Point(12, 9)
        Me.PromptLabel.Name = "PromptLabel"
        Me.PromptLabel.Size = New System.Drawing.Size(300, 20)
        Me.PromptLabel.TabIndex = 35
        Me.PromptLabel.Text = "XXX"
        '
        'ProgressSmall
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(326, 53)
        Me.Controls.Add(Me.PromptLabel)
        Me.Controls.Add(Me.OutputProgessBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Name = "ProgressSmall"
        Me.Text = "fProgressSmall"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents OutputProgessBar As System.Windows.Forms.ProgressBar
    Friend WithEvents PromptLabel As System.Windows.Forms.Label
End Class
