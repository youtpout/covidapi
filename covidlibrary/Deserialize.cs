using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace covidlibrary
{
    public static class Deserialize
    {


        public static List<CsvData> FromFileCsvData(string path)
        {
            List<CsvData> datas = new List<CsvData>();
            FileInfo t = new FileInfo(path);
            Dictionary<ColumnType, int> columnIndex = new Dictionary<ColumnType, int>();
            using (var reader = new StreamReader(path))
            {
                int index = 0;
                while (!reader.EndOfStream)
                {

                    var line = reader.ReadLine();

                    var regex = new Regex("\"[^\"]*\"");
                    if (regex.IsMatch(line))
                    {
                        // remove extra " and ,
                        string text = regex.Match(line).Value;
                        string replace = text.Replace(",", "/").Replace("\"", "").Replace(" ", "");
                        line = line.Replace(text, replace);
                    }
                    var values = line.Split(',');
                    if (index == 0)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            string val = values[i].ToLower();
                            if (val.Contains("province"))
                            {
                                columnIndex.Add(ColumnType.province, i);
                            }
                            else if (val.Contains("country"))
                            {
                                columnIndex.Add(ColumnType.country, i);
                            }
                            else if (val.Contains("confirmed"))
                            {
                                columnIndex.Add(ColumnType.confirmed, i);
                            }
                            else if (val.Contains("recovered"))
                            {
                                columnIndex.Add(ColumnType.recovered, i);
                            }
                            else if (val.Contains("death"))
                            {
                                columnIndex.Add(ColumnType.deaths, i);
                            }
                            else if (val.Contains("update"))
                            {
                                columnIndex.Add(ColumnType.update, i);
                            }
                        }
                    }
                    else if (values.Length > 5)
                    {
                        CsvData data = new CsvData(values, columnIndex);
                        data.Date = DateTime.ParseExact(t.Name.Replace(".csv", ""), "MM-dd-yyyy", new CultureInfo("en-US"));
                        datas.Add(data);
                    }

                    index++;
                }

            }

            return datas;
        }

        public static RootObject FromFileJson(string path)
        {
            List<CsvData> datas = new List<CsvData>();
            string t = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<RootObject>(t);

        }
    }
}
