using System.Collections.Generic;
using CoordinatorService.Interfaces;
using CoordinatorService.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Commands
{
    public partial class BuildIndexCommand : IWorkCommand
    {
        private void ComputeVectors(ILogger logger)
        {
            var client = new MongoClient(_config.MongoDbConnectionString);
            var db = client.GetDatabase(_config.DbName);
            IMongoCollection<InitDoc> _initCol;
            _initCol = db.GetCollection<InitDoc>(_config.InitCol);

            var options = new FindOptions { BatchSize = 100 };
            var cursor = _initCol.Find(x => true, options).ToCursor();
            while (cursor.MoveNext())
            {
                var currentBatch = cursor.Current;
                foreach (var doc in currentBatch)
                {
                    var arguments = new List<object>() { doc.Id };
                    var reqBody = new ExecuteRequestModel() { Command = "computevector", Arguments = arguments };
                    logger.LogInformation("{Class}::{Method} | Call worker for docId = {docId}", nameof(BuildIndexCommand), nameof(ComputeVectors), doc.Id);
                    _workersManager.DoWork("/command/execute", reqBody, null);
                }
            }

            logger.LogInformation("{Class}::{Method} | Waiting results from all workers", nameof(BuildIndexCommand), nameof(ComputeVectors));
            _workersManager.WaitAllResults();
        }
    }
}