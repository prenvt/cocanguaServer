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
                    type = UtilsHelper.ParseEnum<BlockType>(blockCfg.type),
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
                if (block.type == BlockType.House && block.ownerIndex < 0)
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

        public void ProcessSortGamersByPoint()
        {
            var sortByAssetGamersList = this.gamersPropertiesList.OrderByDescending(e => e.point).ToList();
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
        public BlockType type { get; set; }
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
        public int point;
        public CharacterCode currentCharacter;
        public DiceCode currentDice;
        public int currentBlockIndex = 0;
        public bool rematch;

        public void Init(RoomConfig _roomCfg)
        {
            this.rankingIndex = -1;
            this.state = GamerState.ONLINE; 
            this.point = 0;
            this.currentBlockIndex = 0;
        }

        public void ChangePoint(int _deltaPoint)
        {
            this.point += _deltaPoint;
        }
    }
}


