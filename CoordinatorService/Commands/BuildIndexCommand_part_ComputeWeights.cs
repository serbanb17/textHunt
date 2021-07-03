using System.Collections.Generic;
using CoordinatorService.Interfaces;
using CoordinatorService.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Commands
{
    public partial class BuildIndexCommand : IWorkCommand
    {
        private void ComputeWeights(ILogger logger)
        {
            var client = new MongoClient(_config.MongoDbConnectionString);
            var db = client.GetDatabase(_config.DbName);
            IMongoCollection<BsonDocument> _wordsCountCol;
            _wordsCountCol = db.GetCollection<BsonDocument>(_config.WordsCountCol);

            var options = new FindOptions { BatchSize = 100 };
            var cursor = _wordsCountCol.Find(x => true, options).ToCursor();
            while (cursor.MoveNext())
            {
                var currentBatch = cursor.Current;
                foreach (var doc in currentBatch)
                {
                    var arguments = new List<object>() { doc["word"].ToString() };
                    var reqBody = new ExecuteRequestModel() { Command = "computeweights", Arguments = arguments };
                    logger.LogInformation("{Class}::{Method} | Call worker for word = {docId}", nameof(BuildIndexCommand), nameof(ComputeWeights), doc["word"].ToString());
                    _workersManager.DoWork("/command/execute", reqBody, null);
                }
            }

            logger.LogInformation("{Class}::{Method} | Waiting results from all workers", nameof(BuildIndexCommand), nameof(ComputeWeights));
            _workersManager.WaitAllResults();
            logger.LogInformation("{Class}::{Method} | Exiting, no return (void)", nameof(BuildIndexCommand), nameof(ComputeWeights));
        }
    }
}