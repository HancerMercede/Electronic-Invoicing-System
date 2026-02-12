namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

/// <summary>
/// Service for encrypting and decrypting sensitive data using AES-256.
/// Used to protect certificate passwords, API secrets, and digital certificates.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plain text string using AES-256.
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <returns>Base64 encoded encrypted string.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted string back to plain text.
    /// </summary>
    /// <param name="cipherText">The Base64 encoded encrypted string.</param>
    /// <returns>The decrypted plain text.</returns>
    string Decrypt(string cipherText);

    /// <summary>
    /// Encrypts a byte array (useful for digital certificates).
    /// </summary>
    /// <param name="data">The byte array to encrypt.</param>
    /// <returns>Encrypted byte array.</returns>
    byte[] EncryptBytes(byte[] data);

    /// <summary>
    /// Decrypts a byte array back to its original form.
    /// </summary>
    /// <param name="encryptedData">The encrypted byte array.</param>
    /// <returns>The decrypted byte array.</returns>
    byte[] DecryptBytes(byte[] encryptedData);
}
