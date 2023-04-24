using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;

namespace Server.DatabaseUtils
{
    public class MongoDBHelper
    {
        public MongoClient client = GetConnection();
        public static readonly MongoDBHelper instance = new MongoDBHelper();

        public static MongoClient GetConnection()
        {
            return new MongoClient(DataSource.MongoConn);
        }

        public static IMongoDatabase GetDatabase()
        {
            string mongodbName = "Ludo"; // original
            return instance.client.GetDatabase(mongodbName);
        }
    }
}