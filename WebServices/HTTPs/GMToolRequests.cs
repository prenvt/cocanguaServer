using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using CTPServer.MongoDB;
using CBShare.Configuration;
using CBShare.Data;
using CBShare.Common;

namespace WebServices
{
    public partial class BaseWebService
    {
        public static string GetUsersList(string data)
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
    }
}