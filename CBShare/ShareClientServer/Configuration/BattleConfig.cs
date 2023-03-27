using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBShare.Data;
using CBShare.Common;

namespace CBShare.Configuration
{
    public class ActionCardConfig
    {
        public string name { get; set; }
        public string description { get; set; }
        public int value { get; set; }
    }

    public class BattleConfig
    {
        public Dictionary<string, float> waitTimes { get; set; }
        public float TIME_MOVE_CHARACTER_PER_STEP { get; set; }
        public List<BlockConfig> blocks;
    }

    public enum BattleTeam
    {
        NONE,
        Blue,
        Red,
        Green
    }

    public enum BlockType
    {
        NONE,
        Normal,
        Start,
        Star,
        Jump,
        End
    }

    public class BlockConfig : BaseConfig
    {
        public BlockType type { get { return this.codeName.ToEnum<BlockType>(); } }
        public List<int> housesCost { get; set; }
        public List<int> housesToll { get; set; }
        public int coupleIndex { get; set; }
        public int coupleVisualIndex { get; set; }
        public List<int> monopolyIndexs { get; set; }

        public int GetHouseCost(HouseCode houseCode)
        {
            if (houseCode == HouseCode.NONE) return 0;
            var houseCost = 0;
            var houseCodeIndex = (int)houseCode;
            if (houseCodeIndex < this.housesCost.Count)
            {
                houseCost = this.housesCost[houseCodeIndex];
            }
            return houseCost;
        }

        public int GetUpgradeHouseCost(HouseCode currentHouseCode, HouseCode nextHouseCode, float discountHouseCostPercent)
        {
            var currentHouseCost = this.GetHouseCost(currentHouseCode);
            var nextHouseCost = this.GetHouseCost(nextHouseCode);
            var upgradeHouseCost = (int)((nextHouseCost - currentHouseCost) * (100f - discountHouseCostPercent) / 100f);
            return upgradeHouseCost;
        }

        public int GetHouseToll(HouseCode houseCode)
        {
            if (houseCode == HouseCode.NONE) return 0;
            var houseToll = 0;
            var houseCodeIndex = (int)houseCode;
            if (houseCodeIndex < this.housesToll.Count)
            {
                houseToll = this.housesToll[houseCodeIndex];
            }
            return houseToll;
        }
    }
}
