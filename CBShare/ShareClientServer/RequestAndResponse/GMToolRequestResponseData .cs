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
    public class GMToolRequestBase
    {

    }

    public class GMToolUserManagerRequest : GMToolRequestBase
    {
    }

    public class GMToolGetUsersListRequest : GMToolRequestBase
    {
        
    }

    public class GMToolGetUsersListResponse : ResponseBase
    {
       
    }

    public class GMToolUserData
    {
        public int index;
        public int GID;
        public string userName;
        public string displayName;
        public string phone;
        public string email;
        public DateTime registerTime;
        public int money;
        public int gold;
    }
}
