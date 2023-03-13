using System;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Server.DatabaseUtils;

namespace CTPServer.MongoDB
{
    public class CounterMongoDB
    {
        public static void RegisterClass()
        {

            BsonClassMap.RegisterClassMap<CounterData>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.name));
                cm.SetIgnoreExtraElements(true);
            });
        }
        public static readonly string DataCol = "Counter";

        public static IMongoCollection<CounterData> GetDataCollection()
        {
            var col = MongoDBHelper.GetDatabase().GetCollection<CounterData>(DataCol);
            return col;
        }

        public static long GetNextValue(string _name)
        {
            var col = GetDataCollection();
            var filter = Builders<CounterData>.Filter.Eq(e => e.name, _name);
            var update = Builders<CounterData>.Update.Inc(e => e.value, 1);
            var result = col.FindOneAndUpdate(filter, update, new FindOneAndUpdateOptions<CounterData, CounterData> { IsUpsert = true, ReturnDocument = ReturnDocument.After });
            return result.value;
        }
    }

    public class CounterData
    {
        public string name { get; set; }
        public long value { get; set; }
    }
}