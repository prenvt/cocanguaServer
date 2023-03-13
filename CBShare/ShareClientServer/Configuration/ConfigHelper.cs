using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CBShare.Battle;
using CBShare.Configuration;
using System.Data;

namespace CBShare
{
    public static class ConfigHelper
    {
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }

        public static List<RewardData> GetListXReward(List<RewardData> src, int x)
        {
            List<RewardData> result = new List<RewardData>();
            foreach (RewardData reward in src)
            {
                result.Add(new RewardData(reward.CodeName, reward.Quantity * x));
            }
            return result;
        }

        public static RewardConfig GetXReward(RewardConfig src, int x)
        {
            if (x <= 1) return src;
            return RewardConfig.XPhanThuong(src, x);
        }

        public static List<int> GetListShuffleIdx(int count) // shuffle list int
        {
            // System.Random random = new System.Random();

            List<int> lstRandomIdx = new List<int>();
            List<int> lstVitriRandomIdx = new List<int>();
            for (int i = 0; i < count; i++)
            {
                lstRandomIdx.Add(i);
            }
            for (int i = 0; i < count; i++)
            {
                // lstRewardsInfo[i]
                int randomIdx = random.Next(0, lstRandomIdx.Count);
                lstVitriRandomIdx.Add(lstRandomIdx[randomIdx]);
                lstRandomIdx.RemoveAt(randomIdx);
            }
            return lstVitriRandomIdx;
        }

        public static T ToEnum<T>(this string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (System.Exception)
            {
                return (T)Enum.Parse(typeof(T), "NONE", true);
            }
        }

        public static string GetPrefix(this string codeName, bool toUpper = true)
        {
            if (!codeName.Contains("_"))
                return "";

            int prefixLength = codeName.IndexOf('_');

            if (prefixLength == 0)
                return "";

            string prefix = codeName.Substring(0, prefixLength);
            if (toUpper)
                prefix = prefix.ToUpper();
            return prefix;
        }

        public static string GetDetail(this string codeName)
        {
            int detailStartPos = codeName.IndexOf('_') + 1;
            string detail = codeName.Substring(detailStartPos);
            return detail;
        }
    }
}
