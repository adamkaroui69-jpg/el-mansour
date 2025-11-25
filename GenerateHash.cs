using System;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        string password = "123456";
        var (hash, salt) = HashPassword(password);
        
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
        Console.WriteLine($"Salt: {salt}");
        
        // Test verification
        bool isValid = VerifyPassword(password, hash, salt);
        Console.WriteLine($"Verification: {isValid}");
    }
    
    private static (string Hash, string Salt) HashPassword(string password)
    {
        const int SaltSize = 32;
        const int HashSize = 32;
        const int PBKDF2Iterations = 10000;
        
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hash = PBKDF2(password, salt, PBKDF2Iterations, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }
    
    private static bool VerifyPassword(string password, string hash, string salt)
    {
        const int HashSize = 32;
        const int PBKDF2Iterations = 10000;
        
        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hashBytes = Convert.FromBase64String(hash);
            var computedHash = PBKDF2(password, saltBytes, PBKDF2Iterations, HashSize);
            return SlowEquals(hashBytes, computedHash);
        }
        catch
        {
            return false;
        }
    }
    
    private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(outputBytes);
    }
    
    private static bool SlowEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];
        return diff == 0;
    }
}
