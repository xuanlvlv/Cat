using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弹幕对象池管理器
/// </summary>
public class DanmakuPools : MonoBehaviour
{

    // 弹幕类型对应的对象池
    [SerializeField]private ObjectPool normalDanmakuPool;
    [SerializeField] private ObjectPool superChatPool;
    [SerializeField] private ObjectPool badDanmakuPool;
    [SerializeField] private ObjectPool specialDanmakuPool;

    // 单例实现
    private static DanmakuPools _instance;
    public static DanmakuPools Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DanmakuPools>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DanmakuPools");
                    _instance = go.AddComponent<DanmakuPools>();
                }
            }
            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            //InitializePools();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 根据弹幕类型获取对象
    /// </summary>
    public GameObject GetDanmakuByType(int type)
    {
        try
        {
            switch (type)
            {
                case 1: // 普通弹幕
                    return normalDanmakuPool?.GetObject();
                case 2: // 超级聊天
                    return superChatPool?.GetObject();
                case 3: // 负面弹幕
                    return badDanmakuPool?.GetObject();
                case 4: // 特殊弹幕
                    return specialDanmakuPool?.GetObject();
                default:
                    Debug.LogWarning($"未知的弹幕类型: {type}，将使用普通弹幕");
                    return normalDanmakuPool?.GetObject();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"获取弹幕对象时发生错误: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 返回弹幕对象到对应的对象池
    /// </summary>
    public void ReturnDanmaku(GameObject danmaku, int type)
    {
        if (danmaku == null) return;

        try
        {
            switch (type)
            {
                case 1:
                    normalDanmakuPool?.ReturnObject(danmaku);
                    break;
                case 2:
                    superChatPool?.ReturnObject(danmaku);
                    break;
                case 3:
                    badDanmakuPool?.ReturnObject(danmaku);
                    break;
                default:
                    Debug.LogWarning($"未知的弹幕类型: {type}，将返回到普通弹幕池");
                    normalDanmakuPool?.ReturnObject(danmaku);
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"返回弹幕对象时发生错误: {e.Message}");
            Destroy(danmaku);
        }
    }

    /// <summary>
    /// 清空所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        try
        {
            normalDanmakuPool?.Reset();
            superChatPool?.Reset();
            badDanmakuPool?.Reset();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"清空对象池时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 获取对应类型的对象池
    /// </summary>
    public ObjectPool GetPoolByType(int type)
    {
        switch (type)
        {
            case 1:
                return normalDanmakuPool;
            case 2:
                return superChatPool;
            case 3:
                return badDanmakuPool;
            default:
                return normalDanmakuPool;
        }
    }
}
