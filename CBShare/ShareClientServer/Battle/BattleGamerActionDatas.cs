using System;
using System.Collections.Generic;
using CBShare.Common;
using CBShare.Configuration;

namespace CBShare.Data
{
    public enum BattleActionType
    {
        NONE = -1,
        MatchingSuccess = 0,
        StartBattle = 1,
        RollDice = 2,
        ChooseHorse = 3,
    }

    public class BattleActionData
    {
        public GamerColor gamerColor;
        public BattleActionType actionType;
        public float invokeTime;
        //public string jsonParams;
        public bool isInvoked;
    }
}


