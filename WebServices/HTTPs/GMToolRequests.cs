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

        public static string RequestUserManager(string data)
        {
            var response = new GMToolUserManagerResponse();
            try
            {
                //var request = JsonMapper.ToObject<GMToolUserManagerRequest>(data);
                response.ErrorCode = ErrorCode.OK;
                response.totalUsersCount = GamerMongoDB.GetTotalGamersCount();
                return GetResponseStr(response);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                return GetErrorResponse(response, ErrorCode.DISPLAY_MESSAGE, ex.ToString());
            }
        }

        public static string RequestGetUsersList(string data)
        {
            var response = new GMToolGetUsersListResponse();
            try
            {
                var request = JsonMapper.ToObject<GMToolGetUsersListRequest>(data);
                var gamersList = GamerMongoDB.GetGamersList(request.fromIdx, request.toIdx);
                response.usersList = new List<GMToolUserData>();
                var i = 0;
                foreach (var gamerData in gamersList)
                {
                    i++;
                    var userData = new GMToolUserData()
                    {
                        index = i,
                        GID = gamerData.ID,
                        userName = gamerData.displayName,
                    };
                    response.usersList.Add(userData);
                }
                response.ErrorCode = ErrorCode.OK;
                return GetResponseStr(response);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                return GetErrorResponse(response, ErrorCode.DISPLAY_MESSAGE, ex.ToString());
            }
        }

        public static string RequestLockUser(string data)
        {
            var response = new GMToolLockUserResponse();
            try
            {
                var request = JsonMapper.ToObject<GMToolLockUserRequest>(data);
                response.ErrorCode = ErrorCode.OK;
                response.Message = "LOCK_USER_SUCCESS!";
                return GetResponseStr(response);
            }
            catch (Exception ex)
            {
                ExceptionLogMongoDB.add(ex.ToString());
                return GetErrorResponse(response, ErrorCode.DISPLAY_MESSAGE, ex.ToString());
            }
        }
    }
}