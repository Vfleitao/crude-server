namespace CrudeServer.Providers.Contracts
{
    public interface IEncryptionProvider
    {
        byte[] Decrypt(byte[] encryptedBytes, string privateKey);
        string Decrypt(string encryptedText, string privateKey);
        byte[] Encrypt(byte[] dataToEncrypt, string publicKey);
        string Encrypt(string text, string publicKey);
        string HashString(string text);
    }
}