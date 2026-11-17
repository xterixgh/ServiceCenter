$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$srcGood = "C:\Users\xteri\Desktop\КП_Личная_библиотека_игр_work.docx"
$dst = "C:\Users\xteri\Desktop\КП_Личная_библиотека_игр.docx"
$work = Join-Path $env:TEMP "kp_fix_final"
if (Test-Path $work) { Remove-Item $work -Recurse -Force }
New-Item -ItemType Directory -Path (Join-Path $work "x") | Out-Null
Copy-Item -LiteralPath $srcGood -Destination (Join-Path $work "in.zip") -Force
[System.IO.Compression.ZipFile]::ExtractToDirectory((Join-Path $work "in.zip"), (Join-Path $work "x"))

$docPath = Join-Path $work "x\word\document.xml"
[xml]$xml = Get-Content -LiteralPath $docPath -Encoding UTF8
$ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$ns.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main")
$wNs = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
$body = $xml.SelectSingleNode("//w:body", $ns)

function Get-Text([System.Xml.XmlElement]$p) {
    return ((($p.SelectNodes(".//w:t", $ns) | ForEach-Object { $_."#text" }) -join ""))
}

function Set-Text([System.Xml.XmlElement]$p, [string]$text) {
    $tNodes = @($p.SelectNodes(".//w:t", $ns))
    if ($tNodes.Count -eq 0) { throw "no w:t" }
    $tNodes[0].InnerText = $text
    for ($i = 1; $i -lt $tNodes.Count; $i++) {
        $run = $tNodes[$i].ParentNode
        if ($run -ne $null -and $run.LocalName -eq "r") {
            [void]$run.ParentNode.RemoveChild($run)
        } else {
            $tNodes[$i].InnerText = ""
        }
    }
}

function Insert-Lines([System.Xml.XmlElement]$anchor, [bool]$before, $lines, $sampleBody, $sampleCaption) {
    $created = New-Object "System.Collections.Generic.List[System.Xml.XmlNode]"
    foreach ($line in $lines) {
        $clone = $sampleBody.CloneNode($true)
        foreach ($n in @($clone.SelectNodes(".//*[local-name()='drawing' or local-name()='pict' or local-name()='object' or local-name()='hr']", $ns))) {
            [void]$n.ParentNode.RemoveChild($n)
        }
        $styleNode = $clone.SelectSingleNode("./w:pPr/w:pStyle", $ns)
        if ($styleNode -ne $null) {
            if ($line.Style -eq "CourseCaption") { $styleNode.SetAttribute("val", $wNs, "CourseCaption") }
            else { $styleNode.SetAttribute("val", $wNs, "CourseBody") }
        }
        Set-Text $clone $line.Text
        [void]$created.Add($clone)
    }
    if ($before) {
        foreach ($n in $created) { [void]$anchor.ParentNode.InsertBefore($n, $anchor) }
    } else {
        $ref = $anchor.NextSibling
        foreach ($n in $created) {
            if ($ref -eq $null) { [void]$anchor.ParentNode.AppendChild($n) }
            else { [void]$anchor.ParentNode.InsertBefore($n, $ref) }
        }
    }
}

$toRemove = @()
foreach ($p in @($body.SelectNodes("w:p", $ns))) {
    $t = Get-Text $p
    if ($t.Contains("Метод TrySetScore класса GameRules") -or $t.Contains("Пустая оценка разрешена: игра может лежать")) {
        $toRemove += $p
        Write-Output ("REMOVE " + $t.Substring(0, [Math]::Min(50, $t.Length)))
    }
}
foreach ($p in $toRemove) { [void]$p.ParentNode.RemoveChild($p) }

