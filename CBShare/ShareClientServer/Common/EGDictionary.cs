using System.Collections.Generic;

public enum EGTextKey
{
    SuccessfulResponse =0,
    KhongDuTheLuc,
    ChapterChuaMo,
    NhiemVuTruocChuaXong,
    RequestKhongHopLe,
    ChapterKhongLock,
    KhongDuGem,
    ThamSoKhongHopLe,
    KhongDuBP,
    ParentNotUnlocked,
    AlreadyUnlocked,
    Chapter1ChuaClear,
    PVPKhongTimThayDoiThu,
    WeeklyEventNotStart,
    CantVerifyPurchase,
    OrderIDExisted,
    MissingPayment,
    ChuaUnlockSkillNao,
    KhongDungLoaiBoss,
    ChuaDenGioBossH,
    ChuaDenGioBossM,
    ChapterChuaMoParam,
    NhiemVuChuaXongParam,
    MaxLuotGetGemFromAd,
}

public class EGDictionary
{
    public static EGDictionary Instance = new EGDictionary();

    public EGDictionary()
    {
    }

    public Dictionary<EGTextKey, string> GetDic()
    {
        return English;
    }

    public string GetString(EGTextKey key)
    {
        return GetDic()[key];
    }

      
    public static readonly Dictionary<EGTextKey, string> English = new Dictionary<EGTextKey, string>()
    {
        { EGTextKey.SuccessfulResponse, "OK" },
        { EGTextKey.KhongDuTheLuc, "Not enough stamina" },
        { EGTextKey.ChapterChuaMo, "Chapter not open" },
        { EGTextKey.NhiemVuTruocChuaXong, "Pre-mission not clear" },
        { EGTextKey.ChapterChuaMoParam, "Chapter {0} not open" },
        { EGTextKey.NhiemVuChuaXongParam, "Mission {0} chapter {1} not clear" },
        { EGTextKey.RequestKhongHopLe, "Request's invalid" },
        { EGTextKey.ChapterKhongLock, "Chapter not lock" },
        { EGTextKey.KhongDuGem, "Not enough gem" },
        { EGTextKey.ThamSoKhongHopLe, "Invalid request" },
        { EGTextKey.KhongDuBP, "Not enough Battle Point" },
        { EGTextKey.ParentNotUnlocked, "Previous tier not fully unlocked" },
        { EGTextKey.AlreadyUnlocked, "Already unlocked" },
        { EGTextKey.Chapter1ChuaClear, "Chapter1 isn't cleared?" },
        { EGTextKey.PVPKhongTimThayDoiThu, "Cant find any competitor"},
        { EGTextKey.WeeklyEventNotStart, "Event isn't started"},
        { EGTextKey.CantVerifyPurchase, "Can't verify purchase with store"},
        { EGTextKey.OrderIDExisted, "OrderID already exist!"},
        { EGTextKey.MissingPayment, "Cant connect with store server, we'll try again in some mins"},
        { EGTextKey.ChuaUnlockSkillNao, "No Upgrades found. Reset couldn't be done."},
        { EGTextKey.KhongDungLoaiBoss, "Boss is not valid" },
        { EGTextKey.ChuaDenGioBossH, "Boss is coming back in {0:00} hours" },
        { EGTextKey.ChuaDenGioBossM, "Boss is coming back in {0:00} minutes" },
        { EGTextKey.MaxLuotGetGemFromAd, "Only {0} times once day. No free gem more until next day" },
    };
}
