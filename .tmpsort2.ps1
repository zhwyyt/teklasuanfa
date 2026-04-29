$rows = @([pscustomobject]@{A=2;B='b'},[pscustomobject]@{A=1;B='a'})
$rows | Sort-Object @{ Expression = 'A'; Descending = $true }, @{ Expression = 'B'; Descending = $false }
