# builds and runs the calculator sample
set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# requires wasmtime with wasi and http support
# winget install --id BytecodeAlliance.Wasmtime
#
# Optional: set GITHUB_OWNER and GITHUB_REPO to fetch a specific repository's JSON metadata.
# Defaults (if unset): bytecodealliance / componentize-dotnet

# Build the component projects (Adder provides the implementation; CalculatorComposed composes host + adder)
Write-Host "Building Adder..." -ForegroundColor Cyan
dotnet build $PSScriptRoot\Adder | Write-Host
Write-Host "Building CalculatorComposed (will compose wasm)..." -ForegroundColor Cyan
dotnet build $PSScriptRoot\CalculatorComposed | Write-Host
if (-not (Test-Path "$PSScriptRoot\CalculatorComposed\dist\calculator.wasm")) {
	Write-Error "Composition failed: dist/calculator.wasm not found"
}

# Run with wasmtime (needs http since GetRepo performs an outbound HTTP GET)
$wasm = "$PSScriptRoot\CalculatorComposed\dist\calculator.wasm"
Write-Host "Running component: $wasm" -ForegroundColor Green
wasmtime --wasi http $wasm
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
