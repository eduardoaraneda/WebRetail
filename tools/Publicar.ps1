param([switch]$OmitirPruebas)
$ErrorActionPreference='Stop'
$proyectoRaiz=Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $proyectoRaiz
try {
    dotnet restore RetailPOS.slnx
    if($LASTEXITCODE -ne 0){throw 'Falló restore.'}
    dotnet build RetailPOS.slnx --no-restore
    if($LASTEXITCODE -ne 0){throw 'Falló build.'}
    if(-not $OmitirPruebas){
        dotnet run --project tests/RetailPOS.Verificacion --no-build
        if($LASTEXITCODE -ne 0){throw 'Falló verificación; no se publica.'}
    }
    dotnet publish src/RetailPOS.Web -c Release -o artifacts/publish-retail --no-restore
    if($LASTEXITCODE -ne 0){throw 'Falló publish.'}
    Compress-Archive -Path (Join-Path $proyectoRaiz 'artifacts/publish-retail/*') -DestinationPath (Join-Path $proyectoRaiz 'artifacts/RetailPOS.zip') -Force
    Write-Output 'Paquete preparado en artifacts/RetailPOS.zip. Configurar el servidor antes de desplegar.'
}
finally { Pop-Location }


