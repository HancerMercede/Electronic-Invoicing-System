using System.Security.Cryptography;
using System.Text;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using Microsoft.Extensions.Configuration;

namespace ElectronicInvoicing.Infrastructure.Services;

/// <summary>
/// Implements AES-256 encryption for sensitive data.
/// Uses a secure encryption key from configuration.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _encryptionKey;

    public EncryptionService(IConfiguration configuration)
    {
        // Get encryption key from configuration
        var keyString = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption key is not configured. Please set 'Encryption:Key' in appsettings.json");

        // Ensure key is 32 bytes (256 bits) for AES-256
        _encryptionKey = DeriveKey(keyString, 32);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();

        // Write IV to the beginning of the stream
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        // Extract IV from the beginning of the cipher
        var iv = new byte[aes.IV.Length];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    public byte[] EncryptBytes(byte[] data)
    {
        if (data == null || data.Length == 0)
            return data;

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();

        // Write IV to the beginning
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            csEncrypt.Write(data, 0, data.Length);
        }

        return msEncrypt.ToArray();
    }

    public byte[] DecryptBytes(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return encryptedData;

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        // Extract IV from the beginning
        var iv = new byte[aes.IV.Length];
        Array.Copy(encryptedData, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var msOutput = new MemoryStream();

        csDecrypt.CopyTo(msOutput);
        return msOutput.ToArray();
    }

    /// <summary>
    /// Derives a cryptographic key of specified length from a passphrase using PBKDF2.
    /// </summary>
    private static byte[] DeriveKey(string passphrase, int keyLength)
    {
        // Use a static salt (in production, consider Azure Key Vault or environment-specific salt)
        var salt = Encoding.UTF8.GetBytes("ElectronicInvoicing.DGII.2026");

        using var deriveBytes = new Rfc2898DeriveBytes(
            passphrase,
            salt,
            100000, // 100k iterations
            HashAlgorithmName.SHA256
        );

        return deriveBytes.GetBytes(keyLength);
    }
}
