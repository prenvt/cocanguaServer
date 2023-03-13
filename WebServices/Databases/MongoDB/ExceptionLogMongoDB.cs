using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Server.DatabaseUtils;

namespace CTPServer.MongoDB
{
    public class ExceptionLogMongoDB
    {
        public class ExceptionLogData
        {
            public string ID;
            public string log;
            public DateTime time;
        }

        public static void RegisterClass()
        {
            BsonClassMap.RegisterClassMap<ExceptionLogData>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.ID));
            });
        }

        private static string ExceptionCollection = "ExceptionLog";
        public static IMongoCollection<ExceptionLogData> GetExceptionLogCollection()
        {
            var col = MongoDBHelper.GetDatabase().GetCollection<ExceptionLogData>(ExceptionCollection);
            return col;
        }

        public static void add(string log)
        {
            try
            {
                ExceptionLogData ex = new ExceptionLogData();
                ex.ID = ObjectId.GenerateNewId().ToString();
                ex.log = log;
                ex.time = DateTime.Now;
                var col = GetExceptionLogCollection();
                col.InsertOne(ex);
            }
            catch
            {
            }
        }
    }
}