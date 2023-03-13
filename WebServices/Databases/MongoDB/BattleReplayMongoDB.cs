using System;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Server.DatabaseUtils;
using CBShare.Data;
using System.Security.Cryptography;

namespace CTPServer.MongoDB
{
    public class BattleReplayMongoDB
    {
        public static void RegisterClass()
        {
            BsonClassMap.RegisterClassMap<BattleReplayData>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.battleID));
                cm.SetIgnoreExtraElements(true);
            });
        }
        public static readonly string DataCol = "BattleReplays";

        public static IMongoCollection<BattleReplayData> GetDataCollection()
        {
            var col = MongoDBHelper.GetDatabase().GetCollection<BattleReplayData>(DataCol);
            return col;
        }


        public static BattleReplayData GetByBattleID(int _battleID)
        {
            var col = GetDataCollection();
            var builder = Builders<BattleReplayData>.Filter;
            var filter = builder.Eq(e => e.battleID, _battleID);
            var result = col.Find(filter).FirstOrDefault();
            if (result == null)
            {
                result = new BattleReplayData()
                {
                    battleID = _battleID,
                    lastUpdate = DateTime.Now
                };
                col.InsertOne(result);
            }
            return result;
        }

        public static bool Save(BattleReplayData data)
        {
            var col = GetDataCollection();
            var builder = Builders<BattleReplayData>.Filter;
            var filter1 = builder.Eq(e => e.battleID, data.battleID);
            data.lastUpdate = DateTime.Now;
            col.ReplaceOne(filter1, data, new ReplaceOptions() { IsUpsert = true });
            return true;
        }

        public static BattleReplayData Insert(BattleReplayData data)
        {
            var col = GetDataCollection();
            col.InsertOne(data);
            return data;
        }
    }
}