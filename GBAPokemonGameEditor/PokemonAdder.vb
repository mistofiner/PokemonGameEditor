﻿Imports System.IO
Imports VB = Microsoft.VisualBasic

Public Class PokemonAdder
    Private Sub PokemonAdder_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If header = "BPEE" Then

            TabControl1.TabPages(0).Enabled = False
            TabControl1.TabPages(1).Enabled = True
            TabControl1.SelectedIndex = 1

            MsgBox("Not implemented yet!")
            End

        ElseIf header = "BPRE" Then

            TabControl1.TabPages(0).Enabled = True
            TabControl1.TabPages(1).Enabled = False
            TabControl1.SelectedIndex = 0

            Label1.Text = "Number of Pokemon in ROM: " & (GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) - 1
            Label2.Text = "Number of Dex entries in ROM: " & (GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1


            Label6.Text = "Seen flags location: " & Hex(CInt("&H" & TextBox3.Text))
            Label7.Text = "Caught flags location: " & Hex((("&H" & TextBox3.Text) + Math.Ceiling((((GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1) + TextBox2.Text) / 8)))

        Else

            TabControl1.TabPages(0).Enabled = False
            TabControl1.TabPages(1).Enabled = False

            MsgBox("Not supported.")
            Me.Close()

        End If

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        'Declare buffers I will use for adding stuff.

        Dim countervar As Integer

        Dim PokemonNamesBuffer As String
        Dim PokemonNamesNewOffset As String

        Dim PokemonDataBuffer As String
        Dim PokemonDataNewOffset As String

        Dim PokemonAttackDataBuffer As String
        Dim PokemonAttackDataNewOffset As String

        Dim PokemonFrontSpriteTableBuffer As String
        Dim PokemonFrontSpriteTableNewOffset As String

        Dim PokemonBackSpriteTableBuffer As String
        Dim PokemonBackSpriteTableNewOffset As String


        If System.IO.File.Exists((LoadedROM).Substring(0, LoadedROM.Length - 4) & ".ini") = True Then

            MsgBox("An INI for this ROM has been detected! Values will be updated as needed.")

        Else

            MsgBox("INI not found! One will now be created for this ROM in the same location as the ROM. Keep the ini with the ROM so that PGE can know the location of the data.")

            File.Copy(AppPath & "ini\roms.ini", (LoadedROM).Substring(0, LoadedROM.Length - 4) & ".ini", True)


        End If

        'Install's JPAN's save hack.

        If CheckBox1.Checked And GetString(GetINIFileLocation(), header, "JPANSaveHack", "False") = "False" Then

            Dim SaveRoutine As String = "2168FF231B015B189888002809D004280AD00D280CD00D2D1CDD012008BC9846F0BDCC21084A06E096218900074A02E0BA210901064A043B18681060043A04390029F8D1E7E70000C8C0030220C30302C0CE030200480047719E0D08FFFFFFFFFF273F01CF19F880BE88002E08D0042E09D00D2E0BD000000048004723990D08CC23084A06E096239B00074A02E0BA231B01064A043F10683860043A043B002BF8D1E9E7C8C0030220C30302C0CE03020000240F0000F00FF00FF00FE01FF00FD02F980D0000F00FF00FF00FE01FF00FD02FF00FC03FF00FB04FF00FA05FF00F906FF00F807F5004"
            Dim SaveTable As String = "0000240F0000F00FF00FF00FE01FF00FD02F980D0000F00FF00FF00FE01FF00FD02FF00FC03FF00FB04FF00FA05FF00F906FF00F807F5004"
            Dim SaveRoutineOffset As String = ""

            SaveRoutineOffset = SearchFreeSpaceFourAligned(LoadedROM, &HFF, ((Len(SaveRoutine) / 2)), "&H" & GetString(GetINIFileLocation(), header, "StartSearchingForSpaceOffset", "800000"))

            WriteHEX(LoadedROM, SaveRoutineOffset, SaveRoutine)
            WriteHEX(LoadedROM, &H3FEC94, SaveTable)

            WriteHEX(LoadedROM, &HD9EDC, "00480047" & ReverseHEX(Hex((SaveRoutineOffset) + &H8000001)))
            WriteHEX(LoadedROM, &HD991E, "3847")
            WriteHEX(LoadedROM, &HD995C, ReverseHEX(Hex((SaveRoutineOffset) + &H8000061)))


            'Enable the hack in the ini file.
            WriteString(GetINIFileLocation(), header, "JPANSaveHack", "True")

        End If

        'This makes the Pokedex work with the save hack.

        If CheckBox1.Checked Then

            'Disable LR help menu
            WriteHEX(LoadedROM, &H13B8C2, "1DE0")

            'Makes game be able to read the new location of the SEEN flags.

            WriteHEX(LoadedROM, &H104B10, ReverseHEX(VB.Right("00000000" & Hex(CInt("&H" & TextBox3.Text)), 8)))
            WriteHEX(LoadedROM, &H104B00, "0000")
            WriteHEX(LoadedROM, &H104B02, "0000")

            'Makes game be able to read the new location of the CAUGHT flags.

            WriteHEX(LoadedROM, &H104B5C, ReverseHEX(VB.Right("00000000" & Hex((("&H" & TextBox3.Text) + Math.Ceiling((((GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1) + TextBox2.Text) / 8))), 8)))
            WriteHEX(LoadedROM, &H104B16, "0000")
            WriteHEX(LoadedROM, &H104B18, "0000")
            WriteHEX(LoadedROM, &H104B1A, "0000")
            WriteHEX(LoadedROM, &H104B26, "16E0")

            'This makes both of the above bypass redundant SEEN flags.??
            WriteHEX(LoadedROM, &H104B34, "0FE0")

            'This makes the game write to the New SEEN flags.
            WriteHEX(LoadedROM, &H104B94, ReverseHEX(VB.Right("00000000" & Hex(CInt("&H" & TextBox3.Text)), 8)))
            WriteHEX(LoadedROM, &H104B6A, "011C")
            WriteHEX(LoadedROM, &H104B6C, "0000")
            WriteHEX(LoadedROM, &H104B78, "1AE0")

            'This makes the game write to the New CAUGHT flags.
            WriteHEX(LoadedROM, &H104BB8, ReverseHEX(VB.Right("00000000" & Hex((("&H" & TextBox3.Text) + Math.Ceiling((((GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1) + TextBox2.Text) / 8))), 8)))
            WriteHEX(LoadedROM, &H104BA2, "011C")
            WriteHEX(LoadedROM, &H104BA4, "0000")

            'Makes the game clear the flags properly when the player selects New Game at the main menu.
            WriteHEX(LoadedROM, &H549D0, ReverseHEX(VB.Right("00000000" & Hex(CInt("&H" & TextBox3.Text)), 8)))
            WriteHEX(LoadedROM, &H549B0, "201C0000")
            WriteHEX(LoadedROM, &H549B6, Hex(Math.Ceiling((((GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1) + TextBox2.Text) / 8)))
            WriteHEX(LoadedROM, &H549B7, "22")
            WriteHEX(LoadedROM, &H549BC, "201C")
            WriteHEX(LoadedROM, &H549BE, Hex(Math.Ceiling((((GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1) + TextBox2.Text) / 8)))
            WriteHEX(LoadedROM, &H549BF, "30")
            WriteHEX(LoadedROM, &H549C2, Hex(Math.Ceiling((((GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1) + TextBox2.Text) / 8)))
            WriteHEX(LoadedROM, &H549C3, "22")


        End If

        'Pokemon Names

        PokemonNamesBuffer = ReadHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonNames", "")), System.Globalization.NumberStyles.HexNumber), ((GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) + 0) * 11)

        'Deletes old data

        If CheckBox2.Checked Then
            WriteHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonNames", "")), System.Globalization.NumberStyles.HexNumber), MakeFreeSpaceString((Len(PokemonNamesBuffer) / 2)))
        End If

        'Handles Unowns

        If (GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) = "412" Then
            PokemonNamesBuffer = PokemonNamesBuffer & "BFC1C1FF00000000000000CFC8C9D1C8BCFFFFFFFFFFCFC8C9D1C8BDFFFFFFFFFFCFC8C9D1C8BEFFFFFFFFFFCFC8C9D1C8BFFFFFFFFFFFCFC8C9D1C8C0FFFFFFFFFFCFC8C9D1C8C1FFFFFFFFFFCFC8C9D1C8C2FFFFFFFFFFCFC8C9D1C8C3FFFFFFFFFFCFC8C9D1C8C4FFFFFFFFFFCFC8C9D1C8C5FFFFFFFFFFCFC8C9D1C8C6FFFFFFFFFFCFC8C9D1C8C7FFFFFFFFFFCFC8C9D1C8C8FFFFFFFFFFCFC8C9D1C8C9FFFFFFFFFFCFC8C9D1C8CAFFFFFFFFFFCFC8C9D1C8CBFFFFFFFFFFCFC8C9D1C8CCFFFFFFFFFFCFC8C9D1C8CDFFFFFFFFFFCFC8C9D1C8CEFFFFFFFFFFCFC8C9D1C8CFFFFFFFFFFFCFC8C9D1C8D0FFFFFFFFFFCFC8C9D1C8D1FFFFFFFFFFCFC8C9D1C8D2FFFFFFFFFFCFC8C9D1C8D3FFFFFFFFFFCFC8C9D1C8D4FFFFFFFFFFCFC8C9D1C8ABFFFFFFFFFFCFC8C9D1C8ACFF00000000"
        End If

        'Adds new data

        countervar = 0

        While countervar < TextBox1.Text
            countervar = countervar + 1

            PokemonNamesBuffer = PokemonNamesBuffer & "ACACACACACACACACACACFF"

        End While

        PokemonNamesNewOffset = SearchFreeSpaceFourAligned(LoadedROM, &HFF, ((Len(PokemonNamesBuffer) / 2)), "&H" & GetString(GetINIFileLocation(), header, "StartSearchingForSpaceOffset", "800000"))

        WriteHEX(LoadedROM, PokemonNamesNewOffset, PokemonNamesBuffer)

        WriteString(GetINIFileLocation(), header, "PokemonNames", Hex(PokemonNamesNewOffset))

        'Repoint Pokemon Names

        WriteHEX(LoadedROM, &H000144, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H011450, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H01150C, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0115A4, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0116BC, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H034D80, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H040FEC, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0435DC, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H049768, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H053ADC, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H053B44, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H06BCC4, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H06BD10, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H093E8C, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H09F560, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0A0900, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0BD820, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CB120, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CBEAC, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CE0B4, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CE6B8, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CE8D0, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0E651C, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0F3324, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H1035C0, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H103698, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H103738, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H1037D0, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H10386C, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H1038CC, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H104E70, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H105FE0, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H1066B0, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H113EFC, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H119458, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H11ACA0, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H11AFF4, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H11B048, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H12190C, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H136318, ReverseHEX(Hex((PokemonNamesNewOffset) + &H8000000)))

        'This will cause the repointed name table to actually get used.
        WriteHEX(LoadedROM, &H41000, "000000000000")

        'Pokemon Data

        PokemonDataBuffer = ReadHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonData", "")), System.Globalization.NumberStyles.HexNumber), ((GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) + 0) * 28)

        'Deletes old data

        If CheckBox2.Checked Then
            WriteHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonData", "")), System.Globalization.NumberStyles.HexNumber), MakeFreeSpaceString((Len(PokemonDataBuffer) / 2)))
        End If

        'Handles Unowns

        If (GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) = "412" Then
            PokemonDataBuffer = PokemonDataBuffer & "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"
        End If

        'Adds new data

        countervar = 0

        While countervar < TextBox1.Text
            countervar = countervar + 1

            PokemonDataBuffer = PokemonDataBuffer & "00000000000000000000000000000000000000000000000000000000"

        End While

        PokemonDataNewOffset = SearchFreeSpaceFourAligned(LoadedROM, &HFF, ((Len(PokemonDataBuffer) / 2)), "&H" & GetString(GetINIFileLocation(), header, "StartSearchingForSpaceOffset", "800000"))

        WriteHEX(LoadedROM, PokemonDataNewOffset, PokemonDataBuffer)

        WriteString(GetINIFileLocation(), header, "PokemonData", Hex(PokemonDataNewOffset))

        'Repoint Pokemon Data

        WriteHEX(LoadedROM, &H0001BC, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H012754, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H012FA8, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H013230, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H016A60, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H01F008, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H021C28, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0240B4, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H02BABC, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H02CE90, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H02D53C, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H02FFBC, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0300E8, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0301F8, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H039558, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0399BC, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H039A68, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H039FE8, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H03DCC4, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H03DD94, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H03E734, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H03E82C, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H03E898, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H03F780, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H03F7C8, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H040D58, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H040D78, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0413B8, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0419A0, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H043914, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H043928, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H04393C, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H043950, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H043964, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0439A4, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H043B8C, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H043C10, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H044208, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H044458, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H046600, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H049F4C, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H04FC88, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0C84E0, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CA850, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0E7F14, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0E8040, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0E8150, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H103AC4, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H1066B4, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H11AE48, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H1361F8, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H136794, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H13AAE0, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H1569EC, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H156B18, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H156C28, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000000)))

        'egg hatching time
        WriteHEX(LoadedROM, &H46204, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000011)))
        WriteHEX(LoadedROM, &H462A8, ReverseHEX(Hex((PokemonDataNewOffset) + &H8000011)))

        'Pokemon Attack Table

        PokemonAttackDataBuffer = ReadHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonAttackTable", "")), System.Globalization.NumberStyles.HexNumber), ((GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) + 0) * 4)

        'Deletes old data

        If CheckBox2.Checked Then
            WriteHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonAttackTable", "")), System.Globalization.NumberStyles.HexNumber), MakeFreeSpaceString((Len(PokemonAttackDataBuffer) / 2)))
        End If

        'Handles Unowns

        If (GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) = "412" Then
            PokemonAttackDataBuffer = PokemonAttackDataBuffer & "94742508947425089474250894742508947425089474250894742508947425089474250894742508947425089474250894742508947425089474250894742508947425089474250894742508947425089474250894742508947425089474250894742508947425089474250894742508"
        End If

        'Adds new data

        countervar = 0

        While countervar < TextBox1.Text
            countervar = countervar + 1

            PokemonAttackDataBuffer = PokemonAttackDataBuffer & "94742508"

        End While

        PokemonAttackDataNewOffset = SearchFreeSpaceFourAligned(LoadedROM, &HFF, ((Len(PokemonAttackDataBuffer) / 2)), "&H" & GetString(GetINIFileLocation(), header, "StartSearchingForSpaceOffset", "800000"))

        WriteHEX(LoadedROM, PokemonAttackDataNewOffset, PokemonAttackDataBuffer)

        WriteString(GetINIFileLocation(), header, "PokemonAttackTable", Hex(PokemonAttackDataNewOffset))

        'Repoint Pokemon Attack Table

        WriteHEX(LoadedROM, &H3EA7C, ReverseHEX(Hex((PokemonAttackDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H3EB10, ReverseHEX(Hex((PokemonAttackDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H3EB84, ReverseHEX(Hex((PokemonAttackDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H43DC8, ReverseHEX(Hex((PokemonAttackDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H43E20, ReverseHEX(Hex((PokemonAttackDataNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H43F84, ReverseHEX(Hex((PokemonAttackDataNewOffset) + &H8000000)))

        'Pokemon Front Sprite table

        PokemonFrontSpriteTableBuffer = ReadHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonFrontSprites", "")), System.Globalization.NumberStyles.HexNumber), ((GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) + 0) * 8)

        'Deletes old data

        If CheckBox2.Checked Then
            WriteHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonFrontSprites", "")), System.Globalization.NumberStyles.HexNumber), MakeFreeSpaceString((Len(PokemonFrontSpriteTableBuffer) / 2)))
        End If

        'Handles Unowns

        If (GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) = "412" Then
            PokemonFrontSpriteTableBuffer = PokemonFrontSpriteTableBuffer & ReadHEX(LoadedROM, ((GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) * 8) + Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonFrontSprites", "")), System.Globalization.NumberStyles.HexNumber), (28 * 8))

        End If

        'Adds new data

        countervar = 0

        While countervar < TextBox1.Text
            countervar = countervar + 1

            PokemonFrontSpriteTableBuffer = PokemonFrontSpriteTableBuffer & "4807D00800080000"

        End While

        PokemonFrontSpriteTableNewOffset = SearchFreeSpaceFourAligned(LoadedROM, &HFF, ((Len(PokemonFrontSpriteTableBuffer) / 2)), "&H" & GetString(GetINIFileLocation(), header, "StartSearchingForSpaceOffset", "800000"))

        WriteHEX(LoadedROM, PokemonFrontSpriteTableNewOffset, PokemonFrontSpriteTableBuffer)

        WriteString(GetINIFileLocation(), header, "PokemonFrontSprites", Hex(PokemonFrontSpriteTableNewOffset))

        'Repoint Pokemon Front Sprite Table

        WriteHEX(LoadedROM, &H000128, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00ECA8, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00ECEC, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00ED68, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00ED80, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00F0F8, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00F130, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00F1AC, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00F1C4, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0343C4, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H034D60, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H03507C, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H046F64, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H050074, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0500BC, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H052068, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0534C4, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0769C0, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0769E4, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H08F470, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CE0B8, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CE2B8, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CE454, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H0CE6C4, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H10BBE4, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H10BBF0, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H11D900, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H14AED8, ReverseHEX(Hex((PokemonFrontSpriteTableNewOffset) + &H8000000)))

        'Pokemon Back Sprite table

        PokemonBackSpriteTableBuffer = ReadHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonBackSprites", "")), System.Globalization.NumberStyles.HexNumber), ((GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) + 0) * 8)

        'Deletes old data

        If CheckBox2.Checked Then
            WriteHEX(LoadedROM, Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonBackSprites", "")), System.Globalization.NumberStyles.HexNumber), MakeFreeSpaceString((Len(PokemonBackSpriteTableBuffer) / 2)))
        End If

        'Handles Unowns

        If (GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) = "412" Then
            PokemonBackSpriteTableBuffer = PokemonBackSpriteTableBuffer & ReadHEX(LoadedROM, ((GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) * 8) + Int32.Parse((GetString(GetINIFileLocation(), header, "PokemonBackSprites", "")), System.Globalization.NumberStyles.HexNumber), (28 * 8))
        End If

        'Adds new data

        countervar = 0

        While countervar < TextBox1.Text
            countervar = countervar + 1

            PokemonBackSpriteTableBuffer = PokemonBackSpriteTableBuffer & "8C09D00800080000"

        End While

        PokemonBackSpriteTableNewOffset = SearchFreeSpaceFourAligned(LoadedROM, &HFF, ((Len(PokemonBackSpriteTableBuffer) / 2)), "&H" & GetString(GetINIFileLocation(), header, "StartSearchingForSpaceOffset", "800000"))

        WriteHEX(LoadedROM, PokemonBackSpriteTableNewOffset, PokemonBackSpriteTableBuffer)

        WriteString(GetINIFileLocation(), header, "PokemonBackSprites", Hex(PokemonBackSpriteTableNewOffset))

        'Repoint Pokemon Back Sprite Table

        WriteHEX(LoadedROM, &H00012C, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00ED58, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H00F19C, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H034540, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H034584, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H034F04, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H076A58, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H076AC8, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H10BC0C, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))
        WriteHEX(LoadedROM, &H10BC24, ReverseHEX(Hex((PokemonBackSpriteTableNewOffset) + &H8000000)))

        'These will break the limiters preventing sprites from showing up.
        WriteHEX(LoadedROM, &HED72, "07E0")
        WriteHEX(LoadedROM, &HF1B6, "07E0")

        'Updates the number of Pokemon
        If (GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) = "412" Then
            WriteString(GetINIFileLocation(), header, "NumberOfPokemon", CInt((GetString(GetINIFileLocation(), header, "NumberOfPokemon", ""))) + 28 + CInt(TextBox1.Text))
        Else
            WriteString(GetINIFileLocation(), header, "NumberOfPokemon", CInt((GetString(GetINIFileLocation(), header, "NumberOfPokemon", ""))) + CInt(TextBox1.Text))
        End If

        Cursor = Cursors.WaitCursor

        Cursor = Cursors.Arrow

        Label1.Text = "Number of Pokemon in ROM: " & (GetString(GetINIFileLocation(), header, "NumberOfPokemon", "")) - 1
        Label2.Text = "Number of Dex entries in ROM: " & (GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1

        MsgBox("Pokemon added successfully!")

    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("http://www.pokecommunity.com/showthread.php?t=318569")
    End Sub

    Private Sub TextBox3_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox3.KeyPress

        If e.KeyChar = "0" Then
            Exit Sub
        End If

        If e.KeyChar = "1" Then
            Exit Sub
        End If

        If e.KeyChar = "2" Then
            Exit Sub
        End If

        If e.KeyChar = "3" Then
            Exit Sub
        End If

        If e.KeyChar = "4" Then
            Exit Sub
        End If

        If e.KeyChar = "5" Then
            Exit Sub
        End If

        If e.KeyChar = "6" Then
            Exit Sub
        End If

        If e.KeyChar = "6" Then
            Exit Sub
        End If

        If e.KeyChar = "7" Then
            Exit Sub
        End If

        If e.KeyChar = "8" Then
            Exit Sub
        End If

        If e.KeyChar = "9" Then
            Exit Sub
        End If

        If e.KeyChar = "A" Then
            Exit Sub
        End If

        If e.KeyChar = "B" Then
            Exit Sub
        End If

        If e.KeyChar = "C" Then
            Exit Sub
        End If

        If e.KeyChar = "D" Then
            Exit Sub
        End If

        If e.KeyChar = "E" Then
            Exit Sub
        End If

        If e.KeyChar = "F" Then
            Exit Sub
        End If

        If e.KeyChar = "a" Then
            e.KeyChar = "A"
            Exit Sub
        End If

        If e.KeyChar = "b" Then
            e.KeyChar = "B"
            Exit Sub
        End If

        If e.KeyChar = "c" Then
            e.KeyChar = "C"
            Exit Sub
        End If

        If e.KeyChar = "d" Then
            e.KeyChar = "D"
            Exit Sub
        End If

        If e.KeyChar = "e" Then
            e.KeyChar = "E"
            Exit Sub
        End If

        If e.KeyChar = "f" Then
            e.KeyChar = "F"
            Exit Sub
        End If

        If e.KeyChar = Convert.ToChar(Keys.Back) Then
            Exit Sub
        End If

        e.KeyChar = ""

    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
        If TextBox3.Text = "" Then
            TextBox3.Text = 0
        End If

        Label6.Text = "Seen flags location: " & Hex(CInt("&H" & TextBox3.Text))
        Label7.Text = "Caught flags location: " & Hex((("&H" & TextBox3.Text) + Math.Ceiling((((GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1) + TextBox2.Text) / 8)))

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged

        If TextBox2.Text = "" Then
            TextBox2.Text = 0
        End If

        If TextBox3.Text <> "" Then

            Label6.Text = "Seen flags location: " & Hex(CInt("&H" & TextBox3.Text))
            Label7.Text = "Caught flags location: " & Hex((("&H" & TextBox3.Text) + Math.Ceiling((((GetString(GetINIFileLocation(), header, "NumberOfDexEntries", "")) - 1) + TextBox2.Text) / 8)))

        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged

        If CheckBox1.Checked = True Then
            TextBox3.Enabled = True
        End If

        If CheckBox1.Checked = False Then
            TextBox3.Enabled = False
        End If

    End Sub
End Class