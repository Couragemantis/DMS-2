Imports System.IO
Imports System.Data.SqlClient
Imports System.Drawing.Printing

Public Class frmTenantsList
    Dim connectionString As String =
        "Data Source=.\SQLEXPRESS;Initial Catalog=DMS;Integrated Security=True"
    Private printRow As Integer = 0
    Private printColumn As Integer = 0
    Private pdfNumber As Integer = 1
    Private Sub LoadTenants()
        Dim query As String = "SELECT * FROM tenants ORDER BY tenant_id"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvTenantsList.DataSource = dt
            End Using
        End Using
    End Sub
    Private Sub frmTenantsList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadTenants()
    End Sub
    Private Sub btnExportPDF_Click(sender As System.Object, e As System.EventArgs) Handles btnExportPDF.Click
        Dim saveDialog As New SaveFileDialog()

        saveDialog.Title = "Save Tenants Report"
        saveDialog.Filter = "PDF Files (*.pdf)|*.pdf"
        saveDialog.DefaultExt = "pdf"
        saveDialog.AddExtension = True
        saveDialog.FileName = "Tenants_Report_" & pdfNumber & ".pdf"

        If saveDialog.ShowDialog() <> DialogResult.OK Then
            Exit Sub
        End If

        Dim filePath As String = saveDialog.FileName

        printRow = 0
        printColumn = 0

        PrintDocument1.DefaultPageSettings.Landscape = True
        PrintDocument1.DefaultPageSettings.Margins = _
            New Margins(50, 50, 50, 50)

        PrintDocument1.PrinterSettings.PrinterName = "Microsoft Print to PDF"
        PrintDocument1.PrinterSettings.PrintToFile = True
        PrintDocument1.PrinterSettings.PrintFileName = filePath
        PrintDocument1.Print()

        pdfNumber += 1

        MessageBox.Show( _
            "PDF successfully saved!" & vbCrLf & vbCrLf & _
            filePath, _
            "Export Complete", _
            MessageBoxButtons.OK, _
            MessageBoxIcon.Information)
        LoadTenants()

    End Sub

    Private Sub PrintDocument1_PrintPage(sender As System.Object, e As System.Drawing.Printing.PrintPageEventArgs) Handles PrintDocument1.PrintPage

        Dim font As New Font("Arial", 6)
        Dim headerFont As New Font("Arial", 7, FontStyle.Bold)
        Dim left As Integer = e.MarginBounds.Left
        Dim top As Integer = e.MarginBounds.Top
        Dim x As Integer = left
        Dim y As Integer = top
        Dim rowHeight As Integer = 40
        Dim columnWidth As Integer = 100
        Dim titleFont As New Font("Arial", 14, FontStyle.Bold)

        e.Graphics.DrawString( _
            "DORM MANAGEMENT SYSTEM", _
            titleFont, _
            Brushes.Black, _
            left, _
            y)

        y += 30

        e.Graphics.DrawString( _
            "Tenants Report", _
            headerFont, _
            Brushes.Black, _
            left, _
            y)

        y += 25

        For i As Integer = printColumn To dgvTenantsList.Columns.Count - 1

            If dgvTenantsList.Columns(i).Visible Then

                e.Graphics.FillRectangle( _
                    Brushes.LightGray, _
                    x, _
                    y, _
                    columnWidth, _
                    rowHeight)


                e.Graphics.DrawRectangle( _
                    Pens.Black, _
                    x, _
                    y, _
                    columnWidth, _
                    rowHeight)


                e.Graphics.DrawString( _
                    dgvTenantsList.Columns(i).HeaderText, _
                    headerFont, _
                    Brushes.Black, _
                    x + 3, _
                    y + 8)


                x += columnWidth

            End If

        Next


        y += rowHeight

        While printRow < dgvTenantsList.Rows.Count

            If dgvTenantsList.Rows(printRow).IsNewRow Then

                printRow += 1

                Continue While

            End If


            x = left


            For i As Integer = printColumn To dgvTenantsList.Columns.Count - 1

                If dgvTenantsList.Columns(i).Visible Then

                    Dim value As String = ""


                    If dgvTenantsList.Rows(printRow).Cells(i).Value IsNot Nothing Then

                        value = dgvTenantsList.Rows(printRow).Cells(i).Value.ToString()

                    End If

                    e.Graphics.DrawRectangle( _
                        Pens.Black, _
                        x, _
                        y, _
                        columnWidth, _
                        rowHeight)

                    e.Graphics.DrawString( _
                        value, _
                        font, _
                        Brushes.Black, _
                        x + 3, _
                        y + 8)


                    x += columnWidth

                End If

            Next


            y += rowHeight

            printRow += 1

            If y + rowHeight > e.MarginBounds.Bottom Then

                e.HasMorePages = True

                Return

            End If

        End While

        e.HasMorePages = False

        printRow = 0
        printColumn = 0

    End Sub
End Class