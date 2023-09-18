using CBShare.Configuration;
using System;
using System.Collections.Generic;

namespace CBShare.Data
{
    public class GMToolRequestBase
    {

    }

    public class GMToolUserManagerRequest : GMToolRequestBase
    {
    }

    public class GMToolUserManagerResponse : ResponseBase
    {
        public int totalUsersCount;
    }

    public class GMToolGetUsersListRequest : GMToolRequestBase
    {
        public int fromIdx;
        public int toIdx;
    }

    public class GMToolGetUsersListResponse : ResponseBase
    {
        public List<GMToolUserData> usersList;
    }

    public class GMToolUserData
    {
        public int index;
        public long GID;
        public string userName;
        public string displayName;
        public string phone;
        public string email;
        public DateTime registerTime;
        public int money;
        public int gold;
        public bool locking;
    }

    public class GMToolLockUserRequest : GMToolRequestBase
    {
        public long gid;
    }

    public class GMToolLockUserResponse : ResponseBase
    {

    }

    public class GMToolSendRewardToUserRequest : GMToolRequestBase
    {
        public long gid;
        public RewardConfig reward;
    }

    public class GMToolSendRewardToUserResponse : ResponseBase
    {
    }

    public enum BattleLogType
    {
        NONE = -1,
        All = 0,
        Playing = 1
    }

    public class GMToolDashboardBattleLogData
    {
        public int index;
        public int roomID;
        public int point;
        public DateTime startTime;
        public DateTime endTime;
        public string userName;
        public int gold;
        public int result;
        public bool isFinished;
    }

    public class GMToolGetDashboardBattleLogsRequest : GMToolRequestBase
    {
        public BattleLogType logType;
        public DateTime startTime;
        public DateTime endTime;
    }

    public class GMToolGetDashboardBattleLogsResponse : ResponseBase
    {
        public int totalLogsCount;
        public int totalBetGold;
        public List<GMToolDashboardBattleLogData> battleLogsList;
    }

    public class GMToolGetGamerBattleLogsRequest : GMToolRequestBase
    {
        public long GID;
        public DateTime startTime;
        public DateTime endTime;
    }

    public class GMToolGamerBattleLogData
    {
        public int index;
        public int roomID;
        public int point;
        public DateTime startTime;
        public DateTime endTime;
        public BattleType roomMode;
        public int asset;
        public int bet;
        public int booster;
        public int rematch;
        public int result;
        public int movePoint;
        public int rematchBonus;
        public int diceBonus;
        public int mapBonus;
        public int avatarBonus;
        public int sum;
    }

    public class GMToolGetGamerBattleLogsResponse : ResponseBase
    {
        public int totalLogsCount;
        public List<GMToolGamerBattleLogData> battleLogsList;
    }
}
