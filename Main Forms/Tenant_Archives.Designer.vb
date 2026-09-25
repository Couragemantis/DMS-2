<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTenantArchives
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTenantArchives))
        Me.dgvArchive = New System.Windows.Forms.DataGridView()
        Me.btnRestoreArchived = New System.Windows.Forms.Button()
        Me.btnExportPDF = New System.Windows.Forms.Button()
        Me.PrintDocument1 = New System.Drawing.Printing.PrintDocument()
        Me.PrintPreviewDialog1 = New System.Windows.Forms.PrintPreviewDialog()
        CType(Me.dgvArchive, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dgvArchive
        '
        Me.dgvArchive.AllowUserToAddRows = False
        Me.dgvArchive.AllowUserToDeleteRows = False
        Me.dgvArchive.AllowUserToResizeRows = False
        Me.dgvArchive.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvArchive.Location = New System.Drawing.Point(12, 12)
        Me.dgvArchive.MultiSelect = False
        Me.dgvArchive.Name = "dgvArchive"
        Me.dgvArchive.ReadOnly = True
        Me.dgvArchive.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvArchive.Size = New System.Drawing.Size(1026, 233)
        Me.dgvArchive.TabIndex = 0
        '
        'btnRestoreArchived
        '
        Me.btnRestoreArchived.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnRestoreArchived.BackColor = System.Drawing.Color.LightGreen
        Me.btnRestoreArchived.FlatAppearance.BorderColor = System.Drawing.Color.Gray
        Me.btnRestoreArchived.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRestoreArchived.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnRestoreArchived.ForeColor = System.Drawing.Color.Black
        Me.btnRestoreArchived.Location = New System.Drawing.Point(12, 251)
        Me.btnRestoreArchived.Name = "btnRestoreArchived"
        Me.btnRestoreArchived.Size = New System.Drawing.Size(225, 47)
        Me.btnRestoreArchived.TabIndex = 32
        Me.btnRestoreArchived.Text = "&RESTORE FROM ARCHIVES"
        Me.btnRestoreArchived.UseVisualStyleBackColor = False
        '
        'btnExportPDF
        '
        Me.btnExportPDF.BackColor = System.Drawing.Color.PowderBlue
        Me.btnExportPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnExportPDF.Font = New System.Drawing.Font("Tahoma", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnExportPDF.Location = New System.Drawing.Point(882, 251)
        Me.btnExportPDF.Name = "btnExportPDF"
        Me.btnExportPDF.Size = New System.Drawing.Size(156, 47)
        Me.btnExportPDF.TabIndex = 33
        Me.btnExportPDF.Text = "&SAVE REPORT"
        Me.btnExportPDF.UseVisualStyleBackColor = False
        '
        'PrintDocument1
        '
        '
        'PrintPreviewDialog1
        '
        Me.PrintPreviewDialog1.AutoScrollMargin = New System.Drawing.Size(0, 0)
        Me.PrintPreviewDialog1.AutoScrollMinSize = New System.Drawing.Size(0, 0)
        Me.PrintPreviewDialog1.ClientSize = New System.Drawing.Size(400, 300)
        Me.PrintPreviewDialog1.Enabled = True
        Me.PrintPreviewDialog1.Icon = CType(resources.GetObject("PrintPreviewDialog1.Icon"), System.Drawing.Icon)
        Me.PrintPreviewDialog1.Name = "PrintPreviewDialog1"
        Me.PrintPreviewDialog1.Visible = False
        '
        'frmTenantArchives
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(84, Byte), Integer), CType(CType(113, Byte), Integer))
        Me.BackgroundImage = Global.DMS.My.Resources.Resources.BG_LOGO
        Me.ClientSize = New System.Drawing.Size(1050, 310)
        Me.Controls.Add(Me.btnExportPDF)
        Me.Controls.Add(Me.btnRestoreArchived)
        Me.Controls.Add(Me.dgvArchive)
        Me.Name = "frmTenantArchives"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Restore Tenant From Archive"
        CType(Me.dgvArchive, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents dgvArchive As System.Windows.Forms.DataGridView
    Friend WithEvents btnRestoreArchived As System.Windows.Forms.Button
    Friend WithEvents btnExportPDF As System.Windows.Forms.Button
    Friend WithEvents PrintDocument1 As System.Drawing.Printing.PrintDocument
    Friend WithEvents PrintPreviewDialog1 As System.Windows.Forms.PrintPreviewDialog
End Class
