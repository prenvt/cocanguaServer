using System;
using System.Collections.Generic;
using CBShare.Common;
using CBShare.Configuration;

namespace CBShare.Data
{
    public enum BattleActionType
    {
        NONE,
        MatchingSuccess,
        StartBattle,
        RollDice,
    }

    public class BattleActionData
    {
        public GamerColor gamerColor;
        public BattleActionType actionType;
        public float invokeTime;
        public string jsonParams;
        public bool isInvoked;
    }

    public class RollDiceActionParameter
    {
        //public bool isSpecial;
    }
}


