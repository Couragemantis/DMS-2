<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSelectLogSource
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
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lstLogSources = New System.Windows.Forms.ListBox()
        Me.btnViewSelected = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lstLogSources
        '
        Me.lstLogSources.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lstLogSources.FormattingEnabled = True
        Me.lstLogSources.ItemHeight = 16
        Me.lstLogSources.Location = New System.Drawing.Point(12, 21)
        Me.lstLogSources.Name = "lstLogSources"
        Me.lstLogSources.Size = New System.Drawing.Size(277, 116)
        Me.lstLogSources.TabIndex = 0
        '
        'btnViewSelected
        '
        Me.btnViewSelected.BackColor = System.Drawing.Color.PaleTurquoise
        Me.btnViewSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnViewSelected.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnViewSelected.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnViewSelected.Location = New System.Drawing.Point(55, 141)
        Me.btnViewSelected.Name = "btnViewSelected"
        Me.btnViewSelected.Size = New System.Drawing.Size(190, 41)
        Me.btnViewSelected.TabIndex = 1
        Me.btnViewSelected.Text = "&SELECT"
        Me.btnViewSelected.UseVisualStyleBackColor = False
        '
        'frmSelectLogSource
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.BackgroundImage = Global.DMS.My.Resources.Resources.BG_LOGO
        Me.ClientSize = New System.Drawing.Size(301, 194)
        Me.Controls.Add(Me.btnViewSelected)
        Me.Controls.Add(Me.lstLogSources)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmSelectLogSource"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "CONFIG - SELECT LOG SOURCE"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lstLogSources As System.Windows.Forms.ListBox
    Friend WithEvents btnViewSelected As System.Windows.Forms.Button
End Class
