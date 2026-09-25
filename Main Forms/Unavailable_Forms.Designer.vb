<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmUnavailableRooms
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
        Me.dgvUnavailableRooms = New System.Windows.Forms.DataGridView()
        CType(Me.dgvUnavailableRooms, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dgvUnavailableRooms
        '
        Me.dgvUnavailableRooms.AllowUserToAddRows = False
        Me.dgvUnavailableRooms.AllowUserToDeleteRows = False
        Me.dgvUnavailableRooms.AllowUserToResizeRows = False
        Me.dgvUnavailableRooms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvUnavailableRooms.Location = New System.Drawing.Point(12, 12)
        Me.dgvUnavailableRooms.MultiSelect = False
        Me.dgvUnavailableRooms.Name = "dgvUnavailableRooms"
        Me.dgvUnavailableRooms.ReadOnly = True
        Me.dgvUnavailableRooms.Size = New System.Drawing.Size(390, 191)
        Me.dgvUnavailableRooms.TabIndex = 0
        '
        'frmUnavailableRooms
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.BackgroundImage = Global.DMS.My.Resources.Resources.BG_LOGO
        Me.ClientSize = New System.Drawing.Size(414, 215)
        Me.Controls.Add(Me.dgvUnavailableRooms)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "frmUnavailableRooms"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Unavailabe Rooms List"
        CType(Me.dgvUnavailableRooms, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents dgvUnavailableRooms As System.Windows.Forms.DataGridView
End Class
