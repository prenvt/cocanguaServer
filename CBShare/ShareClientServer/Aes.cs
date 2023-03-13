using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using LZ4;

public class Aes
{
#if !UNITY_WP8
    public static Aes instance;
    private const int CHUNK_SIZE = 128;
    private RijndaelManaged rijndael = new RijndaelManaged();
    private static Encoding encoding = Encoding.UTF8;
    public Aes(string base64key, string base64iv)
    {
        this._create(Convert.FromBase64String(base64key), Convert.FromBase64String(base64iv));
    }
    public Aes(byte[] key, byte[] iv)
    {
        this._create(key, iv);
    }
    public Aes()
    {
        byte[] key =  {3,5,31,2,
                          5,6,3,21,
                          6,23,5,32,
                          42,31,86,32};

        this._create(key, key);
        instance = this;
    }
    private void InitializeRijndael()
    {
        this.rijndael.Mode = CipherMode.CBC;
        this.rijndael.Padding = PaddingMode.PKCS7;
        this.rijndael.KeySize = 128;
        this.rijndael.BlockSize = 128;
    }
    private void _create(byte[] key, byte[] iv)
    {
        this.InitializeRijndael();
        this.rijndael.Key = key;
        this.rijndael.IV = iv;
    }

    

    /*public string Encrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return null;
        }
        ICryptoTransform cryptoTransform = this.rijndael.CreateEncryptor();

        try
        {
            byte[] bytes = Aes.encoding.GetBytes(cipherText);
            byte[] inArray = cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(inArray);
        }
        catch (Exception e)
        {
            return cipherText;
        }

    }*/

    public string Encrypt(string cipherText, bool encrypt = true)
    {
        byte[] inputData = Encoding.UTF8.GetBytes(cipherText);
        
        byte[] compressedData = LZ4Codec.Wrap(inputData);

        if (encrypt)
            compressedData = this.EncryptBytes(compressedData);

        return Convert.ToBase64String(compressedData);
    }

    public byte[] EncryptBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        ICryptoTransform cryptoTransform = this.rijndael.CreateEncryptor();

        try
        {
            return cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            return bytes;
        }
    }

    /*public string Decrypt(string encryptedString)
    {
        if (string.IsNullOrEmpty(encryptedString))
        {
            return null;
        }
        ICryptoTransform cryptoTransform = this.rijndael.CreateDecryptor();

        try
        {
            byte[] array = Convert.FromBase64String(encryptedString);
            byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
            return Aes.encoding.GetString(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            return encryptedString;
        }

    }*/

    public string Decrypt(string compressedStr, bool decrypt = true)
    {
        if (compressedStr == null) return null;
        if (compressedStr.Length == 0) return null;
        byte[] compressedData = Convert.FromBase64String(compressedStr);

        if (decrypt)
            compressedData = this.DecryptBytes(compressedData);

        byte[] inputData = LZ4Codec.Unwrap(compressedData);
        return Encoding.UTF8.GetString(inputData);
    }

    public byte[] DecryptBytes(byte[] encryptedBytes)
    {
        if (encryptedBytes == null || encryptedBytes.Length == 0)
        {
            return null;
        }
        ICryptoTransform cryptoTransform = this.rijndael.CreateDecryptor();

        try
        {
            byte[] bytes = cryptoTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return bytes;
        }
        catch (Exception e)
        {
            return encryptedBytes;
        }
    }
#else
	
//	Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(cipherText, Encoding.UTF8.GetBytes("saltsalt"), 10000);
	byte[] wpKey = {	3,5,31,2,
					5,6,3,21,
					6,23,5,32,
					42,31,86,32};
	public string Decrypt(string encryptedString)
	{
		try{
			string decryptedString = string.Empty;
			
			using (AesManaged aesDecryptor = new AesManaged())
			{
				// 1. AES algorith modification
				aesDecryptor.Key = wpKey;
				aesDecryptor.IV = wpKey;
				
				using (MemoryStream aesMemoryStream = new MemoryStream())
				{
					using (CryptoStream aesCryptoStream = new CryptoStream(aesMemoryStream, aesDecryptor.CreateDecryptor(aesDecryptor.Key,aesDecryptor.IV), CryptoStreamMode.Write))
					{
						// 2. convert from base64 string to bytes and decrypt
						byte[] data = Convert.FromBase64String(encryptedString);
						aesCryptoStream.Write(data, 0, data.Length);
						aesCryptoStream.FlushFinalBlock();
						
						byte[] decryptBytes = aesMemoryStream.ToArray();
						
						// 3. convert decrypted message to string
						decryptedString = Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);
					}
				}
			}
			
			return decryptedString;
		} catch(Exception e){
			return encryptedString;		
		}
	}
	public string Encrypt(string cipherText)
	{
		try{
			string encryptedString = string.Empty;
			
			using (AesManaged aesEncryptor = new AesManaged())
			{
				// 1. AES algorith modification
				aesEncryptor.Key = wpKey;
				aesEncryptor.IV = wpKey;
				
				using (MemoryStream aesMemoryStream = new MemoryStream())
				{
					using (CryptoStream aesCryptoStream = new CryptoStream(aesMemoryStream, aesEncryptor.CreateEncryptor(aesEncryptor.Key,aesEncryptor.IV), CryptoStreamMode.Write))
					{
						// 2. Encrypt data
						byte[] data = Encoding.UTF8.GetBytes(cipherText);
						aesCryptoStream.Write(data, 0, data.Length);
						aesCryptoStream.FlushFinalBlock();
						
						// 3. convert encrypted data to base64 string
						encryptedString = Convert.ToBase64String(aesMemoryStream.ToArray());
					}
				}
			}
	//		EGDebug.Log(cipherText + "=" + Decrypt(encryptedString));
			return encryptedString;
		} catch(Exception e){
			return cipherText;		
		}
	}
#endif
}