using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace covidlibrary
{
    public interface ISerieRepository
    {
        Task<List<string>> AddOrUpdateConfirmed(Stream stream);
        Task<List<string>> AddOrUpdateRecovered(Stream stream);
        Task<List<string>> AddOrUpdateDeaths(Stream stream);
    }
}
