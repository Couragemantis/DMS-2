Public Class frmSelectLogSource

    Private Sub frmSelectLogSource_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lstLogSources.Items.Clear()
        lstLogSources.Items.Add("> Tenants Archive")
        lstLogSources.Items.Add("> Maintenance Logs")
        lstLogSources.Items.Add("> Rate Logs")
        lstLogSources.Items.Add("> Settings Access Logs")
        lstLogSources.Items.Add("> Paid Payments Logs")
    End Sub

    Private Sub btnViewSelected_Click(sender As Object, e As EventArgs) Handles btnViewSelected.Click

        If lstLogSources.SelectedItem Is Nothing Then
            MessageBox.Show("Please select a log source.")
            Exit Sub
        End If

        Dim tableName As String = ""
        Dim pkColumn As String = ""

        Select Case lstLogSources.SelectedItem.ToString()
            Case "> Tenants Archive"
                tableName = "tenants_archive"
                pkColumn = "tenant_id"
            Case "> Maintenance Logs"
                tableName = "maintenance_logs"
                pkColumn = "maintenance_id"
            Case "> Rate Logs"
                tableName = "rate_logs"
                pkColumn = "log_id"
            Case "> Settings Access Logs"
                tableName = "settings_access_logs"
                pkColumn = "log_id"
            Case "> Paid Payments Logs"
                tableName = "payments_paid"
                pkColumn = "payment_id"
        End Select

        Dim gridForm As New frmLogDataGrid()
        gridForm.TableName = tableName
        gridForm.PrimaryKeyColumn = pkColumn
        gridForm.ShowDialog()

    End Sub

    Private Sub lstLogSources_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles lstLogSources.SelectedIndexChanged

    End Sub
End Class