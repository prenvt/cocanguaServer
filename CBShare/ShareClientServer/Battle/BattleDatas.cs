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
        public BattleType type;
        public BattleLevel level;
        public BattleState state;
        //public float nextStateTime;
        //public BattleState nextState = BattleState.NONE;
        public float elapsedTime;
        public int firstTurnGamerIndex;
        public int turnGamerIndex = -1;
        public int turnCount = 1;
        public List<GamerBattleProperty> gamersPropertiesList = new List<GamerBattleProperty>();
        public DateTime lastUpdateTime;
        public List<int> spacesList;
        public BattleActionData waitingAction { get; set; }

        public void Init()
        {
            this.elapsedTime = 0f;
            this.spacesList = new List<int>();
            for (int i = 0; i < 42; i++)
            {
                this.spacesList.Add(0);
            }
        }

        public void ProcessSortGamersByPoint()
        {
            var sortByAssetGamersList = this.gamersPropertiesList.OrderByDescending(e => e.point).ToList();
            for (int i = 0; i < sortByAssetGamersList.Count; i++)
            {
                var _rankingIndex = i;
                var gamerProperty = this.gamersPropertiesList.Find(e => e.color == sortByAssetGamersList[i].color);
                gamerProperty.rankingIndex = _rankingIndex;
            }
        }
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
        //public string avatar;
        public GamerColor color;
        public int rankingIndex;
        public GamerState state;
        public DiceType currentDice = DiceType.BASIC;
        public List<int> horseSpaceIndexsList = new List<int>() { -1, -1, -1, -1 };
        public int money;
        public int point;
        public bool rematch;
        public List<int> rollDiceValuesList = new List<int>();
        public Dictionary<string, bool> boosterItemsList = new Dictionary<string, bool>();

        public void Init(RoomConfig _roomCfg)
        {
            this.rankingIndex = -1;
            this.state = GamerState.ONLINE; 
            this.point = 0;
        }

        public void ChangePoint(int _deltaPoint)
        {
            this.point += _deltaPoint;
        }

        public int GetFreeHorse()
        {
            for (int i = 0; i < this.horseSpaceIndexsList.Count; i++)
            {
                var horseSpaceIdx = this.horseSpaceIndexsList[i];
                if (horseSpaceIdx < 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}


