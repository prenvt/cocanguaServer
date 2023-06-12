using System;
using System.Collections.Generic;
using CBShare.Configuration;
using CBShare;
using System.Linq;
using CBShare.Common;

namespace CBShare.Data
{
    public class GameRequests
    {
        public static string PROPS_GAMER_DATA = "gamer";
        //public static string PROPS_EQUIPMENT_DATA = "equipment";
        //public static string PROPS_ITEM_DATA = "item";
        public static string PROPS_CHARACTER_DATA = "characters";
        public static string PROPS_DICE_DATA = "dices";
        public static string PROPS_STAR_CARD_DATA = "starcards";
        //public static string PROPS_MAIL_DATA = "mail";

        public static List<string> BATTLE_PROPS = new List<string>()
        {
            PROPS_GAMER_DATA,//for userName
            PROPS_CHARACTER_DATA,
            PROPS_DICE_DATA,
        };

        public static List<string> DEFAULT_PROPS = new List<string>()
        {
            PROPS_GAMER_DATA,
            PROPS_CHARACTER_DATA,
            PROPS_DICE_DATA,
            PROPS_STAR_CARD_DATA
            //PROPS_EQUIPMENT_DATA,
            //PROPS_ITEM_DATA,
            //PROPS_MAIL_DATA,
        };

        /*public static List<string> LOGIN_INFO_PROPS = new List<string>()
        {
            PROPS_GAMER_DATA,
            //PROPS_SERVER_DATA,
        };

        public static List<string> FULL_INFOR_PROPS = new List<string>()
        {
            PROPS_GAMER_DATA,
            PROPS_EQUIPMENT_DATA,
            PROPS_ITEM_DATA,
            PROPS_CHARACTER_DATA,
        };*/
    }

    public class UserInfo
    {
        public long ServerTimeTick { set; get; }
        public UserInfo()
        {
            ServerTimeTick = DateTime.Now.Ticks;
        }

        public long GID
        {
            get
            {
                if (this.gamerData != null)
                    return this.gamerData.ID;
                /*if (this.ListCharacter != null && this.ListCharacter.Count > 0)
                    return this.ListCharacter[0].GID;
                if (this.ListEquipment != null && this.ListEquipment.Count > 0)
                    return this.ListEquipment[0].GID;
                if (this.ListItem != null && this.ListItem.Count > 0)
                    return this.ListItem[0].GID;
                if (this.AFK != null)
                    return this.AFK.GID;
                if (this.ListMail != null && this.ListMail.Count > 0)
                    return this.ListMail[0].GID;
                if (this.PentagramData != null)
                    return this.PentagramData.GID;
                if (this.FormationData != null)
                    return this.FormationData.GID;*/
                return 0;
            }
        }

        #region Resource
        /*public long GetItemQuantity(string codeName)
        {
            if (ConfigHelper.GetItemTypeFromCodeName(codeName) != ItemType.NONE)
            {
                Item item = GetItemByCodeName(codeName);
                if (item != null)
                    return item.Quantity;
                else
                    return 0;
            }
            else if (ConfigHelper.GetEquipmentTypeFromCodeName(codeName) != EquipmentType.NONE)
            {
                int count = 0;
                if (ListEquipment == null)
                    return 0;
                for (int i = 0; i < ListEquipment.Count; i++)
                {
                    if (ListEquipment[i].CodeName.Equals(codeName))
                    {
                        count++;
                    }
                }
                return count;
            }
            return 0;
        }*/
        #endregion

        #region Gamer
        public class Gamer
        {
            public long ID { get; set; }
            public string DailyNote { get; set; }
            public string LifeTimeNote { get; set; }
            public string displayName { get; set; }
            public bool viewTutorial { get; set; }
            public string statusText { get; set; }
            public string accessToken { get; set; }
            public DateTime registerTime { get; set; }
            public DateTime lastTimeLogin { get; set; }
            public DateTime lastChangeDisplayName { get; set; }
            //public CharacterCode currentCharacter { get; set; }
            public DiceType currentDice { get; set; }
            public Dictionary<string, int> currencies = new Dictionary<string, int>();
            //public long currentStarCardID { get; set; }
            public int totalMatch { get; set; }
            public int winMatch { get; set; }
            public int currentRoomLevel { get; set; }
            public int currentRoomID { get; set; }
            public DateTime lastTimeJoinRoom { get; set; }
            public int languageIndex { get; set; }
            public string Avatar { get; set; }
            public int ChangeDisplayName { get; set; }

            public bool CheckTut(int index)
            {
                int currentStatus = GetLifeTimeNoteCount("TutFlag");

                return CommonHelper.CheckFlag(currentStatus, index);
            }

            public void SetTut(int index)
            {
                if (CheckTut(index))
                    return;

                int currentStatus = GetLifeTimeNoteCount("TutFlag");
                currentStatus = CommonHelper.RaiseFlag(currentStatus, index);

                LifeTimeNote = UpdateLifeTimeNote("TutFlag", currentStatus.ToString());
            }

            public void RevertTut(int index)
            {
                if (!CheckTut(index))
                    return;

                int currentStatus = GetLifeTimeNoteCount("TutFlag");
                currentStatus = CommonHelper.DownFlag(currentStatus, index);

                LifeTimeNote = UpdateLifeTimeNote("TutFlag", currentStatus.ToString());
            }

