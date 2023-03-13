using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBShare.Common;

namespace CBShare.Data
{
    public class CharacterGamerData
    {
        public long ID;
        public long GID;
        public CharacterCode code;
        public DateTime buyTime;
    }

    public class DiceGamerData
    {
        public long ID;
        public long GID;
        public DiceCode code { get; set; }
        public DateTime expiredTime { get; set; }
    }

    public class StarCardGamerData
    {
        public long ID { get; set; }
        public long GID { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public StarCardRank rank { get { return UtilsHelper.ParseEnum<StarCardRank>(this.name.Split('|')[0]); } }
        public CharacterCode characterCode { get { return UtilsHelper.ParseEnum<CharacterCode>(this.name.Split('|')[1]); } }
        public Dictionary<string, float> statsValue { get; set; }
        public DateTime createTime;

        public float GetStatValue(StarCardStat stat)
        {
            var statKey = stat.ToString();
            if (this.statsValue != null && this.statsValue.ContainsKey(statKey))
            {
                return this.statsValue[statKey];
            }
            return 0;
        }
    }

    public class PushNotifyData
    {
        public long GID;
        public int SettingFlag = 0;
        public string ArenaMessage = "";
        public string RiffBossMessage = "";
        public string PrivateMessage = "";
        public string DongNhanMessage = "";

        public bool IsEnabled(Setting setting)
        {
            return CommonHelper.CheckFlag(SettingFlag, (int)setting);
        }

        public void Switch(Setting setting)
        {
            if (IsEnabled(setting))
                SettingFlag = CommonHelper.DownFlag(SettingFlag, (int)setting);
            else
                SettingFlag = CommonHelper.RaiseFlag(SettingFlag, (int)setting);
        }

        public PushNotifyData() { }
        public PushNotifyData(long gid) { this.GID = gid; }

        public enum Setting
        {
            NONE = 0,
            AFK,
            MISSION,
            CHAT,
            RIFF_BOSS,
            ARENA,
            DONGNHAN,
            CONG_THANH_CHIEN
        }
    }
}
