using System;
using System.Collections.Generic;

namespace covidlibrary
{
    public partial class LogsEntity
    {
        public DateTime Date { get; set; }
        public string Level { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public string EventId { get; set; }
        public string Username { get; set; }
        public int Id { get; set; }
    }
}
