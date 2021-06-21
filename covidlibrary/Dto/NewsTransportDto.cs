using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public class NewsTransportDto
    {
        public int TotalNews { get; set; }
        public int NewsByPage { get; set; }
        public int Page { get; set; }

        public List<NewsByDateDto> NewsByDate { get; set; }
    }

    public class NewsByDateDto
    {
        public DateTime Date { get; set; }

        public List<NewsDto> News { get; set; }
    }
}
