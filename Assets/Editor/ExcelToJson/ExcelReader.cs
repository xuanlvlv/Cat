using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ExcelDataReader;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
public static class ExcelReader
{
    
    private static string excelPath = Application.dataPath + "//Datas//ExcelData";
    private static string scriptPath = Application.dataPath + "//Scripts//Datas";
    private static string jsonPath = Application.dataPath + "//Datas//JsonData";
    private static List<ISheetReader> sheetReaders = new List<ISheetReader>()
    {
        new DefaultSheetReader(),
    };
    [MenuItem("Tool/ExcelRefresh")]
    public static void LoadExcel()
    {
        var path = excelPath;
        //todo,后续可以不加载未变动的表格
        var files = Directory.GetFiles(path).Where(n=>!n.EndsWith(".meta"));
        foreach (var file in files)
        {
            LoadExcel(file);
        }
    }
    
    public static void LoadExcel(string name)
    {
        string path = name;
        using (var s = File.Open(path,FileMode.Open,FileAccess.Read,FileShare.Read))
        {
            using (IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(s))
            {
                var sheetCnt = reader.ResultsCount;
                for (int i = 0; i < sheetCnt; i++)
                {
                    var sheetName = reader.Name;
                    var sheetReader = sheetReaders.Where(sheetReader => sheetReader.UseThisReader(sheetName)).OrderByDescending(reader => reader.Priority).First();
                    sheetReader.Load(sheetName,reader,scriptPath,jsonPath);
                    if (i < sheetCnt - 1) // 只在不是最后一个表格时调用NextResult
                    {
                        reader.NextResult();
                    }
                }
            }
        }

        AssetDatabase.Refresh();
    }

    
}
