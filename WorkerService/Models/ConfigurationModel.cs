namespace WorkerService.Models
{
    public class ConfigurationModel
    {
        public string MyUrl { get; set; }
        public string CoordinatorServiceUrl { get; set; }
        public string StemmingServiceUrl {get;set;}
        public string MongoDbConnectionString {get;set;}
        public string DbName {get;set;}
        public string InitCol {get;set;}
        public string StemmedCol {get;set;}
        public string WordsCountCol {get;set;}
        public string WeightsCol {get;set;}
        public string VectorsCol {get;set;}
    }
}