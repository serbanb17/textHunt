using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoordinatorService.Interfaces;
using CoordinatorService.Models;
using CoordinatorService.Services;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Commands
{
    public class SearchCommand : IWorkCommand
    {
        IWorkersManager _workersManager;
        private readonly ConfigurationModel _config;
        private readonly object _lockObj;
        private Dictionary<string, double> _coefficients;
        private List<object[]> GetWeights(string searchText)
        {
            var weights = new List<object[]>();

            var textBuilder = new StringBuilder(searchText);
            for (int idx = 0; idx < textBuilder.Length; idx++)
            {
                if (char.IsLetter(searchText[idx]))
                    textBuilder[idx] = searchText[idx].ToString().ToLower()[0];
                else textBuilder[idx] = ' ';
            }

            var words = textBuilder.ToString().Split(' ');
            var stemmedWords = new List<string>();
            foreach (var word in words)
                if (!string.IsNullOrWhiteSpace(word))
                {
                    if (UrlRequestService.GetRequest(_config.StemmingServiceUrl + '/' + word, out string r))
                        stemmedWords.Add(r);
                    else
                        stemmedWords.Add(word);
                }

            foreach (var word in stemmedWords.Distinct())
            {
                double tf = Math.Log(1 + stemmedWords.Count(x => x == word));
                double idf = 1;
                double weight = tf * idf;
                weights.Add(new object[]{word, weight});
            }

            return weights;
        }
        private void CallbackAction(string id, object result)
        {
            double coefficient = double.Parse(result.ToString());
            lock(_lockObj)
            {
                if (_coefficients.Count == 10)
                {
                    var list = _coefficients.ToList();
                    list.Sort((x1, x2) => Math.Abs(x1.Value) > Math.Abs(x2.Value) ? 1 : -1);
                    var smallest = list.First();
                    _coefficients.Remove(smallest.Key);
                }
                _coefficients[id] = coefficient;
            }
        }
        public SearchCommand(IWorkersManager workersManager, ConfigurationModel config)
        {
            _workersManager = workersManager;
            _config = config;
            _lockObj = new object();
            _coefficients = new Dictionary<string, double>();
        }
        public object Run(IEnumerable<object> args, ILogger logger)
        {
            var searchText = args.First().ToString();
            var weights = GetWeights(searchText);
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
                    var arguments = new List<object>() { doc.Id, weights };
                    _workersManager.DoWork(
                        "/command/execute",
                        new ExecuteRequestModel()
                        {
                            Command = "ComputeCosineSimilarity",
                            Arguments = arguments
                        },
                        (object result) => CallbackAction(doc.Id, result)
                        );
                }
            }

            _workersManager.WaitAllResults();
            var list = _coefficients.ToList();
            list.Sort((x1, x2) => Math.Abs(x1.Value) < Math.Abs(x2.Value) ? 1 : -1);
            return list.Select(x => new Tuple<string, double>(x.Key, x.Value)).ToList();
        }
    }
}