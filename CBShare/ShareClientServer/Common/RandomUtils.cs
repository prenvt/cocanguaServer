using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBShare.Common
{
    public class RandomUtils
    {
        private static Random random = new Random();

        public static int GetRandomInt(int min, int max)
        {
            return random.Next(min, max);
        }

        public static float GetRandomFloat()
        {
            return (float)random.NextDouble();
        }

        public static bool RandomWithChance(float chance)
        {
            return random.NextDouble() < chance;
        }

        public static int GetRandomInArray(List<int> val)
        {
            /*List<int> val = new List<int>();
            for (int i = 0; i < source.Length; i++)
            {
                val.Add(source[i]);
            }*/
            int rand_index = GetRandomInt(0, val.Count - 1);
            int rand_value = val[rand_index];
            return rand_value;
        }

        public static int GetRandomIndexInList(List<float> chances)
        {
            float total = 0;
            foreach (float c in chances)
            {
                total += c;
            }
            float rg = RandomUtils.GetRandomFloat() * total;
            int index = 0;
            float checkChance = 0;
            foreach (float c in chances)
            {
                checkChance += c;
                if (rg < checkChance) return index;
                index++;
            }
            return -1;
        }

        public static int GetRandomIndexInList(int count)
        {
            var index = random.Next(count);
            return index;
        }

        public static List<int> GetRandomIndexInList(int count, int numRands)
        {
            if (numRands > count) return null;
            List<int> valueList = new List<int>();
            List<int> returnList = new List<int>();
            for (int i = 0; i < count; i++)
            {
                valueList.Add(i);
            }
            for (int i = 0; i < numRands; i++)
            {
                int k = random.Next(0, valueList.Count - 1);
                returnList.Add(valueList[k]);
                valueList.RemoveAt(k);
            }
            return returnList;
        }

        public static int GetRandomWithExcepts(int high, int[] excepts)
        {
            List<int> val = new List<int>();
            for (int i = 0; i <= high; i++)
            {
                if (Array.IndexOf(excepts, i) < 0) val.Add(i);
            }
            int rand_index = GetRandomInt(0, val.Count - 1);
            int rand_value = val[rand_index];
            return rand_value;
        }
    }
}
