using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using WorkerService.Interfaces;
using WorkerService.Models;

namespace WorkerService.Commands
{
    public class ComputeCosineSimilarityCommand : IWorkCommand
    {
        private readonly ConfigurationModel _config;

        public ComputeCosineSimilarityCommand(ConfigurationModel config)
        {
            _config = config;
        }

        
        public object Run(IEnumerable<object> args, ILogger logger)
        {
            var id = args.First().ToString();
            var searchVector = JsonSerializer.Deserialize<List<object[]>>(args.Skip(1).Take(1).First().ToString());
            IMongoCollection<BsonDocument> _vectorsCol;
            var client = new MongoClient(_config.MongoDbConnectionString);
            var db = client.GetDatabase(_config.DbName);
            _vectorsCol = db.GetCollection<BsonDocument>(_config.VectorsCol);

            var x = _vectorsCol.Find(x => x["initDocId"] == id);
            var y = x.ToList();
            var itemVector = y[0].ToDictionary();
            itemVector.Remove("_id");
            itemVector.Remove("initDocId");
            
            double dotProduct = 0;
            foreach(var searchWeight in searchVector)
            {
                if(itemVector.ContainsKey(searchWeight[0].ToString()))
                    dotProduct += double.Parse(searchWeight[1].ToString()) * (double)itemVector[searchWeight[0].ToString()];
            }

            double sumSquaresSearch = 0;
            double sumSquaresItem = 0;
            searchVector.ForEach(x => sumSquaresSearch += double.Parse(x[1].ToString())*double.Parse(x[1].ToString()));
            itemVector.Values.ToList().Select(x => (double)x).ToList().ForEach(x => sumSquaresItem += x*x);
            double normSearch = Math.Sqrt(sumSquaresSearch);
            double normItem = Math.Sqrt(sumSquaresItem);

            return (dotProduct/(normSearch*normItem)).ToString();
        }
    }
}