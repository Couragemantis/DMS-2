Imports System.IO
Imports System.Data.SqlClient
Imports System.Drawing.Printing

Public Class frmPendingPayments
    Dim connectionString As String =
        "Data Source=.\SQLEXPRESS;Initial Catalog=DMS;Integrated Security=True"

    Private printRow As Integer = 0
    Private printColumn As Integer = 0
    Private pdfNumber As Integer = 1

    Private Sub frmPendingPayments_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim query As String =
            "SELECT payment_id, room_number, total_amount, due_date, status " &
            "FROM payments WHERE status = 'Pending' ORDER BY due_date ASC"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvPendingPayments.DataSource = dt
            End Using
        End Using
    End Sub
    Private Sub btnExportPDF_Click(sender As System.Object, e As System.EventArgs) Handles btnExportPDF.Click

        Dim saveDialog As New SaveFileDialog()

        saveDialog.Title = "Save Pending Payments Report"
        saveDialog.Filter = "PDF Files (*.pdf)|*.pdf"
        saveDialog.DefaultExt = "pdf"
        saveDialog.AddExtension = True
        saveDialog.FileName = "Pending_Payments_Report_" & pdfNumber & ".pdf"

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

    End Sub


    Private Sub PrintDocument1_PrintPage_1(sender As System.Object, e As System.Drawing.Printing.PrintPageEventArgs) Handles PrintDocument1.PrintPage

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
            "Pending Payments Report", _
            headerFont, _
            Brushes.Black, _
            left, _
            y)

        y += 25

        For i As Integer = printColumn To dgvPendingPayments.Columns.Count - 1

            If dgvPendingPayments.Columns(i).Visible Then

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
                    dgvPendingPayments.Columns(i).HeaderText, _
                    headerFont, _
                    Brushes.Black, _
                    x + 3, _
                    y + 8)


                x += columnWidth

            End If

        Next

        y += rowHeight

        While printRow < dgvPendingPayments.Rows.Count

            If dgvPendingPayments.Rows(printRow).IsNewRow Then

                printRow += 1

                Continue While

            End If


            x = left


            For i As Integer = printColumn To dgvPendingPayments.Columns.Count - 1

                If dgvPendingPayments.Columns(i).Visible Then

                    Dim value As String = ""


                    If dgvPendingPayments.Rows(printRow).Cells(i).Value IsNot Nothing Then

                        value = dgvPendingPayments.Rows(printRow).Cells(i).Value.ToString()

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

    Private Sub PrintPreviewDialog1_Load(sender As System.Object, e As System.EventArgs) Handles PrintPreviewDialog1.Load

    End Sub
End Class