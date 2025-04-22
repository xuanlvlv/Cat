using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏管理器类，实现单例模式，负责管理游戏状态、数据和核心游戏循环
/// </summary>
public class GameManager : MonoBehaviour
{
    #region 单例实现
    private static GameManager _instance;
    
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 尝试在场景中查找实例
                _instance = FindObjectOfType<GameManager>();
                
                // 如果没有找到，创建一个新实例
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }
    #endregion
   

} 