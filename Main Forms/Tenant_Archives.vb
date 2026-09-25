Imports System.IO
Imports System.Data.SqlClient
Imports System.Drawing.Printing

Public Class frmTenantArchives
    Private connectionString As String = "Data Source=.\SQLEXPRESS;Initial Catalog=DMS;Integrated Security=True"


    Private printRow As Integer = 0
    Private printColumn As Integer = 0
    Private pdfNumber As Integer = 1
    Private Sub frmTenantArchives_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadArchivedTenants()
    End Sub
    Public Sub LoadArchivedTenants()

        Try

            Using con As New SqlConnection(connectionString)

                Dim query As String = _
                    "SELECT tenant_id, room_number, lname, fname, age, gender, address, " & _
                    "contact_number, emergency_person, emergency_contact " & _
                    "FROM tenants_archive"

                Using da As New SqlDataAdapter(query, con)

                    Dim dt As New DataTable()

                    da.Fill(dt)

                    dgvArchive.DataSource = dt

                End Using

            End Using

        Catch ex As Exception

            MessageBox.Show( _
                "Error loading archived tenants: " & ex.Message, _
                "Error", _
                MessageBoxButtons.OK, _
                MessageBoxIcon.Error)

        End Try

    End Sub
    Private Sub btnRestoreArchived_Click(sender As Object, e As EventArgs) Handles btnRestoreArchived.Click

        If dgvArchive.CurrentRow Is Nothing Then

            MessageBox.Show( _
                "Please select an archived tenant to restore.", _
                "Selection Required", _
                MessageBoxButtons.OK, _
                MessageBoxIcon.Information)

            Exit Sub

        End If

        Dim tenantID As Integer = CInt(dgvArchive.CurrentRow.Cells("tenant_id").Value)

        Dim tenantName As String = _
            dgvArchive.CurrentRow.Cells("fname").Value.ToString() & " " & _
            dgvArchive.CurrentRow.Cells("lname").Value.ToString()

        Dim result As DialogResult = MessageBox.Show( _
            "Are you sure you want to restore " & tenantName & "?" & vbCrLf & _
            "Their original room assignment will also be restored.", _
            "Confirm Restore", _
            MessageBoxButtons.YesNo, _
            MessageBoxIcon.Question)


        If result = DialogResult.No Then

            Exit Sub

        End If

        Using con As New SqlConnection(connectionString)

            con.Open()

            Dim transaction As SqlTransaction = con.BeginTransaction()

            Try

                Dim checkQuery As String = _
                    "SELECT COUNT(*) FROM tenants WHERE tenant_id = @id"

                Using checkCmd As New SqlCommand(checkQuery, con, transaction)

                    checkCmd.Parameters.AddWithValue("@id", tenantID)

                    Dim existingTenant As Integer = _
                        CInt(checkCmd.ExecuteScalar())

                    If existingTenant > 0 Then

                        transaction.Rollback()

                        MessageBox.Show( _
                            "This tenant already exists in the tenants table.", _
                            "Restore Failed", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)

                        Exit Sub

                    End If

                End Using

                Dim identityOn As String = _
                    "SET IDENTITY_INSERT tenants ON"

                Using cmdIdentityOn As New SqlCommand(identityOn, con, transaction)

                    cmdIdentityOn.ExecuteNonQuery()

                End Using

                Dim restoreQuery As String = _
                    "INSERT INTO tenants " & _
                    "(tenant_id, room_number, lname, fname, age, gender, address, " & _
                    "contact_number, emergency_person, emergency_contact) " & _
                    "SELECT tenant_id, room_number, lname, fname, age, gender, address, " & _
                    "contact_number, emergency_person, emergency_contact " & _
                    "FROM tenants_archive " & _
                    "WHERE tenant_id = @id"


                Using cmdRestore As New SqlCommand(restoreQuery, con, transaction)

                    cmdRestore.Parameters.AddWithValue("@id", tenantID)

                    cmdRestore.ExecuteNonQuery()

                End Using

                Dim identityOff As String = _
                    "SET IDENTITY_INSERT tenants OFF"

                Using cmdIdentityOff As New SqlCommand(identityOff, con, transaction)

                    cmdIdentityOff.ExecuteNonQuery()

                End Using

                Dim deleteQuery As String = _
                    "DELETE FROM tenants_archive WHERE tenant_id = @id"

                Using cmdDelete As New SqlCommand(deleteQuery, con, transaction)

                    cmdDelete.Parameters.AddWithValue("@id", tenantID)

                    cmdDelete.ExecuteNonQuery()

                End Using

                transaction.Commit()


            Catch ex As Exception

                Try

                    transaction.Rollback()

                Catch rollbackEx As Exception

                End Try


                MessageBox.Show( _
                    "Error restoring tenant: " & ex.Message, _
                    "Error", _
                    MessageBoxButtons.OK, _
                    MessageBoxIcon.Error)

                Exit Sub

            End Try

        End Using

        LoadArchivedTenants()

        If Application.OpenForms("Main") IsNot Nothing Then

            DirectCast(Application.OpenForms("Main"), Main).LoadTenants()

        End If

        MessageBox.Show( _
            "Tenant restored successfully!" & vbCrLf & _
            "The original room number has also been restored.", _
            "Success", _
            MessageBoxButtons.OK, _
            MessageBoxIcon.Information)

    End Sub
    Private Sub btnExportPDF_Click(sender As System.Object, e As System.EventArgs) Handles btnExportPDF.Click

        Dim saveDialog As New SaveFileDialog()

        saveDialog.Title = "Save Tenant Archives Report"
        saveDialog.Filter = "PDF Files (*.pdf)|*.pdf"
        saveDialog.DefaultExt = "pdf"
        saveDialog.AddExtension = True

        saveDialog.FileName = "Tenant_Archives_" & pdfNumber & ".pdf"

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
        LoadArchivedTenants()
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
            "Tenant Archives", _
            headerFont, _
            Brushes.Black, _
            left, _
            y)

        y += 25

        For i As Integer = printColumn To dgvArchive.Columns.Count - 1

            If dgvArchive.Columns(i).Visible Then

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
                    dgvArchive.Columns(i).HeaderText, _
                    headerFont, _
                    Brushes.Black, _
                    x + 3, _
                    y + 8)


                x += columnWidth

            End If

        Next


        y += rowHeight

        While printRow < dgvArchive.Rows.Count

            If dgvArchive.Rows(printRow).IsNewRow Then

                printRow += 1

                Continue While

            End If


            x = left


            For i As Integer = printColumn To dgvArchive.Columns.Count - 1

                If dgvArchive.Columns(i).Visible Then

                    Dim value As String = ""


                    If dgvArchive.Rows(printRow).Cells(i).Value IsNot Nothing Then

                        value = dgvArchive.Rows(printRow).Cells(i).Value.ToString()

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