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

    public enum BattleType
    {
        NONE,
        BATTLE_2P = 2,
        BATTLE_3P = 3
    }

    public enum BattleLevel
    {
        NONE,
        Normal,
        Master
    }

    public enum GamerColor
    {
        NONE = -1,
        Blue = 0,
        Red = 1,
        Green = 2
    }

    public class BattleConfig
    {
        public Dictionary<string, float> waitTimes { get; set; }
        public float TIME_MOVE_CHARACTER_PER_STEP { get; set; }
        public int numSpaces;
        public Dictionary<string, int> startIndexs { get; set; }
        public Dictionary<string, int> endIndexs { get; set; }
        public List<int> starIndexs { get; set; }
        public List<int> jumpIndexs { get; set; }
    }

    public enum BoosterItemType
    {
        NONE,
        Luc,
        Sung,
        Khien,
        Sao
    }

    /*public enum SpaceType
    {
        NONE,
        Normal,
        Start,
        Star,
        Jump,
        End
    }

    public class SpaceConfig : BaseConfig
    {
        public int ID;
        public SpaceType type { get { return this.codeName.ToEnum<SpaceType>(); } }
        public BattleTeam team = BattleTeam.NONE;
    }*/
}
