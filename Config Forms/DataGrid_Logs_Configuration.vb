Imports System.Data.SqlClient

Public Class frmLogDataGrid

    Dim connectionString As String =
        "Data Source=.\SQLEXPRESS;Initial Catalog=DMS;Integrated Security=True"

    Public Property TableName As String
    Public Property PrimaryKeyColumn As String

    Private Sub frmLogDataGrid_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = "Viewing: " & TableName

        LoadData()

    End Sub

    Private Sub LoadData()

        Dim query As String = "SELECT * FROM " & TableName

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvLogData.DataSource = dt
            End Using
        End Using

    End Sub

    Private Sub btnDeleteRow_Click(sender As Object, e As EventArgs) Handles btnDeleteRow.Click

        If dgvLogData.CurrentRow Is Nothing Then
            MessageBox.Show("Please select a row to delete.")
            Exit Sub
        End If

        Dim pkValue = dgvLogData.CurrentRow.Cells(PrimaryKeyColumn).Value

        Dim result As DialogResult = MessageBox.Show(
            "Delete this record permanently?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        )

        If result = DialogResult.No Then Exit Sub

        Dim query As String = "DELETE FROM " & TableName & " WHERE " & PrimaryKeyColumn & " = @id"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@id", pkValue)
                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        MessageBox.Show("Row deleted.")

        LoadData()

    End Sub

    Private Sub btnDeleteAll_Click(sender As Object, e As EventArgs) Handles btnDeleteAll.Click

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim result As DialogResult = verifyForm.ShowDialog()

        If result <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim finalConfirm As DialogResult = MessageBox.Show(
            "This will permanently delete ALL rows from " & TableName & "." & vbCrLf & vbCrLf & "This cannot be undone. Continue?",
            "Confirm Delete All",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        )

        If finalConfirm = DialogResult.No Then Exit Sub

        Dim query As String = "DELETE FROM " & TableName

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        MessageBox.Show("All rows deleted from " & TableName & ".", "Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information)

        LoadData()

    End Sub

End Class