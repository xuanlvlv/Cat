using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 弹幕管理器，负责弹幕的生成和管理
/// </summary>
public class DanmakuManager : MonoBehaviour
{
    [Header("弹幕设置")]
    [SerializeField] private Transform danmakuContainer;  // 弹幕容器
    [SerializeField] private Transform spawnPosition; // 生成位置
    [SerializeField] private float defaultSpeed = 100f;   // 默认上升速度
    [SerializeField] private float verticalSpacing = 10f; // 弹幕之间的垂直间距
    [SerializeField] private float minSpawnInterval = 0.5f; // 最小生成间隔
    [SerializeField] private float initialSpawnY = -200f;  // 初始生成Y坐标
    [SerializeField] private float maxSpawnY = 200f;      // 最大Y坐标限制
    
    [Header("弹幕生成设置")]
    private float baseInterval = 10.0f;  // 基础弹幕生成间隔
    private float minInterval = 2.0f;    // 最小弹幕生成间隔
    private int playCount = 0;           // 播放数计数
    [SerializeField] private float scChance = 0.2f;      // SC弹幕概率 (20%)
    [SerializeField] private float badChance = 0.05f;    // 恶评概率 (5%)
    
    private float currentInterval;        // 当前弹幕生成间隔
    private float danmakuTimer;          // 弹幕生成计时器
    private float lastSpawnTime;         // 上次生成弹幕的时间
    private DanmakuPools danmakuPools;   // 弹幕对象池管理器
    
    // 弹幕队列
    private Queue<DanmakuInfo> danmakuQueue = new Queue<DanmakuInfo>();
    
    // 弹幕信息结构
    private struct DanmakuInfo
    {
        public string content;
        public string username;
        public int type;
        
        public DanmakuInfo(string content, string username, int type)
        {
            this.content = content;
            this.username = username;
            this.type = type;
        }
    }

    #region 单例实现
    private static DanmakuManager _instance;
    public static DanmakuManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DanmakuManager>();
                if (_instance == null)
                {
                    Debug.LogError("场景中未找到DanmakuManager实例！");
                    GameObject go = new GameObject("DanmakuManager");
                    _instance = go.AddComponent<DanmakuManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 获取弹幕对象池管理器
            danmakuPools = DanmakuPools.Instance;
            
            // 检查必要组件
            ValidateComponents();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentInterval = baseInterval;
        danmakuTimer = currentInterval;
    }

    private void Update()
    {
        // 处理自动生成弹幕
        ProcessDanmaku();
        
        // 处理队列中的弹幕
        ProcessDanmakuQueue();
    }

    /// <summary>
    /// 验证必要组件
    /// </summary>
    private void ValidateComponents()
    {
        if (danmakuPools == null)
        {
            Debug.LogError("未找到弹幕对象池管理器！");
            return;
        }

        if (danmakuContainer == null)
        {
            Debug.LogWarning("弹幕容器未设置，将创建新容器。");
            GameObject container = new GameObject("DanmakuContainer");
            danmakuContainer = container.transform;
            danmakuContainer.SetParent(transform);
        }
    }

    /// <summary>
    /// 更新播放数并调整弹幕生成间隔
    /// </summary>
    public void IncrementPlayCount()
    {
        playCount++;
        
        // 每50次播放减少0.5秒间隔
        float intervalReduction = (playCount / 50) * 0.5f;
        currentInterval = Mathf.Max(minInterval, baseInterval - intervalReduction);
        
        Debug.Log($"播放数: {playCount}, 当前弹幕间隔: {currentInterval}秒");
    }

    /// <summary>
    /// 处理弹幕队列
    /// </summary>
    private void ProcessDanmakuQueue()
    {
        if (danmakuQueue.Count > 0 && Time.time >= lastSpawnTime + minSpawnInterval)
        {
            DanmakuInfo info = danmakuQueue.Dequeue();
            SpawnDanmaku(info.content, info.username, info.type);
            lastSpawnTime = Time.time;
        }
    }

    /// <summary>
    /// 处理弹幕生成
    /// </summary>
    private void ProcessDanmaku()
    {
        danmakuTimer -= Time.deltaTime;
        if (danmakuTimer <= 0)
        {
            // 获取随机弹幕数据
            Caption caption = DataManager.Instance.GetRandomCaption();
            if (caption != null)
            {
                // 根据概率决定弹幕类型
                float rand = Random.value;
                int danmakuType;
                
                if (rand < badChance)
                {
                    danmakuType = 3; // 恶评 (5%)
                }
                else if (rand < (badChance + scChance))
                {
                    danmakuType = 2; // SC (20%)
                }
                else
                {
                    danmakuType = 1; // 普通弹幕 (75%)
                }
                
                // 将弹幕添加到队列
                EnqueueDanmaku(caption.text, caption.name, danmakuType);
                
                // 增加播放数并更新间隔
                IncrementPlayCount();
            }
            
            // 重置计时器，使用当前计算出的间隔
            danmakuTimer = currentInterval;
        }
    }

    /// <summary>
    /// 将弹幕添加到队列
    /// </summary>
    private void EnqueueDanmaku(string content, string username, int type)
    {
        danmakuQueue.Enqueue(new DanmakuInfo(content, username, type));
    }

