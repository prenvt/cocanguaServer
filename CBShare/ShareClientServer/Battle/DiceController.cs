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

        public static int getRollValue(DiceCode dice_id, bool isSpecialRoll)
        {
            int diceValue = 0;
            if (isSpecialRoll)
            {
                if (dice_id == DiceCode.LE)
                {
                    if (num_dice == 1)
                    {
                        int dice_value = RandomUtils.GetRandomInArray(ListOddValues);
                        dice_values = new List<int> { dice_value, 0 };
                    }
                    else if (num_dice == 2)
                    {
                        int value0 = 0, value1 = 0;
                        value0 = RandomUtils.GetRandomInt(1, 6);
                        if (value0 % 2 == 0) value1 = RandomUtils.GetRandomInArray(ListOddValues);
                        else value1 = RandomUtils.GetRandomInArray(ListEvenValues);

                        dice_values = new List<int> { value0, value1 };
                    }
                }
                else if (dice_id == DiceCode.CHAN)
                {
                    if (num_dice == 1)
                    {
                        int dice_value = RandomUtils.GetRandomInArray(ListEvenValues);
                        dice_values = new List<int> { dice_value, 0 };
                    }
                    else if (num_dice == 2)
                    {
                        int value0 = 0, value1 = 0;
                        value0 = RandomUtils.GetRandomInt(1, 6);
                        if (value0 % 2 == 0) value1 = RandomUtils.GetRandomInArray(ListEvenValues);
                        else value1 = RandomUtils.GetRandomInArray(ListOddValues);

                        dice_values = new List<int> { value0, value1 };
                    }
                }
                else if (dice_id == DiceCode.DAI_BAO)
                {
                    if (num_dice == 1)
                    {

                    }
                    else if (num_dice == 2)
                    {
                        int value0 = 0, value1 = 0;
                        value0 = RandomUtils.GetRandomInt(1, 6);
                        value1 = RandomUtils.GetRandomInt(7 - value0, 6);

                        dice_values = new List<int> { value0, value1 };
                    }
                }
                else if (dice_id == DiceCode.TIEU_BAO)
                {
                    if (num_dice == 1)
                    {
                        int dice_value = RandomUtils.GetRandomInt(1, 6);
                        dice_values = new List<int> { dice_value, 0 };
                    }
                    else if (num_dice == 2)
                    {
                        int value0 = 0, value1 = 0;
                        value0 = RandomUtils.GetRandomInt(1, 5);
                        value1 = RandomUtils.GetRandomInt(1, 6 - value0);
                        dice_values = new List<int> { value0, value1 };
                    }
                }
                
            }
            else
            {
                if (num_dice == 1)
                {
                    int dice_value = RandomUtils.GetRandomInt(1, 6);
                    dice_values = new List<int> { dice_value, 0 };
                }
                else if (num_dice == 2)
                {
                    int index = RandomUtils.GetRandomInt(0, ListDiceValues.Count - 1);
                    dice_values = ListDiceValues[index];
                }
            }
            return dice_values;
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
