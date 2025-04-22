using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏数据类，用于存储和管理游戏中的各种数据
/// </summary>
[Serializable]
public class GameData
{
    #region 游戏进度数据
    // 基础游戏状态
    public int dayCount = 1;            // 当前天数
    public int fansCount = 0;           // 粉丝数量
    public int money = 1000;            // 金钱
    public int mood = 100;              // 心情值
    public int maxMood = 100;           // 最大心情值
    
    // 日常统计数据
    public int dailyFansGained = 0;     // 今日获得粉丝数
    public int dailyMoneyEarned = 0;    // 今日获得金钱
    public int dailyMoodChange = 0;     // 今日心情变化
    
    // 游戏设置
    public float dayDuration = 180f;    // 每天持续时间（秒）
    public float moodDecayRate = 1f;    // 心情衰减速率（每分钟）
    #endregion
    
    #region 成就和事件
    // 解锁的成就
    public List<int> unlockedAchievements = new List<int>();
    
    // 已经发生的事件
    public List<int> triggeredEvents = new List<int>();
    #endregion
    
    #region 升级数据
    // 技能和装备升级
    public int contentQualityLevel = 1;    // 内容质量等级
    public int editingSpeedLevel = 1;      // 编辑速度等级
    public int fanEngagementLevel = 1;     // 粉丝互动等级
    public int marketingLevel = 1;         // 营销等级
    
    // 工作室升级
    public int studioSizeLevel = 1;        // 工作室大小等级
    public int equipmentLevel = 1;         // 设备等级
    public int staffLevel = 1;             // 员工等级
    
    // 解锁的内容类型
    public List<int> unlockedContentTypes = new List<int>();
    #endregion
    
    #region 数据存取方法
    /// <summary>
    /// 重置日常统计数据
    /// </summary>
    public void ResetDailyStats()
    {
        dailyFansGained = 0;
        dailyMoneyEarned = 0;
        dailyMoodChange = 0;
    }
    
    /// <summary>
    /// 添加粉丝数量
    /// </summary>
    /// <param name="amount">添加的粉丝数量</param>
    public void AddFans(int amount)
    {
        dailyFansGained += amount;
        fansCount += amount;
    }
    
    /// <summary>
    /// 添加金钱
    /// </summary>
    /// <param name="amount">添加的金钱</param>
    public void AddMoney(int amount)
    {
        dailyMoneyEarned += amount;
        money += amount;
    }
    
    /// <summary>
    /// 修改心情值
    /// </summary>
    /// <param name="amount">心情变化量</param>
    public void ChangeMood(int amount)
    {
        dailyMoodChange += amount;
        mood = Mathf.Clamp(mood + amount, 0, maxMood);
    }
    
    /// <summary>
    /// 检查是否能够支付指定金额
    /// </summary>
    /// <param name="amount">需要支付的金额</param>
    /// <returns>是否有足够的金钱</returns>
    public bool CanAfford(int amount)
    {
        return money >= amount;
    }
    
