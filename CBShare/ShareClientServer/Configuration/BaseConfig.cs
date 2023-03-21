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
        public string codeName;
    }

    public class OtherConfig
    {
        public List<string> UnlockedLanguages;
        public List<string> banDisplayNames;
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

    public class EventConfig
    {
        public string name { get; set; }
        public string description { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public List<RewardConfig> rewards;
    }
}
