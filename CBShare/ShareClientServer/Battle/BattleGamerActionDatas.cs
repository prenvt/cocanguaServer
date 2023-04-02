using System;
using System.Collections.Generic;
using CBShare.Common;
using CBShare.Configuration;

namespace CBShare.Data
{
    public enum BattleGamerAction
    {
        NONE,
        BuyActionCard,
        RollDice,
        BuildHouse,
        UseActionCard,
        UseCharacterSkill,
        SelectBlock,
        SellHouses,
        ExchangeBlocks
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
        public bool isSpecial;
    }

    public class SelectBlockActionParameter
    {
        public List<int> blockIndexsList;
        public SelectBlockActionCode selectAction;
    }

    public class ExchangeBlocksActionParameter
    {
        public List<int> gamer0_BlockIndexsList;
        public List<int> gamer1_BlockIndexsList;
    }

    public class SellHouseActionParameter
    {
        public List<int> blockIndexsList;
        public List<int> defaultBlockIndexsList;
        public int missingTolls;
        public float reduceSellHouseFeesPercent;
    }
}


