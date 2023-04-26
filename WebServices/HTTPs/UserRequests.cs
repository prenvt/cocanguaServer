using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using LitJson;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System.Threading;
using System.Data;
using CTPServer.MongoDB;
using CBShare.Configuration;
using CBShare.Battle;
using Server.DatabaseUtils;
using CBShare;
using System.Text;
using MongoDB.Bson;
using CBShare.Data;
using CBShare.Common;

namespace WebServices
{
    public partial class BaseWebService
    {
        /*public static string RequestUpdateResourcesConfig(string data)
        {
            UpdateResourceConfigsResponseData response = new UpdateResourceConfigsResponseData();
            try
            {
                UpdateResourceConfigsRequest request = LitJson.JsonMapper.ToObject<UpdateResourceConfigsRequest>(data);
                response.resourceConfigs = ConfigManager.Instance.ResourcesConfigs;
                response.AndroidConfigUrl = ConfigManager.Instance.OtherCfg.AndroidConfigUrl;
                response.AndroidConfigCRC = ConfigManager.Instance.OtherCfg.AndroidConfigCRC;
                response.iOSConfigUrl = ConfigManager.Instance.OtherCfg.iOSConfigUrl;
                response.iOSConfigCRC = ConfigManager.Instance.OtherCfg.iOSConfigCRC;
                response.pcConfigURL = ConfigManager.Instance.OtherCfg.pcConfigURL;
                response.pcConfigCRC = ConfigManager.Instance.OtherCfg.pcConfigCRC;
                return GetResponseStr(response);
            }
            catch (Exception ex)
            {
                ExceptionLogMongo.add(ex.ToString());
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }*/

