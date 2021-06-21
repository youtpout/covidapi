using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace covidlibrary
{
    public class Properties
    {
        public string name { get; set; }
    }

    public class Crs
    {
        public string type { get; set; }
        public Properties properties { get; set; }
    }

    public class Properties2
    {
        public string Name { get; set; }
        public string description { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<object> coordinates { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public Properties2 properties { get; set; }
        public Geometry geometry { get; set; }
    }

    public class RootObject
    {
        public string type { get; set; }
        public string name { get; set; }
        public Crs crs { get; set; }
        public List<Feature> features { get; set; }
    }

}