<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAllRooms
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
        Me.dgvAllRooms = New System.Windows.Forms.DataGridView()
        CType(Me.dgvAllRooms, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dgvAllRooms
        '
        Me.dgvAllRooms.AllowUserToAddRows = False
        Me.dgvAllRooms.AllowUserToDeleteRows = False
        Me.dgvAllRooms.AllowUserToResizeRows = False
        Me.dgvAllRooms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvAllRooms.Location = New System.Drawing.Point(12, 12)
        Me.dgvAllRooms.MultiSelect = False
        Me.dgvAllRooms.Name = "dgvAllRooms"
        Me.dgvAllRooms.ReadOnly = True
        Me.dgvAllRooms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvAllRooms.Size = New System.Drawing.Size(390, 193)
        Me.dgvAllRooms.TabIndex = 0
        '
        'frmAllRooms
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.BackgroundImage = Global.DMS.My.Resources.Resources.BG_LOGO
        Me.ClientSize = New System.Drawing.Size(414, 217)
        Me.Controls.Add(Me.dgvAllRooms)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "frmAllRooms"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Rooms List"
        CType(Me.dgvAllRooms, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents dgvAllRooms As System.Windows.Forms.DataGridView
End Class