        public static string RequestLogin(string data)
        {
            var response = new LoginResponseData();
            try
            {
                var request = LitJson.JsonMapper.ToObject<LoginRequestData>(data);
                /*DataTable userServerDataTable = UserServerUI.GetByUserName(username);
                bool IsWhiteListUser = false;
                if (userServerDataTable.Rows.Count > 0)
                {
                    IsWhiteListUser = Convert.ToInt32(userServerDataTable.Rows[0]["Whitelist"]) > 0;
                }
                ServerData svData = ServerUI.GetByID(request.server);
                if (svData.Status.Contains("MAINTAINCE") && !IsWhiteListUser)
                {
                    return GetErrorResponse(response, ERROR_CODE.SERVER_MAINTANCE);
                }
                if (request.server <= 0)
                {
                    return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InvalidRequest"));
                }*/

                if (string.IsNullOrEmpty(request.username))
                {
                    return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InvalidRequest"));
                }

                var userLoginData = UserLoginMongoDB.GetByUserName(request.username);
                if (request.isRegister)
                {
                    if (userLoginData != null) //UserExist
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("UserExist"));
                    }
                    long gid = CounterMongoDB.GetNextValue("GID");
                    userLoginData = new UserLoginData()
                    {
                        GID = gid,
                        username = request.username,
                        password = request.password
                    };
                    UserLoginMongoDB.Insert(userLoginData);
                }
                else
                {
                    if (userLoginData == null)
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("UserNotExist"));
                    }
                    if (!userLoginData.password.Equals(request.password))
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InvalidPassword"));
                    }
                }

                response.GID = userLoginData.GID;
                // gen new access token
                string access_token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                response.accessToken = access_token;
                response.userInfo = GameManager.GetUserInfo(userLoginData.GID, GameRequests.DEFAULT_PROPS, request.username);
                response.userInfo.gamerData.accessToken = access_token;
                response.username = request.username;
                GamerMongoDB.Save(response.userInfo.gamerData);
                /*if (response.userInfo.GamerData.BanTime > DateTime.Now)
                {
                    if (!string.IsNullOrEmpty(response.userInfo.GamerData.BanMessage))
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, response.userInfo.GamerData.BanMessage);
                    }
                    else
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("BannedUserMessage"));
                    }
                }*/
                /*if (response.userInfo.alertSystemData == null)
                    response.userInfo.alertSystemData = AlertSystemMongoDB.GetByGID(gid);
                if (response.userInfo.timeEvents == null)
                    response.userInfo.timeEvents = TimeEventMutexMongoDB.Get();*/

                response.ErrorCode = ERROR_CODE.OK;
                return GetResponseStr(response);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }

        /*public static string RequestLoginAndGetServersList(string data)
        {
            LoginResponseData response = new LoginResponseData();
            try
            {
                LoginRequestData request = LitJson.JsonMapper.ToObject<LoginRequestData>(data);
                if (request.platform == Platform.ANDROID)
                {
                    response.vesion = GameManager.SERVER_VERSION_ANDROID;
                    if (request.GameVersion < GameManager.SERVER_VERSION_ANDROID)
                    {
                        response.url = ConfigManager.Instance.OtherCfg.androidUrl;
                        return GetErrorResponse(response, ERROR_CODE.GAME_VERSION_INVALID);
                    }
                }
                else if (request.platform == Platform.IOS)
                {
                    response.vesion = GameManager.SERVER_VERSION_IOS;
                    if (request.GameVersion < GameManager.SERVER_VERSION_IOS)
                    {
                        response.url = ConfigManager.Instance.OtherCfg.iosUrl;
                        return GetErrorResponse(response, ERROR_CODE.GAME_VERSION_INVALID);
                    }
                }
                else
                {
                    response.vesion = GameManager.SERVER_VERSION_PC;
                    if (request.GameVersion < GameManager.SERVER_VERSION_PC)
                    {
                        response.url = "http://hikergames.com/";
                        return GetErrorResponse(response, ERROR_CODE.GAME_VERSION_INVALID);
                    }
                }

                var username = request.userName;
                if (string.IsNullOrEmpty(username))
                {
                    return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InvalidRequest"));
                }

                if (request.loginType == LoginType.SOHA)
                {
                    if (!VerifySohaAccount(request.sohaAccessToken))
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InvalidRequest"));
                    }
                    username = string.Format("sh_{0}", request.userName);
                }

                DataTable userServerDataTable = UserServerUI.GetByUserName(username);
                bool IsWhiteListUser = false;
                if (userServerDataTable.Rows.Count > 0)
                {
                    IsWhiteListUser = Convert.ToInt32(userServerDataTable.Rows[0]["Whitelist"]) > 0;
                }
                //Check Maintenance
                bool IgnoreCheckMaintance = false;
                IgnoreCheckMaintance = IsWhiteListUser;

                bool isSohaGM = ConfigManager.Instance.CheckUserIsSohaGM(request.userName);
                if (request.loginType == LoginType.SOHA)
                {
                    username = string.Format("sh_{0}", request.userName);
                    var allSohaProductsList = GetSohaProductLists(request.sohaAccessToken, request.platform);
                    if (allSohaProductsList != null)
                    {
                        for (int i = 0; i < allSohaProductsList.Count; i++)
                        {
                            var pack = allSohaProductsList[i];
                            var packCodeName = IAPConfig.GetPackCodeNameByProductID(pack.order_info);
                            if (packCodeName.Contains("battlepass"))
                            {
                                response.battlepassPacksList.Add(pack);
                            }
                            else if (packCodeName.Contains("lenhbaiknb"))
                            {
                                //response.lenhbaiKNBPacksList.Add(pack);
                            }
                            else if (packCodeName.Contains("The"))
                            {
                                response.thethangPacksList.Add(pack);
                            }
                            else
                            {
                                response.ngocPacksList.Add(pack);
                            }
                        }
                    }
                }
                if (request.loginType != LoginType.SOHA || request.platform == Platform.STANDALONE)
                {
                    for (int i = 0; i < ConfigManager.Instance.IAPConfig.NgocPacks.Count; i++)
                    {
                        var ngocPack = ConfigManager.Instance.IAPConfig.NgocPacks[i];
                        if (ngocPack.store == "Android")
                        {
                            var pack = new SohaIAPPack();
                            pack.image = "";
                            pack.order_info = ngocPack.productID;
                            pack.point = ngocPack.ngocAmount.ToString();
                            pack.price = ngocPack.price.ToString();
                            response.ngocPacksList.Add(pack);
                        }
                    }

                    for (int i = 0; i < ConfigManager.Instance.IAPConfig.EpicPasses.Count; i++)
                    {
                        var passCfg = ConfigManager.Instance.IAPConfig.EpicPasses[i];
                        var pack = new SohaIAPPack();
                        pack.image = "";
                        pack.order_info = "com.game.sg293." + passCfg.CodeName;
                        pack.point = "0";
                        pack.price = passCfg.Price.ToString();
                        response.battlepassPacksList.Add(pack);
                    }
                }

                var allServerDatas = ServerUI.GetAllServerData(isSohaGM);
                //get last server user playing
                int playingServerID = 0;
                if (userServerDataTable.Rows.Count > 0)
                {
                    playingServerID = Convert.ToInt32(userServerDataTable.Rows[0]["ServerIDPlaying"]);
                }

                List<ServerData> serversPlayedList = ServerUI.GetServerByUsername(username);
                if (playingServerID <= 0)
                {
                    if (serversPlayedList.Count == 0)
                    {
                        playingServerID = allServerDatas[allServerDatas.Count - 1].ID;
                    }
                    else if (serversPlayedList.Exists(e => e.ID == playingServerID) == false)
                    {
                        playingServerID = serversPlayedList[serversPlayedList.Count - 1].ID;
                    }
                }

                if (playingServerID > 0)
                {
                    ServerData svData = ServerUI.GetByID(playingServerID);

                    if (svData != null)
                    {
                        response.ServerIDPlaying = playingServerID;
                    }
                }
                response.ListCurrentServer = serversPlayedList;
                response.ListAllServer = allServerDatas;
                response.userName = username;
                response.ErrorCode = ERROR_CODE.OK;
                return GetResponseStr(response);
            }
            catch (Exception ex)
            {
                ExceptionLogMongo.add(ex.ToString());
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }*/

        /*public static string RequestLoginByServerID(string data)
        {
            LoginByServerResponseData response = new LoginByServerResponseData();
            try
            {
                LoginByServerRequestData request = LitJson.JsonMapper.ToObject<LoginByServerRequestData>(data);
                var username = request.userName;

                DataTable userServerDataTable = UserServerUI.GetByUserName(username);
                bool IsWhiteListUser = false;
                if (userServerDataTable.Rows.Count > 0)
                {
                    IsWhiteListUser = Convert.ToInt32(userServerDataTable.Rows[0]["Whitelist"]) > 0;
                }

                ServerData svData = ServerUI.GetByID(request.server);
                if (svData.Status.Contains("MAINTAINCE") && !IsWhiteListUser)
                {
                    return GetErrorResponse(response, ERROR_CODE.SERVER_MAINTANCE);
                }

                if (request.server <= 0)
                {
                    return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InvalidRequest"));
                }

                if (string.IsNullOrEmpty(username))
                {
                    return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InvalidRequest"));
                }

                var email = request.email;
                string device_id = request.deviceID;
                int serverID = request.server;
                var user_login_data = UserUI.GetLoginDataByUsername(username, serverID);

                long gid = 0;
                if (user_login_data != null) //not first time login
                {
                    gid = user_login_data.GID;
                    UserUI.CacheServerID(gid, serverID);
                    if (string.IsNullOrEmpty(user_login_data.Email) && !string.IsNullOrEmpty(email))
                    {
                        UserUI.UserUpdateEmail(username, email);
                    }
                }
                else //first time login
                {
                    int numberOfArrowEffectedByQueryCommand = UserUI.UserAddNew(username, email, serverID);
                    if (numberOfArrowEffectedByQueryCommand <= 0)
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, "Can't Add new User");
                    }

                    gid = GamerUI.GamerAddNew("");
                    if (gid <= 0)
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, "Can't Add new Gamer");
                    }
                    UserUI.UserUpdateGID(username, gid, serverID);
                }

                UserUI.UserUpdateDeviceID(username, device_id);
                UserServerUI.AddNewOrUpdate(username, serverID, device_id);
                response.GID = gid;

                // gen new access token
                string access_token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                GamerUI.UpdateAccessToken(gid, access_token);
                //GameManager.UpdateAccessToken(gid, access_token);

                response.accessToken = access_token;
                response.userInfo = GameManager.GetUserInfo(gid, GameRequests.LOGIN_INFO_PROPS, username);
                if (response.userInfo.GamerData.BanTime > DateTime.Now)
                {
                    if (!string.IsNullOrEmpty(response.userInfo.GamerData.BanMessage))
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, response.userInfo.GamerData.BanMessage);
                    }
                    else
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("BannedUserMessage"));
                    }
                }
                if (response.userInfo.alertSystemData == null)
                    response.userInfo.alertSystemData = AlertSystemMongoDB.GetByGID(gid);
                if (response.userInfo.timeEvents == null)
                    response.userInfo.timeEvents = TimeEventMutexMongoDB.Get();

#if VER_5_5
                if (response.userInfo.GamerData.offlineDay > 0)
                {
                    var offlineRewardData = ConfigManager.Instance.OtherCfg.offlineRewards.FindLast(e => e.offlineDay <= response.userInfo.GamerData.offlineDay);
                    if (offlineRewardData != null)
                    {
                        var offlineReward = offlineRewardData.reward;
                        GameManager.SetReward(gid, response.userInfo, offlineReward, "OfflineReward_" + offlineRewardData.offlineDay);
                        MailContent mailContent = new MailContent();
                        mailContent.reward = offlineReward.Rewards;
                        mailContent.text = Localization.Get("OfflineRewardMail");
                        MailUI.MailAddNew(gid, mailContent, MailStatus.Received);
                    }
                }
#endif

                var resetGamerData = ResetGamerDataMongoDB.GetByGID(gid);
                response.ErrorCode = ERROR_CODE.OK;
                return GetResponseStr(response);
            }
            catch (Exception ex)
            {
                ExceptionLogMongo.add(ex.ToString());
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }*/

        public static string GetUserInfo(string data)
        {
            var response = new UpdateUserInfoResponseData();
            try
            {
                var request = JsonMapper.ToObject<GetUserInfoRequest>(data);
                ERROR_CODE errorCode = GameManager.CheckRequestValidation(request, false);
                if (errorCode != ERROR_CODE.OK)
                {
                    return GetErrorResponse(response, errorCode);
                }
                lock (UserLoginMongoDB.GetLockObj(request.accessToken))
                {
                    long gid = request.GID;
                    if (GamerMongoDB.checkAccessToken(gid, request.accessToken))
                    {
                        UserInfo uInfo = GameManager.GetUserInfo(gid, request.props);
                        response.userInfo = uInfo;
                        response.ErrorCode = ERROR_CODE.OK;
                        return GetResponseStr(response);
                    }
                    else
                    {
                        return GetErrorResponse(response, ERROR_CODE.ACCESS_TOKEN_INVALID);
                    }
                }
            }
            catch (Exception ex)
            {
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }

        public static string ChangeDisplayName(string data)
        {
            var response = new ChangeNameResponseData();

            try
            {
                var request = JsonMapper.ToObject<ChangeNameRequestData>(data);
                ERROR_CODE errorCode = GameManager.CheckRequestValidation(request, false);
                if (errorCode != ERROR_CODE.OK)
                {
                    return GetErrorResponse(new ResponseBase(), errorCode);
                }

                lock (UserLoginMongoDB.GetLockObj(request.accessToken))
                {
                    long gid = request.GID;
                    if (!GamerMongoDB.checkAccessToken(gid, request.accessToken, true))
                    {
                        return GetErrorResponse(response, ERROR_CODE.ACCESS_TOKEN_INVALID);
                    }
                    if (ConfigManager.instance.CheckBanDisplayName(request.DisplayName))
                    {
                        string msg = string.Format(Localization.Get("DISPLAY_NAME_INVALID"), request.DisplayName);
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, msg);
                    }
                    response.userInfo = GameManager.GetUserInfo(gid, new List<string>(){
                            GameRequests.PROPS_GAMER_DATA });

                    if (response.userInfo.gamerData.displayName.Equals(request.DisplayName))
                    {
                        response.DisplayName = request.DisplayName;
                        return GetResponseStr(response);
                    }

                    if (GamerMongoDB.FindByDisplayName(request.DisplayName) != null)
                    {
                        string msg = string.Format(Localization.Get("DISPLAY_NAME_EXIST"), request.DisplayName);
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, msg);
                    }

                    /*if (response.userInfo.GamerData.ChangeDisplayName >= 1)
                    {
                        if ((DateTime.Now - response.userInfo.GamerData.LastTimeChangeDisplayName).TotalHours < ConfigManager.Instance.OtherCfg.ChangeDisplayNameDelayTime)
                        {
                            string msg = "INVALID_REQUEST";
                            return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, msg);
                        }

                        RewardConfig resource = ConfigHelper.GetChangeDisplayNamePrice(response.userInfo.GamerData.ChangeDisplayName);
                        if (!GameManager.CanGetReward(gid, response.userInfo, resource))
                        {
                            //response.updateInfo = userInfo;
                            return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InsufficientResource"));
                        }
                        GameManager.SetReward(gid, response.userInfo, resource, "ChangeDisplayName");
                    }*/
                    response.userInfo.gamerData.displayName = request.DisplayName;
                    response.userInfo.gamerData.ChangeDisplayName++;
                    response.userInfo.gamerData.lastChangeDisplayName = DateTime.Now;
                    GamerMongoDB.Save(response.userInfo.gamerData);
                    response.DisplayName = request.DisplayName;
                    return GetResponseStr(response);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }

        public static string ChangeAvatar(string data)
        {
            var response = new UpdateUserInfoResponseData();
            try
            {
                var request = JsonMapper.ToObject<ChangeAvatarRequestData>(data);
                ERROR_CODE errorCode = GameManager.CheckRequestValidation(request);
                if (errorCode != ERROR_CODE.OK)
                {
                    return GetErrorResponse(new ResponseBase(), errorCode);
                }

                lock (UserLoginMongoDB.GetLockObj(request.accessToken))
                {
                    long gid = request.GID;
                    if (!GamerMongoDB.checkAccessToken(gid, request.accessToken, true))
                    {
                        return GetErrorResponse(response, ERROR_CODE.ACCESS_TOKEN_INVALID);
                    }
                    UserInfo userInfo = GameManager.GetUserInfo(gid, new List<string>()
                        {
                            GameRequests.PROPS_GAMER_DATA });

                    if (userInfo == null)
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("DATABASE_ERROR"));
                    }

                    userInfo.gamerData.Avatar = request.SpriteName;
                    /*GamerUI.GamerUpdateAvatar(gid, userInfo.GamerData.Avatar);
                    GamerClanWarMongoDB.Update_avatar(gid, userInfo.GamerData.Avatar);
                    BXHSkyTowerMongoDB.Update_avatar(gid, userInfo.GamerData.Avatar);*/

                    response.userInfo = userInfo;
                    return GetResponseStr(response);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }

        public static string RequestChooseCharacter(string data)
        {
            var response = new UpdateUserInfoResponseData();
            try
            {
                var request = JsonMapper.ToObject<ChooseItemRequest>(data);
                ERROR_CODE errorCode = GameManager.CheckRequestValidation(request, false);
                if (errorCode != ERROR_CODE.OK)
                {
                    return GetErrorResponse(response, errorCode);
                }
                lock (UserLoginMongoDB.GetLockObj(request.accessToken))
                {
                    long gid = request.GID;
                    if (!GamerMongoDB.checkAccessToken(gid, request.accessToken))
                    {
                        return GetErrorResponse(response, ERROR_CODE.ACCESS_TOKEN_INVALID);
                    }
                    
                    var userInfo = GameManager.GetUserInfo(gid, new List<string>() {
                        GameRequests.PROPS_GAMER_DATA,
                        GameRequests.PROPS_CHARACTER_DATA
                    });
                    var characterData = userInfo.charactersList.Find(e => e.ID == request.itemID);
                    if (characterData == null)
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InvalidRequest"));
                    }
                    //userInfo.gamerData.currentCharacter = characterData.code;
                    GamerMongoDB.Save(userInfo.gamerData);

                    response.userInfo = userInfo;
                    response.ErrorCode = ERROR_CODE.OK;
                    return GetResponseStr(response);
                }
            }
            catch (Exception ex)
            {
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }

        public static string RequestChooseDice(string data)
        {
            var response = new UpdateUserInfoResponseData();
            try
            {
                var request = JsonMapper.ToObject<ChooseItemRequest>(data);
                ERROR_CODE errorCode = GameManager.CheckRequestValidation(request, false);
                if (errorCode != ERROR_CODE.OK)
                {
                    return GetErrorResponse(response, errorCode);
                }
                lock (UserLoginMongoDB.GetLockObj(request.accessToken))
                {
                    long gid = request.GID;
                    if (!GamerMongoDB.checkAccessToken(gid, request.accessToken))
                    {
                        return GetErrorResponse(response, ERROR_CODE.ACCESS_TOKEN_INVALID);
                    }

                    var userInfo = GameManager.GetUserInfo(gid, new List<string>() {
                        GameRequests.PROPS_GAMER_DATA,
                        GameRequests.PROPS_DICE_DATA
                    });
                    var diceData = userInfo.dicesList.Find(e => e.ID == request.itemID);
                    if (diceData == null)
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("InvalidRequest"));
                    }
                    userInfo.gamerData.currentDice = diceData.code;
                    GamerMongoDB.Save(userInfo.gamerData);

                    response.userInfo = userInfo;
                    response.ErrorCode = ERROR_CODE.OK;
                    return GetResponseStr(response);
                }
            }
            catch (Exception ex)
            {
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }

        #region Use Item
        /*public static string UseItem(string data)
        {
            var response = new UseItemResponse();
            try
            {
                var request = JsonMapper.ToObject<UseItemRequest>(data);

                ERROR_CODE errorCode = GameManager.CheckRequestValidation(request, false);
                if (errorCode != ERROR_CODE.OK)
                {
                    return GetErrorResponse(new ResponseBase(), errorCode);
                }

                lock (UserLoginMongoDB.GetLockObj(request.AccessToken))
                {
                    long gid = request.GID;
                    if (!GamerMongoDB.checkAccessToken(gid, request.AccessToken, true))
                    {
                        return GetErrorResponse(response, ERROR_CODE.ACCESS_TOKEN_INVALID);
                    }
                    UserInfo userInfo = GameManager.GetUserInfo(gid, new List<string>()
                    {
                             GameRequests.PROPS_ITEM_DATA, GameRequests.PROPS_AFK_DATA
                    });

                    if (userInfo == null)
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, Localization.Get("DATABASE_ERROR"));
                    }

                    RewardConfig outReward;
                    string result = GameManager.UseItem(gid, userInfo, request.CodeNameItem, request.quantity, out outReward, request.useAll);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, result);
                    }
                    response.reward = outReward;
                    response.ErrorCode = ERROR_CODE.OK;
                    response.CodeNameItem = request.CodeNameItem;
                    response.userInfo = userInfo;
                    return GetResponseStr(response);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogMongo.add(ex.ToString());
                return GetErrorResponse(response, ERROR_CODE.DISPLAY_MESSAGE, ex.ToString());
            }
        }*/
        #endregion
    }
}