using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using WorkerService.Interfaces;
using WorkerService.Models;
using WorkerService.Services;

namespace WorkerService.Commands
{
    public class StemCommand : IWorkCommand
    {
        private readonly ConfigurationModel _config;

        public StemCommand(ConfigurationModel config)
        {
            _config = config;
        }

        public object Run(IEnumerable<object> args, ILogger logger)
        {
            logger.LogInformation("{Class}::{Method} | Enter method", nameof(StemCommand), nameof(Run));
            var id = args.First().ToString();
            logger.LogInformation("{Class}::{Method} | id = {id}", nameof(StemCommand), nameof(Run), id);
            
            IMongoCollection<InitDoc> _initCol;
            IMongoCollection<StemmedDoc> _stemmedCol;
            var client = new MongoClient(_config.MongoDbConnectionString);
            var db = client.GetDatabase(_config.DbName);
            _initCol = db.GetCollection<InitDoc>(_config.InitCol);
            _stemmedCol = db.GetCollection<StemmedDoc>(_config.StemmedCol);

            var result = _initCol.Find(x => x.Id == id).ToList()[0];
            var textBuilder = new StringBuilder(result.Text);

            logger.LogInformation("{Class}::{Method} | Removing non-letters from text of {id}", nameof(StemCommand), nameof(Run), id);
            for (int idx = 0; idx < textBuilder.Length; idx++)
            {
                if (char.IsLetter(result.Text[idx]))
                    textBuilder[idx] = result.Text[idx].ToString().ToLower()[0];
                else textBuilder[idx] = ' ';
            }

            var words = textBuilder.ToString().Split(' ');
            var stemmedWords = new List<string>();
            logger.LogInformation("{Class}::{Method} | Stemming words of {id}", nameof(StemCommand), nameof(Run), id);
            foreach (var word in words)
                if (!string.IsNullOrWhiteSpace(word))
                {
                    if(UrlRequestService.GetRequest(_config.StemmingServiceUrl + '/' + word, out string r))
                        stemmedWords.Add(r);
                    else
                        stemmedWords.Add(word);
                }

            var stemmedItem = new StemmedDoc
            {
                InitDocId = result.Id,
                StemmedWords = stemmedWords
            };

            logger.LogInformation("{Class}::{Method} | Saving stemmed words of {id} to DB", nameof(StemCommand), nameof(Run), id);
            _stemmedCol.InsertOne(stemmedItem);

            logger.LogInformation("{Class}::{Method} | id = {id}; return null", nameof(StemCommand), nameof(Run), id);
            return null;
        }
    }
}