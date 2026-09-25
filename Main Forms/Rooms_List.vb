Imports System.Data.SqlClient

Public Class frmAllRooms
    Dim connectionString As String =
        "Data Source=.\SQLEXPRESS;Initial Catalog=DMS;Integrated Security=True"

    Private Sub frmAllRooms_Load(sender As Object, e As EventArgs) Handles MyBase.Load
       

        Dim query As String = "SELECT room_number, floor_number, status FROM rooms ORDER BY room_number"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvAllRooms.DataSource = dt
            End Using
        End Using
    End Sub

    Private Sub dgvAllRooms_CellContentClick(sender As System.Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvAllRooms.CellContentClick

    End Sub
End Class