Imports System.Data.SqlClient

Public Class frmUnavailableRooms
    Dim connectionString As String =
        "Data Source=.\SQLEXPRESS;Initial Catalog=DMS;Integrated Security=True"

    Private Sub frmUnavailableRooms_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim query As String = "SELECT room_number, floor_number, status FROM rooms WHERE status = 'Unavailable' ORDER BY room_number"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvUnavailableRooms.DataSource = dt
            End Using
        End Using
    End Sub
End Class