using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using WorkerService.Interfaces;
using WorkerService.Models;

namespace WorkerService.Commands
{
    public class CountWordsCommand : IWorkCommand
    {
        private readonly ConfigurationModel _config;

        public CountWordsCommand(ConfigurationModel config)
        {
            _config = config;
        }

        public object Run(IEnumerable<object> args, ILogger logger)
        {
            logger.LogInformation("{Class}::{Method} | Enter method", nameof(CountWordsCommand), nameof(Run));
            var id = args.First().ToString();
            logger.LogInformation("{Class}::{Method} | id = {id}", nameof(CountWordsCommand), nameof(Run), id);

            IMongoCollection<StemmedDoc> _stemmedCol;
            IMongoCollection<BsonDocument> _wordsCountCol;
            var client = new MongoClient(_config.MongoDbConnectionString);
            var db = client.GetDatabase(_config.DbName);
            _stemmedCol = db.GetCollection<StemmedDoc>(_config.StemmedCol);
            _wordsCountCol = db.GetCollection<BsonDocument>(_config.WordsCountCol);
            var item = _stemmedCol.Find(x => x.InitDocId == id).ToList()[0];

            var stemmedWords = item.StemmedWords;
            var distinctWords = stemmedWords.Distinct();
            logger.LogInformation("{Class}::{Method} | Computing count for each word of stemmedCol id = {id}", nameof(CountWordsCommand), nameof(Run), id);
            foreach(var word in distinctWords)
            {
                var wordCount = stemmedWords.Count(x => x == word);
                var filter = Builders<BsonDocument>.Filter.Eq("word", word);
                var update = Builders<BsonDocument>.Update.Set(id, wordCount);
                var options = new UpdateOptions { IsUpsert = true };
                _wordsCountCol.UpdateOne(filter, update, options);
            }

            logger.LogInformation("{Class}::{Method} | id = {id}; return null", nameof(CountWordsCommand), nameof(Run), id);
            return null;
        }
    }
}