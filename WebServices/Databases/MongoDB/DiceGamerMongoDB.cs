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
    public class DiceGamerMongoDB
    {
        public static void RegisterClass()
        {
            BsonClassMap.RegisterClassMap<DiceGamerData>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.ID));
                cm.SetIgnoreExtraElements(true);
            });
        }
        public static readonly string DataCol = "Dices";

        public static IMongoCollection<DiceGamerData> GetDataCollection()
        {
            var col = MongoDBHelper.GetDatabase().GetCollection<DiceGamerData>(DataCol);
            return col;
        }

        public static DiceGamerData GetByID(long characterID)
        {
            var col = GetDataCollection();
            var builder = Builders<DiceGamerData>.Filter;
            var filter = builder.Eq(e => e.ID, characterID);
            var result = col.Find(filter).FirstOrDefault();
            return result;
        }

        public static bool Save(DiceGamerData data)
        {
            var col = GetDataCollection();
            var builder = Builders<DiceGamerData>.Filter;
            var filter1 = builder.Eq(e => e.ID, data.ID);
            col.ReplaceOne(filter1, data, new ReplaceOptions() { IsUpsert = true });
            return true;
        }

        public static DiceGamerData Insert(long gid, DiceCode diceCode, DateTime expiredTime)
        {
            var data = new DiceGamerData()
            {
                ID = CounterMongoDB.GetNextValue("diceID"),
                GID = gid,
                code = diceCode,
                expiredTime = expiredTime
            };
            var col = GetDataCollection();
            col.InsertOne(data);
            return data;
        }

        public static List<DiceGamerData> GetDicesListByGID(long gid)
        {
            var col = GetDataCollection();
            var builder = Builders<DiceGamerData>.Filter;
            var filter = builder.Eq(e => e.GID, gid);
            var result = col.Find(filter).ToList<DiceGamerData>(); ;
            if (result == null)
                result = new List<DiceGamerData>();
            return result;
        }
    }
}