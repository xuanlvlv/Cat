using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池管理器，管理游戏中所有可复用对象
/// </summary>
public class GamePools : MonoBehaviour
{
    [Header("玩家预制体")]
    public GameObject playerObj;

    [Header("数字方块对象池")]
    public ObjectPool[] numberPools = new ObjectPool[9]; // 数字1-9的对象池

    [Header("阻挡方块对象池")]
    public ObjectPool wallPool;

    [Header("特殊方块对象池")]
    public ObjectPool FansTilePool;
    public ObjectPool MoneyTilePool;
    public ObjectPool MoodAddTile;
    public ObjectPool MoodReduceTilePool;


    // 用字典快速查找对象池
    private Dictionary<BlockType, ObjectPool> blockPoolsMap = new Dictionary<BlockType, ObjectPool>();
    private Dictionary<PlayerType, int> playerTypeValueMap = new Dictionary<PlayerType, int>();

    private void Awake()
    {
        InitBlockPoolsMap();
        InitPlayerTypeValueMap();
    }

    /// <summary>
    /// 初始化方块对象池映射
    /// </summary>
    private void InitBlockPoolsMap()
    {
        // 映射数字方块类型到对应的对象池
        for (int i = 0; i < numberPools.Length; i++)
        {
            if (numberPools[i] != null)
            {
                // 注意：枚举值从0开始 (Number1 = 0, Number2 = 1...)
                BlockType blockType = (BlockType)(i);
                blockPoolsMap[blockType] = numberPools[i];
            }
            else
            {
                Debug.LogWarning($"数字对象池 {i} 未设置");
            }
        }

        // 添加特殊方块类型映射
        if (wallPool != null) 
        {
            blockPoolsMap[BlockType.Wall] = wallPool;
        }
        else
        {
            Debug.LogWarning("墙壁对象池未设置");
        }      
    }

    /// <summary>
    /// 初始化玩家类型与数值的映射
    /// </summary>
    private void InitPlayerTypeValueMap()
    {
        // 映射玩家类型到对应的数值
        playerTypeValueMap[PlayerType.PlayerNum0] = 0;
        playerTypeValueMap[PlayerType.PlayerNum1] = 1;
        playerTypeValueMap[PlayerType.PlayerNum2] = 2;
        playerTypeValueMap[PlayerType.PlayerNum3] = 3;
        playerTypeValueMap[PlayerType.PlayerNum4] = 4;
        playerTypeValueMap[PlayerType.PlayerNum5] = 5;
        playerTypeValueMap[PlayerType.PlayerNum6] = 6;
        playerTypeValueMap[PlayerType.PlayerNum7] = 7;
        playerTypeValueMap[PlayerType.PlayerNum8] = 8;
        playerTypeValueMap[PlayerType.PlayerNum9] = 9;
    }

    /// <summary>
    /// 根据指定关卡图块获取对应的图块实体
    /// </summary>
    /// <param name="level">关卡数据</param>
    /// <param name="tile">图块数据</param>
    /// <returns>图块实体</returns>
    public TileEntity GetTileEntity(Level level, LevelTile tile)
    {
        if (tile == null)
        {
            Debug.LogError("传入的图块数据为空");
            return null;
        }
        
        if (tile is BlockTile blockTile)
        {
            return GetBlockTileEntity(level, blockTile);
        }
        else if (tile is BoosterTile specialTile)
        {
            // 处理特殊方块
            if (specialTile.type == SpecialTileType.FansIncrease)
            {
                return FansTilePool.GetObject().GetComponent<TileEntity>();
            }
            else if (specialTile.type == SpecialTileType.MoneyIncrease)
            {
                return MoneyTilePool.GetObject().GetComponent<TileEntity>();
            }
            else if (specialTile.type == SpecialTileType.MoodIncrease)
            {
                return MoodAddTile.GetObject().GetComponent<TileEntity>();
            }
            else if (specialTile.type == SpecialTileType.MoodDecrease)
            {
                return MoodReduceTilePool.GetObject().GetComponent<TileEntity>();
            }
        }
        else if (tile is PlayerTile playerTile)
        {
            return GetPlayerTileEntity(playerTile);
        }
        
        Debug.LogWarning($"未知的图块类型: {tile.GetType().Name}");
        return null;
    }

    /// <summary>
    /// 获取方块图块实体
    /// </summary>
    private TileEntity GetBlockTileEntity(Level level, BlockTile blockTile)
    {
        BlockType type = blockTile.type;
        
        // 处理随机方块类型
        if (type == BlockType.RandomBlock)
        {
            if (level.availableColors.Count > 0)
            {
                var randomIdx = UnityEngine.Random.Range(0, level.availableColors.Count);
                ColorBlockType colorType = level.availableColors[randomIdx];
                // 将ColorBlockType枚举值转换为BlockType
                // ColorBlockType.Number1 = 0 对应 BlockType.Number1 = 0
                type = (BlockType)colorType;
            }
            else
            {
                // 如果没有可用颜色，默认使用Number1
                type = BlockType.Number1;
                Debug.LogWarning("关卡没有定义可用颜色，默认使用Number1");
            }
        }
        
        // 从字典中获取对应的对象池
        if (blockPoolsMap.TryGetValue(type, out ObjectPool pool) && pool != null)
        {
            return pool.GetObject().GetComponent<TileEntity>();
        }       
        Debug.LogWarning($"未找到类型为 {type} 的方块对象池");
        return null;
    }

    /// <summary>
    /// 获取玩家图块实体
    /// </summary>
    private TileEntity GetPlayerTileEntity(PlayerTile playerTile)
    {
        if (playerObj == null)
        {
            Debug.LogError("玩家预制体未设置");
            return null;
        }

        var obj = Instantiate(playerObj);
        var player = obj.GetComponent<Player>();
        
        if (player != null && playerTypeValueMap.TryGetValue(playerTile.type, out int playerNumber))
        {
            player.playerNumber = playerNumber;
        }
        else
        {
            Debug.LogWarning($"未找到玩家类型 {playerTile.type} 的映射值");
        }
        
        return obj.GetComponent<TileEntity>();
    }

    /// <summary>
    /// 根据方块类型获取对象池
    /// </summary>
    public ObjectPool GetPoolByBlockType(BlockType type)
    {
        if (blockPoolsMap.TryGetValue(type, out ObjectPool pool))
        {
            return pool;
        }
        return null;
    }
}
