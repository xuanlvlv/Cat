using System.Collections.Generic;
using System.IO;
using ExcelDataReader;
using Newtonsoft.Json.Linq;

public class ColVariables
    {
        public string typeName;
        public string variableName;
        public List<string> variables=new();
    }
    public interface ISheetReader
    {
        public int Priority { get; }
        public bool UseThisReader(string sheetName);
        public void Load(string sheetName, IExcelDataReader reader,string scriptPath,string jsonPath);
    }

    public class DefaultSheetReader : ISheetReader
    {
        private const string GlobalVariablesSheet = "GlobalVariables";
        public virtual int Priority { get=>0; }
        public virtual bool UseThisReader(string sheetName)
        {
            return true;
        }
        public virtual void Load(string sheetName, IExcelDataReader reader, string scriptPath, string jsonPath)
        {
            List<ColVariables> variablesList = new List<ColVariables>();
            var json = GenJson(sheetName, reader,variablesList);
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
            File.WriteAllText(jsonPath + "//" + sheetName + ".json", json);
            
            
            var script = GenCode(sheetName, variablesList);
            if (!Directory.Exists(scriptPath))
            {
                Directory.CreateDirectory(scriptPath);
            }
            File.WriteAllText(scriptPath + "//" + sheetName + ".cs", script);
        }

        public virtual string GenJson(string sheetName, IExcelDataReader reader,List<ColVariables> variablesList)
        {
            var rowCnt = reader.RowCount;
            var colCnt = reader.FieldCount;
            reader.Read(); //跳过第一行注释行
            JArray array = new();
            JObject root = new JObject();
            root["values"] = array; //父对象为一个名为values的集合
            for (int i = 1; i < rowCnt; ++i)
            {
                reader.Read();
                JObject jObject = null;
                if (i > 2) 
                {
                    jObject = new JObject();
                    array.Add(jObject);
                }
                for (int j = 0; j < colCnt; ++j)
                {
                    if (i == 1) //第一行为变量类型
                    {
                        var typeName = reader.GetString(j);
                        variablesList.Add(new() { typeName = typeName });
                    }
                    else if (i == 2) //第二行为变量名称
                    {
                        var variableName = reader.GetString(j);
                        variablesList[j].variableName = variableName;
                    }
                    else
                    {
                        //如果变量名称或者变量类型为空，则直接跳过该位置的读取
                        if (string.IsNullOrEmpty(variablesList[j].typeName) ||
                            string.IsNullOrEmpty(variablesList[j].variableName)) continue;
                        else
                        {
                            var value = reader.GetValue(j);
                            var valueStr=value?.ToString();
                            variablesList[j].variables.Add(valueStr);
                            ExcelReaderHelper.ParseValue(jObject, variablesList[j].typeName, variablesList[j].variableName,
                                valueStr);
                        }
                    }
                    
                }
            }

            return root.ToString();
        }
        
        public virtual string GenCode(string sheetName, List<ColVariables> variablesList)
        {
            string scriptStr = "using System.Collections.Generic;\n" +
                               "public class " + sheetName + "{";
            foreach (var item in variablesList)
            {
                if (string.IsNullOrEmpty(item.variableName) || string.IsNullOrEmpty(item.typeName)) continue;
                scriptStr += "\n \t public " + item.typeName + " " + item.variableName + ";";
            }

            if (sheetName == GlobalVariablesSheet)
            {
                var v = variablesList.Find(v => v.variableName == "key");
                if (v != null && v.variables != null)
                {
                    foreach (var key in v.variables)
                    {
                        if (!string.IsNullOrEmpty(key))
                        {
                            scriptStr += "\n \t public const string " + key + "=" + "\""+key+"\"" + ";";
                        }
                    }
                }
            }
            
            
            scriptStr += "\n}\n";
            scriptStr += "public class " + sheetName + "Model : IModel{\n" +
                         $"public List<{sheetName}> values;" +
                         "\n}";
            return scriptStr;
        }
    }
