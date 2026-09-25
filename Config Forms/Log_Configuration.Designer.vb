<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLogConfiguration
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
        Me.btnManualDelete = New System.Windows.Forms.Button()
        Me.btnClearAllLogs = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'btnManualDelete
        '
        Me.btnManualDelete.BackColor = System.Drawing.Color.Silver
        Me.btnManualDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnManualDelete.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnManualDelete.ForeColor = System.Drawing.Color.Maroon
        Me.btnManualDelete.Location = New System.Drawing.Point(23, 16)
        Me.btnManualDelete.Name = "btnManualDelete"
        Me.btnManualDelete.Size = New System.Drawing.Size(190, 42)
        Me.btnManualDelete.TabIndex = 0
        Me.btnManualDelete.Text = "&MANUAL DELETE LOGS"
        Me.btnManualDelete.UseVisualStyleBackColor = False
        '
        'btnClearAllLogs
        '
        Me.btnClearAllLogs.BackColor = System.Drawing.Color.Gray
        Me.btnClearAllLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClearAllLogs.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClearAllLogs.ForeColor = System.Drawing.Color.LightSalmon
        Me.btnClearAllLogs.Location = New System.Drawing.Point(23, 75)
        Me.btnClearAllLogs.Name = "btnClearAllLogs"
        Me.btnClearAllLogs.Size = New System.Drawing.Size(190, 41)
        Me.btnClearAllLogs.TabIndex = 1
        Me.btnClearAllLogs.Text = "&CLEAR ALL LOGS"
        Me.btnClearAllLogs.UseVisualStyleBackColor = False
        '
        'frmLogConfiguration
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.BackgroundImage = Global.DMS.My.Resources.Resources.BG_LOGO
        Me.ClientSize = New System.Drawing.Size(236, 133)
        Me.Controls.Add(Me.btnClearAllLogs)
        Me.Controls.Add(Me.btnManualDelete)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmLogConfiguration"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "CONFIG - LOGS"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnManualDelete As System.Windows.Forms.Button
    Friend WithEvents btnClearAllLogs As System.Windows.Forms.Button
End Class
