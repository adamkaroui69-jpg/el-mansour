using System;
using System.Security.Cryptography;
using System.Text;

public class GeneratePasswordHash
{
    private const int PBKDF2Iterations = 10000;
    private const int SaltSize = 32;
    private const int HashSize = 32;

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run GeneratePasswordHash.cs <password>");
            return;
        }

        string password = args[0];
        var (hash, salt) = HashPassword(password);

        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
        Console.WriteLine($"Salt: {salt}");
    }

    private static (string Hash, string Salt) HashPassword(string password)
    {
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hash = PBKDF2(password, salt, PBKDF2Iterations, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(outputBytes);
    }
}