    /// <summary>
    /// 生成单个弹幕
    /// </summary>
    private void SpawnDanmaku(string content, string username, int type)
    {
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogWarning("弹幕内容不能为空！");
            return;
        }

        if (danmakuPools == null || spawnPosition == null)
        {
            Debug.LogError("弹幕系统未正确初始化或生成位置未设置！");
            return;
        }

        try
        {
            // 从对象池获取弹幕对象
            GameObject danmaku = danmakuPools.GetDanmakuByType(type);
            if (danmaku == null)
            {
                Debug.LogError($"无法从对象池获取类型 {type} 的弹幕对象！");
                return;
            }

            // 设置父物体
            if (danmakuContainer != null)
            {
                danmaku.transform.SetParent(danmakuContainer);
            }
            
            // 设置文本
            TextMeshProUGUI contentText = danmaku.GetComponentInChildren<TextMeshProUGUI>();
            if (contentText != null)
            {
                contentText.text = string.IsNullOrEmpty(username) ? content : $"{username}: {content}";
            }
            else
            {
                Debug.LogError("弹幕对象缺少TextMeshProUGUI组件！");
                return;
            }

            // 获取当前弹幕的RectTransform
            RectTransform rectTransform = danmaku.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("弹幕对象缺少RectTransform组件！");
                return;
            }

            // 强制布局刷新以获取正确的高度
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            // 设置到生成位置
            RectTransform spawnRect = spawnPosition as RectTransform;
            if (spawnRect != null)
            {
                // 使用spawnPosition的UI坐标
                rectTransform.anchoredPosition = spawnRect.anchoredPosition;
                rectTransform.anchorMin = spawnRect.anchorMin;
                rectTransform.anchorMax = spawnRect.anchorMax;
                rectTransform.pivot = spawnRect.pivot;
            }
            else
            {
                // 如果spawnPosition不是RectTransform，使用世界坐标转换为UI坐标
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, spawnPosition.position);
                Vector2 localPoint;
                RectTransform parentRect = danmakuContainer as RectTransform;
                if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, Camera.main, out localPoint))
                {
                    rectTransform.anchoredPosition = localPoint;
                }
            }

            rectTransform.localScale = Vector3.one;
            
            // 初始化弹幕组件
            DanmakuItem item = danmaku.GetComponent<DanmakuItem>();
            if (item == null)
            {
                item = danmaku.AddComponent<DanmakuItem>();
            }
            item.Initialize(content, username, type);
            item.speed = defaultSpeed;

            Debug.Log($"生成弹幕 - 位置: {rectTransform.anchoredPosition}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"显示弹幕时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 外部调用生成弹幕
    /// </summary>
    public void ShowDanmaku(string content, string username = "观众", int type = 1)
    {
        EnqueueDanmaku(content, username, type);
    }

    /// <summary>
    /// 根据类型生成随机弹幕（简化的外部调用接口）
    /// </summary>
    /// <param name="type">弹幕类型：1=普通弹幕，2=SC，3=恶评</param>
    public void GenerateRandomDanmaku(int type)
    {
        try
        {
            Caption caption = DataManager.Instance.GetRandomCaptionsByType(type);
            if (caption != null)
            {
                EnqueueDanmaku(caption.text, caption.name, type);
                Debug.Log($"生成类型 {type} 的随机弹幕");
            }
            else
            {
                Debug.LogWarning($"无法获取类型 {type} 的随机弹幕数据");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"生成随机弹幕时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 批量生成指定类型的随机弹幕
    /// </summary>
    /// <param name="type">弹幕类型</param>
    /// <param name="count">生成数量</param>
    public void GenerateRandomDanmakuBatch(int type, int count)
    {
        if (count <= 0) return;

        try
        {
            for (int i = 0; i < count; i++)
            {
                GenerateRandomDanmaku(type);
            }
            Debug.Log($"批量生成 {count} 条类型 {type} 的随机弹幕");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"批量生成随机弹幕时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 清空弹幕队列
    /// </summary>
    public void ClearDanmakuQueue()
    {
        danmakuQueue.Clear();
    }

    /// <summary>
    /// 重置播放数和间隔
    /// </summary>
    public void ResetPlayCount()
    {
        playCount = 0;
        currentInterval = baseInterval;
        danmakuTimer = currentInterval;
        Debug.Log("重置播放数和弹幕间隔");
    }

    /// <summary>
    /// 返回弹幕到对象池
    /// </summary>
    public void ReturnDanmaku(GameObject danmaku, int type)
    {
        if (danmaku == null)
        {
            Debug.LogWarning("尝试返回空弹幕对象！");
            return;
        }

        if (danmakuPools != null)
        {
            try
            {
                danmakuPools.ReturnDanmaku(danmaku, type);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"返回弹幕到对象池时发生错误: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("对象池未初始化，直接销毁弹幕对象");
            Destroy(danmaku);
        }
    }

    /// <summary>
    /// 清空所有弹幕
    /// </summary>
    public void ClearAllDanmakus()
    {
        if (danmakuPools != null)
        {
            try
            {
                danmakuPools.ClearAllPools();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"清空弹幕时发生错误: {e.Message}");
            }
        }
    }
}