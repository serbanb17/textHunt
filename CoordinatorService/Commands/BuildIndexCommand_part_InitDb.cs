using System.Threading;
using CoordinatorService.Interfaces;
using CoordinatorService.Services;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Commands
{
    public partial class BuildIndexCommand : IWorkCommand
    {
        private bool InitDb(ILogger logger)
        {
            int maxAttempts = 10, attempts;
            string outcome;
            logger.LogInformation("{Class}::{Method} | maxAttempts = {maxAttempts}", nameof(BuildIndexCommand), nameof(InitDb), maxAttempts);

            outcome = null;
            attempts = 0;
            while(attempts++ < maxAttempts && string.IsNullOrWhiteSpace(outcome))
            {
                logger.LogInformation("{Class}::{Method} | GET {DbInitServiceUrl}/status", nameof(BuildIndexCommand), nameof(InitDb), _config.DbInitServiceUrl);
                if(UrlRequestService.GetRequest(_config.DbInitServiceUrl + "/status", out string reqRes))
                    outcome = reqRes;
                else
                    Thread.Sleep(1000);
            }

            logger.LogInformation("{Class}::{Method} | outcome = {outcome}", nameof(BuildIndexCommand), nameof(InitDb), outcome);
            if(outcome != "idle")
                return false;
            
            outcome = null;
            attempts = 0;
            while(attempts++ < maxAttempts && string.IsNullOrWhiteSpace(outcome))
            {
                logger.LogInformation("{Class}::{Method} | GET {DbInitServiceUrl}/doinitdb", nameof(BuildIndexCommand), nameof(InitDb), _config.DbInitServiceUrl);
                if(UrlRequestService.GetRequest(_config.DbInitServiceUrl + "/doinitdb", out string reqRes))
                    outcome = reqRes;
                else
                    Thread.Sleep(1000);
            }

            logger.LogInformation("{Class}::{Method} | outcome = {outcome}", nameof(BuildIndexCommand), nameof(InitDb), outcome);
            if(outcome != "ok")
                return false;
            
            outcome = null;
            attempts = 0;
            while(attempts++ < maxAttempts && string.IsNullOrWhiteSpace(outcome))
            {
                logger.LogInformation("{Class}::{Method} | GET {DbInitServiceUrl}/status", nameof(BuildIndexCommand), nameof(InitDb), _config.DbInitServiceUrl);
                if(UrlRequestService.GetRequest(_config.DbInitServiceUrl + "/status", out string reqRes) && reqRes == "idle")
                    outcome = "ok";
                else
                    Thread.Sleep(1000);
            }

            logger.LogInformation("{Class}::{Method} | outcome = {outcome}", nameof(BuildIndexCommand), nameof(InitDb), outcome);
            return outcome == "ok";
        }
    }
}