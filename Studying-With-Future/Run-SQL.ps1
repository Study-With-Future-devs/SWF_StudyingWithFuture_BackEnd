# ---------------------------------------------
# Run-SQL.ps1 - Executa scripts SQL via Docker
# ---------------------------------------------

Param(
    [string]$ScriptFile
)

# Caminho absoluto do script
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# Caminho do JSON de scripts
$scriptsJsonPath = Join-Path $projectRoot "scripts.json"

if (-Not (Test-Path $scriptsJsonPath)) {
    Write-Host "❌ scripts.json não encontrado em $scriptsJsonPath"
    exit 1
}

# Lê o JSON
$scripts = Get-Content $scriptsJsonPath | ConvertFrom-Json

# Se não passou argumento, pede pra escolher
if (-not $ScriptFile) {
    Write-Host "Escolha um script para rodar:"
    for ($i = 0; $i -lt $scripts.scripts.Count; $i++) {
        Write-Host "$i) $($scripts.scripts[$i].name)"
    }

    $choice = Read-Host "Digite o número"
    if (-Not ($choice -match '^\d+$') -or $choice -ge $scripts.scripts.Count) {
        Write-Host "❌ Opção inválida!"
        exit 1
    }

    $FileName = $scripts.scripts[$choice].file
    $ScriptFile = Join-Path $projectRoot "SQL\$FileName"
} else {
    # Caso tenha passado o caminho
    $ScriptFile = $ScriptFile -replace '/', '\'
}

# Verifica se o arquivo SQL existe
if (-Not (Test-Path $ScriptFile)) {
    Write-Host "❌ Arquivo SQL não encontrado: $ScriptFile"
    exit 1
}

# Configurações do banco
$containerName = "mysql_db"   # nome do container
$dbName = "swf"               # nome do banco
$user = "root"                # usuário do MySQL
$password = ""                # senha do usuário root (ajuste conforme seu container)

# Executa o SQL no container Docker usando pipe
Write-Host "🚀 Executando $ScriptFile no banco $dbName..."
Get-Content $ScriptFile | docker exec -i $containerName mysql -u $user $dbName
Write-Host "✅ Script executado com sucesso!"
