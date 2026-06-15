using System;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2("admin123", salt, 100000, HashAlgorithmName.SHA256, 32);
        Console.WriteLine(string.Join(":", Convert.ToBase64String(salt), Convert.ToBase64String(hash)));
    }
}
