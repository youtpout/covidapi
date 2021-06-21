using coviddatabase;
using covidlibrary;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace covidapi.Models
{
    public class LoggerDatabaseProvider : ILoggerProvider
    {
        private DbContextOptions<CovidContext> _options;
        private IHttpContextAccessor _env;

        public LoggerDatabaseProvider(DbContextOptions<CovidContext> options, IHttpContextAccessor env)
        {
            _options = options;
            _env = env;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(categoryName, _options, _env);
        }

        public void Dispose()
        {
        }

        public class Logger : ILogger
        {
            private readonly string _categoryName;
            private DbContextOptions<CovidContext> _options;
            private IHttpContextAccessor _env;

            public Logger(string categoryName, DbContextOptions<CovidContext> options, IHttpContextAccessor env)
            {
                _options = options;
                _env = env;
                _categoryName = categoryName;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Critical || logLevel == LogLevel.Error || logLevel == LogLevel.Warning)
                    RecordMsg(logLevel, eventId, state, exception, formatter);
            }

            private void RecordMsg<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                try
                {
                    using (var ctx = new CovidContext(_options))
                    {
                        var logs = new LogsEntity
                        {
                            Level = logLevel.ToString(),
                            EventId = eventId.ToString(),
                            Category = _categoryName,
                            Username = _env.HttpContext.User.Identity.Name??"unknow",
                            Date = DateTime.Now
                        };

                        StringBuilder message = new StringBuilder(state.ToString());
                        if (exception != null)
                        {
                            message.AppendLine();
                            message.AppendLine(GetMessage(exception));
                        }
                        logs.Message = message.ToString();
                        ctx.Logs.Add(logs);
                        ctx.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    
                }
               
            }

            private string GetMessage(Exception ex)
            {
                StringBuilder message = new StringBuilder(ex.Message);
                if (ex?.InnerException != null)
                {
                    message.AppendLine();
                    message.AppendLine(GetMessage(ex.InnerException));
                }
                return message.ToString();
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return new NoopDisposable();
            }

            private class NoopDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
