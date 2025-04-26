using UnityEngine;

/// <summary>
/// 特殊方块类型
/// </summary>
public enum SpecialTileType
{
    FansIncrease,    // 吸引粉丝：增加粉丝的特殊弹幕
    MoneyIncrease,   // 引导打赏：增加金钱的特殊弹幕
    MoodIncrease,    // 安抚主播：增加心情的特殊弹幕
    MoodDecrease     // 让主播心情变差的特殊弹幕
}

/// <summary>
/// 特殊方块类，用于实现特殊功能的方块
/// </summary>
public class SpecialTile : TileEntity
{
    public SpecialTileType type;
    
    /// <summary>
    /// 触发特殊方块效果
    /// </summary>
    public void TriggerEffect()
    {       
        switch (type)
        {
            case SpecialTileType.FansIncrease:
                // 增加粉丝并显示特殊弹幕
                DanmakuManager.Instance.GenerateRandomDanmaku(1);
                break;
                
            case SpecialTileType.MoneyIncrease:
                // 增加金钱并显示特殊弹幕
                DanmakuManager.Instance.GenerateRandomDanmaku(2);
                break;               
            case SpecialTileType.MoodIncrease:
                // 增加心情并显示普通弹幕
                DanmakuManager.Instance.GenerateRandomDanmaku(2);
                break;               
            default:
                Debug.LogWarning("未知的特殊方块类型：" + type);
                break;
        }
        Destroy(gameObject); // 销毁方块
    }
       
    /// <summary>
    /// 销毁时的处理
    /// </summary>
    private void OnDestroy()
    {
        // 触发特殊效果
        //TriggerEffect();
    }
} 