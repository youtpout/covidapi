using System;
using System.Collections.Generic;

namespace covidlibrary
{
    public partial class ProvinceEntity
    {
        public string DataText
        {
            get
            {
                return $"{this.Country} {this.Province}";
            }
        }

    }
}
