# ---------------------------------------------
# Run-SQL.ps1 - Executa scripts SQL via Docker
# ---------------------------------------------

# Configurações do banco
$containerName = "mysql_db"   # nome do container, ajuste se necessário
$dbName = "swf"               # nome do banco
$user = "root"                # usuário do MySQL
$password = ""          # senha do usuário root (ajuste conforme seu container)
# Caminho do projeto (pasta raiz)
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# Caminho do JSON de scripts
$scriptsJsonPath = Join-Path $projectRoot "scripts.json"

if (-Not (Test-Path $scriptsJsonPath)) {
    Write-Host "❌ scripts.json não encontrado em $scriptsJsonPath"
    exit 1
}

# Lê o JSON
$scripts = Get-Content $scriptsJsonPath | ConvertFrom-Json

# Menu interativo
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

# Caminho do arquivo SQL
$sqlPath = Join-Path $projectRoot "SQL\$FileName"
if (-Not (Test-Path $sqlPath)) {
    Write-Host "❌ Arquivo SQL não encontrado: $sqlPath"
    exit 1
}

# Executa o SQL no container Docker
Write-Host "🚀 Executando $FileName no banco $dbName..."
Get-Content $sqlPath | docker exec -i $containerName mysql -u $user $dbName

Write-Host "✅ Script executado com sucesso!"