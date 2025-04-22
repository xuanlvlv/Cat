using UnityEngine;

/// <summary>
/// 特殊方块类型
/// </summary>
public enum SpecialTileType
{
    FansIncrease,    // 吸引粉丝：增加粉丝的特殊弹幕
    MoneyIncrease,   // 引导打赏：增加金钱的特殊弹幕
    MoodIncrease     // 安抚主播：增加心情的特殊弹幕
}

/// <summary>
/// 特殊方块类，用于实现特殊功能的方块
/// </summary>
public class SpecialTile : TileEntity
{
    public SpecialTileType type;
    public int value = 10; // 默认效果值
    
    private GameManager gameManager;
    
    private void Start()
    {
        // 获取游戏管理器引用
        gameManager = GameObject.FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("找不到GameManager实例！");
        }
    }
    
    /// <summary>
    /// 触发特殊方块效果
    /// </summary>
    public void TriggerEffect()
    {
        if (gameManager == null)
        {
            gameManager = GameObject.FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("找不到GameManager实例！");
                return;
            }
        }
        
        switch (type)
        {
            case SpecialTileType.FansIncrease:
                // 增加粉丝并显示特殊弹幕
                //gameManager.AddFans(value);
                ShowSpecialDanmaku("粉丝增加了！", Color.magenta);
                break;
                
            case SpecialTileType.MoneyIncrease:
                // 增加金钱并显示特殊弹幕
                //gameManager.AddMoney(value);
                ShowSpecialDanmaku("收到打赏！", Color.yellow);
                break;
                
            case SpecialTileType.MoodIncrease:
                // 增加心情并显示普通弹幕
                //gameManager.ChangeMood(value);
                ShowNormalDanmaku("加油！", Color.white);
                break;
                
            default:
                Debug.LogWarning("未知的特殊方块类型：" + type);
                break;
        }
    }
    
    /// <summary>
    /// 显示特殊弹幕
    /// </summary>
    private void ShowSpecialDanmaku(string text, Color color)
    {
        //if (gameManager != null && gameManager.danmakuManager != null)
        //{
        //    gameManager.danmakuManager.ShowSpecialDanmaku(text, color);
        //}
        //else
        //{
        //    Debug.Log("特殊弹幕: " + text);
        //}
    }
    
    /// <summary>
    /// 显示普通弹幕（伪装的特殊弹幕）
    /// </summary>
    private void ShowNormalDanmaku(string text, Color color)
    {
        //if (gameManager != null && gameManager.danmakuManager != null)
        //{
        //    gameManager.danmakuManager.ShowNormalDanmaku(text, color);
        //}
        //else
        //{
        //    Debug.Log("普通弹幕: " + text);
        //}
    }
    
    /// <summary>
    /// 销毁时的处理
    /// </summary>
    private void OnDestroy()
    {
        // 触发特殊效果
        TriggerEffect();
    }
} 