using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using WorkerService.Interfaces;
using WorkerService.Models;

namespace WorkerService.Commands
{
    public class ComputeWeightsCommand : IWorkCommand
    {
        private readonly ConfigurationModel _config;

        public ComputeWeightsCommand(ConfigurationModel config)
        {
            _config = config;
        }

        
        public object Run(IEnumerable<object> args, ILogger logger)
        {
            logger.LogInformation("{Class}::{Method} | Enter method", nameof(ComputeWeightsCommand), nameof(Run));
            var wordToCompute = args.First().ToString();
            logger.LogInformation("{Class}::{Method} | wordToCompute = {wordToCompute}", nameof(ComputeWeightsCommand), nameof(Run), wordToCompute);

            IMongoCollection<InitDoc> _initCol;
            IMongoCollection<StemmedDoc> _stemmedCol;
            IMongoCollection<BsonDocument> _wordsCountCol;
            IMongoCollection<BsonDocument> _weightsCol;
            var client = new MongoClient(_config.MongoDbConnectionString);
            var db = client.GetDatabase(_config.DbName);
            _initCol = db.GetCollection<InitDoc>(_config.InitCol);
            _stemmedCol = db.GetCollection<StemmedDoc>(_config.StemmedCol);
            _wordsCountCol = db.GetCollection<BsonDocument>(_config.WordsCountCol);
            _weightsCol = db.GetCollection<BsonDocument>(_config.WeightsCol);

            long totalItems = _initCol.CountDocuments(x => true);
            var word = _wordsCountCol.Find(x => x["word"] == wordToCompute).ToList()[0].ToDictionary();
            word.Remove("_id");
            word.Remove("word");
            
            logger.LogInformation("{Class}::{Method} | Compute weights for {wordToCompute}", nameof(ComputeWeightsCommand), nameof(Run), wordToCompute);
            foreach(var keyVal in word)
            {
                var wordCountInDoc = long.Parse(keyVal.Value.ToString());
                double tf = Math.Log(1 + (double) wordCountInDoc);
                double idf = Math.Log((double) totalItems / word.Count);
                double weight = tf * idf;
                
                var filter = Builders<BsonDocument>.Filter.Eq("word", wordToCompute);
                var update = Builders<BsonDocument>.Update.Set(keyVal.Key, new Tuple<double,double,double>(tf, idf, weight));
                var options = new UpdateOptions { IsUpsert = true };
                _weightsCol.UpdateOne(filter, update, options);
            }

            logger.LogInformation("{Class}::{Method} | wordToCompute = {wordToCompute}; return null", nameof(ComputeWeightsCommand), nameof(Run), wordToCompute);
            return null;
        }
    }
}