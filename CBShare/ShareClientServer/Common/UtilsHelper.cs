using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Globalization;

namespace CBShare.Common
{
    public class UtilsHelper
    {
        /*public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }*/

        public static string CreateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static string CreateRequestSecretKey(string requestTime)
        {
            string input = string.Format("{0}-{1}", requestTime, Const.APP_SECRET_KEY);
            return MD5Hash.getMd5(input);
        }

        /*public static string GetTimeString(double sec, int maxNum = -1)
        {
            string str = "";
            int day = (int)Math.Floor(sec / (24 * 3600));
            if (day > 0)
            {
                str += string.Format(Localization.Get("daysFormat"), day);
            }
            sec -= day * (24 * 3600);
            int h = (int)Math.Floor(sec / 3600);
            if (h > 0)
            {
                if (day > 0) str += " ";
                str += string.Format(Localization.Get("hoursFormat"), h);
            }
            sec -= h * 3600;
            int m = (int)Math.Floor(sec / 60);
            if (m > 0)
            {
                if (day > 0 || h > 0) str += " ";
                str += string.Format(Localization.Get("minutesFormat"), m);
            }
            sec -= m * 60;

            int s = (int)Math.Floor(sec);
            if (s >= 0 && day == 0)
            {
                str += " ";
                str += string.Format(Localization.Get("secondsFormat"), s);
            }

            if (maxNum != -1)
            {
                string[] list = str.Split(' ');
                if (list.Length <= maxNum) return str;
                str = "";
                for (int i = 0; i < maxNum; i++)
                {
                    str += list[i];
                    if (i < maxNum - 1) str += " ";
                }
            }

            return str;
        }*/

        public static int GetWeekNumberOfYear(DateTime date)
        {
            var currentCulture = CultureInfo.CurrentCulture;
            return currentCulture.Calendar.GetWeekOfYear(
                            date,
                            currentCulture.DateTimeFormat.CalendarWeekRule,
                            currentCulture.DateTimeFormat.FirstDayOfWeek);
        }
    }

    public static class DateTimeExtension
    {
        public static DateTime Trim(this DateTime date, long roundTick)
        {
            return new DateTime(date.Ticks - date.Ticks % roundTick, date.Kind);
        }
    }
}
