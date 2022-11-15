using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Library.Core;

public static class AES
{
    /// <summary>
    /// Decrypts a cipher into a given object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cipherString"></param>
    /// <param name="key"></param>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static bool TryDecrypt<T>(string cipherString, string key, out T result, T defaultValue = default)
    {
        try { result = FromByteArray<T>(DecryptToBytes(cipherString, key)); return true; }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Decrypts a cipher into a given object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cipherString"></param>
    /// <param name="key"></param>
    /// <param name="obj"></param>
    public static T Decrypt<T>(string cipherString, string key) => FromByteArray<T>(DecryptToBytes(cipherString, key));

    /// <summary>
    /// Decrypts a cipher into a string
    /// </summary>
    /// <param name="cipherString"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Decrypt(string cipherString, string key) => Encoding.UTF8.GetString(DecryptToBytes(cipherString, key));

    /// <summary>
    /// Decrypts a file with the option to choose where to save it to
    /// </summary>
    /// <param name="input"></param>
    /// <param name="key"></param>
    /// <param name="saveTo"></param>
    /// <returns></returns>
    public static byte[] DecryptFile(string input, string key, string saveTo = "")
    {
        return DecryptStream(new FileStream(input, FileMode.Open, FileAccess.Read), key, saveTo);
    }

    /// <summary>
    /// Decrypts a stream of data
    /// </summary>
    /// <param name="fs"></param>
    /// <param name="key"></param>
    /// <param name="saveTo"></param>
    /// <returns></returns>
    public static byte[] DecryptStream(FileStream fs, string key, string saveTo = "")
    {
        try
        {
            byte[] decBytes;
            using (fs)
            {
                byte[] inputArr = new byte[fs.Length];
                fs.Read(inputArr, 0, inputArr.Length);
                decBytes = DecryptToBytes(Convert.ToBase64String(inputArr), key);
                if (saveTo?.Length == 0) return decBytes;
            }

            if (File.Exists(saveTo)) File.Delete(saveTo);

            using FileStream decFs = File.Create(saveTo);

            decFs.Write(decBytes, 0, decBytes.Length);

            return decBytes;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Encrypts an object
    /// </summary>
    /// <param name="toEncrypt"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Encrypt(object toEncrypt, string key) => Convert.ToBase64String(EncryptToBytes(ToByteArray(toEncrypt), key));

    /// <summary>
    /// Encrypts a string
    /// </summary>
    /// <param name="toEncrypt"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Encrypt(string toEncrypt, string key) => Convert.ToBase64String(EncryptToBytes(Encoding.UTF8.GetBytes(toEncrypt), key));

    /// <summary>
    /// Encrypts a given file with the option to choose where to save it to
    /// </summary>
    /// <param name="input"></param>
    /// <param name="key"></param>
    /// <param name="saveTo"></param>
    /// <returns></returns>
    public static byte[] EncryptFile(string input, string key, string saveTo = "") => EncryptStream(new FileStream(input, FileMode.Open, FileAccess.Read), key, saveTo);

    /// <summary>
    /// Encrypts a stream of data
    /// </summary>
    /// <param name="fs"></param>
    /// <param name="key"></param>
    /// <param name="saveTo"></param>
    /// <returns></returns>
    public static byte[] EncryptStream(FileStream fs, string key, string saveTo = "")
    {
        try
        {
            byte[] encBytes;
            using (fs)
            {
                byte[] inputArr = new byte[fs.Length];
                fs.Read(inputArr, 0, inputArr.Length);
                encBytes = EncryptToBytes(inputArr, key);
                if (saveTo?.Length == 0) return encBytes;
            }

            if (File.Exists(saveTo)) File.Delete(saveTo);

            using FileStream encFs = File.Create(saveTo);

            encFs.Write(encBytes, 0, encBytes.Length);

            return encBytes;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a byte array to any given object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static T FromByteArray<T>(byte[] data)
    {
        return data == null ? default : JsonSerializer.Deserialize<T>(data);
    }

    /// <summary>
    /// Converts an object to a byte array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static byte[] ToByteArray<T>(T obj)
    {
        if (obj == null) return null;

        var json = JsonSerializer.Serialize(obj);
        return Encoding.UTF8.GetBytes(json);
    }

    /// <summary>
    /// Decrypts the string from bytes aes.
    /// </summary>
    /// <param name="cipherTextCombined">The cipher text combined.</param>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    private static byte[] DecryptToBytes(string cipherString, string key)
    {
        var cryptBytes = Convert.FromBase64String(cipherString);
        var keyArray = MD5.HashData(Encoding.UTF8.GetBytes(key));

        // Create an Aes object
        // with the specified key and IV.
        using Aes aes = Aes.Create();
        aes.Key = keyArray;

        var IV = new byte[aes.BlockSize / 8];
        var cipherText = new byte[cryptBytes.Length - IV.Length];

        Array.Copy(cryptBytes, IV, IV.Length);
        Array.Copy(cryptBytes, IV.Length, cipherText, 0, cipherText.Length);

        aes.IV = IV;
        aes.Mode = CipherMode.CBC;

        // Create a decrytor to perform the stream transform.
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write);

        cryptoStream.Write(cipherText, 0, cipherText.Length);
        cryptoStream.Close();

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Encrypts the string to bytes aes.
    /// </summary>
    /// <param name="plainText">The plain text.</param>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    private static byte[] EncryptToBytes(byte[] toEncryptArray, string key)
    {
        byte[] IV;
        byte[] encrypted;
        var keyArray = MD5.HashData(Encoding.UTF8.GetBytes(key));

        using (var aes = Aes.Create())
        {
            aes.Key = keyArray;

            aes.GenerateIV();
            IV = aes.IV;

            aes.Mode = CipherMode.CBC;

            encrypted = aes.CreateEncryptor().TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        }

        var combinedIvCt = new byte[IV.Length + encrypted.Length];
        Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
        Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

        // Return the encrypted bytes from the memory stream.
        return combinedIvCt;
    }
}