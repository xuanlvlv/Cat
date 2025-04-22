using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 弹幕管理器，负责弹幕的显示、回收和轨道管理
/// </summary>
public class DanmakuManager : MonoBehaviour
{
    #region 弹幕预制体
    [Header("弹幕预制体")]
    [SerializeField] private GameObject normalDanmakuPrefab;    // 普通弹幕预制体
    [SerializeField] private GameObject superChatPrefab;        // SC预制体
    [SerializeField] private GameObject badDanmakuPrefab;       // 负面弹幕预制体
    [SerializeField] private GameObject specialDanmakuPrefab;   // 特殊弹幕预制体
    #endregion

    #region 弹幕设置
    [Header("弹幕设置")]
    [SerializeField] private float defaultSpeed = 80f;         // 默认弹幕移动速度
    [SerializeField] private int maxDanmakuCount = 100;         // 最大同时显示的弹幕数量
    [SerializeField] private int columnCount = 4;               // 弹幕列数
    [SerializeField] private float danmakuWidth = 300f;         // 弹幕宽度
    [SerializeField] private float columnSpacing = 20f;         // 列间距
    [SerializeField] private Transform danmakuContainer;        // 弹幕容器
    
    [Header("聊天窗口设置")]
    [SerializeField] private RectTransform chatWindowRect;      // 聊天窗口矩形区域
    [SerializeField] private float topPadding = 10f;            // 顶部间距
    [SerializeField] private float bottomPadding = 10f;         // 底部间距

    [Header("垂直对齐设置")]
    [SerializeField] private bool useFixedPositions = true;      // 是否使用固定位置
    [SerializeField] private float verticalSpacing = 10f;        // 垂直间距
    [SerializeField] private bool allowOverlap = false;          // 是否允许弹幕重叠
    #endregion

    #region 对象池
    // 弹幕对象池
    private Queue<GameObject> normalDanmakuPool = new Queue<GameObject>();
    private Queue<GameObject> superChatPool = new Queue<GameObject>();
    private Queue<GameObject> badDanmakuPool = new Queue<GameObject>();
    private Queue<GameObject> specialDanmakuPool = new Queue<GameObject>();
    
    // 当前活跃弹幕列表
    private List<GameObject> activeDanmakus = new List<GameObject>();
    #endregion

    #region 列管理
    // 列占用情况（记录每列最上方弹幕的上边缘位置）
    private float[] columnTopEdges;
    #endregion

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
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 初始化列数据
        columnTopEdges = new float[columnCount];
        ResetColumnTopEdges();

        // 初始化对象池
        InitializePools();
        
