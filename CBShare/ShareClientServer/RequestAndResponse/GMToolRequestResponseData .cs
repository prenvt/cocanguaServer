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
        public int fromIdx;
        public int toIdx;
    }

    public class GMToolGetUsersListResponse : ResponseBase
    {
        public List<GMToolUserData> usersList;
    }

    public class GMToolUserData
    {
        public int index;
        public long GID;
        public string userName;
        public string displayName;
        public string phone;
        public string email;
        public DateTime registerTime;
        public int money;
        public int gold;
        public bool locking;
    }

    public class GMToolLockUserRequest : GMToolRequestBase
    {
        public long gid;
    }

    public class GMToolLockUserResponse : ResponseBase
    {
    }
}
