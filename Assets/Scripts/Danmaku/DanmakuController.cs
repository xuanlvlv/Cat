using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DanmakuController : MonoBehaviour
{
    [SerializeField] private DanmakuManager danmakuManager;
    
    private CaptionModel captionData;
    private Dictionary<int, Caption> captionsById;
    private Dictionary<int, List<Caption>> captionsByType;
    
    private float normalDanmakuTimer;
    private float superChatTimer;
    private float badReviewTimer;
    private float specialDanmakuTimer;
    
    [Header("弹幕生成概率与时间设置")]
    [SerializeField] private float normalDanmakuInterval = 3.0f;
    [SerializeField] private float superChatInterval = 15.0f;
    [SerializeField] private float badReviewInterval = 10.0f;
    [SerializeField] private float specialDanmakuInterval = 20.0f;
    
    [SerializeField, Range(0f, 1f)] private float superChatChance = 0.3f;
    [SerializeField, Range(0f, 1f)] private float badReviewChance = 0.2f;
    [SerializeField, Range(0f, 1f)] private float specialDanmakuChance = 0.1f;

    private void Awake()
    {
        LoadCaptionData();
        OrganizeCaptionData();
    }

    private void Start()
    {
        if (danmakuManager == null)
        {
            danmakuManager = FindObjectOfType<DanmakuManager>();
            if (danmakuManager == null)
            {
                Debug.LogError("找不到DanmakuManager组件");
                enabled = false;
                return;
            }
        }
        
        // 初始化计时器
        ResetTimers();
    }

    private void Update()
    {
        // 处理各种弹幕的生成时间
        ProcessNormalDanmaku();
        ProcessSuperChat();
        ProcessBadReview();
        ProcessSpecialDanmaku();
    }

    private void ResetTimers()
    {
        normalDanmakuTimer = normalDanmakuInterval;
        superChatTimer = superChatInterval;
        badReviewTimer = badReviewInterval;
        specialDanmakuTimer = specialDanmakuInterval;
    }

    private void LoadCaptionData()
    {
        try
        {
            // 尝试直接从文件读取JSON数据
            TextAsset jsonFile = Resources.Load<TextAsset>("Datas/Caption");
            
            if (jsonFile == null)
            {
                // 尝试其他可能的路径
                jsonFile = Resources.Load<TextAsset>("Caption");
            }
            
            if (jsonFile == null)
            {
                // 尝试从StreamingAssets目录加载
                string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Datas/Caption.json");
                if (System.IO.File.Exists(filePath))
                {
                    string jsonContent = System.IO.File.ReadAllText(filePath);
                    captionData = JsonUtility.FromJson<CaptionModel>(jsonContent);
                    Debug.Log($"从StreamingAssets加载弹幕配置成功，共{captionData.values.Count}条弹幕");
                    return;
                }
                
                Debug.LogWarning("未能加载Caption.json文件，将使用默认弹幕数据");
                CreateDefaultCaptionData();
                return;
            }
            
            captionData = JsonUtility.FromJson<CaptionModel>(jsonFile.text);
            
            // 检查反序列化结果是否为空
            if (captionData == null || captionData.values == null)
            {
                Debug.LogWarning("JSON反序列化失败，将使用默认弹幕数据");
                CreateDefaultCaptionData();
                return;
            }
            
            Debug.Log($"成功加载弹幕配置，共{captionData.values.Count}条弹幕");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载弹幕配置失败: {e.Message}\n堆栈信息: {e.StackTrace}");
            
            // 使用默认数据
            CreateDefaultCaptionData();
        }
    }
    
    /// <summary>
    /// 创建默认弹幕数据，确保即使配置加载失败也能正常运行
    /// </summary>
    private void CreateDefaultCaptionData()
    {
        captionData = new CaptionModel { values = new List<Caption>() };
        
        // 添加一些普通弹幕 (type=1)
        captionData.values.Add(new Caption {
            id = 1001,
            text = "猫猫好可爱！",
            name = "路人甲",
            type = 1,
            feel = 1,
            money = 0,
            fans = 1,
            chance = 0.1f
        });
        
        captionData.values.Add(new Caption {
            id = 1002,
            text = "主播好厉害",
            name = "路人乙",
            type = 1,
            feel = 1,
            money = 0,
            fans = 0,
            chance = 0.1f
        });
        
        captionData.values.Add(new Caption {
            id = 1003,
            text = "这个游戏真好玩",
            name = "游戏迷",
            type = 1,
            feel = 2,
            money = 0,
            fans = 1,
            chance = 0.1f
        });
        
        // 添加一些SC弹幕 (type=2)
        captionData.values.Add(new Caption {
            id = 2001,
            text = "主播加油~",
            name = "忠实粉丝",
            type = 2,
            feel = 5,
            money = 10,
            fans = 1,
            chance = 0.1f
        });
        
        captionData.values.Add(new Caption {
            id = 2002,
            text = "喜欢主播的直播",
            name = "土豪",
            type = 2,
            feel = 10,
            money = 50,
            fans = 2,
            chance = 0.1f
        });
        
        // 添加一些负面弹幕 (type=3)
        captionData.values.Add(new Caption {
            id = 3001,
            text = "这个游戏好无聊",
            name = "黑粉",
            type = 3,
            feel = -5,
            money = 0,
            fans = 0,
            chance = 0.1f
        });
        
        // 添加一些特殊弹幕 (type=4)
        captionData.values.Add(new Caption {
            id = 4001,
            text = "恭喜主播突破1000粉丝！",
            name = "系统",
            type = 4,
            feel = 20,
            money = 0,
            fans = 10,
            chance = 0.1f
        });
        
        Debug.Log($"已创建默认弹幕数据，共{captionData.values.Count}条弹幕");
    }

    private void OrganizeCaptionData()
    {
        captionsById = new Dictionary<int, Caption>();
        captionsByType = new Dictionary<int, List<Caption>>();
        
        foreach (var caption in captionData.values)
        {
            // 按ID索引
            captionsById[caption.id] = caption;
            
            // 按类型分组
            if (!captionsByType.ContainsKey(caption.type))
            {
                captionsByType[caption.type] = new List<Caption>();
            }
            captionsByType[caption.type].Add(caption);
        }
        
        Debug.Log($"弹幕数据整理完成，共{captionsById.Count}条弹幕，{captionsByType.Count}种类型");
    }

    private void ProcessNormalDanmaku()
    {
        normalDanmakuTimer -= Time.deltaTime;
        if (normalDanmakuTimer <= 0)
        {
            ShowRandomDanmakuByType(1); // 普通弹幕类型=1
            normalDanmakuTimer = normalDanmakuInterval;
        }
    }

    private void ProcessSuperChat()
    {
        superChatTimer -= Time.deltaTime;
        if (superChatTimer <= 0)
        {
            if (Random.value < superChatChance)
            {
                ShowRandomDanmakuByType(2); // 超级聊天类型=2
            }
            superChatTimer = superChatInterval;
        }
    }

    private void ProcessBadReview()
    {
        badReviewTimer -= Time.deltaTime;
        if (badReviewTimer <= 0)
        {
            if (Random.value < badReviewChance)
            {
                ShowRandomDanmakuByType(3); // 差评类型=3
            }
            badReviewTimer = badReviewInterval;
        }
    }

    private void ProcessSpecialDanmaku()
    {
        specialDanmakuTimer -= Time.deltaTime;
        if (specialDanmakuTimer <= 0)
        {
            if (Random.value < specialDanmakuChance)
            {
                ShowRandomDanmakuByType(4); // 特殊弹幕类型=4
            }
            specialDanmakuTimer = specialDanmakuInterval;
        }
    }

    private void ShowRandomDanmakuByType(int type)
    {
        if (!captionsByType.ContainsKey(type) || captionsByType[type].Count == 0)
        {
            Debug.LogWarning($"没有类型为 {type} 的弹幕");
            return;
        }

        int randomIndex = Random.Range(0, captionsByType[type].Count);
        Caption randomCaption = captionsByType[type][randomIndex];
        
        ShowCaptionAsDanmaku(randomCaption);
    }

    private void ShowCaptionAsDanmaku(Caption caption)
    {
        switch (caption.type)
        {
            case 1: // 普通弹幕
                danmakuManager.ShowNormalDanmaku(caption.text, caption.name);
                break;
            case 2: // 超级聊天
                danmakuManager.ShowSuperChatDanmaku(caption.text, caption.name);
                break;
            case 3: // 差评
                danmakuManager.ShowBadDanmaku(caption.text, caption.name);
                break;
            case 4: // 特殊弹幕
                danmakuManager.ShowSpecialDanmaku(caption.text, caption.name);
                break;
            default:
                danmakuManager.ShowNormalDanmaku(caption.text, caption.name);
                break;
        }
        
        // 处理弹幕的游戏效果
        ProcessCaptionEffects(caption);
    }

    private void ProcessCaptionEffects(Caption caption)
    {
        // 这里可以触发游戏效果，比如改变心情、加钱、加粉丝等
        // 可以通过GameManager或其他管理器来实现
        
        // 简单示例：
        if (caption.feel != 0)
        {
            // 增加/减少心情值
            Debug.Log($"弹幕效果：心情变化 {caption.feel}");
        }
        
        if (caption.money > 0)
        {
            // 增加金钱
            Debug.Log($"弹幕效果：获得金钱 {caption.money}");
        }
        
        if (caption.fans > 0)
        {
            // 增加粉丝
            Debug.Log($"弹幕效果：获得粉丝 {caption.fans}");
        }
    }

    // 提供公共方法以便其他系统可以手动触发特定弹幕
    public void ShowCaptionById(int id)
    {
        if (captionsById.TryGetValue(id, out Caption caption))
        {
            ShowCaptionAsDanmaku(caption);
        }
        else
        {
            Debug.LogWarning($"未找到ID为 {id} 的弹幕");
        }
    }

    // 可以添加根据条件筛选弹幕的方法
    public void ShowRandomDanmakuWithFans(int minFans)
    {
        List<Caption> eligibleCaptions = new List<Caption>();
        
        foreach (var caption in captionData.values)
        {
            if (caption.fans >= minFans)
            {
                eligibleCaptions.Add(caption);
            }
        }
        
        if (eligibleCaptions.Count > 0)
        {
            int randomIndex = Random.Range(0, eligibleCaptions.Count);
            ShowCaptionAsDanmaku(eligibleCaptions[randomIndex]);
        }
    }
} 