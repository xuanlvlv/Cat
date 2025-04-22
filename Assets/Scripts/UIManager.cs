using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI管理器类，负责管理游戏界面的UI元素
/// </summary>
public class UIManager : MonoBehaviour
{
    #region 单例实现
    private static UIManager _instance;
    
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("UIManager");
                    _instance = go.AddComponent<UIManager>();
                }
            }
            return _instance;
        }
    }
    #endregion
    
    #region UI元素引用
    [Header("游戏界面")]
    [SerializeField] private TextMeshProUGUI fansCountText;     // 粉丝数文本
    [SerializeField] private TextMeshProUGUI viewCountText;     // 播放数文本
    [SerializeField] private Slider moodSlider;                 // 心情值滑块
    [SerializeField] private TextMeshProUGUI moodPercentText;   // 心情值百分比文本
    [SerializeField] private TextMeshProUGUI scChanceText;      // SC概率文本
    #endregion
    
    #region 游戏数据
    private int fansCount = 0;            // 粉丝数
    private int viewCount = 0;            // 播放数
    private int mood = 100;               // 心情值
    private int maxMood = 100;            // 最大心情值
    private float scChance = 0f;          // SC概率
    private float moodDecayTimer = 0f;    // 心情衰减计时器
    private const float MOOD_DECAY_INTERVAL = 3f; // 心情衰减间隔
    #endregion
    
    #region Unity生命周期方法
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // 初始化UI
        InitializeUI();
    }
    
    private void Update()
    {
        // 处理心情衰减
        HandleMoodDecay();
    }
    #endregion
    
    #region UI初始化和更新
    /// <summary>
    /// 初始化UI
    /// </summary>
    public void InitializeUI()
    {
        // 初始化UI显示
        UpdateFansUI();
        UpdateViewsUI();
        UpdateMoodUI();
        UpdateSCChanceUI();
    }
    
    /// <summary>
    /// 更新所有UI元素
    /// </summary>
    public void UpdateAllUI()
    {
        UpdateFansUI();
        UpdateViewsUI();
        UpdateMoodUI();
        UpdateSCChanceUI();
    }
    
    /// <summary>
    /// 更新粉丝数量UI
    /// </summary>
    public void UpdateFansUI()
    {
        if (fansCountText != null)
        {
            fansCountText.text = $"粉丝: {FormatNumber(fansCount)}";
        }
    }
    
    /// <summary>
    /// 更新播放数UI
    /// </summary>
    public void UpdateViewsUI()
    {
        if (viewCountText != null)
        {
            viewCountText.text = $"播放: {FormatNumber(viewCount)}";
        }
    }
    
    /// <summary>
    /// 更新心情UI
    /// </summary>
    public void UpdateMoodUI()
    {
        if (moodSlider != null)
        {
            float normalizedMood = (float)mood / maxMood;
            moodSlider.value = normalizedMood;
            
            if (moodPercentText != null)
            {
                moodPercentText.text = $"{Mathf.RoundToInt(normalizedMood * 100)}%";
            }
        }
    }
    
    /// <summary>
    /// 更新SC概率UI
    /// </summary>
    public void UpdateSCChanceUI()
    {
        if (scChanceText != null)
        {
            scChanceText.text = $"SC概率: {scChance:0.0}%";
        }
    }
    
    /// <summary>
    /// 格式化数字为易读形式
    /// </summary>
    private string FormatNumber(int number)
    {
        if (number >= 1000000)
        {
            return $"{(number / 1000000f):0.0}M";
        }
        else if (number >= 1000)
        {
            return $"{(number / 1000f):0.0}K";
        }
        else
        {
            return number.ToString();
        }
    }
    #endregion
    
    #region 游戏数据处理
    /// <summary>
    /// 处理心情衰减
    /// </summary>
    private void HandleMoodDecay()
    {
        moodDecayTimer += Time.deltaTime;
        
        if (moodDecayTimer >= MOOD_DECAY_INTERVAL)
        {
            moodDecayTimer = 0f;
            DecreaseMood(1);  // 每3秒心情值-1
        }
    }
    
    /// <summary>
    /// 开始新回合
    /// </summary>
    public void StartNewRound()
    {
        // 重置心情值
        mood = maxMood;
        
        // 计算初始播放数（粉丝数的10%）
        viewCount = Mathf.FloorToInt(fansCount * 0.1f);
        Debug.Log($"新回合开始 - 初始播放数: {viewCount}，基于粉丝数: {fansCount}");
        
        // 更新UI
        UpdateAllUI();
    }
    
    /// <summary>
    /// 增加粉丝数
    /// </summary>
    /// <param name="amount">增加的数量</param>
    public void AddFans(int amount)
    {
        fansCount += amount;
        
        // 每50个粉丝SC概率+1%
        scChance = Mathf.Floor(fansCount / 50f);
        
        UpdateFansUI();
        UpdateSCChanceUI();
    }
    
    /// <summary>
    /// 增加播放数
    /// </summary>
    /// <param name="amount">增加的数量</param>
    public void AddViews(int amount)
    {
        Debug.Log($"播放数增加: +{amount}，来自方块数字");
        viewCount += amount;
        
        // 播放数每满10增加1个粉丝
        int newFans = Mathf.FloorToInt(amount / 10f);
        if (newFans > 0)
        {
            Debug.Log($"播放数转化为粉丝: +{newFans}");
            AddFans(newFans);
        }
        
        UpdateViewsUI();
    }
    
    /// <summary>
    /// 增加心情值
    /// </summary>
    /// <param name="amount">增加的数量</param>
    public void IncreaseMood(int amount)
    {
        mood = Mathf.Min(mood + amount, maxMood);
        UpdateMoodUI();
    }
    
    /// <summary>
    /// 减少心情值
    /// </summary>
    /// <param name="amount">减少的数量</param>
    public void DecreaseMood(int amount)
    {
        mood = Mathf.Max(mood - amount, 0);
        UpdateMoodUI();
        
        // 当心情为0时可以触发游戏结束
        if (mood <= 0)
        {
            // 通知游戏管理器回合结束
            // 可以在这里添加回调或事件
            Debug.Log("主播心情值为0，游戏回合结束！");
        }
    }
    
    /// <summary>
    /// 获取当前SC概率
    /// </summary>
    public float GetSCChance()
    {
        return scChance;
    }
    
    /// <summary>
    /// 获取当前心情值
    /// </summary>
    public int GetMood()
    {
        return mood;
    }
    
    /// <summary>
    /// 获取当前粉丝数
    /// </summary>
    public int GetFansCount()
    {
        return fansCount;
    }
    
    /// <summary>
    /// 获取当前播放数
    /// </summary>
    public int GetViewCount()
    {
        return viewCount;
    }
    #endregion
    
    #region 工具方法
    /// <summary>
    /// 显示通知消息
    /// </summary>
    /// <param name="message">通知消息</param>
    public void ShowNotification(string message)
    {
        Debug.Log($"通知: {message}");
        // 可以在这里实现简单的屏幕提示
    }
    #endregion
} 