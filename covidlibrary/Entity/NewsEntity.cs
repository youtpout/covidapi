using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public class NewsEntity
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string TextSource { get; set; }

        public string Source { get; set; }

        public string Lang { get; set; }

        public DateTime DateCreate { get; set; }

        public DateTime? DateUpdate { get; set; }
    }
}
