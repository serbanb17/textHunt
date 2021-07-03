using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoordinatorService.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Managers
{
    public class CommandManager : ICommandManager
    {
        private object _lockObj;

        private Dictionary<string, IWorkCommand> _commands;

        public Action<string> LogInformation {get; set;}

        private void ExecuteInThread(string commandName, Action<object> callback, IEnumerable<object> args, ILogger logger)
        {
            object result;

            try
            {
                result = _commands[commandName].Run(args, logger);
            }
            catch (Exception exception)
            {
                logger.LogError("{Class}::{Method} | Exception while running command {commandName}:\n{exceptionMessage}}", nameof(CommandManager), nameof(ExecuteInThread), commandName, exception.Message);
                result = exception.Message;
            }

            try
            {
                callback?.Invoke(result);
            }
            catch (Exception exception)
            {
                logger.LogError("{Class}::{Method} | Exception while running callback for command {commandName}:\n{exceptionMessage}}", nameof(CommandManager), nameof(ExecuteInThread), commandName, exception.Message);
            }
            
            _callerId = new Guid();
            _isBusy = false;
        }

        private bool _isBusy;
        public bool IsBusy => _isBusy;

        private Guid _callerId;
        public Guid CallerId => _callerId;

        public CommandManager()
        {
            _lockObj = new object();
            _commands = new Dictionary<string, IWorkCommand>();
            _isBusy = false;
            _callerId = new Guid();
        }

        public bool RegisterCommand(string commandName, IWorkCommand command)
        {
            if (_commands.ContainsKey(commandName.ToLower()))
                return false;

            _commands[commandName.ToLower()] = command;

            return true;
        }

        public Tuple<bool, Guid> Execute(string commandName, Guid callerId, Action<object> callback, IEnumerable<object> args, ILogger logger)
        {
            if (_isBusy || !_commands.ContainsKey(commandName.ToLower()))
            {
                return new Tuple<bool, Guid>(false, new Guid());
            }

            bool firstToLock = false;
            lock (_lockObj)
            {
                if (!_isBusy)
                {
                    _isBusy = true;
                    _callerId = callerId;
                    firstToLock = true;
                }
            }

            if (!firstToLock)
            {
                return new Tuple<bool, Guid>(false, new Guid());
            }

            Task.Factory.StartNew(() => ExecuteInThread(commandName.ToLower(), callback, args, logger), TaskCreationOptions.LongRunning);

            return new Tuple<bool, Guid>(true, callerId);
        }
    }
}