param([ValidateSet('Development','Production')][string]$Entorno='Development',[int]$ProbarId=0)
$ErrorActionPreference='Stop'
$proyectoRaiz=Split-Path -Parent $PSScriptRoot
$entornoAnterior=$env:ASPNETCORE_ENVIRONMENT
Push-Location -LiteralPath $proyectoRaiz
try {
    $env:ASPNETCORE_ENVIRONMENT=$Entorno
    if($ProbarId -gt 0){dotnet run --project src/RetailPOS.Web --no-launch-profile --no-build -- --probar-respaldo $ProbarId}
    else{dotnet run --project src/RetailPOS.Web --no-launch-profile --no-build -- --respaldar}
    if($LASTEXITCODE -ne 0){throw 'No se completó la operación de respaldo.'}
}
finally {$env:ASPNETCORE_ENVIRONMENT=$entornoAnterior;Pop-Location}

