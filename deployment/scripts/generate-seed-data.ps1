# Generate Seed Data with Password Hashes
# This script generates the seed data SQL with proper password hashes

param(
    [string]$AdminCode = "123456",
    [string[]]$MemberCodes = @("111111", "222222", "333333", "444444")
)

$ErrorActionPreference = "Stop"

Write-Host "Generating seed data with password hashes..." -ForegroundColor Green

# Import AuthenticationService to generate hashes
# Note: This requires the compiled application or a separate hash generation tool

function GeneratePasswordHash {
    param([string]$code)
    
    # Use PBKDF2 with 100,000 iterations (same as AuthenticationService)
    $salt = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($salt)
    
    $pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes(
        [System.Text.Encoding]::UTF8.GetBytes($code),
        $salt,
        100000,
        [System.Security.Cryptography.HashAlgorithmName]::SHA256
    )
    
    $hash = $pbkdf2.GetBytes(32)
    $pbkdf2.Dispose()
    
    return @{
        Hash = [Convert]::ToBase64String($hash)
        Salt = [Convert]::ToBase64String($salt)
    }
}

# Generate hashes
Write-Host "Generating password hashes..." -ForegroundColor Yellow
$adminHash = GeneratePasswordHash -code $AdminCode
$memberHashes = $MemberCodes | ForEach-Object { GeneratePasswordHash -code $_ }

# Generate SQL
$sql = @"
-- Admin User
INSERT INTO users (id, house_code, name, surname, password_hash, salt, role, is_active) VALUES
    ('30000000-0000-0000-0000-000000000001', 'B-SYNDIC', 'Admin', 'El Mansour', 
     '$($adminHash.Hash)', '$($adminHash.Salt)', 'Admin', true)
ON CONFLICT (house_code) DO UPDATE SET
    password_hash = EXCLUDED.password_hash,
    salt = EXCLUDED.salt;

-- Syndic Members
INSERT INTO users (id, house_code, name, surname, password_hash, salt, role, is_active) VALUES
"@

$memberIndex = 0
$houseCodes = @('A01', 'B01', 'C01', 'D01')
foreach ($hash in $memberHashes) {
    $memberIndex++
    $houseCode = $houseCodes[$memberIndex - 1]
    $userId = "30000000-0000-0000-0000-00000000000$($memberIndex + 1)"
    
    $sql += @"
    ('$userId', '$houseCode', 'Membre', 'Syndic $memberIndex', 
     '$($hash.Hash)', '$($hash.Salt)', 'SyndicMember', true),
"@
}

$sql = $sql.TrimEnd(',')
$sql += @"
ON CONFLICT (house_code) DO UPDATE SET
    password_hash = EXCLUDED.password_hash,
    salt = EXCLUDED.salt;
"@

# Save to file
$outputPath = Join-Path $PSScriptRoot "..\seed-data\seed-users.sql"
$sql | Out-File -FilePath $outputPath -Encoding UTF8

Write-Host "`nSeed data generated: $outputPath" -ForegroundColor Green
Write-Host "`nAdmin Code: $AdminCode" -ForegroundColor Cyan
Write-Host "Member Codes:" -ForegroundColor Cyan
for ($i = 0; $i -lt $MemberCodes.Length; $i++) {
    Write-Host "  Member $($i+1): $($MemberCodes[$i])" -ForegroundColor Cyan
}

