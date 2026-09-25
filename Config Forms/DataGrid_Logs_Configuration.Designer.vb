<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLogDataGrid
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
        Me.dgvLogData = New System.Windows.Forms.DataGridView()
        Me.btnDeleteRow = New System.Windows.Forms.Button()
        Me.btnDeleteAll = New System.Windows.Forms.Button()
        CType(Me.dgvLogData, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dgvLogData
        '
        Me.dgvLogData.AllowUserToAddRows = False
        Me.dgvLogData.AllowUserToDeleteRows = False
        Me.dgvLogData.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvLogData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvLogData.Location = New System.Drawing.Point(6, 9)
        Me.dgvLogData.MultiSelect = False
        Me.dgvLogData.Name = "dgvLogData"
        Me.dgvLogData.ReadOnly = True
        Me.dgvLogData.Size = New System.Drawing.Size(986, 353)
        Me.dgvLogData.TabIndex = 0
        '
        'btnDeleteRow
        '
        Me.btnDeleteRow.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnDeleteRow.BackColor = System.Drawing.Color.SandyBrown
        Me.btnDeleteRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDeleteRow.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnDeleteRow.Location = New System.Drawing.Point(6, 368)
        Me.btnDeleteRow.Name = "btnDeleteRow"
        Me.btnDeleteRow.Size = New System.Drawing.Size(130, 37)
        Me.btnDeleteRow.TabIndex = 1
        Me.btnDeleteRow.Text = "DELETE &ROW"
        Me.btnDeleteRow.UseVisualStyleBackColor = False
        '
        'btnDeleteAll
        '
        Me.btnDeleteAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnDeleteAll.BackColor = System.Drawing.Color.Salmon
        Me.btnDeleteAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDeleteAll.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnDeleteAll.Location = New System.Drawing.Point(862, 368)
        Me.btnDeleteAll.Name = "btnDeleteAll"
        Me.btnDeleteAll.Size = New System.Drawing.Size(130, 37)
        Me.btnDeleteAll.TabIndex = 2
        Me.btnDeleteAll.Text = "DELETE &ALL"
        Me.btnDeleteAll.UseVisualStyleBackColor = False
        '
        'frmLogDataGrid
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.BackgroundImage = Global.DMS.My.Resources.Resources.BG_LOGO
        Me.ClientSize = New System.Drawing.Size(1001, 411)
        Me.Controls.Add(Me.btnDeleteAll)
        Me.Controls.Add(Me.btnDeleteRow)
        Me.Controls.Add(Me.dgvLogData)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MinimizeBox = False
        Me.Name = "frmLogDataGrid"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "CONFIG - DELETE ROWS"
        CType(Me.dgvLogData, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents dgvLogData As System.Windows.Forms.DataGridView
    Friend WithEvents btnDeleteRow As System.Windows.Forms.Button
    Friend WithEvents btnDeleteAll As System.Windows.Forms.Button
End Class
