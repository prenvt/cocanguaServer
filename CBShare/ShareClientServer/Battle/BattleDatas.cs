using System;
using System.Collections.Generic;
using System.Linq;
using CBShare.Common;
using CBShare.Configuration;

namespace CBShare.Data
{
    public class BattleProperty
    {
        public int ID;
        public RoomTypeCode type;
        public RoomLevelCode level;
        public BattleState state;
        public float nextStateTime;
        public BattleState nextState = BattleState.NONE;
        public float battleTime;
        public MissionCode currentMissionCode;
        public int firstTurnGamerIndex;
        public int turnGamerIndex = -1;
        public List<Block> blocksList;
        public int turnCount = 1;
        public List<GamerBattleProperty> gamersPropertiesList = new List<GamerBattleProperty>();
        public Dictionary<string, bool> chanceCardsList = new Dictionary<string, bool>();
        //public int destBlockIndex;
        public CharacterCode skillCharacter;
        public DateTime lastUpdate;

        public void Init()
        {
            this.battleTime = 0f;
            this.blocksList = new List<Block>();
            for (int i = 0; i < ConfigManager.instance.battleConfig.blocks.Count; i++)
            {
                var blockIndex = i;
                var blockCfg = ConfigManager.instance.GetBlockConfig(blockIndex);
                var block = new Block()
                {
                    index = blockIndex,
                    type = UtilsHelper.ParseEnum<BlockTypeCode>(blockCfg.type),
                    ownerIndex = -1,
                    currentHouseCode = HouseCode.NONE
                };
                this.blocksList.Add(block);
            }

            this.chanceCardsList = new Dictionary<string, bool>();
            foreach (ChanceCardCode chanceCardCode in (ChanceCardCode[])Enum.GetValues(typeof(ChanceCardCode)))
            {
                if (chanceCardCode > ChanceCardCode.NONE)
                {
                    this.chanceCardsList.Add(chanceCardCode.ToString(), false);
                }
            }
        }

        public List<int> GetEmptyHouseBlocks()
        {
            var emptyHouseBlockIndexsList = new List<int>();
            foreach (var block in this.blocksList)
            {
                if (block.type == BlockTypeCode.House && block.ownerIndex < 0)
                {
                    emptyHouseBlockIndexsList.Add(block.index);
                }
            }
            return emptyHouseBlockIndexsList;
        }

        public List<int> GetBlockIndexsByGamer(int gamerIndex)
        {
            var blockIndexsList = new List<int>();
            foreach (var block in this.blocksList)
            {
                if (block.ownerIndex == gamerIndex)
                {
                    blockIndexsList.Add(block.index);
                }
            }
            return blockIndexsList;
        }

        public void ProcessSortGamersByAsset()
        {
            var sortByAssetGamersList = this.gamersPropertiesList.OrderByDescending(e => e.asset).ToList();
            for (int i = 0; i < sortByAssetGamersList.Count; i++)
            {
                var _rankingIndex = i;
                var gamerProperty = this.gamersPropertiesList.Find(e => e.indexInBattle == sortByAssetGamersList[i].indexInBattle);
                gamerProperty.rankingIndex = _rankingIndex;
            }
        }
    }

    public class Block
    {
        public int index { get; set; }
        public BlockTypeCode type { get; set; }
        public HouseCode currentHouseCode { get; set; }
        public int ownerIndex { get; set; }
        public bool isFestival { get; set; }
        public bool isOlympic { get; set; }
        public bool isStarCity { get; set; }
        public bool isPark { get; set; }
        public bool isCouple { get; set; }
        public float tollRateBySkill = 1f;
        public float GetTollRate()
        {
            var _tollRate = 1f;
            for (int i = 0; i < this.tollRatesByTurn.Count; i++)
            {
                var tollRateByTurn = this.tollRatesByTurn[i];
                if (tollRateByTurn.turn > 0)
                {
                    _tollRate *= tollRateByTurn.rate;
                }
            }
            if (this.isOlympic) 
                _tollRate *= ConfigManager.instance.battleConfig.tollRateByOlympic;
            if (this.isFestival) 
                _tollRate *= ConfigManager.instance.battleConfig.tollRateByFestival;
            if (this.isStarCity) 
                _tollRate *= ConfigManager.instance.battleConfig.tollRateByStarCity;
            if (this.isPark) 
                _tollRate *= ConfigManager.instance.battleConfig.tollRateByPark;
            if (this.isCouple) 
                _tollRate *= ConfigManager.instance.battleConfig.tollRateByCouple;
            _tollRate *= this.tollRateBySkill;
            return _tollRate;
        }
        public List<TollRateByTurnData> tollRatesByTurn = new List<TollRateByTurnData>();

        public void DestroyHouses()
        {
            this.ownerIndex = -1;
            this.currentHouseCode = HouseCode.NONE;
            this.tollRatesByTurn.Clear();
            this.isCouple = false;
        }
    }

    public class TollRateByTurnData
    {
        public float rate;
        public int turn;
    }

