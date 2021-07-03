using System;
using CoordinatorService.Models;
using MongoDB.Driver;

namespace CoordinatorService.Services
{
    public class MongoUtilService
    {
        public static string GetItemUrl(ConfigurationModel config, string id)
        {
            IMongoCollection<InitDoc> _initCol;
            var client = new MongoClient(config.MongoDbConnectionString);
            var db = client.GetDatabase(config.DbName);
            _initCol = db.GetCollection<InitDoc>(config.InitCol);

            string result = null;
            var queryResult = _initCol.Find(x => x.Id == id).ToList();
            if(queryResult.Count == 1)
                result = queryResult[0].Item;

            return result;
        }
    }
}