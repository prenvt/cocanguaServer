using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CTPServer.MongoDB;
using CBShare;
using CBShare.Battle;
using CBShare.Configuration;
using LitJson;
using Server.DatabaseUtils;
using System.Drawing;
using CBShare.Data;
using CBShare.Common;
    using Microsoft.Extensions.Caching.Memory;

public class GameManager
{
    public MemoryCache Cache { get; set; }
    private static object syncObj = new object();
    private static GameManager _inst { get; set; }
    public static GameManager Instance
    {
        get
        {
            if (_inst == null)
            {
                lock (syncObj)
                {
                    if (_inst == null) _inst = new GameManager();
                }
            }
            return _inst;
        }
    }
    //public WaitingRoomController waitingRoomController { get; set; }
    public RoomController roomController { get; set; }

    public static int SERVER_VERSION_PC = 00010;
    public static int SERVER_VERSION_ANDROID = 00010;
    public static int SERVER_VERSION_IOS = 00010;
    public const int ClientVersion = 100;

    private GameManager()
    {
        this.Init();
    }

    public void Init()
    {
        this.Cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 100 * 1000,
        });
        //this.waitingRoomController = new WaitingRoomController();
        this.roomController = new RoomController();
    }

    public static string GetGamerVersion(Platform platform)
    {
        //var gVer = platform == Platform.ANDROID ? SERVER_VERSION_ANDROID : SERVER_VERSION_IOS;
        return string.Format("{0}.{1}.{2}", ClientVersion / 100, (ClientVersion % 100) / 10, ClientVersion % 10);
    }

    public static UserInfo GetUserInfo(long gid, List<string> props, string username = "", bool allowResetNewDay = true, bool checkUnlockTinhNang = true)
    {
        /*List<string> requestProps = new List<string>();
        requestProps.AddRange(props);*/
        var requestProps = props.ToList();
        UserInfo userInfo = new UserInfo();

        if (requestProps.Exists(e => e.ToLower() == GameRequests.PROPS_GAMER_DATA))
        {
            userInfo.gamerData = GamerMongoDB.GetByID(gid);
#if TESTING
            if (userInfo.gamerData.GetCurrencyValue(CurrencyCode.MONEY) == 0)
            {
                userInfo.gamerData.currencies[CurrencyCode.MONEY.ToString()] = 100000;
            }
            if (userInfo.gamerData.currentCharacter == CharacterCode.NONE)
            {
                userInfo.gamerData.currentCharacter = (CharacterCode)RandomUtils.GetRandomInt(1, 4);
            }
            if (userInfo.gamerData.currentDice == DiceCode.NONE)
            {
                userInfo.gamerData.currentDice = DiceCode.BASIC;
            }
#endif
        }

        if (requestProps.Exists(e => e.ToLower() == GameRequests.PROPS_CHARACTER_DATA))
        {
            userInfo.charactersList = CharacterGamerMongoDB.GetCharactersListByGID(gid);
            if (userInfo.charactersList == null || userInfo.charactersList.Count == 0)
            {
                userInfo.charactersList = new List<CharacterGamerData>();
#if TESTING
                foreach (CharacterCode characterCode in (CharacterCode[])Enum.GetValues(typeof(CharacterCode)))
                {
                    if (characterCode == CharacterCode.NONE) continue;
                    var characterGamerData = CharacterGamerMongoDB.Insert(gid, characterCode);
                    userInfo.charactersList.Add(characterGamerData);
                }
#endif
            }
        }

        if (requestProps.Exists(e => e.ToLower() == GameRequests.PROPS_DICE_DATA))
        {
            userInfo.dicesList = DiceGamerMongoDB.GetDicesListByGID(gid);
            if (userInfo.dicesList == null || userInfo.dicesList.Count == 0)
            {
                userInfo.dicesList = new List<DiceGamerData>();
#if TESTING
                foreach (DiceCode diceCode in (DiceCode[])Enum.GetValues(typeof(DiceCode)))
                {
                    if (diceCode == DiceCode.NONE) continue;
                    var diceGamerData = DiceGamerMongoDB.Insert(gid, diceCode, DateTime.Now.AddDays(100));
                    userInfo.dicesList.Add(diceGamerData);
                }
#endif
            }
        }

        if (requestProps.Exists(e => e.ToLower() == GameRequests.PROPS_STAR_CARD_DATA))
        {
            userInfo.starCardsList = StarCardGamerMongoDB.GetListByGID(gid);
            if (userInfo.starCardsList == null || userInfo.starCardsList.Count == 0)
            {
                userInfo.starCardsList = new List<StarCardGamerData>();
#if TESTING
                foreach (StarCardRank starCardRank in (StarCardRank[])Enum.GetValues(typeof(StarCardRank)))
                {
                    foreach (CharacterCode characterCode in (CharacterCode[])Enum.GetValues(typeof(CharacterCode)))
                    {
                        var cardName = string.Format("{0}|{1}", starCardRank.ToString(), characterCode.ToString());
                        var cardCfg = ConfigManager.instance.starCardsConfig.Find(e => e.name == cardName);
                        if (cardCfg != null)
                        {
                            var starCardGamerData = StarCardGamerMongoDB.Insert(gid, cardCfg, 1);
                            userInfo.starCardsList.Add(starCardGamerData);
                        }
                    }
                }
#endif
            }
            if (userInfo.starCardsList.Count > 0 && userInfo.gamerData.currentStarCardID <= 0)
            {
                var randIdx = RandomUtils.GetRandomIndexInList(userInfo.starCardsList.Count);
                userInfo.gamerData.currentStarCardID = userInfo.starCardsList[randIdx].ID;
            }
        }

        /*if (checkUnlockTinhNang)
        {
            userInfo.unlockProgressData = UnlockProgressMongoDB.GetUnlockProgressDataByGID(gid);
            if (userInfo.unlockProgressData == null)
            {
                bool unlockedStore = userInfo.GamerData != null && userInfo.GamerData.GetLifeTimeNoteCount("StoreUnlocked") == 1;
                userInfo.unlockProgressData = new UnlockProgressData(gid, userInfo.CampaignData.Count, unlockedStore);
                UnlockProgressMongoDB.InsertOrReplaceUnlockProgressData(userInfo.unlockProgressData);
            }
            else
            {
                bool unlockedStore = userInfo.GamerData != null && userInfo.GamerData.GetLifeTimeNoteCount("StoreUnlocked") == 1;
                var updatedList = UnlockProgressData.GetListPropByUserInfo(userInfo.CampaignData.Count, unlockedStore);
                bool needUpdate = false;
                foreach (string str in updatedList)
                {
                    if (!userInfo.unlockProgressData.UnlockedProps.Contains(str))
                    {
                        userInfo.unlockProgressData.UnlockedProps.Add(str);
                        needUpdate = true;
                    }
                }
                if (needUpdate)
                    UnlockProgressMongoDB.InsertOrReplaceUnlockProgressData(userInfo.unlockProgressData);
            }
        }*/

        bool isNewDay = false;
        DateTime lastTimeLogin = DateTime.Now;

        if (userInfo.gamerData != null && allowResetNewDay)
        {
            userInfo.gamerData.lastTimeLogin = DateTime.Now;
            GamerMongoDB.Save(userInfo.gamerData);
            isNewDay = true;
        }

        if (isNewDay)
        {
            /*if (userInfo.unlockProgressData != null && userInfo.unlockProgressData.UnlockedProps.Exists(e => e.ToLower() == GameRequests.PROPS_DUNGEON_DATA))
            {
                //ResetTruyenTongTran(gid, userInfo);
                userInfo.dungeon = DungeonMongoDB.GetOrResetData(gid, true);
                userInfo.dungeonMap = DungeonMapMongoDB.GetOrResetData(gid, userInfo.dungeon.chapter, true);
            }*/

            /*if (userInfo.unlockProgressData != null && userInfo.unlockProgressData.UnlockedProps.Exists(e => e.ToLower() == GameRequests.PROPS_GAMER_SKY_TOWER_DATA))
            {
                ResetHMN(gid, userInfo);
                //userInfo.skyTower = SkyTowerMongoDB.GetOrResetData(gid, true);
            }*/
        }

        /*if (allowResetNewDay)
            ClanUI.ClanGamerUpdateTimeOnline(gid);*/

        userInfo.ServerTimeTick = DateTime.Now.Ticks;

        if (requestProps.Exists(e => e.ToLower() == GameRequests.PROPS_GAMER_DATA) == false)
            userInfo.gamerData = null;
        /*if (requestProps.Exists(e => e.ToLower() == GameRequests.PROPS_EQUIPMENT_DATA) == false)
            userInfo.ListEquipment = null;*/
        return userInfo;
    }

    /*public static RewardConfig SetReward(long gid, UserInfo userInfo, RewardConfig rewardFromConfig, string descriptionLog, UserInfo responseUserInfo = null)
    {
        if (userInfo == null || rewardFromConfig == null)
        {
            return null;
        }

        RewardConfig rewardCfg = rewardFromConfig.Clone();
        bool haveRandomEquipment = false;
        foreach (var e in rewardFromConfig.Rewards)
        {
            if (string.IsNullOrEmpty(e.CodeName) || e.Quantity == 0)
                continue;
            bool isRandomEq = IsRandomEquipmentCodeName(e.CodeName);
            if (isRandomEq)
            {
                for (int i = 0; i < e.Quantity; i++)
                {
                    string randomEquipmentCodeName = CheckRandomEquipment(e.CodeName);
                    if (string.IsNullOrEmpty(randomEquipmentCodeName) == false)
                    {
                        rewardCfg.Rewards.Add(new Reward(randomEquipmentCodeName, 1));
                    }
                }
                haveRandomEquipment = true;
            }
        }
        if (haveRandomEquipment)
            rewardCfg.Rewards.RemoveAll(e => IsRandomEquipmentCodeName(e.CodeName));

        bool needGetNewItem = false;
        bool needGetNewEquipment = false;
        bool needGetNewCharacter = false;
        bool needGetNewOfferData = false;
        bool needToUpdateLibrary = false;
#if VER_7_0
        bool needGetNewPet = false;
        List<UserInfo.Pet> newPets = new List<UserInfo.Pet>();
#endif
        // add logs
        List<LogGamerData> userlogs = new List<LogGamerData>();
        List<UserInfo.Character> newChars = new List<UserInfo.Character>();
        List<UserInfo.Equipment> newEquipments = new List<UserInfo.Equipment>();

        //Chuan hoa reward
        var tempReward = rewardCfg.Clone();

        if (tempReward.Rewards.Find(e => e.IsHourItem()) != null)
        {
            if (userInfo.GamerData == null)
                userInfo.GamerData = GamerUI.GetUserDataByGID(gid);
            if (userInfo.AFK == null)
                userInfo.AFK = AFKMongoDB.GetAFKDataByGID(gid);
            if (userInfo.TheThangPlayer == null)
                userInfo.TheThangPlayer = TheThangGamerMongoDB.GetDataByGID(userInfo.GID);
            if (userInfo.clanData == null)
                userInfo.clanData = ClanUI.GetByGID(gid);
            rewardCfg = ConfigHelper.ReplaceHourItems(gid, userInfo.TheThangPlayer, tempReward, userInfo.GamerData.VIPLevel, userInfo.AFK.MineLevel, userInfo.clanData);
        }

        if (tempReward.Rewards.Find(e => e.CodeName.Contains("ROLE_")) != null)
        {
            userInfo.HeroLibrary = HeroLibraryMongoDB.GetLibraryDataByGID(gid);
        }

        rewardCfg = ConfigHelper.MergeReward(rewardCfg.Clone());
        if (rewardCfg == null)
            return null;

        //Rewards
        Random shareRand = new Random();
        foreach (Reward reward in rewardCfg.Rewards)
        {
            if (string.IsNullOrEmpty(reward.CodeName))
                continue;

            if (reward.CodeName.ToEnum<Currency>() != Currency.NONE && userInfo.GamerData == null)
                userInfo.GamerData = GamerUI.GetUserDataByGID(gid);

            //SET REWARD
            if (reward.CodeName.ToEnum<Currency>() == Currency.BAC)
            {
                long newGold = userInfo.GamerData.Bac + reward.Quantity;
                if (newGold < 0)
                    return null;
                GamerUI.GamerUpdateBac(gid, newGold);
                userInfo.GamerData.Bac = newGold;

                if (responseUserInfo != null) responseUserInfo.GamerData = userInfo.GamerData;
                userlogs.Add(new LogGamerData(gid, reward.CodeName, "bac", descriptionLog, newGold, reward.Quantity));
                continue;
            }

            if (reward.CodeName.ToEnum<Currency>() == Currency.KNB)
            {
                long newGem = userInfo.GamerData.Knb + reward.Quantity;
                if (newGem < 0)
                    return null;
                GamerUI.GamerUpdateKNB(gid, newGem);
                userInfo.GamerData.Knb = newGem;

                CheckEventsHappening(userInfo, reward.CodeName.ToEnum<Currency>(), reward.Quantity);
                if (responseUserInfo != null) responseUserInfo.GamerData = userInfo.GamerData;
                userlogs.Add(new LogGamerData(gid, reward.CodeName, "knb", descriptionLog, newGem, reward.Quantity));

#if VER_2_0
                if (reward.Quantity < 0)
                {
                    if (userInfo.QuestData == null || userInfo.QuestData.Quests.Count == 0)
                        userInfo.QuestData = GetUserInfo(gid, new List<string>() { GameRequests.PROPS_QUEST_DATA }).QuestData;
                    var usedKNB = -reward.Quantity;
                    var questDaily = userInfo.QuestData.GetQuestByCodeName("QUEST_DailyUseKNB");
                    if (questDaily != null)
                    {
                        GameManager.UpdateQuest(gid, userInfo, "QUEST_DailyUseKNB", questDaily.CurrentProgress + usedKNB);
                    }
                    var questWeekly = userInfo.QuestData.GetQuestByCodeName("QUEST_WeeklyUseKNB");
                    if (questWeekly != null)
                    {
                        GameManager.UpdateQuest(gid, userInfo, "QUEST_WeeklyUseKNB", questWeekly.CurrentProgress + usedKNB);
                    }

#if VER_8_0
                    if (GameManager.CheckEventNamMoi(gid))
                    {
                        GameManager.UpdateNhiemVuNamMoi(gid, userInfo, NhiemVuNamMoiType.NVNM_KNB_TIEU.ToString(), (int)usedKNB);
                    }
#endif
                }
#endif

                continue;
            }

            if (reward.CodeName.ToEnum<Currency>() == Currency.NGOC)
            {
                long newNgoc = userInfo.GamerData.Ngoc + reward.Quantity;
                if (newNgoc < 0)
                    return null;
                GamerUI.GamerUpdateNgoc(gid, newNgoc);
                userInfo.GamerData.Ngoc = newNgoc;

                CheckEventsHappening(userInfo, reward.CodeName.ToEnum<Currency>(), reward.Quantity);

#if VER_8_0
                if (reward.Quantity > 0 && GameManager.CheckEventNamMoi(gid))
                {
                    GameManager.UpdateNhiemVuNamMoi(gid, userInfo, NhiemVuNamMoiType.NVNM_NGOC_NAP.ToString(), (int)reward.Quantity);
                }
#endif

                if (responseUserInfo != null) responseUserInfo.GamerData = userInfo.GamerData;
                userlogs.Add(new LogGamerData(gid, reward.CodeName, "ngoc", descriptionLog, newNgoc, reward.Quantity));
                continue;
            }

            if (reward.CodeName.ToEnum<Currency>() == Currency.EXP)
            {
                long newExp = userInfo.GamerData.Exp + reward.Quantity;
                if (newExp < 0)
                    return null;
                GamerUI.GamerUpdateExp(gid, newExp);
                userInfo.GamerData.Exp = newExp;
                if (responseUserInfo != null) responseUserInfo.GamerData = userInfo.GamerData;
                userlogs.Add(new LogGamerData(gid, reward.CodeName, "exp", descriptionLog, newExp, reward.Quantity));
                continue;
            }

            if (reward.CodeName.ToEnum<Currency>() == Currency.PLAYER_EXP)
            {
                long newExp = userInfo.GamerData.PlayerExp + reward.Quantity;
                if (newExp < 0)
                    return null;

                int oldLevel = userInfo.GamerData.PlayerLevel;

                int newLevel = ConfigHelper.GetNewLevelIncrease(newExp, ConfigManager.Instance.VipCfg.PlayerExpRequire);
                if (newLevel > oldLevel)
                {
                    userInfo.GamerData.PlayerLevel = newLevel;
                    GamerUI.GamerUpdatePlayerLevel(gid, newLevel);
                    if (userInfo.QuestData == null)
                        userInfo.QuestData = MainQuestMongoDB.GetQuestDataByGID(gid);

                    UpdateQuest(gid, userInfo, "QUEST_PlayerLvl", newLevel);
                    UpdateQuestSNGH(gid, userInfo, "QUEST_PlayerLvl", newLevel, true);
                    UpdateThuThachTanThu(gid, userInfo, "QUEST_TTTT_PlayerLvl", newLevel, true);

                    ThongKeTopMongoDB.UpdateDiemUserLevel(userInfo.GamerData);

                    int changedLevel = newLevel - oldLevel;
                    long totalGem = changedLevel * ConfigManager.Instance.OtherCfg.PlayerLevelUpGem;
                    long newGem = userInfo.GamerData.Knb + totalGem;
                    if (newGem < 0)
                        return null;
                    GamerUI.GamerUpdateKNB(gid, newGem);
                    userInfo.GamerData.Knb = newGem;
                    userlogs.Add(new LogGamerData(gid, "KNB", "level up gem", descriptionLog, newGem, reward.Quantity));
                }

                GamerUI.GamerUpdatePlayerExp(gid, newExp);
                userlogs.Add(new LogGamerData(gid, reward.CodeName, "player_exp", descriptionLog, newExp, reward.Quantity));
                userInfo.GamerData.PlayerExp = newExp;
                if (responseUserInfo != null) responseUserInfo.GamerData = userInfo.GamerData;

                for (int i = oldLevel + 1; i <= newLevel; i++)
                {
                    Offer offerCfg = ConfigManager.Instance.IAPConfig.GetPlayerLevelOffer(i);
                    if (offerCfg == null)
                        continue;
                    GameManager.AddNewOffer(gid, userInfo, offerCfg.CodeName);
                    needGetNewOfferData = true;
                }
                continue;
            }

            if (reward.CodeName.ToEnum<Currency>() == Currency.VIP_POINT)
            {
                long newExp = userInfo.GamerData.VIPPoint + reward.Quantity;
                if (newExp < 0)
                    return null;

                List<long> vipExp = ConfigManager.Instance.VipCfg.ExpRequire.GetRange(1, ConfigManager.Instance.VipCfg.ExpRequire.Count - 1);

                int newLevel = ConfigHelper.GetNewLevelIncrease(newExp, vipExp, true);
                if (newLevel > userInfo.GamerData.VIPLevel)
                {
                    userInfo.GamerData.VIPLevel = newLevel;
                    GamerUI.GamerUpdateVIPLevel(gid, newLevel);
                }

                GamerUI.GamerUpdateVIPPoint(gid, newExp);
                userInfo.GamerData.VIPPoint = newExp;
                if (responseUserInfo != null) responseUserInfo.GamerData = userInfo.GamerData;
                userlogs.Add(new LogGamerData(gid, reward.CodeName, "vipPoint", descriptionLog, newExp, reward.Quantity));
                continue;
            }

            if (reward.CodeName.ToEnum<Currency>() == Currency.TOKEN_PVP)
            {
                if (userInfo.luanKiemData == null)
                    userInfo.luanKiemData = LuanKiemUI.GetLuanKiemRecordByGID(gid);

                long newPvPPoint = userInfo.GetResource(Currency.TOKEN_PVP) + reward.Quantity;
                if (newPvPPoint < 0)
                    return null;

                if (userInfo.luanKiemData != null)
                {
                    LuanKiemUI.LuanKiemUpdateDiemTichLuy(gid, newPvPPoint);
                    userInfo.luanKiemData.DiemTichLuy = newPvPPoint;
                    if (responseUserInfo != null) responseUserInfo.luanKiemData = userInfo.luanKiemData;
                    userlogs.Add(new LogGamerData(gid, reward.CodeName, "diemLuanKiem", descriptionLog, newPvPPoint, reward.Quantity));
                }
                continue;
            }

            if (reward.CodeName.ToEnum<Currency>() == Currency.TOKEN_Honor)
            {
                if (userInfo.ListItem == null || userInfo.ListItem.Count == 0)
                    userInfo.ListItem = ItemUI.GetItemDataByGID(gid);

                if (responseUserInfo != null) responseUserInfo.ListItem = userInfo.ListItem;

                long newClanPoint = reward.Quantity;
                UserInfo.Item clanPoint = userInfo.GetItemByCodeName("TOKEN_Honor");
                if (clanPoint != null)
                    newClanPoint += clanPoint.Quantity;

                if (newClanPoint < 0)
                    return null;

                if (clanPoint != null)
                    ItemUI.ItemUpdate(clanPoint.ID, newClanPoint);
                else
                    ItemUI.ItemAddNew(gid, "TOKEN_Honor", newClanPoint);

                needGetNewItem = true;
                userlogs.Add(new LogGamerData(gid, reward.CodeName, "diemClan", descriptionLog, newClanPoint, reward.Quantity));
                continue;
            }

            if (reward.CodeName.ToEnum<Currency>() == Currency.HIGHFIVE)
            {
                long newHighFivePoint = userInfo.GamerData.HighFivePoint + reward.Quantity;
                if (newHighFivePoint < 0)
                    return null;
                GamerUI.GamerUpdateHighFivePoint(gid, newHighFivePoint, userInfo.GamerData.HighFiveCount);
                userInfo.GamerData.HighFivePoint = newHighFivePoint;
                if (responseUserInfo != null) responseUserInfo.GamerData = userInfo.GamerData;
                userlogs.Add(new LogGamerData(gid, reward.CodeName, "highFivePoint", descriptionLog, newHighFivePoint, reward.Quantity));
                continue;
            }

            if (reward.IsHourItem())
            {
                if (userInfo.AFK == null)
                    userInfo.AFK = AFKMongoDB.GetAFKDataByGID(gid);
                if (userInfo.TheThangPlayer == null)
                    userInfo.TheThangPlayer = TheThangGamerMongoDB.GetDataByGID(gid);
#if VER_7_0
                if (userInfo.clanData == null)
                    userInfo.clanData = ClanUI.GetByGID(gid);
#endif
                string prefix = ConfigHelper.GetPrefix(reward.CodeName);
                RewardConfig rewards = new RewardConfig();
                if (reward.CodeName.Contains("HOUR"))
                    rewards.Rewards = ConfigManager.Instance.AfkConfig.GetAFKReward(gid, userInfo.TheThangPlayer, userInfo.GamerData.VIPLevel, userInfo.AFK.MineLevel, 60 * reward.Quantity, null, userInfo.clanData);
                else if (reward.CodeName.Contains("MINUTE"))
                    rewards.Rewards = ConfigManager.Instance.AfkConfig.GetAFKReward(gid, userInfo.TheThangPlayer, userInfo.GamerData.VIPLevel, userInfo.AFK.MineLevel, reward.Quantity, null, userInfo.clanData);

                Reward myReward = rewards.Rewards.FindLast(e => e.CodeName == prefix);
                if (myReward == null)
                    continue;

                RewardConfig finalReward = new RewardConfig();
                finalReward.Rewards.Add(myReward);
                SetReward(gid, userInfo, finalReward, descriptionLog);

                if (responseUserInfo != null) responseUserInfo.AFK = userInfo.AFK;
                reward.CodeName = myReward.CodeName;
                reward.Quantity = myReward.Quantity;

                continue;
            }

            if (reward.CodeName.Contains("MANH_SECRET_MAP"))
            {
                if (userInfo.scaredTombData == null)
                {
                    userInfo.scaredTombData = SacredTombMongoDB.GetByGID(gid);
                }
                if (userInfo.scaredTombData != null)
                {
                    string idstr = reward.CodeName.Replace("MANH_SECRET_MAP", "");
                    int manhID = System.Convert.ToInt32(idstr);

                    bool open1 = userInfo.scaredTombData.openGate;
                    userInfo.scaredTombData.AddManh(manhID, (int)reward.Quantity);
                    bool open2 = userInfo.scaredTombData.openGate;
                    if (open1 == false && open2 == true)
                    {
                        AlertSystemMongoDB.UpdateNewSecretOpen(gid, true);
                        SacredTombMongoDB.UpdateBossLevel(gid, userInfo.scaredTombData);
                    }

                    float r = userInfo.scaredTombData.manhAvaiables.Count / 25f;
                    if (r >= 1f && userInfo.scaredTombData.flagsMocReceived.Contains(5) == false)
                    {
                        userInfo.scaredTombData.flagsMocReceived.Add(5);
                        userInfo.scaredTombData.challengePass += ConfigManager.Instance.sacredConfig.Unlock100Reward;
                        LogGamerUI.Add(gid, "challengePass_moc100", "", "", userInfo.scaredTombData.challengePass);

                        userlogs.Add(new LogGamerData(gid, reward.CodeName, "challengePass", descriptionLog, userInfo.scaredTombData.challengePass, reward.Quantity));
                    }
                    if (r >= 0.2f && userInfo.scaredTombData.flagsMocReceived.Contains(1) == false)
                    {
                        userInfo.scaredTombData.flagsMocReceived.Add(1);
                        userInfo.scaredTombData.challengePass += ConfigManager.Instance.sacredConfig.Unlock20Reward;

                        LogGamerUI.Add(gid, "challengePass_moc20", "", "", userInfo.scaredTombData.challengePass);

                        userlogs.Add(new LogGamerData(gid, reward.CodeName, "challengePass", descriptionLog, userInfo.scaredTombData.challengePass, reward.Quantity));
                    }

                    if (responseUserInfo != null) responseUserInfo.scaredTombData = userInfo.scaredTombData;
                    SacredTombMongoDB.Save(userInfo.scaredTombData);
                }
                continue;
            }

            ItemType itemType = ConfigHelper.GetItemTypeFromCodeName(reward.CodeName);

            if (itemType != ItemType.NONE)
            {
                if (userInfo.ListItem == null || userInfo.ListItem.Count == 0)
                    userInfo.ListItem = ItemUI.GetItemDataByGID(gid);

                if (responseUserInfo != null) responseUserInfo.ListItem = userInfo.ListItem;

                UserInfo.Item item = userInfo.GetItemByCodeName(reward.CodeName);
                long quantity = reward.Quantity;
                if (item != null)
                {
                    ItemUI.ItemUpdate(item.ID, item.Quantity + reward.Quantity);
                    item.Quantity = item.Quantity + reward.Quantity;
                    quantity = item.Quantity;
                    userlogs.Add(new LogGamerData(gid, item.CodeName, "item", descriptionLog, item.Quantity, reward.Quantity));
                }
                else
                {
                    ItemUI.ItemAddNew(gid, reward.CodeName, reward.Quantity);
                    needGetNewItem = true;
                    userlogs.Add(new LogGamerData(gid, reward.CodeName, "item", descriptionLog, reward.Quantity, reward.Quantity));
                }

                if (reward.CodeName == "TOKEN_Daily")
                {
                    UpdateQuest(gid, userInfo, "QUEST_DailyReward", quantity);
                    UpdateEpicPass(gid, userInfo, EpicPassType.Daily, reward.Quantity);
                    ClanUI.UpdateQuestPoint(gid, userInfo.clanData, (int)reward.Quantity);
                    userlogs.Add(new LogGamerData(gid, reward.CodeName, "TOKEN_Daily", descriptionLog, reward.Quantity, reward.Quantity));
                }

                if (reward.CodeName == "TOKEN_Weekly")
                {
                    UpdateQuest(gid, userInfo, "QUEST_WeeklyReward", quantity);
                    UpdateEpicPass(gid, userInfo, EpicPassType.Weekly, reward.Quantity);
                    ClanUI.UpdateQuestPoint(gid, userInfo.clanData, (int)reward.Quantity);
                    userlogs.Add(new LogGamerData(gid, reward.CodeName, "TOKEN_Weekly", descriptionLog, reward.Quantity, reward.Quantity));
                }
                continue;
            }

            var equipType = ConfigHelper.GetEquipmentTypeFromCodeName(reward.CodeName);
            if (equipType != EquipmentType.NONE)
            {
                for (int i = 0; i < reward.Quantity; i++)
                {
                    EquipmentConfig cfg = ConfigManager.Instance.GetEquipment(reward.CodeName);
                    if (cfg != null)
                    {
                        int tangtruongtinhluyen = 0;
                        int star = 0;
                        UserInfo.Equipment.DuyenPhanData duyenPhanData = ConfigManager.Instance.InitDuyenPhanDataFormConfig(reward.CodeName, shareRand);
                        UserInfo.Equipment nEquip = new UserInfo.Equipment(gid, reward.CodeName, star, tangtruongtinhluyen, duyenPhanData);
                        newEquipments.Add(nEquip); // add vao list de insert 1 lan
                        userlogs.Add(new LogGamerData(gid, reward.CodeName, "Equipment", descriptionLog, 0, 0));
                        ThongKeTopMongoDB.UpdateDiemChienLucTrangBi(gid, userInfo, nEquip);

                        int rank = nEquip.GetConfig().Rank;
                        if (rank == (int)RankType.Epic)
                        {
                            GameManager.UpdateThuThachTanThu(gid, userInfo, "QUEST_TTTT_GearEpic", 1, false);
                        }
                        else if (rank >= (int)RankType.Legend && rank < (int)RankType.Mythic)
                        {
                            GameManager.UpdateThuThachTanThu(gid, userInfo, "QUEST_TTTT_GearEpic", 2, false);
                        }
                        else if (rank >= (int)RankType.Mythic)
                        {
                            GameManager.UpdateThuThachTanThu(gid, userInfo, "QUEST_TTTT_GearEpic", 4, false);
                        }
                    }
                    needGetNewEquipment = true;
                }
                continue;
            }

            if (ConfigHelper.GetPrefix(reward.CodeName) == "ROLE")
            {
                UserInfo.Character characterData = new UserInfo.Character();
                characterData.GID = gid;
                characterData.CodeName = reward.CodeName;
                characterData.Rank = (int)reward.Quantity;
                newChars.Add(characterData); // add vao list de insert 1 lan
                                   // CharacterUI.CharacterAddNew(GID, reward.CodeName, (int)reward.Quantity);  //moe edit
                UpdateQuest(gid, userInfo, "QUEST_HeroCollector");
                needGetNewCharacter = true;
                userlogs.Add(new LogGamerData(gid, reward.CodeName, "character", descriptionLog, reward.Quantity, 0));
                if (userInfo.HeroLibrary == null)
                {
                    userInfo.HeroLibrary = GetUserInfo(gid, new List<string>() { GameRequests.PROPS_HERO_LIBRARY_DATA }).HeroLibrary;
                }
                if (!userInfo.HeroLibrary.Unlocked(reward.CodeName))
                {
                    userInfo.HeroLibrary.UnlockedHeroes.Add(reward.CodeName);
                    needToUpdateLibrary = true;
                }
                
                GameManager.OnUpdateCharacterData(gid, userInfo, characterData);
            }
        }

        GameManager.UpdateThuThachTanThu(gid, userInfo, "QUEST_TTTT_HeroRoll", newChars.Count, false);

        foreach (var hero in newChars)
        {
            if (hero.Rank == (int)RankType.Legend)
                GameManager.UpdateThuThachTanThu(gid, userInfo, "QUEST_TTTT_HeroLegend", 1, false);
        }

        if (newChars.Count > 0)
            CharacterUI.AddMulti(newChars);
        if (newEquipments.Count > 0)
            EquipmentUI.AddMulti(newEquipments);

#if VER_7_0
        if (newPets.Count > 0)
            PetUI.AddMulti(newPets);
#endif

        LogGamerUI.AddMulti(userlogs);
        LogGamerUI.Add(gid, JsonMapper.ToJson(tempReward).ToString(), "reward", descriptionLog, 0, 0);

        if (needGetNewItem)
        {
            userInfo.ListItem = ItemUI.GetItemDataByGID(gid);
            if (responseUserInfo != null) responseUserInfo.ListItem = userInfo.ListItem;
        }

        if (needGetNewEquipment)
        {
            userInfo.ListEquipment = EquipmentUI.GetEquipmentDataByGID(gid);
            if (responseUserInfo != null) responseUserInfo.ListEquipment = userInfo.ListEquipment;
        }

#if VER_7_0
        if (needGetNewPet)
        {
            userInfo.listPet = PetUI.GetPetDataByGID(gid);
            if (responseUserInfo != null) responseUserInfo.listPet = userInfo.listPet;
        }
#endif

        if (needGetNewCharacter)
        {
#if VER_2_0
            if (userInfo.CampaignData == null || userInfo.CampaignData.Count <= 0)
            {
                userInfo.CampaignData = CampaignMongoDB.GetCampaignDataByGID(gid);
            }

            int campaignUnlocked = 1;
            if (userInfo.GetLastCampaignIndex() > 0)
                campaignUnlocked = userInfo.GetLastCampaignIndex();
#endif

            userInfo.ListCharacter = CharacterUI.GetCharacterDataByGID(gid
#if VER_2_0
                , campaignUnlocked
#endif
                );
            if (responseUserInfo != null) responseUserInfo.ListCharacter = userInfo.ListCharacter;
        }

        if (needGetNewOfferData)
        {
            userInfo.ListOffer = UserOfferUI.GetOfferByGID(gid);
            if (responseUserInfo != null) responseUserInfo.ListOffer = userInfo.ListOffer;
        }

        if (needToUpdateLibrary)
        {
            HeroLibraryMongoDB.InsertOrReplaceLibraryData(userInfo.HeroLibrary);
            if (responseUserInfo != null) responseUserInfo.HeroLibrary = userInfo.HeroLibrary;
        }

        return rewardCfg;
    }*/

    /*public static bool CanGetReward(long gid, UserInfo userInfo, RewardConfig rewardCfg)
    {
        if (userInfo == null)
        {
            return false;
        }

        if (userInfo.GamerData == null)
            userInfo.GamerData = GamerUI.GetUserDataByGID(gid);
        if (userInfo.ListItem == null || userInfo.ListItem.Count == 0)
            userInfo.ListItem = ItemUI.GetItemDataByGID(gid);

        foreach (Reward reward in rewardCfg.Rewards)
        {
            if (string.IsNullOrEmpty(reward.CodeName))
                continue;

            if (reward.CodeName.Contains("TOKEN_PVP") && userInfo.luanKiemData == null)
            {
                userInfo.luanKiemData = LuanKiemUI.GetLuanKiemRecordByGID(gid);
                break;
            }
        }

        foreach (Reward reward in rewardCfg.Rewards)
        {
            if (string.IsNullOrEmpty(reward.CodeName))
                continue;

            long currentQuantity = userInfo.GetResource(reward.CodeName);
            long total = currentQuantity + reward.Quantity;
            if (total < 0)
                return false;
        }

        return true;
    }*/

    public static ERROR_CODE CheckRequestValidation(RequestBase request, bool checkConfigVersion = true)
    {
        /*string token = null;
        long gid = request.GID;
        //check duplicate request
        string key = GetAccessTokenCacheKey(gid);
        object tokenObj = Utility.GetCacheObject(key);
        if (tokenObj == null || (string)tokenObj != request.AccessToken || getTokenFromDb)
        {
            token = GamerUI.GamerGetTokenByID(gid);
            UpdateAccessToken(gid, token);
        }
        else
        {
            token = (string)tokenObj;
        }
        bool passToken = !string.IsNullOrEmpty(token) && token == request.AccessToken;
        if (!passToken)
        {
            return ERROR_CODE.ACCESS_TOKEN_INVALID;
        }*/

        if (checkConfigVersion)
        {
            //if (ConfigManager.Instance.ConfigsversionCode > request.ConfigVersion)
            {
                return ERROR_CODE.CONFIG_VERSION_INVALID;
            }
        }
        return ERROR_CODE.OK;
    }

    public void AddCache(string cacheKey, Object cacheObj)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60 * 10));
        try
        {
            var cacheItem = this.Cache.Set(cacheKey, cacheObj, cacheEntryOptions);
        }
        catch (Exception e)
        {
            //ExceptionLogUI.AddLogBattle("StartBattle", battleId, "set cache exception " + e.ToString());
        }
    }

    public T GetCache<T>(string cacheKey)
    {
        this.Cache.TryGetValue(cacheKey, out T cacheData);
        return cacheData;
    }
}