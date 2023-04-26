using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBShare.Battle;
using CBShare.Configuration;
using CBShare.Data;
using CBShare.Common;

namespace CBShare.Data
{
    public class RequestBase
    {
        public long GID { get; set; }
        public string accessToken { get; set; }
        public int gameVersion { get; set; }
        public int configVersion { get; set; }
        public List<string> props = new List<string>();

        public void CheckPropsForBattle(UserInfo userInfo)
        {
            /*if (userInfo.ListCharacter == null)
                this.props.Add(GameRequests.PROPS_CHARACTER_DATA);
            if (userInfo.FormationData == null)
                this.props.Add(GameRequests.PROPS_FORMATION_DATA);
            if (userInfo.PentagramData == null)
                this.props.Add(GameRequests.PROPS_PENTAGRAM_DATA);*/
        }
    }

    public class ResponseBase
    {
        public string ErrorMessage { get; set; }
        public ERROR_CODE ErrorCode { get; set; }
        public ResponseBase()
        {
            ErrorCode = ERROR_CODE.OK;
        }
        public void Display(string msg)
        {
            ErrorCode = ERROR_CODE.DISPLAY_MESSAGE;
            ErrorMessage = msg;
        }
    }

    public class UpdateResourceConfigsRequest : RequestBase
    {

    }

    public class UpdateResourceConfigsResponseData : ResponseBase
    {
        public string AndroidConfigUrl;
        public uint AndroidConfigCRC;
        public string iOSConfigUrl;
        public uint iOSConfigCRC;
        public string pcConfigURL;
        public uint pcConfigCRC;
    }

    public class GetUserInfoRequest : RequestBase
    {
    }

    public class LoginRequestData : RequestBase
    {
        public string username;
        public string password;
        public bool isRegister;
        /*public string email;
        public string deviceID;
        public Platform platform;
        public int server;*/
    }

    public class LoginResponseData : ResponseBase
    {
        public long GID;
        public UserInfo userInfo;
        public string accessToken;
        public string url;
        public string username;
    }

    public class UpdateUserInfoResponseData : ResponseBase
    {
        public UserInfo userInfo;
    }

    public class ShowRewardAndUpdateUserInfoResponseData : ResponseBase
    {
        public UserInfo userInfo;
        public RewardConfig rewards;
    }

    public class ChangeNameRequestData : RequestBase
    {
        public string DisplayName { set; get; }
    }

    public class ChangeAvatarRequestData : RequestBase
    {
        public string SpriteName { set; get; }
    }

    public class ChangeNameResponseData : ResponseBase
    {
        public string DisplayName { set; get; }
        public UserInfo userInfo { get; set; }
    }

    public class ChooseItemRequest : RequestBase
    {
        public long itemID;
    }
}
