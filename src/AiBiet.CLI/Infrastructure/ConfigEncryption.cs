using System.Security.Cryptography;
using System.Text;

namespace AiBiet.CLI.Infrastructure;

internal static class ConfigEncryption
{
    private static readonly byte[] Key = GenerateKey();

    private static byte[] GenerateKey()
    {
        var seed = Environment.MachineName + Environment.UserName + "AiBiet-Config-v1";
        return SHA256.HashData(Encoding.UTF8.GetBytes(seed));
    }

    public static string? Encrypt(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
        
        return Convert.ToBase64String(result);
    }

    public static string? Decrypt(string? cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            var fullBytes = Convert.FromBase64String(cipherText);
            
            using var aes = Aes.Create();
            aes.Key = Key;
            
            var iv = new byte[aes.IV.Length];
            Buffer.BlockCopy(fullBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = new byte[fullBytes.Length - iv.Length];
            Buffer.BlockCopy(fullBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);
            
            var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return cipherText;
        }
    }
}
