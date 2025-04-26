using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 数据管理器，负责加载和访问Excel转换的JSON数据
/// </summary>
public class DataManager : MonoBehaviour
{
    private static DataManager _instance;
    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("DataManager");
                _instance = go.AddComponent<DataManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // 存储所有模型数据的字典
    private Dictionary<string, IModel> _dataModels = new Dictionary<string, IModel>();
    
    // 存储弹幕数据的字典
    private Dictionary<int, Caption> _captionsById = new Dictionary<int, Caption>();
    private Dictionary<int, List<Caption>> _captionsByType = new Dictionary<int, List<Caption>>();
    
    // 提供公共访问接口
    public IReadOnlyDictionary<int, Caption> CaptionsById => _captionsById;
    public IReadOnlyDictionary<int, List<Caption>> CaptionsByType => _captionsByType;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
        
        // 初始化时加载所有数据
        LoadAllData();
    }

    /// <summary>
    /// 加载所有JSON数据
    /// </summary>
    public void LoadAllData()
    {
        LoadData<CaptionModel>("Caption");
        OrganizeCaptionData();
    }

    /// <summary>
    /// 加载指定的JSON数据
    /// </summary>
    /// <typeparam name="T">模型类型</typeparam>
    /// <param name="sheetName">表格名称</param>
    /// <returns>加载的数据模型</returns>
    public T LoadData<T>(string sheetName) where T : IModel
    {
        string jsonPath = $"JsonData/{sheetName}";
        TextAsset jsonAsset = Resources.Load<TextAsset>(jsonPath);
        
        if (jsonAsset == null)
        {
            Debug.LogError($"无法加载JSON数据：{jsonPath}");
            return default;
        }
        
        T model = JsonConvert.DeserializeObject<T>(jsonAsset.text);
        _dataModels[sheetName] = model;
        //Debug.Log($"已加载数据：{sheetName}");
        
        return model;
    }

    /// <summary>
    /// 获取指定类型的数据模型
    /// </summary>
    /// <typeparam name="T">模型类型</typeparam>
    /// <param name="sheetName">表格名称</param>
    /// <returns>数据模型</returns>
    public T GetData<T>(string sheetName) where T : IModel
    {
        if (_dataModels.TryGetValue(sheetName, out IModel model))
        {
            return (T)model;
        }
        
        // 如果数据未加载，尝试加载
        return LoadData<T>(sheetName);
    }

    /// <summary>
    /// 获取弹幕配置数据
    /// </summary>
    public CaptionModel GetCaptionData()
    {
        return GetData<CaptionModel>("Caption");
    }

    /// <summary>
    /// 整理弹幕数据到字典中
    /// </summary>
    private void OrganizeCaptionData()
    {
        _captionsById.Clear();
        _captionsByType.Clear();
        
        var captionData = GetCaptionData();
        if (captionData == null || captionData.values == null)
        {
            Debug.LogError("弹幕数据为空！");
            return;
        }
        
        foreach (var caption in captionData.values)
        {
            // 按ID存储
            if (!_captionsById.ContainsKey(caption.id))
            {
                _captionsById.Add(caption.id, caption);
            }
            else
            {
                Debug.LogWarning($"发现重复的弹幕ID: {caption.id}");
            }
            
            // 按类型分组存储
            if (!_captionsByType.ContainsKey(caption.type))
            {
                _captionsByType[caption.type] = new List<Caption>();
            }
            _captionsByType[caption.type].Add(caption);
        }
        
        Debug.Log($"弹幕数据整理完成：共{_captionsById.Count}条弹幕，{_captionsByType.Count}种类型");
    }

    /// <summary>
    /// 根据ID获取弹幕
    /// </summary>
    public Caption GetCaptionById(int id)
    {
        if (_captionsById.TryGetValue(id, out Caption caption))
        {
            return caption;
        }
        return null;
    }

    /// <summary>
    /// 获取指定类型的所有弹幕
    /// </summary>
    public List<Caption> GetCaptionsByType(int type)
    {
        if (_captionsByType.TryGetValue(type, out List<Caption> captions))
        {
            return captions;
        }
        return new List<Caption>();
    }

    /// <summary>
    /// 获取指定类型的随机弹幕
    /// </summary>
    public Caption GetRandomCaptionsByType(int type)
    {
        List<Caption> captions;
        if (_captionsByType.TryGetValue(type, out captions))
        {
            return captions[Random.Range(0, captions.Count)];
        }
        return null;
    }

    /// <summary>
    /// 获取随机弹幕
    /// </summary>
    public Caption GetRandomCaption(int type = 1)
    {
        if (_captionsByType.TryGetValue(type, out List<Caption> captions) && captions.Count > 0)
        {
            return captions[Random.Range(0, captions.Count)];
        }
        return null;
    }
}