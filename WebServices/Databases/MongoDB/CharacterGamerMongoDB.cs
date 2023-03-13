using System;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Server.DatabaseUtils;
using CBShare.Data;
using CBShare.Common;
using System.Collections.Generic;

namespace CTPServer.MongoDB
{
    public class CharacterGamerMongoDB
    {
        public static void RegisterClass()
        {
            BsonClassMap.RegisterClassMap<CharacterGamerData>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.ID));
                cm.SetIgnoreExtraElements(true);
            });
        }
        public static readonly string DataCol = "Characters";

        public static IMongoCollection<CharacterGamerData> GetDataCollection()
        {
            var col = MongoDBHelper.GetDatabase().GetCollection<CharacterGamerData>(DataCol);
            return col;
        }

        public static CharacterGamerData GetByID(long characterID)
        {
            var col = GetDataCollection();
            var builder = Builders<CharacterGamerData>.Filter;
            var filter = builder.Eq(e => e.ID, characterID);
            var result = col.Find(filter).FirstOrDefault();
            return result;
        }

        public static bool Save(CharacterGamerData data)
        {
            var col = GetDataCollection();
            var builder = Builders<CharacterGamerData>.Filter;
            var filter1 = builder.Eq(e => e.ID, data.ID);
            col.ReplaceOne(filter1, data, new ReplaceOptions() { IsUpsert = true });
            return true;
        }

        public static CharacterGamerData Insert(long gid, CharacterCode characterCode)
        {
            var data = new CharacterGamerData()
            {
                ID = CounterMongoDB.GetNextValue("characterID"),
                GID = gid,
                code = characterCode,
                buyTime = DateTime.Now
            };
            var col = GetDataCollection();
            col.InsertOne(data);
            return data;
        }

        public static List<CharacterGamerData> GetCharactersListByGID(long gid)
        {
            var col = GetDataCollection();
            var builder = Builders<CharacterGamerData>.Filter;
            var filter = builder.Eq(e => e.GID, gid);
            var result = col.Find(filter).ToList<CharacterGamerData>(); ;
            return result;
        }
    }
}