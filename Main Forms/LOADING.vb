Public Class LOADING

    Private Sub LOADING_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Main.Show()
        Main.Hide()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        Timer1.Stop()

        Main.Show()
        Me.Hide()

    End Sub

    Private Sub PictureBox1_Click(sender As System.Object, e As System.EventArgs) Handles PictureBox1.Click

    End Sub
End Class