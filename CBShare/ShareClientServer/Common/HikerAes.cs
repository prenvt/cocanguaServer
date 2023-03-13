using System.Security.Cryptography;
using System.Text;
using Exception = System.Exception;
using Convert = System.Convert;

public class HikerAes
{
    public static HikerAes instance;
    byte[] key;

    public HikerAes(string base64key) : this(Convert.FromBase64String(base64key)) { }
    public HikerAes(byte[] key)
    {
        this.key = key;
        instance = this;
    }
    public HikerAes() : this(
        new byte[] { 133, 144, 214, 125, 13, 129, 178, 197, 15, 211, 72, 0, 248, 76, 241, 228 }
    )
    { }

    static void SetupAes(AesManaged aesAlg)
    {
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.KeySize = 128;
    }
    public static string GenerateIV()
    {
        using (AesManaged aesAlg = new AesManaged())
        {
            SetupAes(aesAlg);
            aesAlg.GenerateIV();
            return Convert.ToBase64String(aesAlg.IV);
        }
    }
    static byte[] EncryptBytes_Aes(byte[] input, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (input == null || input.Length <= 0)
            throw new System.ArgumentNullException("input");
        if (Key == null || Key.Length <= 0)
            throw new System.ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new System.ArgumentNullException("IV");
        byte[] encrypted;

        // Create an AesManaged object
        // with the specified key and IV.
        using (AesManaged aesAlg = new AesManaged())
        {
            SetupAes(aesAlg);
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (System.IO.BinaryWriter swEncrypt = new System.IO.BinaryWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(input);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    static byte[] DecryptBytes_Aes(byte[] cipherData, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherData == null || cipherData.Length <= 0)
            throw new System.ArgumentNullException("cipherData");
        if (Key == null || Key.Length <= 0)
            throw new System.ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new System.ArgumentNullException("IV");

        // Create an AesManaged object
        // with the specified key and IV.
        using (AesManaged aesAlg = new AesManaged())
        {
            SetupAes(aesAlg);
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream(cipherData))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (System.IO.BinaryReader srDecrypt = new System.IO.BinaryReader(csDecrypt))
                    {
                        const int bufferSize = 4096;
                        using (System.IO.MemoryStream outputStream = new System.IO.MemoryStream())
                        {
                            var buffer = new byte[bufferSize];
                            int count;
                            while ((count = srDecrypt.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                outputStream.Write(buffer, 0, count);
                            }
                            return outputStream.ToArray();
                        }
                    }
                }
            }
        }
    }

    static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new System.ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new System.ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new System.ArgumentNullException("IV");
        byte[] encrypted;

        // Create an AesManaged object
        // with the specified key and IV.
        using (AesManaged aesAlg = new AesManaged())
        {
            SetupAes(aesAlg);
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (System.IO.StreamWriter swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new System.ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new System.ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new System.ArgumentNullException("IV");

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an AesManaged object
        // with the specified key and IV.
        using (AesManaged aesAlg = new AesManaged())
        {
            SetupAes(aesAlg);
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (System.IO.StreamReader srDecrypt = new System.IO.StreamReader(csDecrypt))
                    {
                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }


    public static bool Encrypt(string plainText, string iv, out string encrypted)
    {
        if (instance == null)
        {
            instance = new HikerAes();
        }

        try
        {
            var bytes = EncryptStringToBytes_Aes(plainText, instance.key, Convert.FromBase64String(iv));
            encrypted = Convert.ToBase64String(bytes);
            return true;
        }
        catch (Exception)
        {
            encrypted = plainText;
            return false;
        }
    }


    public static bool Decrypt(string encryptedBase64, string iv, out string decrypted)
    {
        if (instance == null)
        {
            instance = new HikerAes();
        }
        try
        {
            decrypted = DecryptStringFromBytes_Aes(Convert.FromBase64String(encryptedBase64), instance.key, Convert.FromBase64String(iv));
            return true;
        }
        catch (Exception)
        {
            decrypted = encryptedBase64;
            return false;
        }
    }

    public static bool EncryptBytes(byte[] input, string iv, out byte[] encrypted)
    {
        if (instance == null)
        {
            instance = new HikerAes();
        }

        try
        {
            var bytes = EncryptBytes_Aes(input, instance.key, Convert.FromBase64String(iv));
            encrypted = bytes;
            return true;
        }
        catch (Exception)
        {
            encrypted = null;
            return false;
        }
    }

    public static bool DecryptBytes(byte[] cipherData, string iv, out byte[] decrypted)
    {
        if (instance == null)
        {
            instance = new HikerAes();
        }
        try
        {
            decrypted = DecryptBytes_Aes(cipherData, instance.key, Convert.FromBase64String(iv));
            return true;
        }
        catch (Exception)
        {
            decrypted = null;
            return false;
        }
    }
}