    public class EndBattleGamerData
    {
        public int indexInBattle;
        public int endCash;
        public int asset;
    }

    public class GamerBattleProperty
    {
        public long gid;
        public string name;
        public string avatar;
        public int indexInBattle;
        public int rankingIndex;
        public GamerState state;
        public int money;
        public int asset;
        public int startCash;
        public int cash;
        public int mana;
        public int betCash;
        public int totalSalary;
        public CharacterCode currentCharacter;
        public DiceCode currentDice;
        public int currentBlockIndex = 0;
        public int missionActionCount;
        public bool rematch;

        public void Init(RoomConfig _roomCfg)
        {
            this.rankingIndex = -1;
            this.state = GamerState.ONLINE; 
            this.mana = 0;
            this.totalSalary = 0;
            this.currentBlockIndex = 0;
            this.missionActionCount = 0;
            this.actionCardsList = new Dictionary<string, bool>();
            this.numDicesByMana = 2;
            this.numDicesByTurn = null;
            this.isRollingDoubleDices = false;
            this.isWaitingUseCharacterSkill = false;
            this.currentTollsNeedPay = 0;
            this.discountHouseCostByActionCard = 0;
            this.discountHouseCostByCharacterSkill = 0;

            this.cash = _roomCfg.startCash;
            this.betCash = _roomCfg.startCash;
            this.asset = _roomCfg.startCash;
        }

        public float GetTollRate()
        {
            var _tollRate = 1f;
            for (int i = 0; i < this.tollRatesByTurn.Count; i++)
            {
                var tollRateByTurn = this.tollRatesByTurn[i];
                if (tollRateByTurn.turn > 0)
                {
                    _tollRate *= tollRateByTurn.rate;
                }
            }
            return _tollRate;
        }
        public int skillTurnCount;
        public List<TollRateByTurnData> tollRatesByTurn = new List<TollRateByTurnData>();
        //public Dictionary<string, float> currentStarCardStatValues { get; set; }
        public StarCardGamerData currentStarCard { get; set; }
        public Dictionary<string, bool> actionCardsList = new Dictionary<string, bool>();
        public ActionCardCode waitingActionCardCode { get; set; }
        public int numDicesByMana = 2;
        public GamerStatByTurn numDicesByTurn;
        public int numDices
        {
            get
            {
                if (this.numDicesByTurn != null && this.numDicesByTurn.turn > 0)
                {
                    return Math.Min(this.numDicesByMana, this.numDicesByTurn.statValue);
                }
                else
                {
                    return this.numDicesByMana;
                }
            }
        }
        public bool isRollingDoubleDices { get; set; }
        public bool isWaitingUseCharacterSkill { get; set; }
        public int currentTollsNeedPay { get; set; }
        public ChanceCardCode testChanceCard = ChanceCardCode.NONE;

        public float GetDiscountHouseCostPercent()
        {
            var discountHouseCostPercentByStarCard = this.currentStarCard.GetStatValue(StarCardStat.ReduceBuildHouseCost);
            return discountHouseCostPercentByStarCard + this.discountHouseCostByCharacterSkill + this.discountHouseCostByActionCard;
        }
        public float discountHouseCostByCharacterSkill = 0;
        public float discountHouseCostByActionCard = 0;

        public void UseMana(ManaCode manaCode, int count = 1)
        {
            var usedMana = ConfigManager.instance.GetManaValue(manaCode) * count;
            this.mana -= usedMana;
            if (this.mana < 0) this.mana = 0;
        }

        public void AddMana(ManaCode manaCode)
        {
            var addMana = ConfigManager.instance.GetManaValue(manaCode);
            this.mana += addMana;
        }

        public void AddStartGift(int giftCash)
        {
            this.cash += giftCash;
            this.asset += giftCash;
        }

        public int AddSalary(int roomSalary)
        {
            float bonusPercentByStarCard = this.currentStarCard.GetStatValue(StarCardStat.IncreaseSalary);
            var addSalary = (int)((100f + bonusPercentByStarCard) * roomSalary / 100f);
            this.AddCash(addSalary);
            this.totalSalary += addSalary;
            this.betCash -= addSalary;
            if (this.betCash < 0) this.betCash = 0;
            return addSalary;
        }

        public void AddCash(int addCash)
        {
            if (addCash < 0) return;
            this.cash += addCash;
            this.asset += addCash;
        }

        public void PayToll(int tollsValue)
        {
            this.cash -= tollsValue;
            this.asset -= tollsValue;
        }

        public void ResetSkillTurnCount()
        {
            var characterCfg = ConfigManager.instance.GetCharacterConfig(this.currentCharacter);
            var reduceSkillTurnCount = (int)this.currentStarCard.GetStatValue(StarCardStat.ReduceSkillTurnCount);
            this.skillTurnCount = characterCfg.skillTurn - reduceSkillTurnCount;
        }
    }

    public class GamerStatByTurn
    {
        public int statValue { get; set; }
        public int turn { get; set; }
    }
}


