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

    public class GMToolThongKeNapGoldRequest : GMToolRequestBase
    {
        public long GID;
        public DateTime startTime;
        public DateTime endTime;
    }

    public class GMToolThongKeNapGoldResponse : ResponseBase
    {
        public int tongNap;
        public int shop;
        public int dangKy;
        public int gmTopup;
        public int daily;
        public int events;
        public int luckyWheel;
        public int ranking;
        public int giftCode;
        public int gameBonus;
        public List<GMToolThongKeNapGoldData> logsList;
    }

    public class GMToolThongKeNapGoldData
    {
        public int stt;
        public DateTime ngay;
        public int shop;
        public int register;
        public int gmTopup;
        public int luckyWheel;
        public int daily;
        public int events;
        public int ranking;
        public int giftcode;
        public int exp;
        public int xucxac;
        public int avatar;
        public int tongPhien;
        public int tong;
    }

    public class GMToolThongKeTieuGoldRequest : GMToolRequestBase
    {
        public long GID;
        public DateTime startTime;
        public DateTime endTime;
    }

    public class GMToolThongKeTieuGoldResponse : ResponseBase
    {
        public int tongTieu;
        public int beginner;
        public int pro;
        public int giaiDau;
        public int shop;
        public int spinGo;
        public int bonus;
        public List<GMToolThongKeTieuGoldData> logsList;
    }

    public class GMToolThongKeTieuGoldData
    {
        public int stt;
        public DateTime ngay;
        public int sovanBegin;
        public int userBegin;
        public int rematchBegin;
        public int botBegin;
        public int boosterBegin;

        public int sovanPro;
        public int userPro;
        public int rematchPro;
        public int botPro;
        public int boosterPro;

        public int ticketGiaiDau;
        public int boosterGiaiDau;
        public int shop;
        public int spinGo;
        public int gameBonus;
        public int gameRematch;

        public int tong;
    }

    public class GMToolThongKeNapDiamondRequest : GMToolRequestBase
    {
        public long GID;
        public DateTime startTime;
        public DateTime endTime;
    }

    public class GMToolThongKeNapDiamondResponse : ResponseBase
    {
        public int card;
        public int iap;
        public int register;
        public int ranking;
        public int gmTopup;
        public int daily;
        public int events;
        public int luckyWheel;
        public int giftCode;
        public int tong;
        public List<GMToolThongKeNapDiamondData> logsList;
    }

    public class GMToolThongKeNapDiamondData
    {
        public DateTime ngay;
        public int card;
        public int iap;
        public int register;
        public int ranking;
        public int gmTopup;
        public int daily;
        public int events;
        public int luckyWheel;
        public int giftcode;
        public int tong;
    }

    public class GMToolThongKeTieuDiamondRequest : GMToolRequestBase
    {
        public long GID;
        public DateTime startTime;
        public DateTime endTime;
    }

    public class GMToolThongKeTieuDiamondResponse : ResponseBase
    {
        public int gold;
        public int booster;
        public int dice;
        public int map;
        public int avatar;
        public int ticket;
        public int tong;
        public List<GMToolThongKeTieuDiamondData> logsList;
    }

    public class GMToolThongKeTieuDiamondData
    {
        public int stt;
        public DateTime ngay;
        public int gold;
        public int booster;
        public int dice;
        public int map;
        public int avatar;
        public int ticket;
        public int tong;
    }

    public class GMToolThongKeGiaiDauRequest : GMToolRequestBase
    {
        public DateTime startTime;
        public DateTime endTime;
    }

    public class GMToolThongKeGiaiDauResponse : ResponseBase
    {
        public int tongThu;
        public int tongTraoGiai;
        public int tongBonus;
        public int duaVaoPool;
        public int phiThuDuoc;
        public int pool;
        public List<GMToolThongKeTieuDiamondData> logsList;
    }

    public class GMToolThongKeGiaiDauData
    {
        public int stt;
        public int tonDau;
        public int soVanChoi;
        public int loaiVeTieuHao;
        public int soLuongVeTieuHao;
        public int loaiVePhatSinh;
        public int buyInVePhatSinh;
        public int soLuongVePhatSinh;
        public int rankingVePhatSinh;
        public int eventVePhatSinh;
        public int adminVePhatSinh;
        public int loaiVeConLai;
        public int soLuongVeConLai;
    }

    public class GMToolGetEventsListRequest : GMToolRequestBase
    {
        public DateTime startTime;
        public DateTime endTime;
    }

    public class GMToolGetEventsListResponse : ResponseBase
    {
        public List<GMToolEventData> eventsList;
    }

    public enum EventType
    {
        NONE = -1,
        Daily = 0,
        Festival = 1,
    }

    public class GMToolEventData
    {
        public int ID;
        public EventType type;
        public int soVanChoi;
        public int loaiVeTieuHao;
        public int soLuongVeTieuHao;
        public int loaiVePhatSinh;
        public int buyInVePhatSinh;
        public int soLuongVePhatSinh;
        public int rankingVePhatSinh;
        public int eventVePhatSinh;
        public int adminVePhatSinh;
        public int loaiVeConLai;
        public int soLuongVeConLai;
    }
}
