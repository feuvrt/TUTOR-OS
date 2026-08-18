using System.Security.Cryptography;

namespace TutorOS;


public class UserAccount
{
    public int Id { get; set; }

    public string Login { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public string Role { get; set; } = "";

    public int? StudentId { get; set; }
}


public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        int iterations = 100000;

        byte[] salt =
            RandomNumberGenerator.GetBytes(16);

        byte[] hash =
            Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                32
            );

        return
            $"{iterations}:" +
            $"{Convert.ToBase64String(salt)}:" +
            $"{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(
        string password,
        string storedPassword)
    {
        string[] parts = storedPassword.Split(':');

        if (parts.Length != 3)
        {
            return false;
        }

        int iterations = int.Parse(parts[0]);

        byte[] salt =
            Convert.FromBase64String(parts[1]);

        byte[] savedHash =
            Convert.FromBase64String(parts[2]);

        byte[] enteredHash =
            Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                32
            );

        return CryptographicOperations.FixedTimeEquals(
            savedHash,
            enteredHash
        );
    }
}