using CBShare.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBShare.Battle
{
    public class DiceController
    {
        private static List<List<int>> ListDiceValues = new List<List<int>>
        {
            new List<int>{1, 1}, new List<int>{1, 2}, new List<int>{1, 3}, new List<int>{1, 4}, new List<int>{1, 5}, new List<int>{1, 6},
            new List<int>{2, 1}, new List<int>{2, 2}, new List<int>{2, 3}, new List<int>{2, 4}, new List<int>{2, 5}, new List<int>{2, 6},
            new List<int>{3, 1}, new List<int>{3, 2}, new List<int>{3, 3}, new List<int>{3, 4}, new List<int>{3, 5}, new List<int>{3, 6},
            new List<int>{4, 1}, new List<int>{4, 2}, new List<int>{4, 3}, new List<int>{4, 4}, new List<int>{4, 5}, new List<int>{4, 6},
            new List<int>{5, 1}, new List<int>{5, 2}, new List<int>{5, 3}, new List<int>{5, 4}, new List<int>{5, 5}, new List<int>{5, 6},
            new List<int>{6, 1}, new List<int>{6, 2}, new List<int>{6, 3}, new List<int>{6, 4}, new List<int>{6, 5}, new List<int>{6, 6},
        };

        private static List<List<int>> ListDiceAnimationIndexs0 = new List<List<int>>
        {
            new List<int> { 0, 1 },
            new List<int> { 1, 0 }
        };

        private static List<List<int>> ListDiceAnimationIndexs1 = new List<List<int>>
        {
            new List<int> {0, 0},
            new List<int> {0, 1},
            new List<int> {1, 0},
            new List<int> {1, 1},
        };

        private static List<int> ListOddValues = new List<int> { 1, 3, 5 };
        private static List<int> ListEvenValues = new List<int> { 2, 4, 6 };

        public static int getRollValue(DiceType diceType, bool isSpecialRoll)
        {
            int diceValue = RandomUtils.GetRandomInt(1, 6);
            return diceValue;
        }

        public static List<int> getAnims(bool dice_double)
        {
            List<int> dice_anims = null;
            if (dice_double)
            {
                int index = RandomUtils.GetRandomInt(0, ListDiceAnimationIndexs0.Count - 1);
                dice_anims = ListDiceAnimationIndexs0[index];
            }
            else
            {
                int index = RandomUtils.GetRandomInt(0, ListDiceAnimationIndexs1.Count - 1);
                dice_anims = ListDiceAnimationIndexs1[index];
            }
            return dice_anims;
        }
    }
}