$paras = @($body.SelectNodes("w:p", $ns))
$sampleBody = $null
$sampleCaption = $null
$marks = @{}
$afterGuide = $false
foreach ($p in $paras) {
    $t = Get-Text $p
    if ($t -eq "5 РУКОВОДСТВО ПОЛЬЗОВАТЕЛЯ") { $afterGuide = $true }
    if (-not $afterGuide) { continue }
    if ($t -eq "ЗАКЛЮЧЕНИЕ") { $marks["concl"] = $p; break }
    $styleNode = $p.SelectSingleNode("./w:pPr/w:pStyle", $ns)
    if ($styleNode -ne $null) {
        $sn = $styleNode.GetAttribute("val", $wNs)
        if ($sn -eq "CourseBody" -and $sampleBody -eq $null -and $t.Length -gt 40) { $sampleBody = $p }
        if ($sn -eq "CourseCaption" -and $sampleCaption -eq $null) { $sampleCaption = $p }
    }
    if ($t.Contains("Новая игра") -and $t.Contains("Название обязательно") -and -not $marks.ContainsKey("newgame")) { $marks["newgame"] = $p }
    elseif ($t.StartsWith("Откройте карточку из списка") -and -not $marks.ContainsKey("opencard")) { $marks["opencard"] = $p }
    elseif ($t.StartsWith("Над списком есть строка поиска") -and -not $marks.ContainsKey("search")) { $marks["search"] = $p }
    elseif ($t.Contains("списке желаемого") -and -not $marks.ContainsKey("wish")) { $marks["wish"] = $p }
    elseif ($t.Contains("Играть") -and $t.Contains("доступна") -and -not $marks.ContainsKey("play")) { $marks["play"] = $p }
    elseif ($t.Contains("Рисунок 5.3") -and -not $marks.ContainsKey("fig53")) { $marks["fig53"] = $p }
}
foreach ($k in @("newgame","opencard","search","wish","play","fig53","concl")) {
    if (-not $marks.ContainsKey($k)) { throw "missing $k" }
    Write-Output ("mark $k")
}

Insert-Lines $marks["concl"] $true @(
    @{ Style = "CourseBody"; Text = "Если сервер PostgreSQL не запущен, вход и сохранение карточек не выполнятся. Запустите службу базы, проверьте, что приложение указывает на нужную базу, и повторите вход. Пока соединение недоступно, введённые на форме данные не нужно набирать заново: исправьте связь с сервером и нажмите сохранение ещё раз." },
    @{ Style = "CourseBody"; Text = "Перед повторным добавлением той же игры просмотрите список и поиск по названию, чтобы не создать дубль. Удаление игры убирает карточку вместе со связанными заметками и сессиями времени, которые к ней относились. Справочное значение при этом не удаляется автоматически: его убирают отдельно, и только если оно больше нигде не используется." },
    @{ Style = "CourseBody"; Text = "Кнопка выхода завершает сеанс и возвращает к форме входа. Библиотека, оценки, заметки и время остаются в PostgreSQL. Чтобы снова открыть список, войдите под тем же логином и паролем." },
    @{ Style = "CourseCaption"; Text = "место для рисунка" },
    @{ Style = "CourseCaption"; Text = "Рисунок 5.10 - Сообщение об ошибке при сохранении или подключении" }
) $sampleBody $sampleCaption

Insert-Lines $marks["fig53"] $false @(
    @{ Style = "CourseBody"; Text = "Числа в статистике считаются только по текущей библиотеке пользователя. Средняя оценка строится по играм, у которых оценка заполнена. Общее время складывается из всех сессий. Блок wishlist показывает, сколько игр отмечено как желаемые, без переноса их в отдельную базу." },
    @{ Style = "CourseBody"; Text = "Если после добавления игры или смены статуса цифры старые, закройте статистику и откройте её снова либо нажмите обновление списка перед повторным входом в раздел. Статистика ничего не записывает: с её экрана нельзя удалить игру или сменить оценку." },
    @{ Style = "CourseCaption"; Text = "место для рисунка" },
    @{ Style = "CourseCaption"; Text = "Рисунок 5.9 - Сводка по статусам, платформам и жанрам" }
) $sampleBody $sampleCaption

Insert-Lines $marks["play"] $false @(
    @{ Style = "CourseBody"; Text = "Перед запуском выберите игру в списке или откройте её карточку. Если путь к исполняемому файлу не указан или указан неверно, запуск не начнётся, а на форме останется сообщение об ошибке. Пока процесс открыт, приложение может учитывать время сессии; после закрытия игры сумма минут на карточке обновляется." },
    @{ Style = "CourseBody"; Text = "Если игра запускается, но минуты не появились, проверьте, что сессия действительно создалась в карточке, и нажмите обновление в списке. Ручное добавление минут не мешает автоматическим сессиям: все записи суммируются. Для нескольких платформ путь запуска задаётся у карточки целиком; отдельный ярлык на каждую платформу в первой версии не требуется." },
    @{ Style = "CourseCaption"; Text = "место для рисунка" },
    @{ Style = "CourseCaption"; Text = "Рисунок 5.8 - Запуск игры из библиотеки" }
) $sampleBody $sampleCaption

