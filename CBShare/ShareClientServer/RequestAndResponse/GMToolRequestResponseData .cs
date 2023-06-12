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
    public class GMToolGetUsersRequestData : RequestBase
    {
        
    }

    public class GMToolGetUsersResponseData : ResponseBase
    {
        public long GID;
        public UserInfo userInfo;
        public string accessToken;
        public string url;
        public string username;
    }
}
