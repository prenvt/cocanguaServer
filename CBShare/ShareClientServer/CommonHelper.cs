using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CBShare.Configuration;


public static class EnumerableExtension
{
    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        return source.PickRandom(1).SingleOrDefault();
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
    }
    public static IList<T> Shuffle<T>(this IList<T> list, Random random)
    {
        if (random == null)
            random = new Random(System.DateTime.Now.Millisecond);
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
    public static IList<T> PickRandom<T>(this IList<T> source, int count, Random random = null)
    {
        return source.Shuffle(random).Take(count).ToList();
    }
    public static T PickRandom<T>(this IList<T> source, Random random)
    {
        return source.PickRandom(1, random).SingleOrDefault();
    }
    public static int GetMaxElementCount<T>(this IEnumerable<T> source, int _numberElementDesireToGet)
    {
        if (source.Count() < _numberElementDesireToGet)
            return source.Count();

        return _numberElementDesireToGet;
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    public static void Shuffle2<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    
}

public class CommonHelper
{
    static Random randomCommonHelper = new Random();

    public static void SetRandomSeedForEachBattle()
    {
        int randomSeed = Guid.NewGuid().GetHashCode();
        randomCommonHelper = new Random(randomSeed);
    }
    public static bool CheckSuccess(float percent)
    {
       return CommonHelper.IsRandomSuccess(CommonHelper.randomCommonHelper, percent);
    }
    public static int Round(decimal p)
    {
        int r = (int)System.Math.Truncate(p);
        return r;
    }

    public static int Round(float p)
    {
        int r = (int) p;
        return r;
    }

    public static int Round(double p)
    {
        return Round((decimal)p);
    }

    public static int RaiseFlag(int giatri, int flag_idx)
    {
        return (giatri | (1 << flag_idx));
    }

    public static bool CheckFlag(int giatri, int flag_idx) //gia ti la database,flag_idx laf client truyen len
    {
        int a;
        return ((giatri & (1 << flag_idx)) != 0);
    }

    public static int DownFlag(int giatri, int flag_idx)
    {
        return (giatri & (~(1 << flag_idx)));
    }

    public static long RaiseFlagLong(long giatri, int flag_idx)
    {
        long flag_val = (long)(1) << flag_idx;
        return (giatri | flag_val);
    }

    public static long DownFlagLong(long giatri, int flag_idx)
    {
        long flag_val = (long)(1) << flag_idx;

        return (giatri & (~(flag_val)));
    }

    public static bool CheckFlagLong(long giatri, int flag_idx) //gia ti la database,flag_idx laf client truyen len
    {
        long flag_val = (long)(1) << (int)(flag_idx);
        long and_val = giatri & flag_val;
        return (and_val != 0);
    }

    public static string Compress(string s)
    {
        //byte[] bytes = CLZF2.Compress(Encoding.Unicode.GetBytes(s));
        //byte[] bytes = SevenZipCompressor.CompressBytes(Encoding.Unicode.GetBytes(s));
        byte[] bytes = LZ4.LZ4Codec.Wrap(Encoding.Unicode.GetBytes(s));

        return Convert.ToBase64String(bytes);
    }

    public static string Compress(byte[] s)
    {
        byte[] bytes = LZ4.LZ4Codec.Wrap(s);
        return Convert.ToBase64String(bytes);
    }

    public static string Decompress(string s)
    {
        //byte[] bytes = CLZF2.Decompress(Convert.FromBase64String(s));
        //byte[] bytes = SevenZipExtractor.ExtractBytes(Convert.FromBase64String(s));

        byte[] bytes = LZ4.LZ4Codec.Unwrap(Convert.FromBase64String(s));

        return Encoding.Unicode.GetString(bytes, 0, bytes.Length);
    }

    public static object GetInstance(string strFullyQualifiedName, params object[] argus)
    {
        Type type = Type.GetType(strFullyQualifiedName);
        if (type != null)
            return Activator.CreateInstance(type, argus);
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type, argus);
        }
        return null;
    }

    public static string GetFullNameBattleClass(string className)
    {
        string fullName = string.Format("CBShare.Battle.{0}", className);
        return fullName;
    }

    public static bool IsRandomSuccess(Random random, double _percent)
    {
        if (_percent <= 0) return false;
        if (_percent >= 100) return true;

        int _randomV = random.Next(0, 100);
        if (_randomV <= _percent)
        {
            return true;
        }

        return false;
    }

    public static bool IsRandomSuccess(float _percent, string logChannel)
    {
        if (_percent <= 0) return false;
        if (_percent >= 100) return true;

        int _randomV = randomCommonHelper.Next(0, 100);

        if (_randomV <= _percent)
        {
            return true;
        }

        return false;
    }

    public static int GetRandomIndex(int max)
    {
        return randomCommonHelper.Next(0, max);
    }

    public static float Claim01(float value)
    {
        if (value < 0) return 0;
        if (value > 1) return 1;

        return value;
    }

    public static double Claim01(double value)
    {
        if (value < 0) return 0;
        if (value > 1) return 1;

        return value;
    }

    public static float ClaimRange(float min, float max, float value)
    {
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }

    public static double ClaimRange(double min, double max, double value)
    {
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }
#if !UNITY_WP8
    public static string md5(string data)
    {
        return BitConverter.ToString(encryptData(data)).Replace("-", "").ToLower();
    }

    private static byte[] encryptData(string data)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashedBytes;
        System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
        hashedBytes = md5Hasher.ComputeHash(encoder.GetBytes(data));
        return hashedBytes;

    }

    public static string GetEncryptedData(long gid, string data)
    {
        return md5(string.Format("Hikergames{0}{1}", gid + 12, data));
    }
#endif

    public static string GetKeyChat(long gid1, long gid2)
    {
        long gidMin = Math.Min(gid1, gid2);
        long gidMax = Math.Max(gid1, gid2);
        return gidMin + "_" + gidMax;
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }
}