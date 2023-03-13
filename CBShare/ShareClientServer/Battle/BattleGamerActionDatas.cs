using System;
using System.Collections.Generic;
using CBShare.Common;

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
        public int indexInBattle;
        public BattleGamerAction actionType;
        public float delayTime;
        public string jsonValue;
    }

    public class RollDiceActionParameter
    {
        public bool isSpecial;
    }

    public class BuildHouseActionParameter
    {
        public int blockIndex;
        public HouseCode currentHouse;
        public float discountHouseCostPercent;
    }

    public class UseActionCardActionParameter
    {
        public ActionCardCode actionCard;
    }

    public class UseCharacterSkillActionParameter
    {
        public CharacterCode character;
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


