using System;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Server.DatabaseUtils;
using CBShare.Data;
using System.Collections.Generic;
using CBShare.Configuration;

namespace CTPServer.MongoDB
{
    public class StarCardGamerMongoDB
    {
        public static void RegisterClass()
        {
            BsonClassMap.RegisterClassMap<StarCardGamerData>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.ID));
                cm.SetIgnoreExtraElements(true);
            });
        }
        public static readonly string DataCol = "StarCards";

        public static IMongoCollection<StarCardGamerData> GetDataCollection()
        {
            var col = MongoDBHelper.GetDatabase().GetCollection<StarCardGamerData>(DataCol);
            return col;
        }

        public static StarCardGamerData GetByID(long id)
        {
            var col = GetDataCollection();
            var builder = Builders<StarCardGamerData>.Filter;
            var filter = builder.Eq(e => e.ID, id);
            var result = col.Find(filter).FirstOrDefault();
            /*if (result == null)
            {
                result = new CharacterGamerData()
                {
                    ID = characterID,
                    //registerTime = DateTime.Now,
                    //displayName = string.Format("User_{0}", gid)
                };
                col.InsertOne(result);
            }*/
            return result;
        }

        public static bool Save(StarCardGamerData data)
        {
            var col = GetDataCollection();
            var builder = Builders<StarCardGamerData>.Filter;
            var filter1 = builder.Eq(e => e.ID, data.ID);
            col.ReplaceOne(filter1, data, new ReplaceOptions() { IsUpsert = true });
            return true;
        }

        public static StarCardGamerData Insert(long gid, StarCardConfig cardCfg, int _level)
        {
            var data = new StarCardGamerData()
            {
                ID = CounterMongoDB.GetNextValue("starCardID"),
                GID = gid,
                name = cardCfg.name,
                level = _level,
                statsValue = new Dictionary<string, float>()
                {
                    { "IncreaseSalary", 1 },
                    { "IncreaseReward", 1 },
                    { "ReduceSellHouseFees", 1 },
                    { "ReduceBuildHouseCost", 1 },
                    { "ReduceToll", 1 },
                    { "ReduceSkillTurnCount", 1 },
                    { "BonusSkillValue", 1 }
                },
                createTime = DateTime.Now,
            };
            /*foreach (var statKey in cardCfg.statsValues.Keys)
            {
                data.statsValue[statKey] = cardCfg.statsValues[statKey][0];
            }*/
            var col = GetDataCollection();
            col.InsertOne(data);
            return data;
        }

        public static List<StarCardGamerData> GetListByGID(long gid)
        {
            var col = GetDataCollection();
            var builder = Builders<StarCardGamerData>.Filter;
            var filter = builder.Eq(e => e.GID, gid);
            var result = col.Find(filter).ToList<StarCardGamerData>(); ;
            if (result == null)
                result = new List<StarCardGamerData>();
            return result;
        }
    }
}