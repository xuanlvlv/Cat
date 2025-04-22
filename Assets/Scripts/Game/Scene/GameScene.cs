using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class GameScene:MonoBehaviour
{
    public GamePools gamePools;
    
    private Level level;

    private List<GameObject> tileEntities = new List<GameObject>();

    private List<Vector2> tilePositions = new List<Vector2>();

    // 地图尺寸
    private float blockWidth;
    private float blockHeight;

    [Header("地图间隔")]
    public float horizontalSpacing;
    public float verticalSpacing;

    public Transform levelLocation;

    [SerializeField] private int levelIndex = 1;

    private void Start()
    {
        LoadLevel();
        CreateMap();
    }

    /// <summary>
    /// 加载关卡数据
    /// </summary>
    public void LoadLevel()
    {
        var serializer = new fsSerializer();
        if (levelIndex != -1)
        {
            level = FileUtils.LoadJsonFile<Level>(serializer, "GameLevel/" + levelIndex);
        }
        else
        {
            Debug.Log("关卡ID不存在，加载失败");
            level = FileUtils.LoadJsonFile<Level>(serializer, "GameLevel/" + 1);
        }
    }

    /// <summary>
    /// 创建地图
    /// </summary>
    public void CreateMap()
    {
        // 清理现有地图
        foreach (var pool in gamePools.GetComponentsInChildren<ObjectPool>())
        {
            pool.Reset();
        }
        tileEntities.Clear();
        tilePositions.Clear();

        // 创建地图块
        for (var j = 0; j < level.height; j++)
        {
            for (var i = 0; i < level.width; i++)
            {
                var tileIndex = i + (j * level.width);
                var tileToGet = gamePools.GetTileEntity(level, level.tiles[tileIndex]);
                var tile = CreateBlock(tileToGet.gameObject);
                var spriteRenderer = tile.GetComponent<SpriteRenderer>();
                blockWidth = spriteRenderer.bounds.size.x;
                blockHeight = spriteRenderer.bounds.size.y;
                tile.transform.position = new Vector2(i * (blockWidth + horizontalSpacing),
                    -j * (blockHeight + verticalSpacing));
                tileEntities.Add(tile);
                spriteRenderer.sortingOrder = level.height - j;
            }
        }

        // 居中地图
        var totalWidth = (level.width - 1) * (blockWidth + horizontalSpacing);
        var totalHeight = (level.height - 1) * (blockHeight + verticalSpacing);
        foreach (var block in tileEntities)
        {
            var newPos = block.transform.position;
            newPos.x -= totalWidth / 2;
            newPos.y += totalHeight / 2;
            newPos.y += levelLocation.position.y;
            block.transform.position = newPos;
            tilePositions.Add(newPos);
        }
    }

    /// <summary>
    /// 创建一个新的方块实例
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private GameObject CreateBlock(GameObject go)
    {
        go.GetComponent<TileEntity>().Spawn();
        return go;
    }
}
