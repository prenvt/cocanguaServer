using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
/// <summary>
/// Summary description for MD5Hash
/// </summary>
public class MD5Hash
{
    public static MD5 md5Hash = MD5.Create();
    public static string getMd5(string input)
    {
        
        byte[] data = md5Hash.ComputeHash(Encoding.Unicode.GetBytes(input));
        StringBuilder str = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            str.Append(data[i].ToString("X2"));
        }
        return str.ToString();
    }
    public static bool checkMD5Same(string input, string check)
    {
        string temp = getMd5(input);
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        if (0 == comparer.Compare(temp, check))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}