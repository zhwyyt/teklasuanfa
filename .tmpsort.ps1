$rows = @([pscustomobject]@{A=@(1);B=@(2)})
$rows | Sort-Object -Property @{ Expression = { @($_.A).Count }; Descending = $true } -Stable
