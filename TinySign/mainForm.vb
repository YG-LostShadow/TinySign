﻿Imports System.IO

Public Class mainForm

    Dim inspectResult As String
    Dim mapInformation As String()
    Dim mapStream As FileStream = Nothing

    Private Sub MainWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        closeMapToolStripMenuItem.Enabled = False
        resignMapToolStripMenuItem.Enabled = False
        mapInfoToolStripMenuItem.Enabled = False
    End Sub

    'TODO: Fix the crazy stuff going on in this sub
    Public Sub OpenMapToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles openMapToolStripMenuItem.Click

        'Open File Dialogue: http://msdn.microsoft.com/en-us/library/system.windows.forms.openfiledialog(v=vs.110).aspx
        'File Streams: http://msdn.microsoft.com/en-us/library/system.io.filestream.aspx
        Dim openFileDialog1 As New OpenFileDialog()

        openFileDialog1.Filter = "Halo 2 map files (*.map)|*.map"
        openFileDialog1.FilterIndex = 1
        openFileDialog1.RestoreDirectory = True

        If openFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try
                mapStream = openFileDialog1.OpenFile()
                'Read the .map file

                If (mapStream IsNot Nothing) Then
                    If mapStream.CanRead Then

                        'Check to see if the file is a valid Halo 2 .map file
                        If mapStream.Length < 10000000 Then
                            mapStream.Close()
                            MsgBox("The file you are attempting to open is not a valid Halo 2 .map file. File unloaded.")
                            Exit Sub
                        Else
                            'Pass header onto inspector
                            Dim tempHandler As New mapHandler()
                            Dim headerLocation(4) As Byte
                            mapStream.Position = 2044
                            mapStream.Read(headerLocation, 0, 4)
                            inspectResult = tempHandler.inspectMapFile(headerLocation)
                            If inspectResult = "Valid" Then
                                'Do nothing
                            Else
                                mapStream.Close()
                                MsgBox("The file you are attempting to open is not a valid Halo 2 .map file. File unloaded.")
                                toolStripStatusLabel.Text = "//Invalid file. File unloaded."
                                toolStripStatusLabel.ToolTipText = "//Invalid file. File unloaded."
                                Exit Sub
                            End If
                        End If

                        'Gets the original bytes
                        'Decimal positions:
                        '408 = internal name, 444 = scenario path, 720 = signature addr
                        Dim nameLocation(35) As Byte
                        mapStream.Position = 408
                        mapStream.Read(nameLocation, 0, 35)

                        'Give me those bytes that need to be changed!!
                        Dim map As New mapHandler()
                        mapInformation = map.readInternalName(nameLocation)

                        'Update the UI
                        'Display what the current signature is and what it should be
                        'Finding strings in an array: http://msdn.microsoft.com/en-us/library/vstudio/eefw3xsy(v=vs.100).aspx
                        Dim queryResults As String() = map.queryMapList(mapInformation(0))
                        Dim currentSig As String = map.readCurrentSig(mapStream)
                        Dim actualSig As String = queryResults(4)

                        currentSigTextBox.Text = currentSig

                        For Each Str As String In queryResults
                            If Str.Contains(currentSig) Then
                                'If the current signature matches
                                applySigTextBox.Text = currentSig
                                applySigLabel.ForeColor = Color.Green
                                currentSigLabel.ForeColor = Color.Green
                            Else
                                'If the current signature does not match
                                applySigTextBox.Text = actualSig
                                applySigLabel.ForeColor = Color.Red
                                currentSigLabel.ForeColor = Color.Red
                            End If
                        Next

                        'Display the map image
                        Dim mapNameToString As String = "_" & mapInformation(0).ToString
                        Dim mapImage As Image = My.Resources.ResourceManager.GetObject(mapNameToString)
                        mapIconBox.Image = mapImage

                        'Update toolstrip status
                        Dim mapPath As String = openFileDialog1.FileName
                        If mapPath.Length > 45 Then
                            Dim mapPathShortened As String = Microsoft.VisualBasic.Right(mapPath, 45)
                            toolStripStatusLabel.Text = "..." & mapPathShortened
                            toolStripStatusLabel.ToolTipText = "..." & mapPathShortened
                        Else
                            toolStripStatusLabel.Text = mapPath
                            toolStripStatusLabel.ToolTipText = mapPath
                        End If

                        'Enable menu buttons
                        closeMapToolStripMenuItem.Enabled = True
                        resignMapToolStripMenuItem.Enabled = True
                        mapInfoToolStripMenuItem.Enabled = True

                    End If
                End If
            Catch Ex As Exception
                MessageBox.Show("Cannot read file from disk. Original error: " & Ex.Message)
            Finally
                'Check this again, since we need to make sure we didn't throw an exception on open. 
                If (mapStream IsNot Nothing) Then
                    'mapStream.Close()
                End If

            End Try
        End If

    End Sub

    Public Sub CloseMapToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles closeMapToolStripMenuItem.Click

        'Clean up the UI
        mapIconBox.Image = My.Resources.Unknown_Map
        currentSigTextBox.Text = ""
        currentSigLabel.ForeColor = Color.Black
        applySigTextBox.Text = ""
        applySigLabel.ForeColor = Color.Black
        resignMapToolStripMenuItem.Enabled = False
        mapInfoToolStripMenuItem.Enabled = False
        closeMapToolStripMenuItem.Enabled = False
        'Tool Strip formatting: http://stackoverflow.com/questions/16189893/cut-status-strip-label-to-width-of-form
        toolStripStatusLabel.Text = "//Map unloaded."
        toolStripStatusLabel.ToolTipText = "//Map unloaded."

        'The file is closed at the end of OpenFileDialoge sub

    End Sub

    ' Where it resigns AKA Where it all goes wrong
    Private Sub ResignMapToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles resignMapToolStripMenuItem.Click
        Dim aMapHandler As New mapHandler
        mapStream.Position = 720

        ' convert mapInformation sig into an array of bytes
        Dim bytesToWrite() As Byte
        bytesToWrite = aMapHandler.giveBytes(mapInformation(4))

        ' array As Byte(), _offset As Integer, _count As Integer _ -> the 4 could be something else, is it 4 bytes long? I think so
        Try
            mapStream.Write(bytesToWrite, 0, 4)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "error")
        End Try

    End Sub

    Private Sub MapInfoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles mapInfoToolStripMenuItem.Click
        Dim PassMe As New mapInfoForm
        PassMe.updateValues(mapInformation)
        PassMe.Show()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles aboutToolStripMenuItem.Click
        aboutBoxForm.Show()
    End Sub

End Class
