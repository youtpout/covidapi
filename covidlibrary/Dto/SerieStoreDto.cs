using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public class SerieStoreDto
    {
        public ProvinceEntity ProvinceEntity { get; set; }

        public List<SerieEntity> SerieEntities { get; set; }
    }

    public enum SerieType
    {
        confirmed = 0,
        recovered = 1,
        deaths = 2
    }
}
