using System.Collections;
using System.Text;
using LZ4;
using System.Text.RegularExpressions;
using Base64FormattingOptions = System.Base64FormattingOptions;
using Convert = System.Convert;

public class ReadText
{
    public static bool IsBase64String(string s)
    {
        s = s.Trim();
        return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
    }

    public static string DecompressString(string compressedStr, string iv, bool decrypt = false)
    {
        if (compressedStr == null) return null;
        if (compressedStr.Length == 0) return null;

        byte[] compressedData = Convert.FromBase64String(compressedStr);
        if (decrypt)
        {
            if (HikerAes.DecryptBytes(compressedData, iv, out byte[] decrypted))
            {
                compressedData = decrypted;
            }
            else
            {
                throw new System.Exception("Fail decrypt data");
            }
        }
        byte[] inputData = LZ4Codec.Unwrap(compressedData);
        return Encoding.UTF8.GetString(inputData);
    }

    public static string CompressString(string str, string iv, bool encrypt = false)
    {
        byte[] inputData = Encoding.UTF8.GetBytes(str);
        byte[] compressedData = LZ4Codec.Wrap(inputData);
        if (encrypt)
        {
            if (HikerAes.EncryptBytes(compressedData, iv, out byte[] encrypted))
            {
                compressedData = encrypted;
            }
            else
            {
                throw new System.Exception("Fail encrypt data");
            }
        }
        return Convert.ToBase64String(compressedData, Base64FormattingOptions.None);
    }
}
