using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using WorkerService.Interfaces;
using WorkerService.Models;

namespace WorkerService.Commands
{
    public class ComputeVectorCommand : IWorkCommand
    {
        private readonly ConfigurationModel _config;

        public ComputeVectorCommand(ConfigurationModel config)
        {
            _config = config;
        }

        
        public object Run(IEnumerable<object> args, ILogger logger)
        {
            logger.LogInformation("{Class}::{Method} | Enter method", nameof(ComputeVectorCommand), nameof(Run));
            var id = args.First().ToString();
            logger.LogInformation("{Class}::{Method} | id = {id}", nameof(ComputeVectorCommand), nameof(Run), id);

            IMongoCollection<StemmedDoc> _stemmedCol;
            IMongoCollection<BsonDocument> _weightsCol;
            IMongoCollection<BsonDocument> _vectorsCol;
            var client = new MongoClient(_config.MongoDbConnectionString);
            var db = client.GetDatabase(_config.DbName);
            _stemmedCol = db.GetCollection<StemmedDoc>(_config.StemmedCol);
            _weightsCol = db.GetCollection<BsonDocument>(_config.WeightsCol);
            _vectorsCol = db.GetCollection<BsonDocument>(_config.VectorsCol);

            var stemmedItem = _stemmedCol.Find(x => x.InitDocId == id).ToList()[0];
            
            logger.LogInformation("{Class}::{Method} | Compute vectors for each word of {id}", nameof(ComputeVectorCommand), nameof(Run), id);
            foreach(var word in stemmedItem.StemmedWords)
            {
                var wordFinding = _weightsCol.Find(x => x["word"] == word).ToList();
                if(wordFinding.Count == 0)
                    continue;

                var wordWeight = wordFinding[0].ToDictionary();
                double weight = (double)((wordWeight[id] as object[])[2]);

                var filter = Builders<BsonDocument>.Filter.Eq("initDocId", id);
                var update = Builders<BsonDocument>.Update.Set(word, weight);
                var options = new UpdateOptions { IsUpsert = true };
                _vectorsCol.UpdateOne(filter, update, options);
            }

            logger.LogInformation("{Class}::{Method} | id = {id}; return null", nameof(ComputeVectorCommand), nameof(Run), id);
            return null;
        }
    }
}