    /// <summary>
    /// 尝试支付指定金额
    /// </summary>
    /// <param name="amount">需要支付的金额</param>
    /// <returns>支付是否成功</returns>
    public bool TrySpendMoney(int amount)
    {
        if (CanAfford(amount))
        {
            money -= amount;
            dailyMoneyEarned -= amount;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 增加天数
    /// </summary>
    public void AdvanceDay()
    {
        dayCount++;
    }
    
    /// <summary>
    /// 检查是否已解锁成就
    /// </summary>
    /// <param name="achievementId">成就ID</param>
    /// <returns>是否已解锁</returns>
    public bool IsAchievementUnlocked(int achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }
    
    /// <summary>
    /// 解锁成就
    /// </summary>
    /// <param name="achievementId">成就ID</param>
    public void UnlockAchievement(int achievementId)
    {
        if (!IsAchievementUnlocked(achievementId))
        {
            unlockedAchievements.Add(achievementId);
        }
    }
    
    /// <summary>
    /// 检查事件是否已触发
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <returns>是否已触发</returns>
    public bool IsEventTriggered(int eventId)
    {
        return triggeredEvents.Contains(eventId);
    }
    
    /// <summary>
    /// 记录事件已触发
    /// </summary>
    /// <param name="eventId">事件ID</param>
    public void TriggerEvent(int eventId)
    {
        if (!IsEventTriggered(eventId))
        {
            triggeredEvents.Add(eventId);
        }
    }
    #endregion
    
    #region 升级方法
    /// <summary>
    /// 升级内容质量
    /// </summary>
    /// <returns>是否升级成功</returns>
    public bool UpgradeContentQuality()
    {
        int cost = GetUpgradeCost(contentQualityLevel);
        if (TrySpendMoney(cost))
        {
            contentQualityLevel++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 升级编辑速度
    /// </summary>
    /// <returns>是否升级成功</returns>
    public bool UpgradeEditingSpeed()
    {
        int cost = GetUpgradeCost(editingSpeedLevel);
        if (TrySpendMoney(cost))
        {
            editingSpeedLevel++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 升级粉丝互动
    /// </summary>
    /// <returns>是否升级成功</returns>
    public bool UpgradeFanEngagement()
    {
        int cost = GetUpgradeCost(fanEngagementLevel);
        if (TrySpendMoney(cost))
        {
            fanEngagementLevel++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 升级营销
    /// </summary>
    /// <returns>是否升级成功</returns>
    public bool UpgradeMarketing()
    {
        int cost = GetUpgradeCost(marketingLevel);
        if (TrySpendMoney(cost))
        {
            marketingLevel++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 升级工作室大小
    /// </summary>
    /// <returns>是否升级成功</returns>
    public bool UpgradeStudioSize()
    {
        int cost = GetUpgradeCost(studioSizeLevel) * 2;
        if (TrySpendMoney(cost))
        {
            studioSizeLevel++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 升级设备
    /// </summary>
    /// <returns>是否升级成功</returns>
    public bool UpgradeEquipment()
    {
        int cost = GetUpgradeCost(equipmentLevel) * 2;
        if (TrySpendMoney(cost))
        {
            equipmentLevel++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 升级员工
    /// </summary>
    /// <returns>是否升级成功</returns>
    public bool UpgradeStaff()
    {
        int cost = GetUpgradeCost(staffLevel) * 3;
        if (TrySpendMoney(cost))
        {
            staffLevel++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 解锁内容类型
    /// </summary>
    /// <param name="contentTypeId">内容类型ID</param>
    /// <returns>是否解锁成功</returns>
    public bool UnlockContentType(int contentTypeId)
    {
        int cost = 2000 + contentTypeId * 1000;
        if (!unlockedContentTypes.Contains(contentTypeId) && TrySpendMoney(cost))
        {
            unlockedContentTypes.Add(contentTypeId);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取升级成本
    /// </summary>
    /// <param name="currentLevel">当前等级</param>
    /// <returns>升级成本</returns>
    private int GetUpgradeCost(int currentLevel)
    {
        // 基础成本500，每级递增500
        return 500 * currentLevel;
    }
    #endregion
    
    #region 存档与读档
    /// <summary>
    /// 将游戏数据转换为JSON字符串
    /// </summary>
    /// <returns>JSON字符串</returns>
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
    
    /// <summary>
    /// 从JSON字符串加载游戏数据
    /// </summary>
    /// <param name="jsonData">JSON字符串</param>
    /// <returns>加载的游戏数据</returns>
    public static GameData FromJson(string jsonData)
    {
        try
        {
            return JsonUtility.FromJson<GameData>(jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError("加载游戏数据时出错: " + e.Message);
            return new GameData(); // 返回默认数据
        }
    }
    #endregion
} 