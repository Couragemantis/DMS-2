Imports System.Data.SqlClient
Imports System.Net
Imports System.Linq
Imports System.Management
Imports System.Diagnostics
Imports System.Drawing.Printing

Public Class Main
    Dim connectionString As String =
    "Data Source=.\SQLEXPRESS;Initial Catalog=DMS;Integrated Security=True"

    Dim selectedTenantId As Integer? = Nothing
    Dim selectedRoomNumber As Integer? = Nothing
    Dim tenantsTable As DataTable
    Dim paymentsTable As DataTable
    Dim roomsTable As DataTable
    Dim roomAssignmentsTable As DataTable
    Dim maintenanceTable As DataTable
    Dim unavailableRoomsForm As frmUnavailableRooms = Nothing
    Dim tenantsListForm As frmTenantsList = Nothing
    Dim maintenanceRoomsForm As frmMaintenanceRooms = Nothing
    Dim allRoomsForm As frmAllRooms = Nothing
    Dim pendingPaymentsForm As frmPendingPayments = Nothing
    Dim paidForm As frmPaidPayments = Nothing
    Dim logConfigForm As frmLogConfiguration = Nothing

    'payment dimenionss
    Dim printPaymentId As Integer
    Dim printRoomNumber As Integer
    Dim printTenantList As New List(Of String())
    Dim printElectricityConsumption As Decimal
    Dim printWaterConsumption As Decimal
    Dim printElectricityRate As Decimal
    Dim printWaterRate As Decimal
    Dim printElectricityAmount As Decimal
    Dim printWaterAmount As Decimal
    Dim printRentalAmount As Decimal
    Dim printTotalAmount As Decimal
    Dim printDueDate As DateTime
    Dim printDateCreated As DateTime
    Dim pdfNumber As Integer = 1

    Dim backupFolder As String = "C:\DMS_Backups\"
    Private Sub MakePanelsDoubleBuffered(ByVal parent As Control)

        For Each ctrl As Control In parent.Controls

            If TypeOf ctrl Is Panel Then

                ctrl.GetType().GetProperty("DoubleBuffered",
                    Reflection.BindingFlags.Instance Or
                    Reflection.BindingFlags.NonPublic).SetValue(ctrl, True, Nothing)

            End If

            If ctrl.HasChildren Then
                MakePanelsDoubleBuffered(ctrl)
            End If

        Next

    End Sub
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

        LoadBackupFolderSetting()

        If Not IsPhilippineTimeValid() Then
            Application.Exit()
            Exit Sub
        End If

        Me.DoubleBuffered = True

        MakePanelsDoubleBuffered(Me)

        PHeaderDashboard.BringToFront()
        PDashboard.BringToFront()

        lblSectionName.Text = "D A S H B O A R D"

        PDashboard.Show()
        PTenants.Hide()
        PRooms.Hide()
        PRoomStat.Hide()
        PPayment.Hide()
        PSettings.Hide()
        PHeaderManagement.Hide()
        PHeaderMaintenance.Hide()
        PHeaderPayments.Hide()
        PHeaderSetting.Hide()
        PHeaderDashboard.Show()

        LoadDashboard()

    End Sub
    Private Sub EnsureBackupFolderExists()

        If Not IO.Directory.Exists(backupFolder) Then
            IO.Directory.CreateDirectory(backupFolder)
        End If

        Try
            Dim psi As New ProcessStartInfo("icacls.exe")
            psi.Arguments = """" & backupFolder.TrimEnd("\"c) & """ /grant Everyone:(OI)(CI)F /T"
            psi.UseShellExecute = False
            psi.CreateNoWindow = True
            psi.RedirectStandardOutput = True
            psi.RedirectStandardError = True

            Using p As Process = Process.Start(psi)
                p.WaitForExit()
            End Using

        Catch ex As Exception

        End Try

    End Sub
    Private Function PerformBackup() As Boolean

        EnsureBackupFolderExists()

        Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
        Dim backupFileName As String = "DMS_Backup_" & timestamp & ".bak"
        Dim fullPath As String = backupFolder & backupFileName

        Dim query As String =
            "BACKUP DATABASE [DMS] TO DISK = @path WITH INIT, NAME = 'DMS Full Backup'"

        Try
            Using con As New SqlConnection(connectionString)
                Using cmd As New SqlCommand(query, con)
                    cmd.Parameters.AddWithValue("@path", fullPath)
                    cmd.CommandTimeout = 120
                    con.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            CleanupOldBackups()

            Return True

        Catch ex As Exception

            If ex.Message.Contains("Access is denied") OrElse ex.Message.Contains("Operating system error 5") Then
                MessageBox.Show(
                    "Backup failed: SQL Server does not have permission to write to:" & vbCrLf & backupFolder & vbCrLf & vbCrLf &
                    "Try choosing a different folder, or run this application as Administrator once to fix folder permissions.",
                    "Backup Permission Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )
            Else
                MessageBox.Show("Backup failed: " & ex.Message)
            End If

            Return False

        End Try

    End Function
    Private Sub LoadBackupFolderSetting()

        Dim query As String = "SELECT TOP 1 backup_folder_path FROM settings"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                con.Open()
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then
                    backupFolder = result.ToString()
                End If
            End Using
        End Using

    End Sub
    Private Sub LoadBackupList()

        If Not IO.Directory.Exists(backupFolder) Then Exit Sub

        Dim files = IO.Directory.GetFiles(backupFolder, "*.bak")

        Dim dt As New DataTable()
        dt.Columns.Add("File Name")
        dt.Columns.Add("Date Created")
        dt.Columns.Add("Size (KB)")

        For Each filePath In files
            Dim fileInfo As New IO.FileInfo(filePath)
            Dim row As DataRow = dt.NewRow()
            row("File Name") = fileInfo.Name
            row("Date Created") = fileInfo.CreationTime.ToString("MMMM dd, yyyy - hh:mm tt")
            row("Size (KB)") = Math.Round(fileInfo.Length / 1024, 2)
            dt.Rows.Add(row)
        Next

        dgvBackups.DataSource = dt


    End Sub
    Private Sub CleanupOldBackups()

        Const maxBackups As Integer = 5

        If Not IO.Directory.Exists(backupFolder) Then Exit Sub

        Dim files = IO.Directory.GetFiles(backupFolder, "DMS_Backup_*.bak")
        Dim sortedFiles = files.OrderBy(Function(f) New IO.FileInfo(f).CreationTime).ToList()

        While sortedFiles.Count > maxBackups
            Try
                IO.File.Delete(sortedFiles(0))
                sortedFiles.RemoveAt(0)
            Catch ex As Exception
                sortedFiles.RemoveAt(0)
            End Try
        End While

    End Sub
    Private Sub LoadDriveInfo()

        Try

            Dim driveLetter As String = IO.Path.GetPathRoot(backupFolder)
            Dim drive As New IO.DriveInfo(driveLetter)

            If drive.IsReady Then

                Dim totalSize As Long = drive.TotalSize
                Dim freeSpace As Long = drive.AvailableFreeSpace
                Dim usedSpace As Long = totalSize - freeSpace

                Dim usedPercent As Integer = CInt((usedSpace / totalSize) * 100)

                pbStorageUsage.Minimum = 0
                pbStorageUsage.Maximum = 100
                pbStorageUsage.Value = usedPercent

                Dim totalGB As Double = Math.Round(totalSize / (1024 * 1024 * 1024), 2)
                Dim usedGB As Double = Math.Round(usedSpace / (1024 * 1024 * 1024), 2)
                Dim freeGB As Double = Math.Round(freeSpace / (1024 * 1024 * 1024), 2)

                lblStorageInfo.Text = "Drive " & driveLetter & " — " & usedGB & " GB used of " & totalGB & " GB (" & freeGB & " GB free) — " & usedPercent & "% used"

                If usedPercent >= 90 Then
                    pbStorageUsage.ForeColor = Color.Red
                ElseIf usedPercent >= 75 Then
                    pbStorageUsage.ForeColor = Color.Orange
                Else
                    pbStorageUsage.ForeColor = Color.Green
                End If

            Else
                lblStorageInfo.Text = "Drive not ready."
            End If

        Catch ex As Exception
            lblStorageInfo.Text = "Unable to read drive information: " & ex.Message
        End Try

        LoadDriveHealth()

    End Sub

    Private Sub LoadDriveHealth()

        Try
            Dim searcher As New ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive")

            Dim healthReport As String = ""

            For Each disk As ManagementObject In searcher.Get()
                Dim model As String = disk("Model").ToString()
                Dim status As String = disk("Status").ToString()

                healthReport &= model & ": " & status & vbCrLf
            Next

            If healthReport = "" Then
                lblDriveHealth.Text = "No drive health data available."
            Else
                lblDriveHealth.Text = healthReport.TrimEnd(vbCrLf.ToCharArray())
            End If

            If healthReport.Contains("OK") AndAlso Not healthReport.Contains("Bad") AndAlso Not healthReport.Contains("Unknown") Then
                lblDriveHealth.ForeColor = Color.Green
            Else
                lblDriveHealth.ForeColor = Color.Red
            End If

        Catch ex As Exception
            lblDriveHealth.Text = "Unable to check drive health: " & ex.Message
        End Try

    End Sub
    Private Sub ApplyFilters()

        If tenantsTable Is Nothing Then Exit Sub

        Dim searchText As String = txtSearch.Text.Replace("'", "''")

        Dim filterParts As New List(Of String)

        If searchText <> "" Then
            filterParts.Add("(lname LIKE '%" & searchText & "%' OR fname LIKE '%" & searchText & "%')")
        End If

        If cmbGenderFilter.SelectedItem IsNot Nothing AndAlso cmbGenderFilter.Text <> "All" Then
            Dim genderText As String = cmbGenderFilter.Text.Replace("'", "''")
            filterParts.Add("gender = '" & genderText & "'")
        End If

        Dim finalFilter As String = String.Join(" AND ", filterParts)

        Dim sortExpression As String = ""

        Select Case cmbSortBy.Text
            Case "ID: Low to High"
                sortExpression = "tenant_id ASC"
            Case "ID: High to Low"
                sortExpression = "tenant_id DESC"
            Case "Name: A-Z"
                sortExpression = "lname ASC, fname ASC"
            Case "Name: Z-A"
                sortExpression = "lname DESC, fname DESC"
            Case "Room: Low to High"
                sortExpression = "room_number ASC"
            Case "Room: High to Low"
                sortExpression = "room_number DESC"
        End Select

        Try
            tenantsTable.DefaultView.RowFilter = finalFilter
            tenantsTable.DefaultView.Sort = sortExpression
        Catch ex As Exception
        End Try

    End Sub
    Private Sub ApplyMaintenanceFilters()

        If maintenanceTable Is Nothing Then Exit Sub

        Dim searchText As String = txtSearchMaintenance.Text.Replace("'", "''")

        Dim filter As String = ""

        If searchText <> "" Then
            filter =
                "CONVERT(room_number, System.String) LIKE '%" & searchText & "%' OR " &
                "CONVERT(maintenance_id, System.String) LIKE '%" & searchText & "%' OR " &
                "maintenance_reason LIKE '%" & searchText & "%'"
        End If

        Dim sortExpression As String = ""

        Select Case cmbSortMaintenance.Text
            Case "Room: Low to High"
                sortExpression = "room_number ASC"
            Case "Room: High to Low"
                sortExpression = "room_number DESC"
            Case "Date Reported: Newest First"
                sortExpression = "date_reported DESC"
            Case "Date Reported: Oldest First"
                sortExpression = "date_reported ASC"
        End Select

        Try
            maintenanceTable.DefaultView.RowFilter = filter
            maintenanceTable.DefaultView.Sort = sortExpression
        Catch ex As Exception
        End Try

    End Sub
    Private Sub ApplyPaymentFilters()

        If paymentsTable Is Nothing Then Exit Sub

        Dim searchText As String = txtSearchPayment.Text.Replace("'", "''")

        Dim filter As String = ""

        If searchText <> "" Then
            filter =
                "CONVERT(payment_id, System.String) LIKE '%" & searchText & "%' OR " &
                "CONVERT(room_number, System.String) LIKE '%" & searchText & "%'"
        End If

        Dim sortExpression As String = ""

        Select Case cmbSortPayments.Text
            Case "Payment ID: Low to High"
                sortExpression = "payment_id ASC"
            Case "Payment ID: High to Low"
                sortExpression = "payment_id DESC"
            Case "Room: Low to High"
                sortExpression = "room_number ASC"
            Case "Room: High to Low"
                sortExpression = "room_number DESC"
            Case "Total Amount: Low to High"
                sortExpression = "total_amount ASC"
            Case "Total Amount: High to Low"
                sortExpression = "total_amount DESC"
            Case "Due Date: Soonest First"
                sortExpression = "due_date ASC"
            Case "Due Date: Latest First"
                sortExpression = "due_date DESC"
        End Select

        Try
            paymentsTable.DefaultView.RowFilter = filter
            paymentsTable.DefaultView.Sort = sortExpression
        Catch ex As Exception
        End Try

    End Sub
    Private Sub ApplyRoomAssignmentFilters()

        If roomAssignmentsTable Is Nothing Then Exit Sub

        Dim searchText As String = txtSearchRoomAssign.Text.Replace("'", "''")

        Dim filter As String = ""

        If searchText <> "" Then
            filter =
                "CONVERT(room_number, System.String) LIKE '%" & searchText & "%' OR " &
                "lname LIKE '%" & searchText & "%' OR " &
                "fname LIKE '%" & searchText & "%'"
        End If

        Dim sortExpression As String = ""

        Select Case cmbSortRoomAssign.Text
            Case "Room: Low to High"
                sortExpression = "room_number ASC"
            Case "Room: High to Low"
                sortExpression = "room_number DESC"
            Case "Floor: Low to High"
                sortExpression = "floor_number ASC"
            Case "Floor: High to Low"
                sortExpression = "floor_number DESC"
            Case "Tenant Name: A-Z"
                sortExpression = "lname ASC, fname ASC"
            Case "Tenant Name: Z-A"
                sortExpression = "lname DESC, fname DESC"
        End Select

        Try
            roomAssignmentsTable.DefaultView.RowFilter = filter
            roomAssignmentsTable.DefaultView.Sort = sortExpression
        Catch ex As Exception

        End Try

    End Sub
    Private Sub ApplyRoomFilters()

        If roomsTable Is Nothing Then Exit Sub

        Dim searchText As String = txtSearchRoom.Text.Replace("'", "''")

        Dim filter As String = ""

        If searchText <> "" Then
            filter =
                "CONVERT(room_number, System.String) LIKE '%" & searchText & "%' OR " &
                "CONVERT(floor_number, System.String) LIKE '%" & searchText & "%'"
        End If

        Dim sortExpression As String = ""

        Select Case cmbSortRooms.Text
            Case "Room Number: Low to High"
                sortExpression = "room_number ASC"
            Case "Room Number: High to Low"
                sortExpression = "room_number DESC"
            Case "Floor: Low to High"
                sortExpression = "floor_number ASC"
            Case "Floor: High to Low"
                sortExpression = "floor_number DESC"
            Case "Status: A-Z"
                sortExpression = "status ASC"
        End Select

        Try
            roomsTable.DefaultView.RowFilter = filter
            roomsTable.DefaultView.Sort = sortExpression
        Catch ex As Exception

        End Try

    End Sub
    Private Sub cmbSortMaintenance_SelectedIndexChanged(sender As Object, e As EventArgs)
        ApplyMaintenanceFilters()
    End Sub

    Private Sub cmbSortRoomAssign_SelectedIndexChanged(sender As Object, e As EventArgs)
        ApplyRoomAssignmentFilters()
    End Sub

    Private Sub cmbSortRooms_SelectedIndexChanged(sender As Object, e As EventArgs)
        ApplyRoomFilters()
    End Sub
    Private Sub LoadAvailableRooms()
        cmbAvailableRooms.Items.Clear()
        cmbAvailableRooms.DisplayMember = "Text"
        cmbAvailableRooms.ValueMember = "Value"

        Dim query As String =
            "SELECT r.room_number, r.floor_number, " &
            "(SELECT COUNT(*) FROM tenants t WHERE t.room_number = r.room_number) AS occupied " &
            "FROM rooms r " &
            "WHERE r.status <> 'Unavailable' " &
            "GROUP BY r.room_number, r.floor_number " &
            "HAVING (SELECT COUNT(*) FROM tenants t WHERE t.room_number = r.room_number) < 4"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim roomNum As Integer = CInt(reader("room_number"))
                        Dim floorNum As Integer = CInt(reader("floor_number"))
                        Dim occupied As Integer = CInt(reader("occupied"))

                        cmbAvailableRooms.Items.Add(New With {
                            .Text = "Room " & roomNum,
                            .Value = roomNum,
                            .Floor = floorNum,
                            .Occupied = occupied
                        })
                    End While
                End Using
            End Using
        End Using

        lblOccupancy.Text = ""
        lblFloor.Text = ""

    End Sub
    Private Sub LoadCurrentRates()

        Dim query As String = "SELECT electricity_rate, water_rate, rental_rate FROM settings"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvCurrentRates.DataSource = dt
            End Using
        End Using

    End Sub
    Private Sub LoadDashboard()

        Using con As New SqlConnection(connectionString)
            con.Open()

            Using cmd As New SqlCommand("SELECT COUNT(*) FROM tenants", con)
                lblTotalTenants.Text = cmd.ExecuteScalar().ToString()
            End Using

            Using cmd As New SqlCommand("SELECT COUNT(*) FROM rooms WHERE status = 'Under Maintenance'", con)
                lblRoomsMaintenance.Text = cmd.ExecuteScalar().ToString()
            End Using

            Using cmd As New SqlCommand("SELECT COUNT(*) FROM rooms WHERE status = 'Unavailable'", con)
                lblRoomsUnavailable.Text = cmd.ExecuteScalar().ToString()
            End Using

            Using cmd As New SqlCommand("SELECT COUNT(*) FROM rooms", con)
                lblTotalRooms.Text = cmd.ExecuteScalar().ToString()
            End Using

            Using cmd As New SqlCommand("SELECT COUNT(*) FROM payments WHERE status = 'Pending'", con)
                lblPendingPayments.Text = cmd.ExecuteScalar().ToString()
            End Using

            Using cmd As New SqlCommand("SELECT COUNT(*) FROM payments_paid", con)
                lblPaidPayments.Text = cmd.ExecuteScalar().ToString()
            End Using

            Using cmd As New SqlCommand("SELECT TOP 1 electricity_rate, water_rate, rental_rate FROM settings", con)
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        lblCurrentRental.Text = "₱" & CDec(reader("rental_rate")).ToString("N2")
                        lblCurrentElectricity.Text = "₱" & CDec(reader("electricity_rate")).ToString("N2") & "/kWh"
                        lblCurrentWater.Text = "₱" & CDec(reader("water_rate")).ToString("N2") & "/m³"
                    End If
                End Using
            End Using

        End Using

    End Sub
    Private Sub LoadMaintenanceList()
        Dim query As String = "SELECT * FROM maintenance ORDER BY date_reported DESC"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                maintenanceTable = New DataTable()
                da.Fill(maintenanceTable)
                dgvMaintenance.DataSource = maintenanceTable
            End Using
        End Using
    End Sub
    Private Sub LoadMaintenanceRoomOptions()
        cmbMaintenanceRoom.Items.Clear()
        cmbMaintenanceRoom.DisplayMember = "Text"
        cmbMaintenanceRoom.ValueMember = "Value"

        Dim query As String = "SELECT room_number, status FROM rooms WHERE status <> 'Unavailable' ORDER BY room_number"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim roomNum As Integer = CInt(reader("room_number"))
                        Dim status As String = reader("status").ToString()

                        cmbMaintenanceRoom.Items.Add(New With {
                            .Text = "Room " & roomNum & " (" & status & ")",
                            .Value = roomNum
                        })
                    End While
                End Using
            End Using
        End Using
    End Sub
    Private Sub LoadPaymentRoomOptions()
        cmbPaymentRoom.Items.Clear()
        cmbPaymentRoom.DisplayMember = "Text"
        cmbPaymentRoom.ValueMember = "Value"

        Dim query As String = "SELECT room_number FROM rooms WHERE status <> 'Unavailable' ORDER BY room_number"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim roomNum As Integer = CInt(reader("room_number"))

                        cmbPaymentRoom.Items.Add(New With {
                            .Text = "Room " & roomNum,
                            .Value = roomNum
                        })
                    End While
                End Using
            End Using
        End Using
    End Sub
    Private Sub LoadRoomAssignments()
        Dim query As String =
            "SELECT r.room_number, r.floor_number, " &
            "t.tenant_id, t.lname, t.fname " &
            "FROM rooms r " &
            "LEFT JOIN tenants t ON t.room_number = r.room_number " &
            "ORDER BY r.room_number"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                roomAssignmentsTable = New DataTable()
                da.Fill(roomAssignmentsTable)
                dgvRoomAssignments.DataSource = roomAssignmentsTable
            End Using
        End Using
    End Sub
    Private Sub LoadRoomsManage()

        Dim query As String = "SELECT room_number, floor_number, status FROM rooms ORDER BY room_number"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                roomsTable = New DataTable()
                da.Fill(roomsTable)
                dgvRoomsManage.DataSource = roomsTable
            End Using
        End Using

    End Sub
    Private Sub LoadRoomStatus()
        Dim query As String = "SELECT room_number, floor_number, status FROM rooms ORDER BY room_number"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                Dim dt As New DataTable()
                da.Fill(dt)
                dgvRoomStat.DataSource = dt
            End Using
        End Using
    End Sub
    Private Sub LoadSettings()

        Dim query As String = "SELECT TOP 1 electricity_rate, water_rate, rental_rate FROM settings"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        txtElectricityRate.Text = reader("electricity_rate").ToString()
                        txtWaterRate.Text = reader("water_rate").ToString()
                        txtRentalRate.Text = reader("rental_rate").ToString()
                    End If
                End Using
            End Using
        End Using

    End Sub
    Public Sub LoadTenants()

        Dim query As String = "SELECT * FROM tenants"

        Using con As New SqlConnection(connectionString)
            Using adapter As New SqlDataAdapter(query, con)
                tenantsTable = New DataTable()
                adapter.Fill(tenantsTable)
                dgvTenants.DataSource = tenantsTable
            End Using
        End Using

    End Sub
    Private Sub LoadUnassignedTenants()
        cmbUnassignedTenants.Items.Clear()
        cmbUnassignedTenants.DisplayMember = "Text"
        cmbUnassignedTenants.ValueMember = "Value"

        Dim query As String =
            "SELECT tenant_id, lname, fname FROM tenants WHERE room_number IS NULL"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim tenantId As Integer = CInt(reader("tenant_id"))
                        Dim displayText As String =
                            reader("lname").ToString() & ", " & reader("fname").ToString() &
                            " (ID: " & tenantId & ")"

                        cmbUnassignedTenants.Items.Add(New With {
                            .Text = displayText,
                            .Value = tenantId
                        })
                    End While
                End Using
            End Using
        End Using
    End Sub
    Private Sub ShowRoomInfo(roomNumber As Integer)

        Dim floorNum As Integer = 0
        Dim occupied As Integer = 0

        Dim roomQuery As String =
            "SELECT floor_number, " &
            "(SELECT COUNT(*) FROM tenants t WHERE t.room_number = r.room_number) AS occupied " &
            "FROM rooms r WHERE r.room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(roomQuery, con)
                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        floorNum = CInt(reader("floor_number"))
                        occupied = CInt(reader("occupied"))
                    End If
                End Using
            End Using
        End Using

        lblFloor.Text = "Floor: " & floorNum
        lblOccupancy.Text = "Occupancy: " & occupied & "/4"

        Dim tenantNames As New List(Of String)

        Dim tenantQuery As String =
            "SELECT lname, fname FROM tenants WHERE room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(tenantQuery, con)
                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim fullName As String =
                            reader("lname").ToString() & ", " & reader("fname").ToString()
                        tenantNames.Add(fullName)
                    End While
                End Using
            End Using
        End Using

        Dim tenantLabels As Label() = {lblTenant1, lblTenant2, lblTenant3, lblTenant4}

        For i As Integer = 0 To 3
            If i < tenantNames.Count Then
                tenantLabels(i).Text = tenantNames(i)
            Else
                tenantLabels(i).Text = "Empty"
            End If
        Next

    End Sub
    Private Sub txtSearchMaintenance_TextChanged(sender As Object, e As EventArgs)
        ApplyMaintenanceFilters()
    End Sub
    Private Sub txtSearchRoom_TextChanged(sender As Object, e As EventArgs)
        ApplyRoomFilters()
    End Sub
    Private Sub txtSearchRoomAssign_TextChanged(sender As Object, e As EventArgs)
        ApplyRoomAssignmentFilters()
    End Sub
    Private Function IsPhilippineTimeValid() As Boolean

        Dim localOffset As TimeSpan = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)
        Dim philippinesOffset As TimeSpan = New TimeSpan(8, 0, 0)

        If localOffset <> philippinesOffset Then
            MessageBox.Show(
                "Your system's timezone is not set to Philippine Time (UTC+8)." & vbCrLf & "Please correct it in your Windows Date & Time settings before using this application.",
                "Incorrect Timezone",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
            Return False
        End If

        Dim currentYear As Integer = DateTime.Now.Year

        If currentYear < 2024 OrElse currentYear > 2030 Then
            MessageBox.Show(
                "Your system date appears incorrect (" & DateTime.Now.ToString("MMMM dd, yyyy") & ")." & vbCrLf & "Please correct your system date and time before using this application.",
                "Incorrect System Date",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )
            Return False
        End If

        Return True

    End Function
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles btnIcon.Click

        If Panel1.Width = 43 Then
            Panel1.Width = 175
        Else
            Panel1.Width = 43
        End If
    End Sub
    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click

        If selectedTenantId IsNot Nothing Then
            MessageBox.Show("You're currently editing a tenant. Click 'Update' to save changes, or 'Clear' to add a new tenant instead.")
            Exit Sub
        End If

        Dim fields As New List(Of KeyValuePair(Of Control, String))
        fields.Add(New KeyValuePair(Of Control, String)(txtLname, "last name"))
        fields.Add(New KeyValuePair(Of Control, String)(txtFname, "first name"))
        fields.Add(New KeyValuePair(Of Control, String)(txtAge, "age"))
        fields.Add(New KeyValuePair(Of Control, String)(cmbGender, "gender"))
        fields.Add(New KeyValuePair(Of Control, String)(txtAddress, "address"))
        fields.Add(New KeyValuePair(Of Control, String)(txtContactNumber, "contact number"))
        fields.Add(New KeyValuePair(Of Control, String)(txtEmergencyPerson, "emergency person's name"))
        fields.Add(New KeyValuePair(Of Control, String)(txtEmergencyContact, "emergency contact number"))

        Dim emptyCount As Integer = 0
        Dim firstEmptyField As KeyValuePair(Of Control, String) = Nothing

        For Each field As KeyValuePair(Of Control, String) In fields
            If field.Key.Text = "" Then
                emptyCount += 1
                If emptyCount = 1 Then
                    firstEmptyField = field
                End If
            End If
        Next

        If emptyCount > 1 Then
            MessageBox.Show("Please fill all fields.")
            txtLname.Focus()
            Exit Sub
        ElseIf emptyCount = 1 Then
            MessageBox.Show("Please enter the " & firstEmptyField.Value & ".")
            firstEmptyField.Key.Focus()
            Exit Sub
        End If

        Dim age As Integer
        If Not Integer.TryParse(txtAge.Text, age) Then
            MessageBox.Show("Please enter a valid number for age.")
            txtAge.Focus()
            Exit Sub
        End If

        Dim contactNumber As Long
        If Not Long.TryParse(txtContactNumber.Text, contactNumber) Then
            MessageBox.Show("Please enter a valid contact number.")
            txtContactNumber.Focus()
            Exit Sub
        End If

        Dim emergencyContact As Long
        If Not Long.TryParse(txtEmergencyContact.Text, emergencyContact) Then
            MessageBox.Show("Please enter a valid emergency contact number.")
            txtEmergencyContact.Focus()
            Exit Sub
        End If

        Dim checkQuery As String =
            "SELECT COUNT(*) FROM tenants WHERE lname = @lname AND fname = @fname"

        Using con As New SqlConnection(connectionString)
            Using checkCmd As New SqlCommand(checkQuery, con)
                checkCmd.Parameters.AddWithValue("@lname", txtLname.Text)
                checkCmd.Parameters.AddWithValue("@fname", txtFname.Text)
                con.Open()
                Dim existingCount As Integer = CInt(checkCmd.ExecuteScalar())

                If existingCount > 0 Then
                    MessageBox.Show("A tenant with this first and last name already exists.")
                    Exit Sub
                End If
            End Using
        End Using

        Dim confirmResult As DialogResult = MessageBox.Show(
            "Are you sure you want to register " & txtFname.Text & " " & txtLname.Text & " as a new tenant?",
            "Confirm Registration",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If confirmResult = DialogResult.No Then
            Exit Sub
        End If

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim query As String =
            "INSERT INTO tenants " &
            "(lname, fname, age, gender, address, contact_number, " &
            "emergency_person, emergency_contact) " &
            "VALUES " &
            "(@lname, @fname, @age, @gender, @address, @contact_number, " &
            "@emergency_person, @emergency_contact)"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)

                cmd.Parameters.AddWithValue("@lname", txtLname.Text)
                cmd.Parameters.AddWithValue("@fname", txtFname.Text)
                cmd.Parameters.AddWithValue("@age", age)
                cmd.Parameters.AddWithValue("@gender", cmbGender.Text)
                cmd.Parameters.AddWithValue("@address", txtAddress.Text)
                cmd.Parameters.AddWithValue("@contact_number", contactNumber)
                cmd.Parameters.AddWithValue("@emergency_person", txtEmergencyPerson.Text)
                cmd.Parameters.AddWithValue("@emergency_contact", emergencyContact)

                con.Open()
                cmd.ExecuteNonQuery()

            End Using
        End Using

        MessageBox.Show("Tenant successfully registered!")
        txtFname.Text = ""
        txtLname.Text = ""
        txtAge.Text = ""
        txtContactNumber.Text = ""
        txtEmergencyPerson.Text = ""
        txtEmergencyContact.Text = ""
        txtAddress.Text = ""
        cmbGender.Text = ""
        selectedTenantId = Nothing

        LoadTenants()

    End Sub
    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click

        If dgvTenants.CurrentRow Is Nothing Then
            MessageBox.Show("Please select a tenant to archive.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        ' --- ROOM NUMBER CHECK ---
        Dim roomVal As Object = dgvTenants.CurrentRow.Cells("room_number").Value

        If roomVal IsNot Nothing AndAlso Not IsDBNull(roomVal) AndAlso Not String.IsNullOrWhiteSpace(roomVal.ToString()) Then
            MessageBox.Show("This tenant cannot be archived because they are currently assigned to Room " & roomVal.ToString() & ". Please unassign or remove them from the room first.",
                            "Archive Denied",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Exit Sub
        End If
        ' -------------------------

        Dim tenantID As Integer = CInt(dgvTenants.CurrentRow.Cells("tenant_id").Value)

        Dim result As DialogResult = MessageBox.Show(
            "Are you sure you want to archive this tenant?",
            "Confirm Archive",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        )

        If result = DialogResult.No Then
            Exit Sub
        End If

        ' Require typing CONFIRM before actually archiving
        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Using con As New SqlConnection(connectionString)
            con.Open()

            Dim transaction As SqlTransaction = con.BeginTransaction()

            Try

                Dim insertQuery As String =
                    "INSERT INTO tenants_archive " &
                    "(tenant_id, room_number, lname, fname, age, gender, address, " &
                    "contact_number, emergency_person, emergency_contact) " &
                    "SELECT tenant_id, room_number, lname, fname, age, gender, address, " &
                    "contact_number, emergency_person, emergency_contact " &
                    "FROM tenants WHERE tenant_id = @id"

                Using insertCmd As New SqlCommand(insertQuery, con, transaction)
                    insertCmd.Parameters.AddWithValue("@id", tenantID)
                    insertCmd.ExecuteNonQuery()
                End Using

                Dim deleteQuery As String = "DELETE FROM tenants WHERE tenant_id = @id"

                Using deleteCmd As New SqlCommand(deleteQuery, con, transaction)
                    deleteCmd.Parameters.AddWithValue("@id", tenantID)
                    deleteCmd.ExecuteNonQuery()
                End Using

                transaction.Commit()

            Catch ex As Exception
                transaction.Rollback()
                MessageBox.Show("Error archiving tenant: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End Try

        End Using

        MessageBox.Show("Tenant archived successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

        txtFname.Text = ""
        txtLname.Text = ""
        txtAge.Text = ""
        txtContactNumber.Text = ""
        txtEmergencyPerson.Text = ""
        txtEmergencyContact.Text = ""
        txtAddress.Text = ""
        cmbGender.Text = ""
        selectedTenantId = Nothing

        LoadTenants()

    End Sub
    Private Sub btnManage_Click(sender As System.Object, e As System.EventArgs) Handles btnManage.Click
        btnManage.BackColor = Color.FromArgb(240, 236, 225)

        btnDashboard.BackColor = Color.FromArgb(221, 220, 226)
        btnMaintenance.BackColor = Color.FromArgb(221, 220, 226)
        btnPayment.BackColor = Color.FromArgb(221, 220, 226)
        btnSettings.BackColor = Color.FromArgb(221, 220, 226)

        PHeaderManagement.BringToFront()
        PTenants.BringToFront()


        lblSectionName.Text = "M A N A G E M E N T"

        cmbGender.Items.Clear()
        cmbGender.Items.Add("Male")
        cmbGender.Items.Add("Female")

        cmbSortBy.Items.Clear()
        cmbSortBy.Items.Add("ID: Low to High")
        cmbSortBy.Items.Add("ID: High to Low")
        cmbSortBy.Items.Add("Name: A-Z")
        cmbSortBy.Items.Add("Name: Z-A")
        cmbSortBy.Items.Add("Room: Low to High")
        cmbSortBy.Items.Add("Room: High to Low")
        cmbSortBy.SelectedIndex = 0

        cmbGenderFilter.Items.Clear()
        cmbGenderFilter.Items.Add("All")
        cmbGenderFilter.Items.Add("Male")
        cmbGenderFilter.Items.Add("Female")
        cmbGenderFilter.SelectedIndex = 0

        LoadTenants()

        PDashboard.Hide()
        PTenants.Show()
        PRooms.Hide()
        PRoomStat.Hide()
        PPayment.Hide()
        PSettings.Hide()
        PHeaderManagement.Show()
        PHeaderMaintenance.Hide()
        PHeaderPayments.Hide()
        PHeaderSetting.Hide()
        PHeaderDashboard.Hide()

    End Sub
    Private Sub btnClear_Click(sender As System.Object, e As System.EventArgs) Handles btnClear.Click
        Dim confirmClear As DialogResult = MessageBox.Show(
             "Are you sure you want to clear all Fields?",
             "Clear",
             MessageBoxButtons.YesNo,
             MessageBoxIcon.Question
         )

        If confirmClear = DialogResult.No Then
            Exit Sub
        End If

        txtFname.Text = ""
        txtLname.Text = ""
        txtAge.Text = ""
        txtContactNumber.Text = ""
        txtEmergencyPerson.Text = ""
        txtEmergencyContact.Text = ""
        txtAddress.Text = ""
        cmbGender.Text = ""
        selectedTenantId = Nothing
    End Sub
   Private Sub btnAssign_Click(sender As System.Object, e As System.EventArgs) Handles btnAssign.Click
        If cmbUnassignedTenants.SelectedItem Is Nothing Then
            MessageBox.Show("Please select a tenant.")
            Exit Sub
        End If

        If cmbAvailableRooms.SelectedItem Is Nothing Then
            MessageBox.Show("Please select a room.")
            Exit Sub
        End If

        Dim tenantId As Integer = CInt(cmbUnassignedTenants.SelectedItem.Value)
        Dim roomNumber As Integer = CInt(cmbAvailableRooms.SelectedItem.Value)

        Dim newTenantGender As String = ""

        Dim genderQuery As String = "SELECT gender FROM tenants WHERE tenant_id = @tenant"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(genderQuery, con)
                cmd.Parameters.AddWithValue("@tenant", tenantId)
                con.Open()
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then
                    newTenantGender = result.ToString()
                End If
            End Using
        End Using

        Dim existingGender As String = ""

        Dim existingGenderQuery As String = "SELECT TOP 1 gender FROM tenants WHERE room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(existingGenderQuery, con)
                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then
                    existingGender = result.ToString()
                End If
            End Using
        End Using

        If existingGender <> "" AndAlso existingGender <> newTenantGender Then
            MessageBox.Show("This room already has a " & existingGender & " tenant. Only " & existingGender & " tenants can be assigned to this room.")
            cmbUnassignedTenants.SelectedIndex = -1
            cmbUnassignedTenants.Text = ""
            Exit Sub
        End If

        Dim pendingDateCreated As DateTime
        Dim pendingDueDate As DateTime
        Dim hasPendingPayment As Boolean = False

        Dim pendingQuery As String =
            "SELECT TOP 1 date_created, due_date FROM payments WHERE room_number = @room AND status = 'Pending' ORDER BY date_created DESC"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(pendingQuery, con)
                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        hasPendingPayment = True
                        pendingDateCreated = CDate(reader("date_created"))
                        pendingDueDate = CDate(reader("due_date"))
                    End If
                End Using
            End Using
        End Using

        If hasPendingPayment Then
            Dim pendingWarning As DialogResult = MessageBox.Show(
                "Room " & roomNumber & " has a pending payment:" & vbCrLf & vbCrLf &
                "Start Date: " & pendingDateCreated.ToString("MMMM dd, yyyy") & vbCrLf &
                "Due Date: " & pendingDueDate.ToString("MMMM dd, yyyy") & vbCrLf & vbCrLf &
                "Do you wish to continue assigning a tenant to this room?",
                "Pending Payment Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            )

            If pendingWarning = DialogResult.No Then
                Exit Sub
            End If
        Else
            Dim confirmResult As DialogResult = MessageBox.Show(
                "Assign " & cmbUnassignedTenants.Text & " to Room " & roomNumber & "?",
                "Confirm Assignment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

            If confirmResult = DialogResult.No Then
                Exit Sub
            End If
        End If

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim query As String =
            "UPDATE tenants SET room_number = @room WHERE tenant_id = @tenant"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@room", roomNumber)
                cmd.Parameters.AddWithValue("@tenant", tenantId)
                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        lblOccupancy.Text = ""
        lblFloor.Text = ""
        MessageBox.Show("Tenant assigned to room successfully!")

        LoadUnassignedTenants()
        LoadAvailableRooms()
        LoadRoomAssignments()

        ShowRoomInfo(roomNumber)

        cmbUnassignedTenants.SelectedIndex = -1
        cmbUnassignedTenants.Text = ""
        cmbAvailableRooms.SelectedIndex = -1
        cmbAvailableRooms.Text = ""
        dgvRoomAssignments.ClearSelection()
        dgvRoomAssignments.CurrentCell = Nothing

        lblOccupancy.Text = ""
        lblFloor.Text = ""

    End Sub
    Private Sub btnUnnasign_Click(sender As System.Object, e As System.EventArgs) Handles btnUnnasign.Click
        If dgvRoomAssignments.CurrentRow Is Nothing Then
            MessageBox.Show("Please select a tenant row to unassign.")
            Exit Sub
        End If

        Dim tenantIdCell As Object = dgvRoomAssignments.CurrentRow.Cells("tenant_id").Value

        If tenantIdCell Is DBNull.Value OrElse tenantIdCell Is Nothing Then
            MessageBox.Show("This room slot is already empty.")
            Exit Sub
        End If

        Dim tenantId As Integer = CInt(tenantIdCell)
        Dim roomNumber As Integer = CInt(dgvRoomAssignments.CurrentRow.Cells("room_number").Value)

        Dim lname As String = dgvRoomAssignments.CurrentRow.Cells("lname").Value.ToString()
        Dim fname As String = dgvRoomAssignments.CurrentRow.Cells("fname").Value.ToString()

        Dim pendingDateCreated As DateTime
        Dim pendingDueDate As DateTime
        Dim hasPendingPayment As Boolean = False

        Dim pendingQuery As String =
            "SELECT TOP 1 date_created, due_date FROM payments WHERE room_number = @room AND status = 'Pending' ORDER BY date_created DESC"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(pendingQuery, con)
                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        hasPendingPayment = True
                        pendingDateCreated = CDate(reader("date_created"))
                        pendingDueDate = CDate(reader("due_date"))
                    End If
                End Using
            End Using
        End Using

        If hasPendingPayment Then
            Dim pendingWarning As DialogResult = MessageBox.Show(
                "Room " & roomNumber & " has a pending payment:" & vbCrLf & vbCrLf &
                "Start Date: " & pendingDateCreated.ToString("MMMM dd, yyyy") & vbCrLf &
                "Due Date: " & pendingDueDate.ToString("MMMM dd, yyyy") & vbCrLf & vbCrLf &
                "Do you wish to continue unassigning this tenant?",
                "Pending Payment Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            )

            If pendingWarning = DialogResult.No Then
                Exit Sub
            End If
        Else
            Dim confirmResult As DialogResult = MessageBox.Show(
                "Unassign " & fname & " " & lname & " from Room " & roomNumber & "?",
                "Confirm Unassign",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

            If confirmResult = DialogResult.No Then
                Exit Sub
            End If
        End If

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim query As String = "UPDATE tenants SET room_number = NULL WHERE tenant_id = @tenant"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@tenant", tenantId)
                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        MessageBox.Show("Tenant unassigned from room.")

        
        LoadUnassignedTenants()
        LoadAvailableRooms()
        LoadRoomAssignments()

        ShowRoomInfo(roomNumber)

        cmbAvailableRooms.SelectedIndex = -1
        cmbAvailableRooms.Text = ""
        dgvRoomAssignments.ClearSelection()
        dgvRoomAssignments.CurrentCell = Nothing

        lblOccupancy.Text = ""
        lblFloor.Text = ""

    End Sub
    Private Sub cmbAvailableRooms_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAvailableRooms.SelectedIndexChanged

        If cmbAvailableRooms.SelectedItem Is Nothing Then
            lblOccupancy.Text = ""
            lblFloor.Text = ""
            lblTenant1.Text = ""
            lblTenant2.Text = ""
            lblTenant3.Text = ""
            lblTenant4.Text = ""
            Exit Sub
        End If

        Dim selected = cmbAvailableRooms.SelectedItem
        Dim roomNumber As Integer = CInt(selected.Value)
        ShowRoomInfo(roomNumber)

        lblOccupancy.Text = "Occupancy: " & selected.Occupied & "/4"
        lblFloor.Text = "Floor: " & selected.Floor

        Dim tenantNames As New List(Of String)

        Dim query As String =
            "SELECT lname, fname FROM tenants WHERE room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim fullName As String =
                            reader("lname").ToString() & ", " & reader("fname").ToString()
                        tenantNames.Add(fullName)
                    End While
                End Using
            End Using
        End Using

        Dim tenantLabels As Label() = {lblTenant1, lblTenant2, lblTenant3, lblTenant4}

        For i As Integer = 0 To 3
            If i < tenantNames.Count Then
                tenantLabels(i).Text = tenantNames(i)
            Else
                tenantLabels(i).Text = "Empty"
            End If
        Next

    End Sub
    Private Sub dgvTenants_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTenants.CellClick

        If e.RowIndex < 0 Then Exit Sub

        Dim row As DataGridViewRow = dgvTenants.Rows(e.RowIndex)

        If row.Cells("tenant_id").Value Is Nothing OrElse row.Cells("tenant_id").Value Is DBNull.Value Then
            Exit Sub
        End If

        selectedTenantId = CInt(row.Cells("tenant_id").Value)

        txtLname.Text = row.Cells("lname").Value.ToString()
        txtFname.Text = row.Cells("fname").Value.ToString()
        txtAge.Text = row.Cells("age").Value.ToString()
        cmbGender.Text = row.Cells("gender").Value.ToString()
        txtAddress.Text = row.Cells("address").Value.ToString()
        txtContactNumber.Text = row.Cells("contact_number").Value.ToString()
        txtEmergencyPerson.Text = row.Cells("emergency_person").Value.ToString()
        txtEmergencyContact.Text = row.Cells("emergency_contact").Value.ToString()

    End Sub
    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click

        If selectedTenantId Is Nothing Then
            MessageBox.Show("Please select a tenant from the list first.")
            Exit Sub
        End If

        Dim fields As New List(Of KeyValuePair(Of Control, String))
        fields.Add(New KeyValuePair(Of Control, String)(txtLname, "last name"))
        fields.Add(New KeyValuePair(Of Control, String)(txtFname, "first name"))
        fields.Add(New KeyValuePair(Of Control, String)(txtAge, "age"))
        fields.Add(New KeyValuePair(Of Control, String)(cmbGender, "gender"))
        fields.Add(New KeyValuePair(Of Control, String)(txtAddress, "address"))
        fields.Add(New KeyValuePair(Of Control, String)(txtContactNumber, "contact number"))
        fields.Add(New KeyValuePair(Of Control, String)(txtEmergencyPerson, "emergency person's name"))
        fields.Add(New KeyValuePair(Of Control, String)(txtEmergencyContact, "emergency contact number"))

        Dim emptyCount As Integer = 0
        Dim firstEmptyField As KeyValuePair(Of Control, String) = Nothing

        For Each field As KeyValuePair(Of Control, String) In fields
            If field.Key.Text = "" Then
                emptyCount += 1
                If emptyCount = 1 Then
                    firstEmptyField = field
                End If
            End If
        Next

        If emptyCount > 1 Then
            MessageBox.Show("Please fill all fields.")
            txtLname.Focus()
            Exit Sub
        ElseIf emptyCount = 1 Then
            MessageBox.Show("Please enter the " & firstEmptyField.Value & ".")
            firstEmptyField.Key.Focus()
            Exit Sub
        End If

        Dim age As Integer
        If Not Integer.TryParse(txtAge.Text, age) Then
            MessageBox.Show("Please enter a valid number for age.")
            txtAge.Focus()
            Exit Sub
        End If

        Dim contactNumber As Long
        If Not Long.TryParse(txtContactNumber.Text, contactNumber) Then
            MessageBox.Show("Please enter a valid contact number.")
            txtContactNumber.Focus()
            Exit Sub
        End If

        Dim emergencyContact As Long
        If Not Long.TryParse(txtEmergencyContact.Text, emergencyContact) Then
            MessageBox.Show("Please enter a valid emergency contact number.")
            txtEmergencyContact.Focus()
            Exit Sub
        End If

        Dim oldLname As String = ""
        Dim oldFname As String = ""
        Dim oldAge As Integer = 0
        Dim oldGender As String = ""
        Dim oldAddress As String = ""
        Dim oldContactNumber As String = ""
        Dim oldEmergencyPerson As String = ""
        Dim oldEmergencyContact As String = ""

        Dim getOldQuery As String =
            "SELECT lname, fname, age, gender, address, contact_number, emergency_person, emergency_contact " &
            "FROM tenants WHERE tenant_id = @id"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(getOldQuery, con)
                cmd.Parameters.AddWithValue("@id", selectedTenantId)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        oldLname = reader("lname").ToString()
                        oldFname = reader("fname").ToString()
                        oldAge = CInt(reader("age"))
                        oldGender = reader("gender").ToString()
                        oldAddress = reader("address").ToString()
                        oldContactNumber = reader("contact_number").ToString()
                        oldEmergencyPerson = reader("emergency_person").ToString()
                        oldEmergencyContact = reader("emergency_contact").ToString()
                    End If
                End Using
            End Using
        End Using

        Dim changedFields As New List(Of String)

        If oldLname <> txtLname.Text Then changedFields.Add("Last Name")
        If oldFname <> txtFname.Text Then changedFields.Add("First Name")
        If oldAge <> age Then changedFields.Add("Age")
        If oldGender <> cmbGender.Text Then changedFields.Add("Gender")
        If oldAddress <> txtAddress.Text Then changedFields.Add("Address")
        If oldContactNumber <> contactNumber.ToString() Then changedFields.Add("Contact Number")
        If oldEmergencyPerson <> txtEmergencyPerson.Text Then changedFields.Add("Emergency Person")
        If oldEmergencyContact <> emergencyContact.ToString() Then changedFields.Add("Emergency Contact")

        If changedFields.Count = 0 Then
            MessageBox.Show("No changes were made.")
            Exit Sub
        End If

        Dim changedFieldsText As String = String.Join(", ", changedFields.ToArray())

        Dim confirmResult As DialogResult = MessageBox.Show(
            "Are you sure you want to update " & txtFname.Text & " " & txtLname.Text & "'s information?" & vbCrLf & vbCrLf &
            "Fields changed: " & changedFieldsText,
            "Confirm Update",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If confirmResult = DialogResult.No Then
            Exit Sub
        End If

        ' Require typing CONFIRM before actually saving
        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim query As String =
            "UPDATE tenants SET " &
            "lname = @lname, fname = @fname, age = @age, gender = @gender, " &
            "address = @address, contact_number = @contact_number, " &
            "emergency_person = @emergency_person, emergency_contact = @emergency_contact " &
            "WHERE tenant_id = @tenant_id"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)

                cmd.Parameters.AddWithValue("@lname", txtLname.Text)
                cmd.Parameters.AddWithValue("@fname", txtFname.Text)
                cmd.Parameters.AddWithValue("@age", age)
                cmd.Parameters.AddWithValue("@gender", cmbGender.Text)
                cmd.Parameters.AddWithValue("@address", txtAddress.Text)
                cmd.Parameters.AddWithValue("@contact_number", contactNumber)
                cmd.Parameters.AddWithValue("@emergency_person", txtEmergencyPerson.Text)
                cmd.Parameters.AddWithValue("@emergency_contact", emergencyContact)
                cmd.Parameters.AddWithValue("@tenant_id", selectedTenantId)

                con.Open()
                cmd.ExecuteNonQuery()

            End Using
        End Using

        MessageBox.Show("Tenant updated successfully!" & vbCrLf & vbCrLf & "Updated fields: " & changedFieldsText)

        btnClear_Click(sender, e)
        selectedTenantId = Nothing

        LoadTenants()

    End Sub
    Private Sub Button3_Click_1(sender As System.Object, e As System.EventArgs) Handles btnRoomAssignment.Click

        PRooms.BringToFront()

        PTenants.Hide()
        PRooms.Show()


        selectedTenantId = Nothing

        LoadUnassignedTenants()
        LoadAvailableRooms()
        LoadRoomAssignments()

        cmbSortRoomAssign.Items.Clear()
        cmbSortRoomAssign.Items.Add("Room: Low to High")
        cmbSortRoomAssign.Items.Add("Room: High to Low")
        cmbSortRoomAssign.Items.Add("Floor: Low to High")
        cmbSortRoomAssign.Items.Add("Floor: High to Low")
        cmbSortRoomAssign.Items.Add("Tenant Name: A-Z")
        cmbSortRoomAssign.Items.Add("Tenant Name: Z-A")
        cmbSortRoomAssign.SelectedIndex = 0
    End Sub
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles btnTenants.Click
        cmbGender.Items.Clear()
        cmbGender.Items.Add("Male")
        cmbGender.Items.Add("Female")

        cmbSortBy.Items.Clear()
        cmbSortBy.Items.Add("ID: Low to High")
        cmbSortBy.Items.Add("ID: High to Low")
        cmbSortBy.Items.Add("Name: A-Z")
        cmbSortBy.Items.Add("Name: Z-A")
        cmbSortBy.Items.Add("Room: Low to High")
        cmbSortBy.Items.Add("Room: High to Low")
        cmbSortBy.SelectedIndex = 0

        cmbGenderFilter.Items.Clear()
        cmbGenderFilter.Items.Add("All")
        cmbGenderFilter.Items.Add("Male")
        cmbGenderFilter.Items.Add("Female")
        cmbGenderFilter.SelectedIndex = 0

        LoadTenants()

        PRooms.Hide()
        PRoomStat.Hide()
        PTenants.Show()
    End Sub
    Private Sub btnMaintenance_Click(sender As System.Object, e As System.EventArgs) Handles btnMaintenance.Click

        btnMaintenance.BackColor = Color.FromArgb(240, 236, 225)

        btnManage.BackColor = Color.FromArgb(221, 220, 226)
        btnDashboard.BackColor = Color.FromArgb(221, 220, 226)
        btnPayment.BackColor = Color.FromArgb(221, 220, 226)
        btnSettings.BackColor = Color.FromArgb(221, 220, 226)

        PHeaderMaintenance.BringToFront()
        PRoomStat.BringToFront()

        lblSectionName.Text = "M A I N T E N A N C E"

        PDashboard.Hide()
        PTenants.Hide()
        PRooms.Hide()
        PRoomStat.Show()
        PPayment.Hide()
        PSettings.Hide()
        PHeaderManagement.Hide()
        PHeaderMaintenance.Show()
        PHeaderPayments.Hide()
        PHeaderSetting.Hide()
        PHeaderDashboard.Hide()

        cmbSortMaintenance.Items.Clear()
        cmbSortMaintenance.Items.Add("Room: Low to High")
        cmbSortMaintenance.Items.Add("Room: High to Low")
        cmbSortMaintenance.Items.Add("Date Reported: Newest First")
        cmbSortMaintenance.Items.Add("Date Reported: Oldest First")
        cmbSortMaintenance.SelectedIndex = 0

        LoadMaintenanceRoomOptions()
        LoadMaintenanceList()
        LoadRoomStatus()
    End Sub
    Private Sub btnMaintenanceStat_Click(sender As System.Object, e As System.EventArgs) Handles btnMaintenanceStat.Click

        PRoomStat.BringToFront()

        PRoomStat.Show()


        LoadMaintenanceRoomOptions()
        LoadMaintenanceList()
    End Sub
    Private Sub btnReportMaintenance_Click(sender As Object, e As EventArgs) Handles btnReportMaintenance.Click

        If cmbMaintenanceRoom.SelectedItem Is Nothing Then
            MessageBox.Show("Please select a room.")
            Exit Sub
        End If

        If txtMaintenanceReason.Text = "" Then
            MessageBox.Show("Please enter the maintenance reason.")
            txtMaintenanceReason.Focus()
            Exit Sub
        End If

        Dim roomNumber As Integer = CInt(cmbMaintenanceRoom.SelectedItem.Value)

        Dim confirmResult As DialogResult = MessageBox.Show(
            "Report a maintenance issue for Room " & roomNumber & "?",
            "Confirm Maintenance Report",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If confirmResult = DialogResult.No Then
            Exit Sub
        End If

        Using con As New SqlConnection(connectionString)
            con.Open()
            Dim transaction As SqlTransaction = con.BeginTransaction()

            Try
                Dim insertQuery As String =
                    "INSERT INTO maintenance (room_number, maintenance_reason) " &
                    "VALUES (@room, @reason)"

                Using cmd As New SqlCommand(insertQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@room", roomNumber)
                    cmd.Parameters.AddWithValue("@reason", txtMaintenanceReason.Text)
                    cmd.ExecuteNonQuery()
                End Using

                Dim updateQuery As String =
                    "UPDATE rooms SET status = 'Under Maintenance' WHERE room_number = @room"

                Using cmd As New SqlCommand(updateQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@room", roomNumber)
                    cmd.ExecuteNonQuery()
                End Using

                transaction.Commit()

            Catch ex As Exception
                transaction.Rollback()
                MessageBox.Show("Error reporting maintenance: " & ex.Message)
                Exit Sub
            End Try
        End Using

        MessageBox.Show("Maintenance reported successfully!")
        txtMaintenanceReason.Text = ""
        cmbMaintenanceRoom.Text = ""

        LoadMaintenanceList()
        LoadRoomStatus()
        LoadMaintenanceRoomOptions()

    End Sub
    Private Sub btnResolveMaintenance_Click(sender As Object, e As EventArgs) Handles btnResolveMaintenance.Click

        If dgvMaintenance.CurrentRow Is Nothing Then
            MessageBox.Show("Please select a maintenance record to resolve.")
            Exit Sub
        End If

        Dim maintenanceId As Integer = CInt(dgvMaintenance.CurrentRow.Cells("maintenance_id").Value)
        Dim roomNumber As Integer = CInt(dgvMaintenance.CurrentRow.Cells("room_number").Value)

        ' Require typing CONFIRM before actually resolving
        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"

        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Using con As New SqlConnection(connectionString)
            con.Open()
            Dim transaction As SqlTransaction = con.BeginTransaction()

            Try
                Dim insertQuery As String =
                    "INSERT INTO maintenance_logs " &
                    "(maintenance_id, room_number, maintenance_reason, date_reported) " &
                    "SELECT maintenance_id, room_number, maintenance_reason, date_reported " &
                    "FROM maintenance WHERE maintenance_id = @id"

                Using cmd As New SqlCommand(insertQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@id", maintenanceId)
                    cmd.ExecuteNonQuery()
                End Using

                Dim deleteQuery As String =
                    "DELETE FROM maintenance WHERE maintenance_id = @id"

                Using cmd As New SqlCommand(deleteQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@id", maintenanceId)
                    cmd.ExecuteNonQuery()
                End Using

                Dim countQuery As String =
                    "SELECT COUNT(*) FROM maintenance WHERE room_number = @room"

                Dim remainingCount As Integer = 0

                Using cmd As New SqlCommand(countQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@room", roomNumber)
                    remainingCount = CInt(cmd.ExecuteScalar())
                End Using

                If remainingCount = 0 Then

                    Dim updateQuery As String =
                        "UPDATE rooms SET status = 'Available' WHERE room_number = @room"

                    Using cmd As New SqlCommand(updateQuery, con, transaction)
                        cmd.Parameters.AddWithValue("@room", roomNumber)
                        cmd.ExecuteNonQuery()
                    End Using

                End If

                transaction.Commit()

            Catch ex As Exception

                transaction.Rollback()
                MessageBox.Show("Error resolving maintenance: " & ex.Message)
                Exit Sub

            End Try
        End Using

        MessageBox.Show("Maintenance resolved and logged!")

        txtMaintenanceReason.Text = ""
        cmbMaintenanceRoom.Text = ""

        LoadMaintenanceList()
        LoadRoomStatus()
        LoadMaintenanceRoomOptions()

    End Sub
    Private Sub btnPayment_Click(sender As System.Object, e As System.EventArgs) Handles btnPayment.Click

        btnPayment.BackColor = Color.FromArgb(240, 236, 225)


        btnManage.BackColor = Color.FromArgb(221, 220, 226)
        btnDashboard.BackColor = Color.FromArgb(221, 220, 226)
        btnMaintenance.BackColor = Color.FromArgb(221, 220, 226)
        btnSettings.BackColor = Color.FromArgb(221, 220, 226)

        PPayment.BringToFront()
        PHeaderPayments.BringToFront()

        lblSectionName.Text = "P A Y M E N T S"

        LoadPayments()
        LoadPaymentRoomOptions()


        PDashboard.Hide()
        PTenants.Hide()
        PRooms.Hide()
        PRoomStat.Hide()
        PPayment.Show()
        PSettings.Hide()
        PHeaderManagement.Hide()
        PHeaderMaintenance.Hide()
        PHeaderPayments.Show()
        PHeaderSetting.Hide()
        PHeaderDashboard.Hide()


        cmbSortPayments.Items.Clear()
        cmbSortPayments.Items.Add("Payment ID: Low to High")
        cmbSortPayments.Items.Add("Payment ID: High to Low")
        cmbSortPayments.Items.Add("Room: Low to High")
        cmbSortPayments.Items.Add("Room: High to Low")
        cmbSortPayments.Items.Add("Total Amount: Low to High")
        cmbSortPayments.Items.Add("Total Amount: High to Low")
        cmbSortPayments.Items.Add("Due Date: Soonest First")
        cmbSortPayments.Items.Add("Due Date: Latest First")
        cmbSortPayments.SelectedIndex = 0
    End Sub
    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged

        If tenantsTable Is Nothing Then Exit Sub

        Dim searchText As String = txtSearch.Text.Replace("'", "''")

        Dim filter As String =
            "lname LIKE '%" & searchText & "%' OR " &
            "fname LIKE '%" & searchText & "%' OR " &
            "CONVERT(tenant_id, System.String) LIKE '%" & searchText & "%' OR " &
            "CONVERT(room_number, System.String) LIKE '%" & searchText & "%'"

        Try
            tenantsTable.DefaultView.RowFilter = filter
        Catch ex As Exception
        End Try
        ApplyFilters()
    End Sub
    Private Sub cmbSortBy_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbSortBy.SelectedIndexChanged
        ApplyFilters()
    End Sub
    Private Sub cmbGenderFilter_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbGenderFilter.SelectedIndexChanged
        ApplyFilters()
    End Sub
    Private Sub Label12_Click(sender As System.Object, e As System.EventArgs) Handles Label12.Click

    End Sub
    Private Sub dgvRoomAssignments_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvRoomAssignments.CellClick

        If e.RowIndex < 0 Then Exit Sub

        Dim row As DataGridViewRow = dgvRoomAssignments.Rows(e.RowIndex)

        If row.Cells("room_number").Value Is Nothing OrElse row.Cells("room_number").Value Is DBNull.Value Then
            Exit Sub
        End If

        Dim roomNumber As Integer = CInt(row.Cells("room_number").Value)
        ShowRoomInfo(roomNumber)

        Dim found As Boolean = False
        For i As Integer = 0 To cmbAvailableRooms.Items.Count - 1
            If cmbAvailableRooms.Items(i).Value = roomNumber Then
                cmbAvailableRooms.SelectedIndex = i
                found = True
                Exit For
            End If
        Next

        If Not found Then
            cmbAvailableRooms.SelectedIndex = -1
            cmbAvailableRooms.Text = ""
        End If

    End Sub
    Private Sub LoadPayments()

        Dim query As String =
            "SELECT payment_id, room_number, electricity_consumption, water_consumption, " &
            "electricity_amount, water_amount, rental_amount, total_amount, " &
            "due_date, status, date_created, date_paid " &
            "FROM payments ORDER BY date_created DESC"

        Using con As New SqlConnection(connectionString)
            Using da As New SqlDataAdapter(query, con)
                paymentsTable = New DataTable()
                da.Fill(paymentsTable)
                dgvPayments.DataSource = paymentsTable
            End Using
        End Using

        FormatPesoColumn(dgvPayments, "total_amount")

    End Sub
    Private Sub FormatPesoColumn(dgv As DataGridView, columnName As String)
        If dgv.Columns.Contains(columnName) Then
            dgv.Columns(columnName).DefaultCellStyle.Format = "₱#,##0.00"
        End If
    End Sub
    Private Sub btnMarkPaid_Click(sender As Object, e As EventArgs) Handles btnMarkPaid.Click

        If dgvPayments.CurrentRow Is Nothing Then
            MessageBox.Show("Please select a payment to mark as paid.")
            Exit Sub
        End If

        Dim paymentId As Integer = CInt(dgvPayments.CurrentRow.Cells("payment_id").Value)

        Dim result As DialogResult = MessageBox.Show(
            "Confirm this payment has been paid?",
            "Confirm Payment",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If result = DialogResult.No Then
            Exit Sub
        End If

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Using con As New SqlConnection(connectionString)
            con.Open()
            Dim transaction As SqlTransaction = con.BeginTransaction()

            Try
                ' 1. Copy into payments_paid, with date_paid set now
                Dim insertQuery As String =
                    "INSERT INTO payments_paid " &
                    "(payment_id, room_number, electricity_consumption, water_consumption, " &
                    "electricity_rate, water_rate, rental_amount, electricity_amount, " &
                    "water_amount, total_amount, date_created, due_date, date_paid) " &
                    "SELECT payment_id, room_number, electricity_consumption, water_consumption, " &
                    "electricity_rate, water_rate, rental_amount, electricity_amount, " &
                    "water_amount, total_amount, date_created, due_date, GETDATE() " &
                    "FROM payments WHERE payment_id = @id"

                Using cmd As New SqlCommand(insertQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@id", paymentId)
                    cmd.ExecuteNonQuery()
                End Using

                ' 2. Delete from payments
                Dim deleteQuery As String = "DELETE FROM payments WHERE payment_id = @id"

                Using cmd As New SqlCommand(deleteQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@id", paymentId)
                    cmd.ExecuteNonQuery()
                End Using

                transaction.Commit()

            Catch ex As Exception
                transaction.Rollback()
                MessageBox.Show("Error marking payment as paid: " & ex.Message)
                Exit Sub
            End Try
        End Using

        MessageBox.Show("Payment marked as paid and archived!")

        txtElectricityConsumption.Text = ""
        txtWaterConsumption.Text = ""
        cmbPaymentRoom.Text = ""

        LoadPayments()

    End Sub
    Private Sub txtSearchPayment_TextChanged(sender As Object, e As EventArgs) Handles txtSearchPayment.TextChanged
        ApplyPaymentFilters()
    End Sub

    Private Sub cmbSortPayments_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSortPayments.SelectedIndexChanged
        ApplyPaymentFilters()
    End Sub
    Private Sub btnCreatePayment_Click(sender As Object, e As EventArgs) Handles btnCreatePayment.Click

        If cmbPaymentRoom.SelectedItem Is Nothing Then
            MessageBox.Show("Please select a room.")
            Exit Sub
        End If

        Dim electricityConsumption As Decimal
        If Not Decimal.TryParse(txtElectricityConsumption.Text, electricityConsumption) Then
            MessageBox.Show("Please enter valid electricity consumption (kWh).")
            Exit Sub
        End If

        Dim waterConsumption As Decimal
        If Not Decimal.TryParse(txtWaterConsumption.Text, waterConsumption) Then
            MessageBox.Show("Please enter valid water consumption (cubic meter).")
            Exit Sub
        End If

        Dim selectedRoom = cmbPaymentRoom.SelectedItem
        Dim roomNumber As Integer = CInt(selectedRoom.Value)
        Dim tenantCheckQuery As String = "SELECT COUNT(*) FROM tenants WHERE room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(tenantCheckQuery, con)
                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()
                Dim tenantCount As Integer = CInt(cmd.ExecuteScalar())

                If tenantCount = 0 Then
                    MessageBox.Show(
                        "No tenants assigned to this room.",
                        "Cannot Create Payment",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    )
                    Exit Sub
                End If
            End Using
        End Using

        Dim electricityRate As Decimal = 0
        Dim waterRate As Decimal = 0
        Dim rentalAmount As Decimal = 0

        Dim rateQuery As String = "SELECT TOP 1 electricity_rate, water_rate, rental_rate FROM settings"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(rateQuery, con)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        electricityRate = CDec(reader("electricity_rate"))
                        waterRate = CDec(reader("water_rate"))
                        rentalAmount = CDec(reader("rental_rate"))
                    End If
                End Using
            End Using
        End Using

        Dim confirmPayment As DialogResult = MessageBox.Show(
             "Are you sure you want to add payment to Room " & roomNumber & "?",
             "Confirm Payment",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
         )

        If confirmPayment = DialogResult.No Then
            Exit Sub
        End If

        ' Require typing CONFIRM before actually creating the payment
        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim insertQuery As String =
            "INSERT INTO payments " &
            "(room_number, electricity_consumption, water_consumption, " &
            "electricity_rate, water_rate, rental_amount) " &
            "VALUES " &
            "(@room, @econsumption, @wconsumption, @erate, @wrate, @rental)"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(insertQuery, con)
                cmd.Parameters.AddWithValue("@room", roomNumber)
                cmd.Parameters.AddWithValue("@econsumption", electricityConsumption)
                cmd.Parameters.AddWithValue("@wconsumption", waterConsumption)
                cmd.Parameters.AddWithValue("@erate", electricityRate)
                cmd.Parameters.AddWithValue("@wrate", waterRate)
                cmd.Parameters.AddWithValue("@rental", rentalAmount)
                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        MessageBox.Show("Payment created successfully!")

        txtElectricityConsumption.Text = ""
        txtWaterConsumption.Text = ""
        cmbPaymentRoom.Text = ""

        LoadPayments()

    End Sub
    Private Sub btnSettings_Click(sender As System.Object, e As System.EventArgs) Handles btnSettings.Click

        Dim verifyForm As New frmSettingsVerification()
        Dim result As DialogResult = verifyForm.ShowDialog()

        If result <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim logQuery As String = "INSERT INTO settings_access_logs DEFAULT VALUES"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(logQuery, con)
                con.Open()
                cmd.ExecuteNonQuery()
            End Using
        End Using

        btnSettings.BackColor = Color.FromArgb(240, 236, 225)

        btnManage.BackColor = Color.FromArgb(221, 220, 226)
        btnDashboard.BackColor = Color.FromArgb(221, 220, 226)
        btnMaintenance.BackColor = Color.FromArgb(221, 220, 226)
        btnPayment.BackColor = Color.FromArgb(221, 220, 226)

        PHeaderSetting.BringToFront()
        PSettings.BringToFront()

        lblSectionName.Text = "S E T T I N G S"

        PDashboard.Hide()
        PTenants.Hide()
        PRooms.Hide()
        PRoomStat.Hide()
        PPayment.Hide()
        PSettings.Show()
        PHeaderManagement.Hide()
        PHeaderMaintenance.Hide()
        PHeaderPayments.Hide()
        PHeaderSetting.Show()
        PHeaderDashboard.Hide()


        cmbSortRooms.Items.Clear()
        cmbSortRooms.Items.Add("Room Number: Low to High")
        cmbSortRooms.Items.Add("Room Number: High to Low")
        cmbSortRooms.Items.Add("Floor: Low to High")
        cmbSortRooms.Items.Add("Floor: High to Low")
        cmbSortRooms.Items.Add("Status: A-Z")
        cmbSortRooms.SelectedIndex = 0


        LoadCurrentRates()
        LoadRoomsManage()

    End Sub
    Private Sub btnSaveElectricity_Click(sender As Object, e As EventArgs) Handles btnSaveElectricity.Click

        Dim electricityRate As Decimal
        If Not Decimal.TryParse(txtElectricityRate.Text, electricityRate) Then
            MessageBox.Show("Please enter a valid electricity rate.")
            Exit Sub
        End If

        ' Get current value first, to show in the confirmation message
        Dim currentValue As Decimal = 0
        Dim getCurrentQuery As String = "SELECT TOP 1 electricity_rate FROM settings"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(getCurrentQuery, con)
                con.Open()
                currentValue = CDec(cmd.ExecuteScalar())
            End Using
        End Using

        Dim confirmResult As DialogResult = MessageBox.Show(
            "Change electricity rate from ₱" & currentValue.ToString("N2") & " to ₱" & electricityRate.ToString("N2") & "?",
            "Confirm Rate Change",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If confirmResult = DialogResult.No Then
            Exit Sub
        End If

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Using con As New SqlConnection(connectionString)
            con.Open()
            Dim transaction As SqlTransaction = con.BeginTransaction()

            Try
                Dim oldValue As Decimal = 0
                Dim getOldQuery As String = "SELECT TOP 1 electricity_rate FROM settings"

                Using cmd As New SqlCommand(getOldQuery, con, transaction)
                    oldValue = CDec(cmd.ExecuteScalar())
                End Using

                Dim updateQuery As String = "UPDATE settings SET electricity_rate = @erate"

                Using cmd As New SqlCommand(updateQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@erate", electricityRate)
                    cmd.ExecuteNonQuery()
                End Using

                Dim logQuery As String =
                    "INSERT INTO rate_logs (rate_type, old_value, new_value) " &
                    "VALUES ('Electricity', @old, @new)"

                Using cmd As New SqlCommand(logQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@old", oldValue)
                    cmd.Parameters.AddWithValue("@new", electricityRate)
                    cmd.ExecuteNonQuery()
                End Using

                transaction.Commit()

            Catch ex As Exception
                transaction.Rollback()
                MessageBox.Show("Error saving rate: " & ex.Message)
                Exit Sub
            End Try
        End Using

        MessageBox.Show("Electricity rate updated successfully!")

        txtElectricityRate.Text = ""

        LoadCurrentRates()

    End Sub
    Private Sub btnSaveWater_Click(sender As Object, e As EventArgs) Handles btnSaveWater.Click

        Dim waterRate As Decimal
        If Not Decimal.TryParse(txtWaterRate.Text, waterRate) Then
            MessageBox.Show("Please enter a valid water rate.")
            Exit Sub
        End If

        Dim currentValue As Decimal = 0
        Dim getCurrentQuery As String = "SELECT TOP 1 water_rate FROM settings"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(getCurrentQuery, con)
                con.Open()
                currentValue = CDec(cmd.ExecuteScalar())
            End Using
        End Using

        Dim confirmResult As DialogResult = MessageBox.Show(
            "Change water rate from ₱" & currentValue.ToString("N2") & " to ₱" & waterRate.ToString("N2") & "?",
            "Confirm Rate Change",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If confirmResult = DialogResult.No Then
            Exit Sub
        End If

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Using con As New SqlConnection(connectionString)
            con.Open()
            Dim transaction As SqlTransaction = con.BeginTransaction()

            Try
                Dim oldValue As Decimal = 0
                Dim getOldQuery As String = "SELECT TOP 1 water_rate FROM settings"

                Using cmd As New SqlCommand(getOldQuery, con, transaction)
                    oldValue = CDec(cmd.ExecuteScalar())
                End Using

                Dim updateQuery As String = "UPDATE settings SET water_rate = @wrate"

                Using cmd As New SqlCommand(updateQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@wrate", waterRate)
                    cmd.ExecuteNonQuery()
                End Using

                Dim logQuery As String =
                    "INSERT INTO rate_logs (rate_type, old_value, new_value) " &
                    "VALUES ('Water', @old, @new)"

                Using cmd As New SqlCommand(logQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@old", oldValue)
                    cmd.Parameters.AddWithValue("@new", waterRate)
                    cmd.ExecuteNonQuery()
                End Using

                transaction.Commit()

            Catch ex As Exception
                transaction.Rollback()
                MessageBox.Show("Error saving rate: " & ex.Message)
                Exit Sub
            End Try
        End Using

        MessageBox.Show("Water rate updated successfully!")

        txtWaterRate.Text = ""

        LoadCurrentRates()

    End Sub
    Private Sub btnSaveRental_Click(sender As Object, e As EventArgs) Handles btnSaveRental.Click

        Dim rentalRate As Decimal
        If Not Decimal.TryParse(txtRentalRate.Text, rentalRate) Then
            MessageBox.Show("Please enter a valid rental rate.")
            Exit Sub
        End If

        ' Get current value first, to show in the confirmation message
        Dim currentValue As Decimal = 0
        Dim getCurrentQuery As String = "SELECT TOP 1 rental_rate FROM settings"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(getCurrentQuery, con)
                con.Open()
                currentValue = CDec(cmd.ExecuteScalar())
            End Using
        End Using

        Dim confirmResult As DialogResult = MessageBox.Show(
            "Change rental rate from ₱" & currentValue.ToString("N2") & " to ₱" & rentalRate.ToString("N2") & "?",
            "Confirm Rate Change",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If confirmResult = DialogResult.No Then
            Exit Sub
        End If

        ' Require typing CONFIRM before actually saving the rate change
        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Using con As New SqlConnection(connectionString)
            con.Open()
            Dim transaction As SqlTransaction = con.BeginTransaction()

            Try
                Dim oldValue As Decimal = 0
                Dim getOldQuery As String = "SELECT TOP 1 rental_rate FROM settings"

                Using cmd As New SqlCommand(getOldQuery, con, transaction)
                    oldValue = CDec(cmd.ExecuteScalar())
                End Using

                Dim updateQuery As String = "UPDATE settings SET rental_rate = @rrate"

                Using cmd As New SqlCommand(updateQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@rrate", rentalRate)
                    cmd.ExecuteNonQuery()
                End Using

                Dim logQuery As String =
                    "INSERT INTO rate_logs (rate_type, old_value, new_value) " &
                    "VALUES ('Rental', @old, @new)"

                Using cmd As New SqlCommand(logQuery, con, transaction)
                    cmd.Parameters.AddWithValue("@old", oldValue)
                    cmd.Parameters.AddWithValue("@new", rentalRate)
                    cmd.ExecuteNonQuery()
                End Using

                transaction.Commit()

            Catch ex As Exception
                transaction.Rollback()
                MessageBox.Show("Error saving rate: " & ex.Message)
                Exit Sub
            End Try
        End Using

        MessageBox.Show("Rental rate updated successfully!")

        txtRentalRate.Text = ""

        LoadCurrentRates()

    End Sub
    Private Sub btnViewRateHistory_Click(sender As System.Object, e As System.EventArgs) Handles btnViewRateHistory.Click
        frmRateHistory.Show()
    End Sub

    Private Sub btnViewPaidPayments_Click(sender As System.Object, e As System.EventArgs) Handles btnViewPaidPayments.Click
        frmPaidPayments.Show()
    End Sub
    Private Sub btnSetUnavailable_Click(sender As Object, e As EventArgs) Handles btnSetUnavailable.Click

        If selectedRoomNumber Is Nothing Then
            MessageBox.Show("Please select a room from the list first.")
            Exit Sub
        End If

        Dim roomNumber As Integer = selectedRoomNumber.Value

        Dim tenantCheckQuery As String =
            "SELECT COUNT(*) FROM tenants WHERE room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(tenantCheckQuery, con)

                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()

                Dim tenantCount As Integer = CInt(cmd.ExecuteScalar())

                If tenantCount > 0 Then
                    MessageBox.Show(
                        "Cannot set Room " & roomNumber &
                        " to Unavailable — it still has " & tenantCount &
                        " tenant(s) assigned. Unassign them first."
                    )
                    Exit Sub
                End If

            End Using
        End Using

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"

        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim query As String =
            "UPDATE rooms SET status = 'Unavailable' WHERE room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)

                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()
                cmd.ExecuteNonQuery()

            End Using
        End Using

        MessageBox.Show("Room set to Unavailable.")

        selectedRoomNumber = Nothing
        lblSelectedRoom.Text = ""
        lblFloorNumber.Text = ""

        LoadRoomsManage()

    End Sub
    Private Sub btnSetAvailable_Click(sender As Object, e As EventArgs) Handles btnSetAvailable.Click

        If selectedRoomNumber Is Nothing Then
            MessageBox.Show("Please select a room from the list first.")
            Exit Sub
        End If

        Dim roomNumber As Integer = selectedRoomNumber.Value

        ' Check current room status
        Dim currentStatus As String = ""

        Dim statusQuery As String =
            "SELECT status FROM rooms WHERE room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(statusQuery, con)

                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()

                Dim result As Object = cmd.ExecuteScalar()

                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    currentStatus = result.ToString()
                End If

            End Using
        End Using

        ' If already Available, show message and stop
        If currentStatus = "Available" Then
            MessageBox.Show(
                "Room " & roomNumber & " is already Available.",
                "Room Status",
                MessageBoxButtons.OK,
                MessageBoxIcon.Question
            )
            Exit Sub
        End If

        ' Require typing CONFIRM before changing status
        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"

        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim query As String =
            "UPDATE rooms SET status = 'Available' WHERE room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(query, con)

                cmd.Parameters.AddWithValue("@room", roomNumber)
                con.Open()
                cmd.ExecuteNonQuery()

            End Using
        End Using

        MessageBox.Show("Room set back to Available.")

        selectedRoomNumber = Nothing
        lblSelectedRoom.Text = ""
        lblFloorNumber.Text = ""

        LoadRoomsManage()

    End Sub
    Private Sub dgvRoomsManage_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvRoomsManage.CellClick

        If e.RowIndex < 0 Then Exit Sub

        Dim row As DataGridViewRow = dgvRoomsManage.Rows(e.RowIndex)

        If row.Cells("room_number").Value Is Nothing OrElse row.Cells("room_number").Value Is DBNull.Value Then
            Exit Sub
        End If

        selectedRoomNumber = CInt(row.Cells("room_number").Value)

        lblSelectedRoom.Text = row.Cells("room_number").Value.ToString()
        lblFloorNumber.Text = row.Cells("floor_number").Value.ToString()

    End Sub
    Public Class InputValidation

        Private Shared Function CountChar(text As String, ch As Char) As Integer
            Dim count As Integer = 0
            For Each c As Char In text
                If c = ch Then count += 1
            Next
            Return count
        End Function
        Public Shared Sub NameKeyPress(sender As Object, e As KeyPressEventArgs)
            Dim tb As TextBox = DirectCast(sender, TextBox)

            If e.KeyChar = ControlChars.Back Then Exit Sub

            Dim isLetter As Boolean = Char.IsLetter(e.KeyChar)
            Dim isSpace As Boolean = (e.KeyChar = " "c)
            Dim isDash As Boolean = (e.KeyChar = "-"c)
            Dim isDot As Boolean = (e.KeyChar = "."c)
            Dim isQuote As Boolean = (e.KeyChar = "'"c)

            If Not (isLetter OrElse isSpace OrElse isDash OrElse isDot OrElse isQuote) Then
                e.Handled = True
                Exit Sub
            End If

            ' Max 50 characters, matching VARCHAR(50)
            If tb.Text.Length >= 50 Then
                e.Handled = True
                Exit Sub
            End If

            If isSpace AndAlso tb.SelectionStart = 0 Then
                e.Handled = True
                Exit Sub
            End If

            If isSpace AndAlso tb.SelectionStart > 0 AndAlso tb.Text(tb.SelectionStart - 1) = " "c Then
                e.Handled = True
                Exit Sub
            End If

            If isDash AndAlso CountChar(tb.Text, "-"c) >= 1 Then
                e.Handled = True
                Exit Sub
            End If

            If isDot AndAlso CountChar(tb.Text, "."c) >= 1 Then
                e.Handled = True
                Exit Sub
            End If

            If isQuote AndAlso CountChar(tb.Text, "'"c) >= 1 Then
                e.Handled = True
                Exit Sub
            End If

            If isLetter Then
                e.KeyChar = Char.ToUpper(e.KeyChar)
            End If

        End Sub
        Public Shared Sub ContactNumberKeyPress(sender As Object, e As KeyPressEventArgs)
            Dim tb As TextBox = DirectCast(sender, TextBox)

            If Not (Char.IsDigit(e.KeyChar) OrElse e.KeyChar = ControlChars.Back) Then
                e.Handled = True
                Exit Sub
            End If

            If e.KeyChar = ControlChars.Back Then Exit Sub

            If tb.SelectionStart = 0 AndAlso e.KeyChar <> "0"c Then
                e.Handled = True
                Exit Sub
            End If

            If tb.SelectionStart = 1 AndAlso e.KeyChar <> "9"c Then
                e.Handled = True
                Exit Sub
            End If

            If tb.Text.Length >= 11 Then
                e.Handled = True
            End If

        End Sub
        Public Shared Sub AgeKeyPress(sender As Object, e As KeyPressEventArgs)
            Dim tb As TextBox = DirectCast(sender, TextBox)

            If Not (Char.IsDigit(e.KeyChar) OrElse e.KeyChar = ControlChars.Back) Then
                e.Handled = True
                Exit Sub
            End If

            If e.KeyChar <> ControlChars.Back AndAlso tb.Text.Length = 0 AndAlso e.KeyChar = "0"c Then
                e.Handled = True
                Exit Sub
            End If

            If e.KeyChar <> ControlChars.Back AndAlso tb.Text.Length >= 2 Then
                e.Handled = True
            End If

        End Sub
        Public Shared Function ValidateAgeRange(tb As TextBox) As Boolean

            If tb.Text = "" Then Return True

            Dim age As Integer
            If Not Integer.TryParse(tb.Text, age) Then
                Return True
            End If

            If age >= 1 AndAlso age <= 17 Then
                MessageBox.Show("Age must be 18 and above. Minors are not accepted.")
                tb.Text = ""
                tb.Focus()
                Return False
            End If

            If age >= 50 AndAlso age <= 99 Then
                MessageBox.Show("This boarding house does not accommodate senior citizens (50 and above).")
                tb.Text = ""
                tb.Focus()
                Return False
            End If

            Return True

        End Function
        Public Shared Sub RateKeyPress(sender As Object, e As KeyPressEventArgs)
            Dim tb As TextBox = DirectCast(sender, TextBox)
            Dim currentText As String = tb.Text

            If Not (Char.IsDigit(e.KeyChar) OrElse e.KeyChar = ControlChars.Back OrElse e.KeyChar = "."c) Then
                e.Handled = True
                Exit Sub
            End If

            If e.KeyChar = "."c AndAlso currentText.Contains(".") Then
                e.Handled = True
                Exit Sub
            End If

            If currentText = "0" AndAlso Char.IsDigit(e.KeyChar) Then
                e.Handled = True
                Exit Sub
            End If

        End Sub
        Public Shared Sub AddressKeyPress(sender As Object, e As KeyPressEventArgs)
            Dim tb As TextBox = DirectCast(sender, TextBox)

            If e.KeyChar = ControlChars.Back Then Exit Sub

            Dim isLetter As Boolean = Char.IsLetter(e.KeyChar)
            Dim isDigit As Boolean = Char.IsDigit(e.KeyChar)
            Dim isSpace As Boolean = (e.KeyChar = " "c)
            Dim isDash As Boolean = (e.KeyChar = "-"c)
            Dim isDot As Boolean = (e.KeyChar = "."c)
            Dim isQuote As Boolean = (e.KeyChar = "'"c)

            If Not (isLetter OrElse isDigit OrElse isSpace OrElse isDash OrElse isDot OrElse isQuote) Then
                e.Handled = True
                Exit Sub
            End If

            ' Maximum 50 characters
            If tb.Text.Length >= 50 Then
                e.Handled = True
                Exit Sub
            End If

            If isSpace AndAlso tb.SelectionStart = 0 Then
                e.Handled = True
                Exit Sub
            End If

            If isSpace AndAlso tb.SelectionStart > 0 AndAlso tb.Text(tb.SelectionStart - 1) = " "c Then
                e.Handled = True
                Exit Sub
            End If

            If isDash AndAlso CountChar(tb.Text, "-"c) >= 2 Then
                e.Handled = True
                Exit Sub
            End If

            If isDot AndAlso CountChar(tb.Text, "."c) >= 2 Then
                e.Handled = True
                Exit Sub
            End If

            If isQuote AndAlso CountChar(tb.Text, "'"c) >= 2 Then
                e.Handled = True
                Exit Sub
            End If

        End Sub


        Public Shared Sub ConsumptionKeyPress(sender As Object, e As KeyPressEventArgs)
            Dim tb As TextBox = DirectCast(sender, TextBox)
            Dim currentText As String = tb.Text

            If Not (Char.IsDigit(e.KeyChar) OrElse e.KeyChar = ControlChars.Back OrElse e.KeyChar = "."c) Then
                e.Handled = True
                Exit Sub
            End If

            If e.KeyChar = ControlChars.Back Then Exit Sub

            If e.KeyChar = "."c AndAlso currentText.Contains(".") Then
                e.Handled = True
                Exit Sub
            End If

            If currentText = "0" AndAlso Char.IsDigit(e.KeyChar) Then
                e.Handled = True
                Exit Sub
            End If

            If currentText.Contains(".") AndAlso Char.IsDigit(e.KeyChar) Then
                Dim decimalIndex As Integer = currentText.IndexOf(".")
                Dim digitsAfterDecimal As Integer = currentText.Length - decimalIndex - 1

                If digitsAfterDecimal >= 2 Then
                    e.Handled = True
                    Exit Sub
                End If
            End If

            Dim prospectiveText As String = currentText.Insert(tb.SelectionStart, e.KeyChar.ToString())

            Dim prospectiveValue As Decimal
            If Decimal.TryParse(prospectiveText, prospectiveValue) Then
                If prospectiveValue > 10000 Then
                    e.Handled = True
                    Exit Sub
                End If
            End If

        End Sub
        Public Shared Sub SearchKeyPress(sender As Object, e As KeyPressEventArgs)
            Dim tb As TextBox = DirectCast(sender, TextBox)

            If e.KeyChar = Chr(13) Then
                e.Handled = True
                Exit Sub
            End If

            If e.KeyChar = ControlChars.Back Then Exit Sub
            If tb.Text.Length >= 50 Then
                e.Handled = True
                Exit Sub
            End If

        End Sub
    End Class
    Private Sub Names_KeyPress(sender As Object, e As KeyPressEventArgs) _
    Handles txtLname.KeyPress, txtFname.KeyPress, txtEmergencyPerson.KeyPress
        InputValidation.NameKeyPress(sender, e)
    End Sub
    Private Sub Contact_KeyPress(sender As Object, e As KeyPressEventArgs) _
        Handles txtContactNumber.KeyPress, txtEmergencyContact.KeyPress
        InputValidation.ContactNumberKeyPress(sender, e)
    End Sub
    Private Sub Age_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtAge.KeyPress
        InputValidation.AgeKeyPress(sender, e)
    End Sub
    Private Sub Age_Leave(sender As Object, e As EventArgs) Handles txtAge.Leave
        InputValidation.ValidateAgeRange(txtAge)
    End Sub
    Private Sub Rate_KeyPress(sender As Object, e As KeyPressEventArgs) _
        Handles txtElectricityRate.KeyPress, txtWaterRate.KeyPress, _
        txtElectricityConsumption.KeyPress, txtWaterConsumption.KeyPress
        InputValidation.RateKeyPress(sender, e)
    End Sub
    Private Sub Address_KeyPress(sender As Object, e As KeyPressEventArgs) _
    Handles txtAddress.KeyPress, txtMaintenanceReason.KeyPress
        InputValidation.AddressKeyPress(sender, e)
    End Sub
    Private Sub Consumption_KeyPress(sender As Object, e As KeyPressEventArgs) _
        Handles txtElectricityConsumption.KeyPress, txtWaterConsumption.KeyPress, _
        txtRentalRate.KeyPress, txtElectricityRate.KeyPress, txtWaterRate.KeyPress
        InputValidation.ConsumptionKeyPress(sender, e)
    End Sub
    Private Sub Search_KeyPress(sender As Object, e As KeyPressEventArgs) _
    Handles txtSearch.KeyPress, txtSearchPayment.KeyPress, txtSearchRoom.KeyPress, _
    txtSearchRoomAssign.KeyPress, txtSearchMaintenance.KeyPress
        InputValidation.SearchKeyPress(sender, e)
    End Sub
    Private Sub btnDashboardViewTenants_Click(sender As Object, e As EventArgs) Handles btnDashboardViewTenants.Click
        If tenantsListForm Is Nothing OrElse tenantsListForm.IsDisposed Then
            tenantsListForm = New frmTenantsList()
            tenantsListForm.Show()
        Else
            tenantsListForm.BringToFront()
            tenantsListForm.Focus()
        End If
    End Sub
    Private Sub btnDashboardViewMaintenance_Click(sender As Object, e As EventArgs) Handles btnDashboardViewMaintenance.Click
        If maintenanceRoomsForm Is Nothing OrElse maintenanceRoomsForm.IsDisposed Then
            maintenanceRoomsForm = New frmMaintenanceRooms()
            maintenanceRoomsForm.Show()
        Else
            maintenanceRoomsForm.BringToFront()
            maintenanceRoomsForm.Focus()
        End If
    End Sub
    Private Sub btnDashboardViewRooms_Click(sender As Object, e As EventArgs) Handles btnDashboardViewRooms.Click
        If allRoomsForm Is Nothing OrElse allRoomsForm.IsDisposed Then
            allRoomsForm = New frmAllRooms()
            allRoomsForm.Show()
        Else
            allRoomsForm.BringToFront()
            allRoomsForm.Focus()
        End If
    End Sub
    Private Sub btnDashboardViewPending_Click(sender As Object, e As EventArgs) Handles btnDashboardViewPending.Click
        If pendingPaymentsForm Is Nothing OrElse pendingPaymentsForm.IsDisposed Then
            pendingPaymentsForm = New frmPendingPayments()
            pendingPaymentsForm.Show()
        Else
            pendingPaymentsForm.BringToFront()
            pendingPaymentsForm.Focus()
        End If
    End Sub
    Private Sub btnDashboardViewPaid_Click(sender As Object, e As EventArgs) Handles btnDashboardViewPaid.Click

        If paidForm Is Nothing OrElse paidForm.IsDisposed Then
            paidForm = New frmPaidPayments()
            paidForm.Show()
        Else
            paidForm.BringToFront()
            paidForm.Focus()
        End If

    End Sub
    Private Sub btnDashboard_Click(sender As System.Object, e As System.EventArgs) Handles btnDashboard.Click

        btnDashboard.BackColor = Color.FromArgb(240, 236, 225)

        btnManage.BackColor = Color.FromArgb(221, 220, 226)
        btnMaintenance.BackColor = Color.FromArgb(221, 220, 226)
        btnPayment.BackColor = Color.FromArgb(221, 220, 226)
        btnSettings.BackColor = Color.FromArgb(221, 220, 226)

        PHeaderDashboard.BringToFront()
        PDashboard.BringToFront()

        lblSectionName.Text = "D A S H B O A R D"

        PDashboard.Show()
        PTenants.Hide()
        PRooms.Hide()
        PRoomStat.Hide()
        PPayment.Hide()
        PSettings.Hide()
        PHeaderManagement.Hide()
        PHeaderMaintenance.Hide()
        PHeaderPayments.Hide()
        PHeaderSetting.Hide()
        PHeaderDashboard.Show()

        LoadDashboard()
    End Sub

    Private Sub txtSearchMaintenance_TextChanged_1(sender As System.Object, e As System.EventArgs) Handles txtSearchMaintenance.TextChanged
        ApplyMaintenanceFilters()
    End Sub
    Private Sub btnManualBackup_Click(sender As Object, e As EventArgs) Handles btnManualBackup.Click

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim result As DialogResult = verifyForm.ShowDialog()

        If result <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        If PerformBackup() Then
            MessageBox.Show("Backup completed successfully!" & vbCrLf & "Saved to: " & backupFolder, "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadBackupList()
        End If

    End Sub

    Private Sub chkAutoBackup_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoBackup.CheckedChanged

        If chkAutoBackup.Checked Then

            Dim verifyForm As New frmSettingsVerification()
            verifyForm.RequiredText = "CONFIRM"
            Dim result As DialogResult = verifyForm.ShowDialog()

            If result <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
                chkAutoBackup.Checked = False
                Exit Sub
            End If

            tmrAutoBackup.Start()
            MessageBox.Show("Auto-backup enabled. The database will back up automatically every 15 Seconds.", "Auto-Backup Enabled", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Else
            tmrAutoBackup.Stop()
            MessageBox.Show("Auto-backup disabled.", "Auto-Backup Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If

    End Sub
    Private Sub btnChooseBackupFolder_Click(sender As Object, e As EventArgs) Handles btnChooseBackupFolder.Click

        Using fileDialog As New OpenFileDialog()
            fileDialog.InitialDirectory = backupFolder
            fileDialog.Title = "Browse to the folder you want to use, then select any file or type a filename and click Open"
            fileDialog.Filter = "All Files (*.*)|*.*"
            fileDialog.CheckFileExists = False
            fileDialog.FileName = "Select Folder"

            If fileDialog.ShowDialog() = DialogResult.OK Then

                Dim newPath As String = IO.Path.GetDirectoryName(fileDialog.FileName) & "\"

                Dim query As String = "UPDATE settings SET backup_folder_path = @path"

                Using con As New SqlConnection(connectionString)
                    Using cmd As New SqlCommand(query, con)
                        cmd.Parameters.AddWithValue("@path", newPath)
                        con.Open()
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                backupFolder = newPath
                lblBackupPath.Text = "Backup folder: " & backupFolder
                MessageBox.Show("Backup folder updated and saved permanently!")
                LoadBackupList()

            End If
        End Using

    End Sub
    Private Sub btnRestoreBackup_Click(sender As Object, e As EventArgs) Handles btnRestoreBackup.Click

        Dim selectedFilePath As String = ""
        Dim selectedFileName As String = ""

        Using openDialog As New OpenFileDialog()
            openDialog.InitialDirectory = backupFolder
            openDialog.Filter = "DMS Backup Files (*.bak)|*.bak"
            openDialog.Title = "Select a DMS backup file to restore"

            If openDialog.ShowDialog() <> DialogResult.OK Then
                Exit Sub
            End If

            selectedFilePath = openDialog.FileName
            selectedFileName = IO.Path.GetFileName(selectedFilePath)
        End Using

        If Not (selectedFileName.StartsWith("DMS_Backup_") AndAlso selectedFileName.EndsWith(".bak")) Then
            MessageBox.Show(
                "This file does not match the expected DMS backup format (DMS_Backup_YYYY-MM-DD_HH-mm-ss.bak)." & vbCrLf & vbCrLf & "Only backups created by this application can be restored.",
                "Invalid Backup File",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
            Exit Sub
        End If

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"
        Dim result As DialogResult = verifyForm.ShowDialog()

        If result <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        Dim finalConfirm As DialogResult = MessageBox.Show(
            "Restoring this backup will REPLACE all current data with:" & vbCrLf & selectedFileName & vbCrLf & vbCrLf & "This cannot be undone. Continue?",
            "Confirm Restore",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        )

        If finalConfirm = DialogResult.No Then Exit Sub

        Try
            Dim masterConnectionString As String =
                "Data Source=.\SQLEXPRESS;Initial Catalog=master;Integrated Security=True"

            Using con As New SqlConnection(masterConnectionString)
                con.Open()

                Using cmd As New SqlCommand("ALTER DATABASE [DMS] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", con)
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New SqlCommand("RESTORE DATABASE [DMS] FROM DISK = @path WITH REPLACE", con)
                    cmd.Parameters.AddWithValue("@path", selectedFilePath)
                    cmd.CommandTimeout = 120
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd As New SqlCommand("ALTER DATABASE [DMS] SET MULTI_USER", con)
                    cmd.ExecuteNonQuery()
                End Using

            End Using

            MessageBox.Show("Database restored successfully! The application will now restart.", "Restore Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Application.Restart()
            Me.Close()

        Catch ex As Exception
            MessageBox.Show("Restore failed: " & ex.Message)
        End Try

    End Sub
    Private Sub tmrAutoBackup_Tick(sender As System.Object, e As System.EventArgs) Handles tmrAutoBackup.Tick
        PerformBackup()
        LoadBackupList()
    End Sub
    Private Sub btnManageConsumption_Click(sender As System.Object, e As System.EventArgs) Handles btnManageConsumption.Click

        PDashboard.Hide()
        PTenants.Hide()
        PRooms.Hide()
        PRoomStat.Hide()
        PPayment.Hide()
        PSettings.Show()
        PHeaderManagement.Hide()
        PHeaderMaintenance.Hide()
        PHeaderPayments.Hide()
        PHeaderSetting.Show()
        PHeaderDashboard.Hide()


        cmbSortRooms.Items.Clear()
        cmbSortRooms.Items.Add("Room Number: Low to High")
        cmbSortRooms.Items.Add("Room Number: High to Low")
        cmbSortRooms.Items.Add("Floor: Low to High")
        cmbSortRooms.Items.Add("Floor: High to Low")
        cmbSortRooms.Items.Add("Status: A-Z")
        cmbSortRooms.SelectedIndex = 0


        LoadCurrentRates()
        LoadRoomsManage()
    End Sub

    Private Sub btnDatabaseSettings_Click(sender As System.Object, e As System.EventArgs) Handles btnDatabaseSettings.Click
        PDashboard.Hide()
        PTenants.Hide()
        PRooms.Hide()
        PRoomStat.Hide()
        PPayment.Hide()
        PSettings.Hide()
        PHeaderManagement.Hide()
        PHeaderMaintenance.Hide()
        PHeaderPayments.Hide()
        PHeaderSetting.Show()
        PHeaderDashboard.Hide()
        PDatabaseSettings.Show()

        LoadSettings()
        LoadCurrentRates()
        LoadRoomsManage()
        LoadBackupList()
        LoadDriveInfo()

        lblBackupPath.Text = "Backup folder: " & backupFolder

    End Sub
    Private Sub btnConfigureLogs_Click(sender As Object, e As EventArgs) Handles btnConfigureLogs.Click

        Dim verifyForm As New frmSettingsVerification()
        verifyForm.RequiredText = "CONFIRM"

        Dim verifyResult As DialogResult = verifyForm.ShowDialog()

        If verifyResult <> DialogResult.OK OrElse Not verifyForm.Confirmed Then
            Exit Sub
        End If

        If logConfigForm Is Nothing OrElse logConfigForm.IsDisposed Then
            logConfigForm = New frmLogConfiguration()
            logConfigForm.Show()
        Else
            logConfigForm.BringToFront()
            logConfigForm.Focus()
        End If

    End Sub
    Private Sub txtSearchRoomAssign_TextChanged_1(sender As System.Object, e As System.EventArgs) Handles txtSearchRoomAssign.TextChanged
        ApplyRoomAssignmentFilters()
    End Sub
    Private Sub btnDashboardViewUnavailable_Click(sender As Object, e As EventArgs) Handles btnDashboardViewUnavailable.Click
        If unavailableRoomsForm Is Nothing OrElse unavailableRoomsForm.IsDisposed Then
            unavailableRoomsForm = New frmUnavailableRooms()
            unavailableRoomsForm.Show()
        Else
            unavailableRoomsForm.BringToFront()
            unavailableRoomsForm.Focus()
        End If
    End Sub

    Private Sub btnViewArchived_Click(sender As System.Object, e As System.EventArgs) Handles btnViewArchived.Click
        frmTenantArchives.Show()

        ' Refresh the main grid after closing the archive window
        LoadTenants()
    End Sub
   Private Sub btnSaveReport_Click(sender As System.Object, e As System.EventArgs) Handles btnSaveReport.Click

        If dgvPayments.CurrentRow Is Nothing Then
            MessageBox.Show("Please select a payment to generate a receipt for.")
            Exit Sub
        End If

        printPaymentId = CInt(dgvPayments.CurrentRow.Cells("payment_id").Value)

        Dim paymentQuery As String =
            "SELECT room_number, electricity_consumption, water_consumption, " &
            "electricity_rate, water_rate, electricity_amount, water_amount, " &
            "rental_amount, total_amount, due_date, date_created " &
            "FROM payments WHERE payment_id = @id"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(paymentQuery, con)
                cmd.Parameters.AddWithValue("@id", printPaymentId)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        printRoomNumber = CInt(reader("room_number"))
                        printElectricityConsumption = CDec(reader("electricity_consumption"))
                        printWaterConsumption = CDec(reader("water_consumption"))
                        printElectricityRate = CDec(reader("electricity_rate"))
                        printWaterRate = CDec(reader("water_rate"))
                        printElectricityAmount = CDec(reader("electricity_amount"))
                        printWaterAmount = CDec(reader("water_amount"))
                        printRentalAmount = CDec(reader("rental_amount"))
                        printTotalAmount = CDec(reader("total_amount"))
                        printDueDate = CDate(reader("due_date"))
                        printDateCreated = CDate(reader("date_created"))
                    Else
                        MessageBox.Show("Payment record not found.")
                        Exit Sub
                    End If
                End Using
            End Using
        End Using

        printTenantList.Clear()

        Dim tenantQuery As String = "SELECT lname, fname, contact_number FROM tenants WHERE room_number = @room"

        Using con As New SqlConnection(connectionString)
            Using cmd As New SqlCommand(tenantQuery, con)
                cmd.Parameters.AddWithValue("@room", printRoomNumber)
                con.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        printTenantList.Add(New String() {
                            reader("lname").ToString() & ", " & reader("fname").ToString(),
                            reader("contact_number").ToString()
                        })
                    End While
                End Using
            End Using
        End Using

        Dim saveDialog As New SaveFileDialog()

        saveDialog.Title = "Save Payment Receipt"
        saveDialog.Filter = "PDF Files (*.pdf)|*.pdf"
        saveDialog.DefaultExt = "pdf"
        saveDialog.AddExtension = True

        saveDialog.FileName = "Receipt_Payment_" & printPaymentId & ".pdf"

        If saveDialog.ShowDialog() <> DialogResult.OK Then
            Exit Sub
        End If

        Dim filePath As String = saveDialog.FileName

        PrintDocument1.PrinterSettings.PrinterName = "Microsoft Print to PDF"
        PrintDocument1.PrinterSettings.PrintToFile = True
        PrintDocument1.PrinterSettings.PrintFileName = filePath

        For Each ps As PaperSize In PrintDocument1.PrinterSettings.PaperSizes
            If ps.Kind = PaperKind.Statement Then
                PrintDocument1.DefaultPageSettings.PaperSize = ps
                Exit For
            End If
        Next

        PrintDocument1.DefaultPageSettings.Landscape = False

        PrintDocument1.DefaultPageSettings.Margins = _
            New Margins(30, 30, 30, 30)

        PrintDocument1.Print()

        pdfNumber += 1

        MessageBox.Show( _
            "Receipt successfully saved!" & vbCrLf & vbCrLf & _
            filePath, _
            "Export Complete", _
            MessageBoxButtons.OK, _
            MessageBoxIcon.Information)

    End Sub
    Private Sub PrintDocument1_PrintPage(sender As System.Object, e As System.Drawing.Printing.PrintPageEventArgs) Handles PrintDocument1.PrintPage

        Dim titleFont As New Font("Arial", 11, FontStyle.Bold)
        Dim subtitleFont As New Font("Arial", 8, FontStyle.Italic)
        Dim headerFont As New Font("Arial", 8, FontStyle.Bold)
        Dim font As New Font("Arial", 7.5)
        Dim totalFont As New Font("Arial", 9, FontStyle.Bold)

        Dim left As Integer = e.MarginBounds.Left
        Dim top As Integer = e.MarginBounds.Top
        Dim x As Integer = left
        Dim y As Integer = top
        Dim widthLimit As Integer = e.MarginBounds.Width

        ' Title (centered)
        Dim titleSize As SizeF = e.Graphics.MeasureString("Vista - DMS 2026", titleFont)
        e.Graphics.DrawString("Vista - DMS 2026", titleFont, Brushes.Black, left + (widthLimit - titleSize.Width) / 2, y)
        y += 18

        Dim subtitleSize As SizeF = e.Graphics.MeasureString("Payment Receipt", subtitleFont)
        e.Graphics.DrawString("Payment Receipt", subtitleFont, Brushes.Black, left + (widthLimit - subtitleSize.Width) / 2, y)
        y += 18

        e.Graphics.DrawLine(Pens.Black, x, y, x + widthLimit, y)
        y += 10

        e.Graphics.DrawString("Payment ID: " & printPaymentId, font, Brushes.Black, x, y)
        y += 14
        e.Graphics.DrawString("Room: " & printRoomNumber, font, Brushes.Black, x, y)
        y += 14
        e.Graphics.DrawString("Date: " & printDateCreated.ToString("MM/dd/yyyy"), font, Brushes.Black, x, y)
        y += 14
        e.Graphics.DrawString("Due: " & printDueDate.ToString("MM/dd/yyyy"), font, Brushes.Black, x, y)
        y += 16

        e.Graphics.DrawLine(Pens.Black, x, y, x + widthLimit, y)
        y += 10

        e.Graphics.DrawString("TENANTS:", headerFont, Brushes.Black, x, y)
        y += 14

        If printTenantList.Count = 0 Then
            e.Graphics.DrawString("None assigned", font, Brushes.Black, x, y)
            y += 14
        Else
            For Each t In printTenantList
                e.Graphics.DrawString(t(0), font, Brushes.Black, x, y)
                y += 12
                e.Graphics.DrawString(t(1), font, Brushes.Black, x + 10, y)
                y += 14
            Next
        End If

        y += 6
        e.Graphics.DrawLine(Pens.Black, x, y, x + widthLimit, y)
        y += 10

        e.Graphics.DrawString("CHARGES:", headerFont, Brushes.Black, x, y)
        y += 14

        e.Graphics.DrawString("Rental", font, Brushes.Black, x, y)
        y += 12
        e.Graphics.DrawString("₱" & printRentalAmount.ToString("N2"), font, Brushes.Black, x + 10, y)
        y += 16

        e.Graphics.DrawString("Electricity", font, Brushes.Black, x, y)
        y += 12
        e.Graphics.DrawString(printElectricityConsumption.ToString("N2") & " kWh @ ₱" & printElectricityRate.ToString("N2"), font, Brushes.Black, x + 10, y)
        y += 12
        e.Graphics.DrawString("₱" & printElectricityAmount.ToString("N2"), font, Brushes.Black, x + 10, y)
        y += 16

        e.Graphics.DrawString("Water", font, Brushes.Black, x, y)
        y += 12
        e.Graphics.DrawString(printWaterConsumption.ToString("N2") & " m³ @ ₱" & printWaterRate.ToString("N2"), font, Brushes.Black, x + 10, y)
        y += 12
        e.Graphics.DrawString("₱" & printWaterAmount.ToString("N2"), font, Brushes.Black, x + 10, y)
        y += 18

        e.Graphics.DrawLine(Pens.Black, x, y, x + widthLimit, y)
        y += 12

        Dim totalText As String = "TOTAL: ₱" & printTotalAmount.ToString("N2")
        Dim totalSize As SizeF = e.Graphics.MeasureString(totalText, totalFont)
        e.Graphics.DrawString(totalText, totalFont, Brushes.Black, left + (widthLimit - totalSize.Width) / 2, y)

        e.HasMorePages = False

    End Sub

    Private Sub cmbPaymentRoom_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbPaymentRoom.SelectedIndexChanged

    End Sub
End Class
