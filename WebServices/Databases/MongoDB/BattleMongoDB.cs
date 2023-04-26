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
    public class BattleMongoDB
    {
        public static void RegisterClass()
        {
            BsonClassMap.RegisterClassMap<BattleProperty>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.ID));
                cm.SetIgnoreExtraElements(true);
            });
        }
        public static readonly string DataCol = "Battles";

        public static IMongoCollection<BattleProperty> GetDataCollection()
        {
            var col = MongoDBHelper.GetDatabase().GetCollection<BattleProperty>(DataCol);
            return col;
        }

        public static BattleProperty GetByID(int _id)
        {
            var col = GetDataCollection();
            var builder = Builders<BattleProperty>.Filter;
            var filter = builder.Eq(e => e.ID, _id);
            var result = col.Find(filter).FirstOrDefault();
            return result;
        }

        public static List<BattleProperty> GetAllsPlayingList()
        {
            var col = GetDataCollection();
            var builder = Builders<BattleProperty>.Filter;
            var result = col.Find(builder.Where(e => e.state != BattleState.Finised)).ToList<BattleProperty>(); ;
            return result;
        }

        public static bool Save(BattleProperty data)
        {
            var col = GetDataCollection();
            var builder = Builders<BattleProperty>.Filter;
            var filter1 = builder.Eq(e => e.ID, data.ID);
            data.lastUpdateTime = DateTime.Now;
            col.ReplaceOne(filter1, data, new ReplaceOptions() { IsUpsert = true });
            return true;
        }

        public static BattleProperty Insert(BattleProperty data)
        {
            var col = GetDataCollection();
            col.InsertOne(data);
            return data;
        }
    }
}