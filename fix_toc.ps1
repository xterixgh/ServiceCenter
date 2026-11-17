$ErrorActionPreference = "Stop"
$path = "C:\Users\xteri\Desktop\421п_Замалиев_2026_КП_Учёт_заявок.docx"
$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
$doc = $null
try {
    $doc = $word.Documents.Open($path)
    foreach ($field in @($doc.Fields)) {
        $code = ""
        try { $code = $field.Code.Text } catch { $code = "" }
        if ($code -match "PAGEREF") { $field.Delete() }
    }
    $range = $doc.Content
    $found = $range.Find.Execute("СОДЕРЖАНИЕ")
    if (-not $found) { throw "СОДЕРЖАНИЕ не найдено" }
    $insert = $doc.Range($range.End, $range.End)
    $insert.InsertParagraphAfter() | Out-Null
    $insert = $doc.Range($range.End, $range.End)
    $toc = $doc.TablesOfContents.Add($insert, $false, 1, 3, $false, "", $true, $true, "", $true, $true, $true)
    $toc.Update()
    $doc.Save()
    Write-Output "TOC_OK"
}
finally {
    if ($doc -ne $null) { $doc.Close() }
    $word.Quit()
}
