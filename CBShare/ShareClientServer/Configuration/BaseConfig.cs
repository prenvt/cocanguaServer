using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBShare.Data;
using CBShare.Common;

namespace CBShare.Configuration
{
    [Serializable]
    public class BaseConfig
    {
        public string CodeName;
    }

    public class OtherConfig
    {
        public List<string> UnlockedLanguages;
        public List<string> banDisplayNames;
    }

    public class StarCardConfig
    {
        public string name { get; set; }
        public StarCardRank rank { get { return UtilsHelper.ParseEnum<StarCardRank>(this.name.Split('|')[0]); } }
        public CharacterCode characterCode { get { return UtilsHelper.ParseEnum<CharacterCode>(this.name.Split('|')[1]); } }
        public int maxLevel { get { return Int32.Parse(this.name.Split('|')[2]); } }
        public int sellCost;
        public int upgradeCost;
        public float skillValue;
        public float upgradeChance;
        public Dictionary<string, List<float>> statsValues;
    }

    public class StarCardPackConfig
    {
        public string packName { get; set; }
        public Dictionary<string, int> buyCost { get; set; }
        public Dictionary<string, float> spawnCardByRankRates { get; set; }
    }

    public class CharacterConfig
    {
        public int skillTurn;
    }

    public class DiceConfig
    {
        public int skillTurn { get; set; }
    }

    public class RoomConfig
    {
        public int multiply { get; set; }
        public int limitAsset { get; set; }
        public int startCash { get; set; }
        public int salary { get; set; }
        public List<int> actionCardCosts { get; set; }
        public int minJewel { get; set; }
        public int commission { get; set; }
        public int maxTurn { get; set; }

    }

    public class MissionConfig
    {
        public string name { get; set; }
        public string description { get; set; }
        public int time { get; set; }
        public float parameter { get; set; }
        public object data { get; set; }
    }

    public class ManaBoosterConfig
    {
        public string name { get; set; }
        public string description { get; set; }
        public int manaValue { get; set; }
        public Dictionary<string, int> costByRoomLevels { get; set; }
    }

    public class GiftConfig
    {
        public Dictionary<string, int> buyCost { get; set; }
    }

    public class JewelPackConfig
    {
        public long jewelValue { get; set; }
        public long bonus { get; set; }
        public int realMoneyCost { get; set; }
    }

    /*public class MoneyPackConfig
    {
        public int moneyValue { get; set; }
        public int jewelCost { get; set; }
    }*/

    public class ShopItemConfig
    {
        public string codeName;
        public int quantity;
        public Dictionary<string, int> price;
        public float iapPrice;
    }

    public class ShopConfig
    {
        public List<ShopItemConfig> characterPacksList;
        public List<ShopItemConfig> dicePacksList;
        public List<ShopItemConfig> starCardPacksList;
        public List<ShopItemConfig> emoticonPacksList;
        public List<ShopItemConfig> moneyPacksList;
        public List<ShopItemConfig> jewelPacksList;
        public List<ShopItemConfig> buyByHeartPacksList;
    }

    /*public class RewardConfig
    {
        public string name { get; set; }
        public RewardCode code { get { return Utils.ParseEnum<RewardCode>(this.name); } }
        public int value { get; set; }
        public int parameter { get; set; }
    }*/

    public class GoldHeartRewardConfig
    {
        public int numGoldHeart { get; set; }
        public List<RewardConfig> rewards { get; set; }
    }

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
        public float tollRateByOlympic = 3f;
        public float tollRateByFestival = 2f;
        public float tollRateByStarCity = 2f;
        public float tollRateByPark = 2f;
        public float tollRateByCouple = 2f;
        public Dictionary<string, MissionConfig> missions;
        public Dictionary<string, int> manas;
        public List<BlockConfig> blocks;
    }

    public class BlockConfig
    {
        public string name { get; set; }
        public string type { get; set; }
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

    public class EventConfig
    {
        public string name { get; set; }
        public string description { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public List<RewardConfig> rewards;
    }
}
