using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using ConnectUs.Utilities;

using CrudeServer.Providers.Contracts;

namespace CrudeServer.Providers
{
    public class EncryptionProvider : IEncryptionProvider
    {
        private const int EncryptionChunkSize = 256;
        private const int EncryptionKeySize = 4096;

        public string Encrypt(string text, string publicKey)
        {
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = Encrypt(dataToEncrypt, publicKey);

            return Convert.ToBase64String(encryptedBytes);
        }

        public byte[] Encrypt(byte[] dataToEncrypt, string publicKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                byte[] symmetricKey = aes.Key;
                byte[] symmetricIV = aes.IV;

                byte[] encryptedData = EncryptWithAes(dataToEncrypt, symmetricKey, symmetricIV);

                byte[] encryptedSymmetricKey = EncryptWithRsa(symmetricKey, publicKey);
                byte[] encryptedSymmetricIV = EncryptWithRsa(symmetricIV, publicKey);

                byte[] combinedEncryptedData = CombineEncryptedKeyAndData(encryptedSymmetricKey, encryptedSymmetricIV, encryptedData);

                return combinedEncryptedData;
            }
        }

        public byte[] Decrypt(byte[] encryptedBytes, string privateKey)
        {
            (byte[] encryptedKey, byte[] decryptedIV, byte[] encryptedData) keyIVAndData = SeparateEncryptedKeyAndData(encryptedBytes);

            byte[] decryptedKey = DecryptWithRsa(keyIVAndData.encryptedKey, privateKey);
            byte[] decryptedIV = DecryptWithRsa(keyIVAndData.decryptedIV, privateKey);

            byte[] decryptedData = DecryptWithAes(keyIVAndData.encryptedData, decryptedKey, decryptedIV);

            return decryptedData;
        }

        public string Decrypt(string encryptedText, string privateKey)
        {
            byte[] dataToDecrypt = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = Decrypt(dataToDecrypt, privateKey);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public string HashString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            using (SHA512 sha256Hash = SHA512.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private byte[] CombineEncryptedKeyAndData(byte[] encryptedKey, byte[] encryptedIV, byte[] encryptedData)
        {
            // Calculate the total length needed: 4 bytes for each length, plus the lengths of the key, iv, and data.
            byte[] combinedData = new byte[4 + encryptedKey.Length + 4 + encryptedIV.Length + encryptedData.Length];

            // Copy the length and content of the encryptedKey.
            Buffer.BlockCopy(BitConverter.GetBytes(encryptedKey.Length), 0, combinedData, 0, 4);
            Buffer.BlockCopy(encryptedKey, 0, combinedData, 4, encryptedKey.Length);

            // Offset for IV starts after the encryptedKey length and content.
            int ivOffset = 4 + encryptedKey.Length;

            // Copy the length and content of the IV.
            Buffer.BlockCopy(BitConverter.GetBytes(encryptedIV.Length), 0, combinedData, ivOffset, 4);
            Buffer.BlockCopy(encryptedIV, 0, combinedData, ivOffset + 4, encryptedIV.Length);

            // Offset for encryptedData starts after the IV length and content.
            int encryptedDataOffset = ivOffset + 4 + encryptedIV.Length;

            // Copy the encryptedData.
            Buffer.BlockCopy(encryptedData, 0, combinedData, encryptedDataOffset, encryptedData.Length);

            return combinedData;
        }

        private byte[] EncryptWithRsa(byte[] dataToEncrypt, string publicKey)
        {
            IEnumerable<IEnumerable<byte>> chunks = dataToEncrypt.Split(EncryptionChunkSize);

            List<byte> encryptedBytes = new List<byte>();
            byte[][] encryptedChunks = new byte[chunks.Count()][];

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(EncryptionKeySize))
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportCspBlob(Convert.FromBase64String(publicKey));

                Parallel.ForEach(
                    chunks,
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = 32
                    },
                    (bytes, __, index) =>
                    {
                        encryptedChunks[index] = rsa.Encrypt(bytes.ToArray(), RSAEncryptionPadding.Pkcs1);
                    });

                foreach (byte[] decryptedChunk in encryptedChunks)
                {
                    encryptedBytes.AddRange(decryptedChunk);
                }
            }

            return encryptedBytes.ToArray();

        }
        private byte[] DecryptWithRsa(byte[] dataToDecrypt, string privateKey)
        {
            IEnumerable<IEnumerable<byte>> chunks = dataToDecrypt.Split(512).Where(x => x.Count() > 0);
            byte[][] decryptedChunks = new byte[chunks.Count()][];
            List<byte> decryptedBytes = new List<byte>();

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(EncryptionKeySize))
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportCspBlob(Convert.FromBase64String(privateKey));

                Parallel.ForEach(
                    chunks,
                    new ParallelOptions() { MaxDegreeOfParallelism = 16 },
                    (bytes, state, index) =>
                    {
                        decryptedChunks[index] = rsa.Decrypt(bytes.ToArray(), RSAEncryptionPadding.Pkcs1);
                    });

                foreach (byte[] decryptedChunk in decryptedChunks)
                {
                    decryptedBytes.AddRange(decryptedChunk);
                }
            }

            return decryptedBytes.ToArray();
        }

        private byte[] EncryptWithAes(byte[] dataToEncrypt, byte[] symmetricKey, byte[] symmetricIV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = symmetricKey;
                aesAlg.IV = symmetricIV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                        csEncrypt.FlushFinalBlock();
                    }
                    return msEncrypt.ToArray();
                }
            }
        }

        private byte[] DecryptWithAes(byte[] dataToDecrypt, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(dataToDecrypt))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var reader = new MemoryStream())
                {
                    csDecrypt.CopyTo(reader);
                    return reader.ToArray();
                }
            }
        }

        private (byte[] encryptedKey, byte[] encryptedIV, byte[] encryptedData) SeparateEncryptedKeyAndData(byte[] combinedData)
        {
            // Read the length of the encrypted key and extract it.
            int encryptedKeyLength = BitConverter.ToInt32(combinedData, 0);

            byte[] encryptedKey = new byte[encryptedKeyLength];
            Buffer.BlockCopy(combinedData, 4, encryptedKey, 0, encryptedKeyLength);

            // Calculate offset for the IV length and extract the IV.
            int ivOffset = 4 + encryptedKeyLength;
            int ivLength = BitConverter.ToInt32(combinedData, ivOffset);
            byte[] iv = new byte[ivLength];
            Buffer.BlockCopy(combinedData, ivOffset + 4, iv, 0, ivLength);

            // Calculate offset for the encrypted data and extract it.
            int encryptedDataOffset = ivOffset + 4 + ivLength;
            int encryptedDataLength = combinedData.Length - encryptedDataOffset;
            byte[] encryptedData = new byte[encryptedDataLength];
            Buffer.BlockCopy(combinedData, encryptedDataOffset, encryptedData, 0, encryptedDataLength);

            return (encryptedKey, iv, encryptedData);
        }
    }
}
