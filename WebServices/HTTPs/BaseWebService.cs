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
using CBShare.Data;
using CBShare.Common;
using System.Text;
using System.Threading.Tasks;

namespace WebServices
{
    public partial class BaseWebService
    {
        static string GetResponseStr(ResponseBase response)
        {
            return JsonMapper.ToJson(response);
        }

        static string GetErrorResponse(ResponseBase response, ErrorCode code, string str = null)
        {
            response.ErrorCode = code;
            response.ErrorMessage = str;
            return GetResponseStr(response);
        }
    }
}
