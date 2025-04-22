    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

    public static class ExcelReaderHelper
    {
        public static void ParseValue(JObject jObject, string type, string name, string value)
        {
            switch (type)
            {
                case "int":
                    jObject[name] = GetValue<int>(value);
                    break;
                case "float":
                    jObject[name] = GetValue<float>(value);
                    break;
                case "string":
                    jObject[name] = value;
                    break;
                case "bool":
                    jObject[name] = GetValue<bool>(value);
                    break;
                case "int[]":
                    jObject[name] = new JArray(GetList<int>(value));
                    break;
                case "float[]":
                    jObject[name] = new JArray(GetList<float>(value));
                    break;
                case "float[,]":
                    jObject[name] = new JArray(GetTwoList<float>(value));
                    break;
                case "string[]":
                    jObject[name] = new JArray(GetList<string>(value));
                    break;
                case "bool[]":
                    jObject[name] = new JArray(GetList<bool>(value));
                    break;
            }
        }

        public static T GetValue<T>(string value)
        {
            if (string.IsNullOrEmpty(value)) return default(T);
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static List<T> GetList<T>(string value, string split = ",")
        {
            value = value.Replace("{", string.Empty).Replace("}", string.Empty);
            var arr = value.Split(split);
            List<T> list = new();
            for (int i = 0; i < arr.Length; ++i)
            {
                list.Add(GetValue<T>(arr[i]));
            }

            return list;
        }

        public static JArray GetTwoList<T>(string value, string split = ",")
        {

            string patten = "{.*?}";
            Regex regex = new Regex(patten);
            var collection = regex.Matches(value.Substring(1, value.Length - 2));
            JArray res = new();
            for (var i = 0; i < collection.Count; i++)
            {
                var str = collection[i].Value;
                res.Add(new JArray(GetList<T>(str.Substring(1, str.Length - 2), split)));
            }

            return res;
        }
    }