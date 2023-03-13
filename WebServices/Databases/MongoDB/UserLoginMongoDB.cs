using System;
using System.Linq;
using LitJson;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Server.DatabaseUtils;
using CBShare.Data;
using System.Collections.Generic;

namespace CTPServer.MongoDB
{
    public class UserLoginMongoDB
    {
        public static void RegisterClass()
        {

            BsonClassMap.RegisterClassMap<UserLoginData>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.username));
                cm.SetIgnoreExtraElements(true);
            });
        }
        public static readonly string DataCol = "Users";

        public static IMongoCollection<UserLoginData> GetDataCollection()
        {
            var col = MongoDBHelper.GetDatabase().GetCollection<UserLoginData>(DataCol);
            return col;
        }

        public static UserLoginData GetByUserName(string userName)
        {
            var col = GetDataCollection();
            var builder = Builders<UserLoginData>.Filter;
            var filter = builder.Eq(e => e.username, userName);
            var result = col.Find(filter);
            return result.FirstOrDefault();
        }

        public static bool Insert(UserLoginData data)
        {
            var col = GetDataCollection();
            col.InsertOne(data);
            return true;
        }

        public static bool Save(UserLoginData data)
        {
            var col = GetDataCollection();
            var builder = Builders<UserLoginData>.Filter;
            var filter1 = builder.Eq(e => e.username, data.username);
            col.ReplaceOne(filter1, data, new ReplaceOptions() { IsUpsert = true });
            return true;
        }

        public static Dictionary<string, object> m_LockOject = new Dictionary<string, object>();

        public static object GetLockObj(string token)
        {
            if (m_LockOject.ContainsKey(token))
            {
                return m_LockOject[token];
            }
            else
            {
                if (m_LockOject.Count > 1000000000)
                {
                    m_LockOject.Remove(m_LockOject.Keys.First());
                }
                m_LockOject.Add(token, new object());
                return m_LockOject[token];
            }
        }
    }

    public class UserLoginData
    {
        public string username { get; set; }
        public string password { get; set; }
        public long GID { get; set; }
        public string email { get; set; }
        public string deviceID { get; set; }
    }
}