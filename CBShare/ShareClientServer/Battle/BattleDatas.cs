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
        public RoomType type;
        public RoomLevel level;
        public BattleState state;
        public float nextStateTime;
        public BattleState nextState = BattleState.NONE;
        public float battleTime;
        public int firstTurnGamerIndex;
        public int turnGamerIndex = -1;
        public int turnCount = 1;
        public List<GamerBattleProperty> gamersPropertiesList = new List<GamerBattleProperty>();
        public DateTime lastUpdate;

        public void Init()
        {
            this.battleTime = 0f;
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
        public string avatar;
        public BattleColor color;
        public int rankingIndex;
        public GamerState state;
        public DiceType currentDice = DiceType.BASIC;
        public List<int> horseSpaceIndexsList = new List<int>() { -1, -1, -1, -1 };
        public int money;
        public int point;
        public bool rematch;

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
    }
}


