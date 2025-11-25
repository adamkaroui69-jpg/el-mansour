# Create Code Signing Certificate for MSIX Package
# Note: For production, use a trusted certificate authority

param(
    [string]$CertificateName = "ElMansourSyndic",
    [string]$Password = "ChangeThisPassword123!"
)

$ErrorActionPreference = "Stop"

Write-Host "Creating code signing certificate..." -ForegroundColor Green

# Create self-signed certificate (for testing only)
# For production, use a certificate from a trusted CA

$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject "CN=$CertificateName" `
    -KeyUsage DigitalSignature `
    -FriendlyName "$CertificateName Code Signing" `
    -CertStoreLocation Cert:\CurrentUser\My `
    -KeyExportPolicy Exportable `
    -KeySpec Signature `
    -KeyLength 2048 `
    -KeyAlgorithm RSA `
    -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddYears(3)

# Export certificate to PFX
$certPath = Join-Path $PSScriptRoot "..\MSIX\$CertificateName.pfx"
$securePassword = ConvertTo-SecureString -String $Password -Force -AsPlainText

Export-PfxCertificate `
    -Cert $cert `
    -FilePath $certPath `
    -Password $securePassword

Write-Host "`nCertificate created: $certPath" -ForegroundColor Green
Write-Host "Password: $Password" -ForegroundColor Yellow
Write-Host "`nWARNING: This is a self-signed certificate for testing only!" -ForegroundColor Red
Write-Host "For production, use a certificate from a trusted Certificate Authority." -ForegroundColor Red

# Install certificate to Trusted Root (for testing)
# Install for CurrentUser
$storeCurrentUser = New-Object System.Security.Cryptography.X509Certificates.X509Store(
    [System.Security.Cryptography.X509Certificates.StoreName]::Root,
    "CurrentUser"
)
$storeCurrentUser.Open("ReadWrite")
$storeCurrentUser.Add($cert)
$storeCurrentUser.Close()
Write-Host "Certificate installed to Trusted Root (CurrentUser)" -ForegroundColor Green

# Install for LocalMachine (requires elevated privileges)
$storeLocalMachine = New-Object System.Security.Cryptography.X509Certificates.X509Store(
    [System.Security.Cryptography.X509Certificates.StoreName]::Root,
    "LocalMachine"
)
$storeLocalMachine.Open("ReadWrite")
$storeLocalMachine.Add($cert)
$storeLocalMachine.Close()
Write-Host "Certificate installed to Trusted Root (LocalMachine)" -ForegroundColor Green
