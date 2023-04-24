using System;
using System.Collections.Generic;
using CBShare.Common;
using CBShare.Configuration;

namespace CBShare.Data
{
    public enum BattleGamerAction
    {
        NONE,
        MatchingSuccess,
        BuyBoosterItem,
        RollDice,
    }

    public class BattleGamerActionData
    {
        public GamerColor gamerColor;
        public BattleGamerAction actionType;
        public float delayTime;
        public string jsonValue;
    }

    public class RollDiceActionParameter
    {
        //public bool isSpecial;
    }
}


