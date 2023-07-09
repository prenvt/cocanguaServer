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
