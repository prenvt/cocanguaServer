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
    public class GamerMongoDB
    {
        public static void RegisterClass()
        {
            BsonClassMap.RegisterClassMap<UserInfo.Gamer>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.ID));
                cm.SetIgnoreExtraElements(true);
            });
        }
        public static readonly string DataCol = "Gamers";

        public static IMongoCollection<UserInfo.Gamer> GetDataCollection()
        {
            var col = MongoDBHelper.GetDatabase().GetCollection<UserInfo.Gamer>(DataCol);
            return col;
        }

        public static UserInfo.Gamer GetByID(long gid)
        {
            var col = GetDataCollection();
            var builder = Builders<UserInfo.Gamer>.Filter;
            var filter = builder.Eq(e => e.ID, gid);
            var result = col.Find(filter).FirstOrDefault();
            if (result == null)
            {
                result = new UserInfo.Gamer()
                {
                    ID = gid,
                    registerTime = DateTime.Now,
                    displayName = string.Format("User_{0}", gid)
                };
                col.InsertOne(result);
            }
            return result;
        }

        public static UserInfo.Gamer FindByDisplayName(string displayName)
        {
            var col = GetDataCollection();
            var builder = Builders<UserInfo.Gamer>.Filter;
            var filter = builder.Eq(e => e.displayName, displayName);
            var result = col.Find(filter).FirstOrDefault();
            return result;
        }

        public static string GetTokenByID(long gid)
        {
            var col = GetDataCollection();
            var builder = Builders<UserInfo.Gamer>.Filter;
            var filter = builder.Eq(e => e.ID, gid);
            var result = col.Find(filter).FirstOrDefault();
            return result.accessToken;
        }

        public static bool Save(UserInfo.Gamer data)
        {
            var col = GetDataCollection();
            var builder = Builders<UserInfo.Gamer>.Filter;
            var filter1 = builder.Eq(e => e.ID, data.ID);
            col.ReplaceOne(filter1, data, new ReplaceOptions() { IsUpsert = true });
            return true;
        }

        public static void insertAccessTokenCache(long gid, string token)
        {
            Utility.InsertCache("token_" + gid, token, 60 * 60);
        }

        public static bool checkAccessToken(long gid, string token, bool checkFromDB = false)
        {
            var locking = Utility.GetCacheObject(string.Format("lockByGMT_{0}", gid));
            if (locking != null)
            {
                return false;
            }

            string tk = "";
            if (Utility.GetCacheObject("token_" + gid) == null || checkFromDB)
            {
                tk = GetTokenByID(gid);
            }
            else
            {
                tk = Utility.GetCacheObject("token_" + gid).ToString();
            }

            if (token.Equals(tk))
            {
                insertAccessTokenCache(gid, token);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateRoomState(long gid, int roomID)
        {
            var col = GetDataCollection();
            var updateOperation = Builders<UserInfo.Gamer>.Update.Set("currentRoomID", roomID);
            var result = col.UpdateOne(new BsonDocument("_id", gid), updateOperation);
        }
    }
}