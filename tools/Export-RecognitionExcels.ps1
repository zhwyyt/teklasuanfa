param(
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$scriptPath = Join-Path $PSScriptRoot 'Export-RecognitionExcels.py'
python $scriptPath $OutputPath
