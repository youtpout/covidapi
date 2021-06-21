using GeoCoordinatePortable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace covidlibrary
{
    public static class Tools
    {
        public static List<CaseByCity> CalculateDistanceWithCase(this IEnumerable<CaseByCity> cities, double latitude, double longitude)
        {

            if (cities != null && cities.Any())
            {
                foreach (var item in cities)
                {
                    if (item.Coord != null)
                    {
                        GeoCoordinate pin1 = new GeoCoordinate(latitude, longitude);
                        GeoCoordinate pin2 = new GeoCoordinate(item.Coord.Latitude, item.Coord.Longitude);

                        // distance in meter
                        item.Distance = pin1.GetDistanceTo(pin2) / 1000;
                    }
                    else
                    {
                        item.Distance = 9999999999999;
                    }


                }
            }
            return cities != null ? cities.ToList() : new List<CaseByCity>();
        }

        public static List<CaseByCity> GetCityWithCase(this RootObject ro)
        {
            List<CaseByCity> closeCases = new List<CaseByCity>();
            Regex reg = new Regex(@"(?<=\[).+?(?=\])");
            Regex comma = new Regex(@"(?<=\])[^,]+(?=,)");
            string[] stringSeparators = new string[] { @"\/" };
            if (ro?.features?.Count > 0)
            {
                foreach (var item in ro.features)
                {
                    var featCoord = item?.geometry?.coordinates;
                    if (item?.geometry?.type == "Point" && featCoord?.Count == 2)
                    {
                        CaseByCity closeCase = new CaseByCity();
                        try
                        {
                            closeCase.Coord = new Coord()
                            {
                                Latitude = (double)featCoord[1],
                                Longitude = (double)featCoord[0]
                            };
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (item?.properties?.Name != null)
                        {
                            if (item.properties.Name.Contains("Meaning of colors"))
                            {
                                continue;
                            }
                            if (reg.IsMatch(item.properties.Name))
                            {
                                string val = reg.Match(item.properties.Name).Value;
                                string[] split = val.Split('-');
                                if (split.Length > 1)
                                {
                                    closeCase.Country = split[0].Trim();

                                    if (split[1].Contains(@"\/"))
                                    {
                                        string[] splitCase = split[1].Split(stringSeparators, StringSplitOptions.None);
                                        if (splitCase.Length > 1)
                                        {
                                            int.TryParse(splitCase[0].Trim(), out int nbc);
                                            closeCase.Confirmed = nbc;
                                            int.TryParse(splitCase[1].Trim(), out int nbd);
                                            closeCase.Deaths = nbd;
                                        }
                                    }
                                    else
                                    {
                                        int.TryParse(split[1].Trim(), out int nb);
                                        closeCase.Confirmed = nb;
                                    }
                                }
                                else
                                {
                                    closeCase.Country = val;
                                }

                                if (comma.IsMatch(item.properties.Name))
                                {
                                    string city = comma.Match(item.properties.Name).Value;
                                    closeCase.City = city.Trim();
                                }
                            }
                            else
                            {
                                closeCase.City = item.properties.Name.Trim();
                            }
                        }
                        closeCases.Add(closeCase);
                    }

                }
            }

            return closeCases;
        }
    }
}
