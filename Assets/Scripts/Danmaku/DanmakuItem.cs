using System;
using UnityEngine;

/// <summary>
/// 弹幕项组件，管理单个弹幕的状态和行为
/// </summary>
public class DanmakuItem : MonoBehaviour
{
    // 弹幕内容
    public string content { get; private set; }
    public string username { get; private set; }
    
    // 弹幕参数
    public float speed { get; set; } = 100f;
    public float lifetime { get; private set; }
    public int type { get; private set; } = 1;  // 默认为普通弹幕
    
    // 初始生命周期
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private bool _isInitialized;
    
    private void Awake()
    {
        // 确保有必要组件
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform == null)
        {
            Debug.LogError($"弹幕对象 {gameObject.name} 缺少RectTransform组件！");
        }
    }
    
    /// <summary>
    /// 初始化弹幕项
    /// </summary>
    public void Initialize(string content, string username, int type = 1)
    {
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogWarning("初始化弹幕时内容为空！");
            content = "空弹幕";
        }
        
        this.content = content;
        this.username = username ?? "未知用户";
        this.type = type;
          
        this.lifetime = 10f;
        
        // 重置状态
        ResetState();
       
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// 重置弹幕状态
    /// </summary>
    private void ResetState()
    {
        // 重置透明度
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }
        
        // 重置变换
        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one;
        }
    }
    
    /// <summary>
    /// 设置弹幕的初始位置
    /// </summary>
    private void SetInitialPosition()
    {
        if (_rectTransform != null)
        {
            // 保持当前X轴位置不变，设置统一的Y轴位置（这里设置为-100，您可以根据需要调整）
            Vector3 position = _rectTransform.localPosition;
            position.y = -100f; // 统一的Y轴初始位置
            _rectTransform.localPosition = position;
        }
    }
    
    /// <summary>
    /// 更新弹幕生命周期和位置
    /// </summary>
    private void Update()
    {
        if (!_isInitialized) return;
        
        try
        {
            // 更新生命周期
            lifetime -= Time.deltaTime;
            
            // 只在Y轴方向移动
            if (_rectTransform != null)
            {
                _rectTransform.Translate(Vector3.up * speed * Time.deltaTime);
            }
            
            // 淡出效果
            if (lifetime < 1.0f && _canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Max(0, lifetime / 1.0f);
            }
            
            // 生命周期结束后返回对象池
            if (lifetime <= 0)
            {
                ReturnToPool();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"更新弹幕时发生错误: {e.Message}");
            ReturnToPool();
        }
    }
    
    /// <summary>
    /// 返回对象池
    /// </summary>
    private void ReturnToPool()
    {
        if (DanmakuManager.Instance != null)
        {
            _isInitialized = false;
            DanmakuManager.Instance.ReturnDanmaku(gameObject, type);
        }
        else
        {
            Debug.LogWarning("DanmakuManager实例不存在，直接销毁弹幕");
            Destroy(gameObject);
        }
    }
    
    private void OnDisable()
    {
        // 禁用时重置状态
        _isInitialized = false;
        lifetime = 0;
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }
    }
    
    private void OnDestroy()
    {
        // 清理引用
        _canvasGroup = null;
        _rectTransform = null;
    }
} 