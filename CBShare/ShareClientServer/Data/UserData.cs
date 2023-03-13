using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBShare.Data
{
    public class UserData
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public long GID { get; set; }
        public string Email { get; set; }
        public DateTime RegisterTime { get; set; }
    }

    public class AlertSystem
    {
        public long GID;
        public bool newLuanKiem = false;
        public bool newMail = false;
        public bool newFriend = false;
        public bool newChat = false;
        public bool newSecretOpened = false;
        public bool supportTeamChanged = false;
        public bool newOffer = false;
        public bool newRequestJoinClan = false;
        public bool newRiftReward = false;
        public bool khamPha = false;
        public bool newSohaPayment = false;

        public bool GetAlertStatus(AlertType type)
        {
            switch (type)
            {
                case AlertType.LuanKiem:
                    return newLuanKiem;
                case AlertType.Friend:
                    return newFriend;
                case AlertType.Mail:
                    return newMail;
                case AlertType.Chat:
                    return newChat;
                case AlertType.SecretTreasue:
                    return newSecretOpened;
                case AlertType.SupportTeamChanged:
                    return supportTeamChanged;
                case AlertType.NewOffer:
                    return newOffer;
                case AlertType.NewJoinClan:
                    return newRequestJoinClan;
                case AlertType.RiftBossClaimReward:
                    return newRiftReward;
                case AlertType.KhamPha:
                    return khamPha;
            }

            return false;
        }

        public void SetAlert(AlertType type, bool value = false)
        {
            switch (type)
            {
                case AlertType.LuanKiem:
                    newLuanKiem = value;
                    break;
                case AlertType.Friend:
                    newFriend = value;
                    break;
                case AlertType.Mail:
                    newMail = value;
                    break;
                case AlertType.Chat:
                    newChat = value;
                    break;
                case AlertType.SecretTreasue:
                    newSecretOpened = value;
                    break;
                case AlertType.SupportTeamChanged:
                    supportTeamChanged = value;
                    break;
                case AlertType.NewOffer:
                    newOffer = value;
                    break;
                case AlertType.NewJoinClan:
                    newRequestJoinClan = value;
                    break;
                case AlertType.RiftBossClaimReward:
                    newRiftReward = value;
                    break;
                case AlertType.KhamPha:
                    khamPha = value;
                    break;
            }
        }

        public enum AlertType
        {
            LuanKiem,
            Friend,
            Mail,
            Chat,
            Quest,
            DailyQuest,
            WeeklyQuest,
            Achievement,
            PurchaseBonus,
            NewOffer,
            FastReward,
            SupportTeamChanged,
            SecretTreasue,
            ClanUpdate,
            VIPReward,
            Bounty,
            FirstPurchaseReward,
            NewJoinClan,
            Inventory,
            SupportTeamAvailable,
            RiftBossClaimReward,
            ClanDailyBoss,
            FreeDailyExchange,
            Dungeon,
            RiftBossOpen,
            ScrollSummon,
            HighfiveSummon,
            PentagramSlot,
            AccendAvailable,
            Clanwar,
            FormationLuanKiem,
            PickLane,
            FreeDailyDeal,
            HuyetChien,
            NewChapter,
            GrandOpenGift,
            DonDau,
            DongNhan,
            Events,
            NewOfferInMainScreen,
            KhamPha,
            DailyBattlePass,
            WeeklyBattlePass,
            ThuThachTanThu,
            TamBao,
            ThatTinhDai,
            DoiHinhLK3,
            NhanThuongLienDauRank1,
            NhanThuongLienDauRank2,
            DHLK3_1,
            DHLK3_2,
            DHLK3_3,
            GiaoDichKNB,
#if VER_5_0
            BossKyLan,
            HiepKhachDao,
#endif
#if VER_5_5
            CheckIn30Days,
            TaiXuatGiangHo,
#endif
#if VER_6_0
            DonDau60,
            ThuongUuDaiChieuMo,
            CongThanhChien,
#endif
#if VER_8_0
            TongKim,
#endif
        }
    }
}