        // 如果没有设置聊天窗口矩形，则使用弹幕容器的矩形
        if (chatWindowRect == null && danmakuContainer != null)
        {
            chatWindowRect = danmakuContainer as RectTransform;
        }
    }

    private void ResetColumnTopEdges()
    {
        for (int i = 0; i < columnCount; i++)
        {
            // 初始时列为空，设置为聊天窗口底部
            columnTopEdges[i] = GetChatWindowBottomEdge();
        }
    }
    
    private float GetChatWindowBottomEdge()
    {
        if (chatWindowRect != null)
        {
            return chatWindowRect.rect.yMin + bottomPadding;
        }
        return bottomPadding;
    }
    
    private float GetChatWindowTopEdge()
    {
        if (chatWindowRect != null)
        {
            return chatWindowRect.rect.yMax - topPadding;
        }
        return Screen.height - topPadding;
    }
    
    private float GetChatWindowWidth()
    {
        if (chatWindowRect != null)
        {
            return chatWindowRect.rect.width;
        }
        return Screen.width;
    }

    private void Update()
    {
        // 更新弹幕位置和状态
        UpdateDanmakus();
    }

    /// <summary>
    /// 初始化弹幕对象池
    /// </summary>
    private void InitializePools()
    {
        // 初始化各种弹幕池
        for (int i = 0; i < 20; i++)
        {
            CreatePoolItem(normalDanmakuPrefab, normalDanmakuPool);
            CreatePoolItem(superChatPrefab, superChatPool);
            CreatePoolItem(badDanmakuPrefab, badDanmakuPool);
            CreatePoolItem(specialDanmakuPrefab, specialDanmakuPool);
        }
    }

    /// <summary>
    /// 创建对象池项
    /// </summary>
    private void CreatePoolItem(GameObject prefab, Queue<GameObject> pool)
    {
        if (prefab == null) return;

        GameObject item = Instantiate(prefab, danmakuContainer);
        item.SetActive(false);
        pool.Enqueue(item);
    }

    /// <summary>
    /// 从池中获取弹幕对象
    /// </summary>
    private GameObject GetPoolItem(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count == 0)
        {
            // 池中无可用对象，创建新对象
            CreatePoolItem(prefab, pool);
        }

        GameObject item = pool.Dequeue();
        item.SetActive(true);
        activeDanmakus.Add(item);
        return item;
    }

    /// <summary>
    /// 返回弹幕对象到池中
    /// </summary>
    private void ReturnToPool(GameObject item)
    {
        // 按弹幕类型返回到对应的池
        activeDanmakus.Remove(item);
        item.SetActive(false);

        // 重置弹幕状态
        DanmakuItem danmakuItem = item.GetComponent<DanmakuItem>();
        if (danmakuItem != null)
        {
            danmakuItem.Reset();
        }

        // 判断弹幕类型并归还到正确的池
        if (item.name.Contains(normalDanmakuPrefab.name))
        {
            normalDanmakuPool.Enqueue(item);
        }
        else if (item.name.Contains(superChatPrefab.name))
        {
            superChatPool.Enqueue(item);
        }
        else if (item.name.Contains(badDanmakuPrefab.name))
        {
            badDanmakuPool.Enqueue(item);
        }
        else if (item.name.Contains(specialDanmakuPrefab.name))
        {
            specialDanmakuPool.Enqueue(item);
        }
    }

    /// <summary>
    /// 更新所有活跃弹幕状态
    /// </summary>
    private void UpdateDanmakus()
    {
        // 临时列表存储需要回收的弹幕
        List<GameObject> toRecycle = new List<GameObject>();
        
        // 聊天窗口上边界(移除的位置)
        float chatWindowTopEdge = GetChatWindowTopEdge();
        
        // 更新每个弹幕位置和状态
        foreach (GameObject danmaku in activeDanmakus)
        {
            DanmakuItem item = danmaku.GetComponent<DanmakuItem>();
            if (item == null) continue;

            // 更新生命周期
            item.UpdateLifetime(Time.deltaTime);
            
            // 更新位置
            RectTransform rectTransform = danmaku.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 向上移动弹幕
                Vector2 position = rectTransform.anchoredPosition;
                position.y += item.speed * Time.deltaTime;
                rectTransform.anchoredPosition = position;
                
                // 获取弹幕高度
                float danmakuHeight = rectTransform.rect.height;
                
                // 更新该列的最上边缘位置
                if (item.columnIndex >= 0 && item.columnIndex < columnCount)
                {
                    float topEdge = position.y + danmakuHeight / 2; // 计算弹幕上边缘
                    if (topEdge > columnTopEdges[item.columnIndex])
                    {
                        columnTopEdges[item.columnIndex] = topEdge;
                    }
                }

                // 检查是否移出聊天窗口或生命周期结束
                if (position.y > chatWindowTopEdge || item.lifetime <= 0)
                {
                    toRecycle.Add(danmaku);
                }
            }
        }

        // 回收需要回收的弹幕
        foreach (GameObject danmaku in toRecycle)
        {
            ReturnToPool(danmaku);
        }
        
        // 如果使用固定位置，重新整理每列中弹幕的位置
        if (useFixedPositions)
        {
            // 为每列创建弹幕列表
            List<GameObject>[] columnDanmakus = new List<GameObject>[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                columnDanmakus[i] = new List<GameObject>();
            }
            
            // 收集每列中的弹幕
            foreach (GameObject danmaku in activeDanmakus)
            {
                DanmakuItem item = danmaku.GetComponent<DanmakuItem>();
                if (item != null && item.columnIndex >= 0 && item.columnIndex < columnCount)
                {
                    columnDanmakus[item.columnIndex].Add(danmaku);
                }
            }
            
            // 处理每列中的弹幕
            for (int i = 0; i < columnCount; i++)
            {
                // 按Y坐标从小到大排序弹幕
                columnDanmakus[i].Sort((a, b) => {
                    RectTransform rectA = a.GetComponent<RectTransform>();
                    RectTransform rectB = b.GetComponent<RectTransform>();
                    return rectA.anchoredPosition.y.CompareTo(rectB.anchoredPosition.y);
                });
                
                // 调整弹幕垂直位置
                float lastYPos = GetChatWindowBottomEdge();
                for (int j = 0; j < columnDanmakus[i].Count; j++)
                {
                    GameObject danmaku = columnDanmakus[i][j];
                    RectTransform rectTransform = danmaku.GetComponent<RectTransform>();
                    
                    if (!allowOverlap) // 如果不允许重叠
                    {
                        float danmakuHeight = rectTransform.rect.height;
                        float desiredYPos = lastYPos + danmakuHeight / 2 + verticalSpacing;
                        
                        // 只有当希望位置比当前位置低时才上移
                        if (desiredYPos > rectTransform.anchoredPosition.y)
                        {
                            Vector2 pos = rectTransform.anchoredPosition;
                            pos.y = desiredYPos;
                            rectTransform.anchoredPosition = pos;
                        }
                        
                        lastYPos = rectTransform.anchoredPosition.y + danmakuHeight / 2;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 获取可用列
    /// </summary>
    private int GetAvailableColumn()
    {
        // 找出最空闲的列（上边缘位置最小的）
        int bestColumn = 0;
        float minTopEdge = float.MaxValue;
        
        for (int i = 0; i < columnCount; i++)
        {
            if (columnTopEdges[i] < minTopEdge)
            {
                minTopEdge = columnTopEdges[i];
                bestColumn = i;
            }
        }
        
        return bestColumn;
    }

    /// <summary>
    /// 获取列X坐标位置
    /// </summary>
    private float GetColumnXPosition(int columnIndex)
    {
        // 计算聊天窗口宽度
        float chatWindowWidth = GetChatWindowWidth();
        // 计算聊天窗口左边缘
        float chatWindowLeft = chatWindowRect != null ? chatWindowRect.rect.xMin : 0f;
        
        // 均匀分配列的位置，确保垂直对齐
        float columnSpacingTotal = chatWindowWidth / (columnCount + 1);
        // 从左到右布局列，返回列的中心X坐标
        return chatWindowLeft + columnSpacingTotal * (columnIndex + 1);
    }

    /// <summary>
    /// 获取列起始Y坐标位置（弹幕从此位置开始向上移动）
    /// </summary>
    private float GetColumnYStartPosition(int columnIndex, float danmakuHeight)
    {
        // 获取聊天窗口底部边缘
        float chatWindowBottom = GetChatWindowBottomEdge();
        // 弹幕底部边缘与聊天窗口底部对齐
        return chatWindowBottom + danmakuHeight * 0.5f;
    }

    #region 公共方法
    /// <summary>
    /// 显示普通弹幕
    /// </summary>
    public void ShowNormalDanmaku(string content, string username = "观众")
    {
        // 检查最大数量限制
        if (activeDanmakus.Count >= maxDanmakuCount)
        {
            // 找到最老的弹幕并回收
            GameObject oldestDanmaku = activeDanmakus[0];
            ReturnToPool(oldestDanmaku);
        }

        // 获取可用列
        int columnIndex = GetAvailableColumn();
        
        // 获取弹幕对象
        GameObject danmaku = GetPoolItem(normalDanmakuPool, normalDanmakuPrefab);
        
        // 设置内容
        TextMeshProUGUI contentText = danmaku.GetComponentInChildren<TextMeshProUGUI>();
        if (contentText != null)
        {
            contentText.text = string.IsNullOrEmpty(username) ? content : $"{username}: {content}";
            contentText.alignment = TextAlignmentOptions.Center; // 文本居中对齐
        }
        
        // 设置位置和尺寸
        RectTransform rectTransform = danmaku.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 确保弹幕宽度合适
            rectTransform.sizeDelta = new Vector2(danmakuWidth, rectTransform.sizeDelta.y);
            
            // 计算起始位置
            float xPosition = GetColumnXPosition(columnIndex);
            float yPosition = GetColumnYStartPosition(columnIndex, rectTransform.rect.height);
            
            // 设置弹幕位置
            rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
            
            // 更新列占用情况
            columnTopEdges[columnIndex] = yPosition + rectTransform.rect.height / 2;
        }
        
        // 初始化弹幕项组件
        DanmakuItem item = danmaku.GetComponent<DanmakuItem>();
        if (item == null)
        {
            item = danmaku.AddComponent<DanmakuItem>();
        }
        item.Initialize(content, username, columnIndex, defaultSpeed);
    }

    /// <summary>
    /// 显示SC弹幕(打赏)
    /// </summary>
    public void ShowSuperChatDanmaku(string content, string username)
    {
        int columnIndex = GetAvailableColumn();
        
        GameObject danmaku = GetPoolItem(superChatPool, superChatPrefab);
        
        // 设置内容和用户名
        TextMeshProUGUI contentText = danmaku.GetComponentInChildren<TextMeshProUGUI>();
        if (contentText != null)
        {
            // 为SC弹幕添加特殊格式
            contentText.text = $"{username}: {content}";
            contentText.alignment = TextAlignmentOptions.Center; // 文本居中对齐
        }
        
        // 设置位置和尺寸
        RectTransform rectTransform = danmaku.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // SC弹幕通常会比普通弹幕宽一些
            rectTransform.sizeDelta = new Vector2(danmakuWidth * 1.2f, rectTransform.sizeDelta.y);
            
            // 计算起始位置
            float xPosition = GetColumnXPosition(columnIndex);
            float yPosition = GetColumnYStartPosition(columnIndex, rectTransform.rect.height);
            
            // 设置弹幕位置
            rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
            
            // 更新列占用情况
            columnTopEdges[columnIndex] = yPosition + rectTransform.rect.height / 2;
        }
        
        // 初始化弹幕项
        DanmakuItem item = danmaku.GetComponent<DanmakuItem>();
        if (item == null)
        {
            item = danmaku.AddComponent<DanmakuItem>();
        }
        // SC弹幕移动较慢，给予较长生命周期
        item.Initialize(content, username, columnIndex, defaultSpeed * 0.7f);
    }

    /// <summary>
    /// 显示负面弹幕
    /// </summary>
    public void ShowBadDanmaku(string content, string username = "观众")
    {
        int columnIndex = GetAvailableColumn();
        
        GameObject danmaku = GetPoolItem(badDanmakuPool, badDanmakuPrefab);
        
        // 设置内容
        TextMeshProUGUI contentText = danmaku.GetComponentInChildren<TextMeshProUGUI>();
        if (contentText != null)
        {
            contentText.text = string.IsNullOrEmpty(username) ? content : $"{username}: {content}";
            contentText.alignment = TextAlignmentOptions.Center; // 文本居中对齐
        }
        
        // 设置位置和尺寸
        RectTransform rectTransform = danmaku.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(danmakuWidth, rectTransform.sizeDelta.y);
            
            // 计算起始位置
            float xPosition = GetColumnXPosition(columnIndex);
            float yPosition = GetColumnYStartPosition(columnIndex, rectTransform.rect.height);
            
            // 设置弹幕位置
            rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
            
            // 更新列占用情况
            columnTopEdges[columnIndex] = yPosition + rectTransform.rect.height / 2;
        }
        
        // 初始化弹幕项
        DanmakuItem item = danmaku.GetComponent<DanmakuItem>();
        if (item == null)
        {
            item = danmaku.AddComponent<DanmakuItem>();
        }
        item.Initialize(content, username, columnIndex, defaultSpeed * 0.9f);
    }

    /// <summary>
    /// 显示特殊弹幕
    /// </summary>
    public void ShowSpecialDanmaku(string content, string username = "系统")
    {
        int columnIndex = GetAvailableColumn();
        
        GameObject danmaku = GetPoolItem(specialDanmakuPool, specialDanmakuPrefab);
        
        // 设置内容
        TextMeshProUGUI contentText = danmaku.GetComponentInChildren<TextMeshProUGUI>();
        if (contentText != null)
        {
            contentText.text = string.IsNullOrEmpty(username) ? content : $"{username}: {content}";
            contentText.alignment = TextAlignmentOptions.Center; // 文本居中对齐
        }
        
        // 设置位置和尺寸
        RectTransform rectTransform = danmaku.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 特殊弹幕通常会比普通弹幕宽一些
            rectTransform.sizeDelta = new Vector2(danmakuWidth * 1.5f, rectTransform.sizeDelta.y);
            
            // 计算起始位置
            float xPosition = GetColumnXPosition(columnIndex);
            float yPosition = GetColumnYStartPosition(columnIndex, rectTransform.rect.height);
            
            // 设置弹幕位置
            rectTransform.anchoredPosition = new Vector2(xPosition, yPosition);
            
            // 更新列占用情况
            columnTopEdges[columnIndex] = yPosition + rectTransform.rect.height / 2;
        }
        
        // 初始化弹幕项
        DanmakuItem item = danmaku.GetComponent<DanmakuItem>();
        if (item == null)
        {
            item = danmaku.AddComponent<DanmakuItem>();
        }
        // 特殊弹幕移动较慢，给予较长生命周期
        item.Initialize(content, username, columnIndex, defaultSpeed * 0.6f);
    }

    /// <summary>
    /// 清空所有弹幕
    /// </summary>
    public void ClearAllDanmakus()
    {
        // 复制列表以避免在遍历时修改集合
        List<GameObject> danmakusToRemove = new List<GameObject>(activeDanmakus);
        
        foreach (GameObject danmaku in danmakusToRemove)
        {
            ReturnToPool(danmaku);
        }
        
        // 重置列状态
        ResetColumnTopEdges();
    }

    /// <summary>
    /// 设置弹幕速度
    /// </summary>
    public void SetDanmakuSpeed(float speed)
    {
        defaultSpeed = speed;
        
        // 更新现有弹幕的速度
        foreach (GameObject danmaku in activeDanmakus)
        {
            DanmakuItem item = danmaku.GetComponent<DanmakuItem>();
            if (item != null)
            {
                item.speed = speed;
            }
        }
    }

    /// <summary>
    /// 设置弹幕最大数量
    /// </summary>
    public void SetMaxDanmakuCount(int count)
    {
        maxDanmakuCount = count;
        
        // 如果当前活跃弹幕数超过新限制，删除多余弹幕
        while (activeDanmakus.Count > maxDanmakuCount && activeDanmakus.Count > 0)
        {
            GameObject danmakuToRemove = activeDanmakus[0];
            ReturnToPool(danmakuToRemove);
        }
    }
    
    /// <summary>
    /// 设置弹幕宽度
    /// </summary>
    public void SetDanmakuWidth(float width)
    {
        danmakuWidth = width;
        
        // 注意：这只会影响新生成的弹幕
    }
    
    /// <summary>
    /// 设置列数
    /// </summary>
    public void SetColumnCount(int count)
    {
        if (count < 1) count = 1;
        
        columnCount = count;
        columnTopEdges = new float[columnCount];
        ResetColumnTopEdges();
    }
    #endregion
}