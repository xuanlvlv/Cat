using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ���ӻ��༭��������ͼ��Ļ���
/// </summary>
public class LevelTile
{
    
}

/// <summary>
/// ���ӻ��༭�������ڵ���ͼ��Ļ���
/// </summary>
public class BlockTile : LevelTile
{
    public BlockType type;
}

/// <summary>
/// ���ӻ��༭�������ڻ���ͼ��Ļ���
/// </summary>
public class BoosterTile : LevelTile
{
    public SpecialTileType type;
}

public class PlayerTile : LevelTile
{
    public PlayerType type;
}
