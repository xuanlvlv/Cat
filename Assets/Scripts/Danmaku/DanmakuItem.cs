using UnityEngine;
using System.Collections;

/// <summary>
/// 弹幕项组件，附加到每个弹幕对象上，用于管理单个弹幕的状态和行为
/// </summary>
public class DanmakuItem : MonoBehaviour
{
    // 弹幕内容
    public string content { get; private set; }
    public string username { get; private set; }
    
    // 弹幕参数
    public int columnIndex { get; private set; }  // 改为列索引
    public float speed { get; set; }
    public float lifetime { get; private set; }
    
    // 初始生命周期
    private float initialLifetime = 8f;
    
    /// <summary>
    /// 初始化弹幕项
    /// </summary>
    public void Initialize(string content, string username, int columnIndex, float speed)
    {
        this.content = content;
        this.username = username;
        this.columnIndex = columnIndex;
        this.speed = speed;
        this.lifetime = initialLifetime;
    }
    
    /// <summary>
    /// 更新弹幕生命周期
    /// </summary>
    public void UpdateLifetime(float deltaTime)
    {
        lifetime -= deltaTime;
        
        // 在生命周期结束前开始淡出
        if (lifetime < 1.5f)
        {
            // 应用淡出效果
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = Mathf.Clamp01(lifetime / 1.5f);
        }
    }
    
    /// <summary>
    /// 重置弹幕状态，在归还对象池时调用
    /// </summary>
    public void Reset()
    {
        content = string.Empty;
        username = string.Empty;
        lifetime = initialLifetime;
        
        // 重置透明度
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
} 