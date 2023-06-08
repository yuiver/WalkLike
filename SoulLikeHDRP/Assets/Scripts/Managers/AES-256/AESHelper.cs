using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class AESHelper : MonoBehaviour
{
    public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            if (aes == null)
            {
                Debug.LogError("Failed to create AES instance.");
                return null;
            }

            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = key;
            aes.IV = iv;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                }

                return ms.ToArray();
            }
        }
    }

    public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            if (aes == null)
            {
                Debug.LogError("Failed to create AES instance.");
                return null;
            }

            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = key;
            aes.IV = iv;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                }

                return ms.ToArray();
            }
        }
    }
}