            public int GetNoteCount(string noteStr)
            {
                if (!DailyNote.Contains(noteStr))
                    return 0;
                try
                {
                    int startIdx = DailyNote.IndexOf(noteStr) + noteStr.Length;
                    int length = DailyNote.IndexOf(';', DailyNote.IndexOf(noteStr)) - startIdx;

                    return int.Parse(DailyNote.Substring(startIdx, length));
                }
                catch (System.Exception)
                {
                    return 999;
                }
            }

            public string GetNoteString(string noteStr)
            {
                if (!DailyNote.Contains(noteStr))
                    return "";
                try
                {
                    int startIdx = DailyNote.IndexOf(noteStr) + noteStr.Length;
                    int length = DailyNote.IndexOf(';', DailyNote.IndexOf(noteStr)) - startIdx;

                    return DailyNote.Substring(startIdx, length);
                }
                catch (System.Exception)
                {
                    return "";
                }
            }

            public string UpdateDailyNote(string noteStr, string newSubNote)
            {
                if (!DailyNote.Contains(noteStr))
                    return DailyNote + noteStr + newSubNote + ";";

                int startIdx = DailyNote.IndexOf(noteStr) + noteStr.Length;
                int length = DailyNote.IndexOf(';', DailyNote.IndexOf(noteStr)) - startIdx;

                string tempNote = DailyNote.Remove(startIdx, length);
                return tempNote.Insert(startIdx, newSubNote);
            }

            public string GetLifeTimeNoteString(string noteStr)
            {
                if (!LifeTimeNote.Contains(noteStr))
                    return "";
                try
                {
                    int startIdx = LifeTimeNote.IndexOf(noteStr) + noteStr.Length;
                    int length = LifeTimeNote.IndexOf(';', LifeTimeNote.IndexOf(noteStr)) - startIdx;

                    return LifeTimeNote.Substring(startIdx, length);
                }
                catch (System.Exception)
                {
                    return "";
                }
            }

            public int GetLifeTimeNoteCount(string noteStr)
            {
                if (!LifeTimeNote.Contains(noteStr))
                    return 0;
                try
                {
                    int startIdx = LifeTimeNote.IndexOf(noteStr) + noteStr.Length;
                    int length = LifeTimeNote.IndexOf(';', LifeTimeNote.IndexOf(noteStr)) - startIdx;

                    return int.Parse(LifeTimeNote.Substring(startIdx, length));
                }
                catch (System.Exception)
                {
                    return 999;
                }
            }

            public string UpdateLifeTimeNote(string noteStr, string newSubNote)
            {
                if (!LifeTimeNote.Contains(noteStr))
                    return LifeTimeNote + noteStr + newSubNote + ";";

                int startIdx = LifeTimeNote.IndexOf(noteStr) + noteStr.Length;
                int length = LifeTimeNote.IndexOf(';', LifeTimeNote.IndexOf(noteStr)) - startIdx;

                string tempNote = LifeTimeNote.Remove(startIdx, length);
                return tempNote.Insert(startIdx, newSubNote);
            }

            public bool CheckGetGrandOpenGift(int index)
            {
                string note = "GrandOpen" + index + ";";
                return LifeTimeNote.Contains(note);
            }

            public string UpdateGrandOpenNote(int index)
            {
                return LifeTimeNote + "GrandOpen" + index + ";";
            }

            public int GetCurrencyValue(CurrencyCode currencyType)
            {
                var currencyKey = currencyType.ToString();
                if (this.currencies.ContainsKey(currencyKey))
                {
                    return this.currencies[currencyKey];
                }
                return 0;
            }

            public bool CheckEnoughCurrencies(Dictionary<string, int> price)
            {
                foreach (var currencyTxt in price.Keys)
                {
                    var priceValue = price[currencyTxt];
                    if (this.currencies[currencyTxt] < priceValue)
                    {
                        return false;
                    }
                }
                return true;
            }

            public string GetAvatar()
            {
                string avatar = Avatar;
                if (string.IsNullOrEmpty(avatar))
                    return "DefaultAvatar";

                return avatar;
            }
        }
        public Gamer gamerData;
        #endregion

        public List<CharacterGamerData> charactersList { get; set; }
        public List<DiceGamerData> dicesList { get; set; }

        public System.Action CallSyncObjectNetwork;
        public System.Action CallbackUpdateFormationData;

        public void UpdateInfo(UserInfo other)
        {
            if (other == null)
                return;

            if (other.gamerData != null)
            {
                this.gamerData = other.gamerData;
            }
            if (other.charactersList != null)
            {
                this.charactersList = other.charactersList;
            }
            if (other.dicesList != null)
            {
                this.dicesList = other.dicesList;
            }
           
            if (CallSyncObjectNetwork != null)
            {
                CallSyncObjectNetwork.Invoke();
            }
        }

        public static string GetGameVersionString(int game_version)
        {

            int v = game_version;
            int v1 = v / 100;
            int v2 = (v % 100) / 10;
            int v3 = v % 10;
            return string.Format("v.{0}.{1}.{2}", v1, v2, v3);

        }

        /*public int GetDayFromRegist()
        {
            if (GamerData != null)
            {
                var startDate = GamerData.RegisterTime.Trim(TimeSpan.TicksPerDay);
                var currentDate = System.DateTime.Now.Trim(TimeSpan.TicksPerDay);

                var curDay = (int)((currentDate - startDate).TotalDays + 1);
                return curDay;
            }
            else
            {
                return 0;
            }
        }*/
    }
}
