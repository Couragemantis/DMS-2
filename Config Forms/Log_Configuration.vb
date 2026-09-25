Imports System.Data.SqlClient
Public Class frmLogConfiguration

    Dim connectionString As String =
        "Data Source=.\SQLEXPRESS;Initial Catalog=DMS;Integrated Security=True"

    Private Sub btnManualDelete_Click(sender As Object, e As EventArgs) Handles btnManualDelete.Click
        Dim selectForm As New frmSelectLogSource()
        selectForm.ShowDialog()
    End Sub

    Private Sub btnClearAllLogs_Click(sender As Object, e As EventArgs) Handles btnClearAllLogs.Click

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "DMS - Vista 2026"
        Dim result As DialogResult = verifyForm.ShowDialog()

        If result <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim finalConfirm As DialogResult = MessageBox.Show(
            "This will permanently delete ALL rows from tenants_archive, maintenance_logs, rate_logs, and settings_access_logs." & vbCrLf & vbCrLf & "This cannot be undone. Continue?",
            "Confirm Clear All Logs",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        )

        If finalConfirm = DialogResult.No Then Exit Sub

        Try
            Using con As New SqlConnection(connectionString)
                con.Open()
                Dim transaction As SqlTransaction = con.BeginTransaction()

                Try
                    Dim tables() As String = {"tenants_archive", "maintenance_logs", "rate_logs", "settings_access_logs"}

                    For Each tbl In tables
                        Using cmd As New SqlCommand("DELETE FROM " & tbl, con, transaction)
                            cmd.ExecuteNonQuery()
                        End Using
                    Next

                    transaction.Commit()

                Catch ex As Exception
                    transaction.Rollback()
                    MessageBox.Show("Error clearing logs: " & ex.Message)
                    Exit Sub
                End Try
            End Using

            MessageBox.Show("All logs cleared successfully.", "Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try

    End Sub

End Class