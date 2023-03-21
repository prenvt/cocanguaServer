namespace CBShare.Common
{
    public enum Platform
    {
        STANDALONE,
        ANDROID,
        IOS
    }

    public enum CharacterCode
    {
        NONE,
        PHI_CONG,
        DOANH_NHAN,
        CO_GAI,
        TEN_TROM,
        ELON_MUSK,
        DONAL_TRUMP,
        RONALDO,
        DR_STRANGE
    }

    public enum StarCardRank
    {
        BRONZE,
        SILVER,
        GOLD,
        PURPLE,
        TITAN
    }

    public enum StarCardStat
    {
        IncreaseSalary,
        IncreaseReward,
        ReduceSellHouseFees,
        ReduceBuildHouseCost,
        ReduceToll,
        ReduceSkillTurnCount,
        BonusSkillValue
    }

    public enum CurrencyCode
    {
        MONEY,
        JEWEL,
        HEART
    }

    public enum DiceCode
    {
        NONE = -1,
        BASIC,
        CHAN,
        LE,
        DAI_BAO,
        TIEU_BAO,
        TAI_LOC,
        LOC_PHAT
    }

    /*public enum RollDiceType
    {
        NORMAL,
        SPECIAL
    }*/

    public enum EmoticonCode
    {
        NONE,
        EMO_01,
        EMO_02,
        EMO_03,
        EMO_04,
        EMO_05,
        EMO_06,
        EMO_07,
        EMO_08,
        EMO_09,
        EMO_10
    }

    public enum RoomTypeCode
    {
        BATTLE_2P = 2,
        BATTLE_3P = 3
    }

    public enum RoomLevelCode
    {
        Rookie = 1,
        Expert = 2,
        Master = 3
    }

    public enum GamerColor
    {
        GREEN,
        RED,
        YELLOW,
        BLUE
    }

    /*public enum RewardCode
    {
        Heart,
        Money,
        Jewel,
        StarCard
    }*/

    public enum GoldHeartRewardCode
    {
        GoldHeart5,
        GoldHeart10,
        GoldHeart15
    }

    public enum TollRateType
    {
        NONE,
        Olympic,
        Festival,
        Park,
        StarCity,
        Couple
    }

    public enum HouseCode
    {
        NONE = -1,
        LAND,
        VILLA,
        BUILDING,
        HOTEL
    }

    public enum ActionCardCode
    {
        None,
        StartGift,
        BonusSalaryWhenAtGO,
        DiscountHouseCost,
        FreeTaxes,
        StarCity,
        //UpgradeHouseWhenAtGO,
    }

    public enum MissionCode
    {
        DoubleDices,
        GoToSpecialBlock,
        BuildHouse,
        BuildPark,
        OwnBlock,
        ChooseChanceCard
    }

    public enum ChanceCardCode
    {
        NONE,
        MoveForward_1,
        MoveForward_2,
        //MoveBack_1,
        //MoveBack_2,
        GoToTornado,
        GoToCannon,
        GoToPark,
        SetStarCity,
        GoToStarCity,
        OrganizeOlympic,
        GoToOlympic,
        OrganizeFestival,
        GoToFestival,
        DowngradeOpponentHouse,
        ReduceOpponentBlockToll,
        SellOpponentHouse,
        IncreaseAllBlocksToll,
        ExchangeBlocks,
        Help,
        Donate,
        RagsToRich
    }

    public enum ManaCode
    {
        Start,
        EveryMoveStep,
        UseSkill,
        Roll1Dice,
        Roll2Dices,
        OpponentRollDice
    }

    public enum ERROR_CODE
    {
        OK,
        DISPLAY_MESSAGE,
        CONFIG_VERSION_INVALID,
        GAME_VERSION_INVALID,
        ACCESS_TOKEN_INVALID,
        SERVER_MAINTANCE,
        UNKNOW_ERROR,
    }

    public enum QuestCode
    {
        Login,
        InviteFriend,
        SendHeart,
        BuyItem,
        UpgradeStarCard
    }

    public enum EventCode
    {
        Login,
        MoiInviteFriendBan,
        FriendsRanking,
        WorldRanking
    }

    public enum BattleState
    {
        MATCHING,
        BUY_ACTION_CARD,
        START_BATTLE,
        START_TURN,
        CONTINUE_TURN,
        END_TURN,
        NONE,
        /*
        ROLL_DICE,
        MOVE_TO_BLOCK,
        STAY_AT_BLOCK,
        DRAW_CHANCE_CARD,

        WAITING_BUY_ACTION_CARD,
        WAITING_ROLL_DICE,
        WAITING_BUILD_HOUSE,
        WAITING_USE_ACTION_CARD,
        WAITING_USE_CHARACTER_SKILL,
        WAITING_SELECT_BLOCK,
        WAITING_SELECT_MULTI_BLOCKS,
        WAITING_SELL_HOUSES,
        WAITING_EXCHANGE_BLOCKS,

        */
        END_BATTLE
    }

    /*public enum BattleWaitingActionCode
    {
        NONE,
        WAITING_BUY_ACTION_CARD,
        WAITING_ROLL_DICE,
        WAITING_BUILD_HOUSE,
        WAITING_USE_ACTION_CARD,
        WAITING_USE_CHARACTER_SKILL,
        WAITING_SELECT_BLOCK,
        WAITING_SELL_HOUSES,
        WAITING_EXCHANGE_BLOCKS
    }*/

    public enum SelectBlockActionCode
    {
        SET_CASINO,
        SET_OLYMPIC,
        SET_FESTIVAL,
        SET_STAR_CITY,
        SET_DONATE,
        DOWNGRADE_OPPONENT_HOUSE,
        REDUCE_OPPONENT_BLOCK_TOLL,
        SELL_OPPONENT_HOUSE,
        SET_BLOCK_FOR_CHARACTER_SKILL,
    }

    public enum GamerState
    {
        ONLINE,
        OFFLINE
    }

    /*public enum PeerState
    {
        NONE,
        IN_LOBBY,
        IN_WAITING_ROOM,
        IN_BATTLE
    }*/

    /*public enum ReplayEventCode
    {
        UpdateBoardProperties,
        UpdateGamerProperties,
        RollDice,
        MoveToBlock,
        FallToBlock,
        UseActionCard,
        DrawChanceCard,
        UseSkill,
        ChangeCash,
        SetHouseAtBlock,
        CannonShotToBlock,
        Warning,
        Message
    }*/

    public enum EndBattleType
    {
        MONOPOLY,
        BANKRUPT,
        TURN_OFF
    }

    public enum ManaBoosterCode
    {
        None,
        PEPSI,
        DR_THANH,
        TRA_XANH_0_DO,
        COCACOLA,
        BIA_HA_NOI,
        BIA_TRUC_BACH,
        BIA_SAI_GON_LON,
        BIA_SAI_GON_CHAI,
        C2,
        SUA_VINAMILK,
        VITAMIN
    }

    public enum GiftCode
    {
        GIFT_01 = 1,
        GIFT_02,
        GIFT_03,
        GIFT_04,
        GIFT_05,
        GIFT_06,
    }

    public enum PointAccumulationCode
    {
        LOGIN,
        INVITE_FRIEND,
        SEND_HEART_TO_FRIEND,
        BUY_ITEM,
        UPGRADE_CARD,
    }

    public enum SearchPlayerType
    {
        MAKE_FRIEND,
        INVITE_PLAY,
    }

    public enum PlatformCode
    {
        ANDROID,
        IOS,
        WEB,
        PC,
        WINDOWS_PHONE,
    }

    public enum PlayerStatus
    {
        OUT_GAME,
        IN_WAITING_ROOM,
        PLAYING,
        BANKRUPT,
        REMATCH,
    }

    public enum MoveCharacterType
    {
        NORMAL,
        SKILL,
    }

    public enum ChooseBlockType
    {
        MINE,
        OPPONENT,
        BOTH,
        SKILL,
        UPGRADE_HOUSE_AT_GO,
    }

    public enum DestroyBlockType
    {
        CANNON,
        SALE,
        DONATE,
    }

    public enum PriceType
    {
        NONE,
        MONEY,
        JEWEL,
        HEART,
        JEWEL1WEEK,
        JEWEL2WEEK,
        JEWEL3WEEK,
    }

    public enum StarCardPackCode
    {
        NONE,
        NORMAL,
        PREMIUM,
        KHUNG_LONG,
        KIM_CHI,
        CU_CAI,
        CU_HEN,
    }

    public enum BattleWarningType
    {
        WARNING_MONOPOLY,
        WARNING_REMAIN_3_TURN,
        END_BATTLE_MONOPOLY,
        END_BATTLE_BANKRUPT,
        END_BATTLE_TURN_OFF
    }

    public enum MoneyPackCode
    {
        NONE = -1,
        MONEY_01,
        MONEY_02,
        MONEY_03,
        MONEY_04,
        MONEY_05,
        MONEY_06,
        MONEY_07,
        MONEY_08
    }

    public enum JewelPackCode
    {
        NONE = -1,
        JEWEL_01,
        JEWEL_02,
        JEWEL_03,
        JEWEL_04,
        JEWEL_05,
        JEWEL_06
    }
}
