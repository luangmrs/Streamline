param(
    [string]$BaseUrl = "http://localhost",
    [int]$IntervalSeconds = 2
)

$ErrorActionPreference = "Stop"
$base = $BaseUrl.TrimEnd('/')
$targets = @(
    @{ Name = "API raiz"; Url = "$base/" },
    @{ Name = "Swagger"; Url = "$base/swagger/index.html" },
    @{ Name = "Listagem de pedidos"; Url = "$base/api/v1/order?page=1&limit=10" }
)
$notFoundProbe = @{
    Name = "Rota inexistente (esperado 404)"
    Url = "$base/api/v1/__demo-not-found__"
    ExpectedStatus = 404
}
$badQueryProbe = @{
    Name = "Parâmetro inválido (esperado 400)"
    Url = "$base/api/v1/order?page=nao-e-numero&limit=10"
    ExpectedStatus = 400
}

Write-Host "Gerando tráfego em $base. Pressione Ctrl+C para parar."
Write-Host "A listagem de pedidos é somente leitura; ela atualiza o painel de negócio no Grafana."
Write-Host "Também serão geradas respostas 400 e 404 intencionais para demonstrar os códigos no dashboard."

function Invoke-DemoRequest($target, $cycle) {
    $timer = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        $response = Invoke-WebRequest -Uri $target.Url -Method Get -TimeoutSec 10
        $statusCode = [int]$response.StatusCode
        Write-Host ("[{0:HH:mm:ss}] ciclo {1}: {2} -> HTTP {3} ({4} ms)" -f (Get-Date), $cycle, $target.Name, $statusCode, $timer.ElapsedMilliseconds)
    }
    catch {
        $statusCode = "erro"
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        if ($target.ExpectedStatus -and $statusCode -eq $target.ExpectedStatus) {
            Write-Host ("[{0:HH:mm:ss}] ciclo {1}: {2} -> HTTP {3} (esperado, {4} ms)" -f (Get-Date), $cycle, $target.Name, $statusCode, $timer.ElapsedMilliseconds)
        }
        else {
            Write-Warning ("[{0:HH:mm:ss}] ciclo {1}: {2} -> HTTP {3}" -f (Get-Date), $cycle, $target.Name, $statusCode)
        }
    }
    finally {
        $timer.Stop()
    }
}

$cycle = 0
while ($true) {
    $cycle++

    # Mantém as chamadas principais, mas alterna a ordem em cada ciclo.
    foreach ($target in (Get-Random -InputObject $targets -Count $targets.Count)) {
        Invoke-DemoRequest $target $cycle
    }

    # 60% de sondagens válidas, 20% 404 e 20% 400.
    $roll = Get-Random -Minimum 1 -Maximum 101
    if ($roll -le 60) {
        Invoke-DemoRequest (Get-Random -InputObject $targets) $cycle
    }
    elseif ($roll -le 80) {
        Invoke-DemoRequest $notFoundProbe $cycle
    }
    else {
        Invoke-DemoRequest $badQueryProbe $cycle
    }

    Start-Sleep -Seconds ([Math]::Max(1, $IntervalSeconds))
}