Insert-Lines $marks["wish"] $false @(
    @{ Style = "CourseBody"; Text = "В карточке блок заметок хранит произвольный текст пользователя. Чтобы добавить заметку, введите текст и подтвердите сохранение. Пустая заметка не записывается. Лишнюю заметку удаляют из списка заметок этой же карточки. Удаление заметки не трогает название, оценку и время игры." },
    @{ Style = "CourseBody"; Text = "Блок времени показывает сумму минут по всем сессиям. Новую сессию можно добавить вручную, указав длительность, либо получить автоматически после запуска, если путь к файлу задан. Ошибочную сессию удаляют; сумма на карточке после этого пересчитывается." },
    @{ Style = "CourseCaption"; Text = "место для рисунка" },
    @{ Style = "CourseCaption"; Text = "Рисунок 5.7 - Заметки и учёт времени в карточке" }
) $sampleBody $sampleCaption

Insert-Lines $marks["search"] $false @(
    @{ Style = "CourseBody"; Text = "Фильтры можно сочетать: например, платформа PC и статус в процессе. Чтобы вернуть полный список, очистите строку поиска и сбросьте выбранные значения в фильтрах, затем при необходимости нажмите обновление. Пустой результат после фильтра не означает, что база пуста: чаще всего просто не совпало условие. Отметка wishlist работает отдельно от статуса и может использоваться как ещё один способ отобрать игры." },
    @{ Style = "CourseCaption"; Text = "место для рисунка" },
    @{ Style = "CourseCaption"; Text = "Рисунок 5.6 - Поиск и фильтры в списке игр" }
) $sampleBody $sampleCaption

Insert-Lines $marks["opencard"] $true @(
    @{ Style = "CourseBody"; Text = "Если нужно поправить уже сохранённую игру, откройте её из списка. Название по-прежнему обязательно. Год, описание, статус, оценку, платформы, жанры и теги можно менять в любой комбинации. Оценка принимается от 1 до 10; пустое поле оценки допустимо и в среднюю по библиотеке не входит." },
    @{ Style = "CourseBody"; Text = "Платформы, жанры и теги выбираются только из справочника. Если нужного значения нет, закройте карточку, добавьте строку в справочник и откройте игру снова. Кнопка сохранения записывает карточку в PostgreSQL. Закрытие без сохранения оставляет прежние данные." },
    @{ Style = "CourseCaption"; Text = "место для рисунка" },
    @{ Style = "CourseCaption"; Text = "Рисунок 5.5 - Редактирование карточки игры" }
) $sampleBody $sampleCaption

Insert-Lines $marks["newgame"] $true @(
    @{ Style = "CourseBody"; Text = "Справочник открывается отдельным списком. Чтобы добавить значение, введите название и нажмите кнопку сохранения. Пустая строка не записывается. Если такое название уже есть, появится сообщение об ошибке, и дубль не создастся." },
    @{ Style = "CourseBody"; Text = "Чтобы изменить значение, выберите строку в списке, поправьте название и сохраните снова. Удаление сработает только тогда, когда на это значение не ссылается ни одна игра. Если связь есть, запись останется, а на форме появится пояснение. Статусы прохождения правят так же, как платформы и жанры: сначала справочник, потом выбор в карточке." },
    @{ Style = "CourseCaption"; Text = "место для рисунка" },
    @{ Style = "CourseCaption"; Text = "Рисунок 5.4 - Справочник платформ" }
) $sampleBody $sampleCaption

$utf8 = New-Object System.Text.UTF8Encoding $false
$settings = New-Object System.Xml.XmlWriterSettings
$settings.Encoding = $utf8
$settings.OmitXmlDeclaration = $false
$writer = [System.Xml.XmlWriter]::Create($docPath, $settings)
$xml.Save($writer)
$writer.Close()

$out = Join-Path $work "out.docx"
if (Test-Path $out) { Remove-Item $out -Force }
$zip = [System.IO.Compression.ZipFile]::Open($out, "Create")
$root = Join-Path $work "x"
Get-ChildItem -LiteralPath $root -Recurse -File | ForEach-Object {
    $rel = $_.FullName.Substring($root.Length).TrimStart("\").Replace("\", "/")
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $rel, [System.IO.Compression.CompressionLevel]::Optimal)
}
$zip.Dispose()
Copy-Item -LiteralPath $out -Destination $dst -Force
Write-Output "WRITTEN"
