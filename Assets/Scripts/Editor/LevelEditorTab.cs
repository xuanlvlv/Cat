using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// 关卡编辑器标签页类
/// </summary>
public class LevelEditorTab : EditorTab
{
    private int prevWidth = -1;
    private int prevHeight = -1;

    /// <summary>
    /// 编辑器可绘制画块类型
    /// </summary>
    private enum BrushType
    {
        Block,
        Booster,
        Player
    }

    private BrushType currentBrushType;
    private BlockType currentBlockType;
    private SpecialTileType currentBoosterType;
    private PlayerType currentPlayerType;

    private enum BrushMode
    {
        Tile,
        Row,
        Column,
        Fill
    }

    private BrushMode currentBrushMode = BrushMode.Tile;

    private readonly Dictionary<string, Texture> tileTextures = new Dictionary<string, Texture>();

    private Level currentLevel;

    private ReorderableList goalList;

    private ReorderableList availableColorBlocksList;
    private ColorBlockType currentColorBlock;

    private Vector2 scrollPos;
    private Vector2 colorBlocksScrollPos;
    private Vector2 mapScrollPos;

    // 添加缩放比例控制变量
    private float tileScale = 1.0f;
    // 添加设置面板的滚动位置
    private Vector2 settingsScrollPos;

    /// <summary>
    /// 初始化图片
    /// </summary>
    public LevelEditorTab(GameEditor editor) : base(editor)
    {
        var editorImagesPath = new DirectoryInfo(Application.dataPath + "/Resources/Game");
        if (editorImagesPath.Exists)
        {
            var fileInfo = editorImagesPath.GetFiles("*.png", SearchOption.TopDirectoryOnly);
            foreach (var file in fileInfo)
            {
                var filename = Path.GetFileNameWithoutExtension(file.Name);
                tileTextures[filename] = Resources.Load("Game/" + filename) as Texture;
            }
        }
        else
        {
            Debug.LogWarning("图像资源目录不存在: " + editorImagesPath.FullName);
        }
    }

    /// <summary>
    /// 绘制此选项卡时调用
    /// </summary>
    public override void Draw()
    {
        // 使用水平分割的整体布局
        EditorGUILayout.BeginHorizontal();
        
        // 左侧：地图编辑区域（占据大部分空间）
        EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.7f));
        
        // 左侧内部的滚动视图
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        var oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 90;

        GUILayout.Space(15);

        DrawMenu();

        if (currentLevel != null)
        {
            var level = currentLevel;
            prevWidth = level.width;

            // 地图编辑区域
            EditorGUILayout.LabelField("地图编辑器", EditorStyles.boldLabel);
            DrawMapEditor();
        }

        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
        // 右侧：设置面板（固定宽度）
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        
        // 右侧内部的滚动视图
        settingsScrollPos = EditorGUILayout.BeginScrollView(settingsScrollPos);
        
        GUILayout.Space(15);
        
        if (currentLevel != null)
        {
            // 画笔设置区域
            EditorGUILayout.LabelField("画笔设置", EditorStyles.boldLabel);
            DrawBrushSettings();
            
            GUILayout.Space(15);
            
            // 随机色块设置
            DrawAvailableColorBlockSettings();
        }
        else
        {
            EditorGUILayout.HelpBox("请先新建或打开一个关卡", MessageType.Info);
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制菜单
    /// </summary>
    private void DrawMenu()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("新建", GUILayout.Width(100), GUILayout.Height(50)))
        {
            currentLevel = new Level();
            InitializeNewLevel();
            CreateAvailableColorBlocksList();
        }

        if (GUILayout.Button("打开", GUILayout.Width(100), GUILayout.Height(50)))
        {
            var path = EditorUtility.OpenFilePanel("打开关卡", Application.dataPath + "/Resources/GameLevel", "json");
            if (!string.IsNullOrEmpty(path))
            {
                currentLevel = LoadJsonFile<Level>(path);
                CreateAvailableColorBlocksList();
            }
        }

        if (GUILayout.Button("保存", GUILayout.Width(100), GUILayout.Height(50)))
        {
            SaveLevel(Application.dataPath + "/Resources");
        }

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制画笔设置区域
    /// </summary>
    private void DrawBrushSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 地图尺寸设置
        EditorGUILayout.LabelField("地图尺寸", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("宽度", "地图宽度"), GUILayout.Width(EditorGUIUtility.labelWidth));
        int newWidth = EditorGUILayout.IntField(currentLevel.width, GUILayout.Width(50));
        // 限制宽度在1-20之间
        currentLevel.width = Mathf.Clamp(newWidth, 1, 20);
        GUILayout.EndHorizontal();

        prevHeight = currentLevel.height;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("长度", "地图长度"), GUILayout.Width(EditorGUIUtility.labelWidth));
        int newHeight = EditorGUILayout.IntField(currentLevel.height, GUILayout.Width(50));
        // 限制高度在1-20之间
        currentLevel.height = Mathf.Clamp(newHeight, 1, 20);
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // 画笔类型设置
        EditorGUILayout.LabelField("画笔设置", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("画块类型", "当前画块类型"), GUILayout.Width(EditorGUIUtility.labelWidth));
        currentBrushType = (BrushType)EditorGUILayout.EnumPopup(currentBrushType, GUILayout.Width(120));
        GUILayout.EndHorizontal();

        if (currentBrushType == BrushType.Block)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("普通画块", "常规的地图画块"), GUILayout.Width(EditorGUIUtility.labelWidth));
            currentBlockType = (BlockType)EditorGUILayout.EnumPopup(currentBlockType, GUILayout.Width(120));
            GUILayout.EndHorizontal();
        }
        else if (currentBrushType == BrushType.Booster)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("道具画块", "道具画块特殊画块"), GUILayout.Width(EditorGUIUtility.labelWidth));
            currentBoosterType = (SpecialTileType)EditorGUILayout.EnumPopup(currentBoosterType, GUILayout.Width(120));
            GUILayout.EndHorizontal();
        }
        else if(currentBrushType == BrushType.Player)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("玩家画块", "玩家初始生成位置"), GUILayout.Width(EditorGUIUtility.labelWidth));
            currentPlayerType = (PlayerType)EditorGUILayout.EnumPopup(currentPlayerType, GUILayout.Width(120));
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("画笔模式", "画笔绘制模式"), GUILayout.Width(EditorGUIUtility.labelWidth));
        currentBrushMode = (BrushMode)EditorGUILayout.EnumPopup(currentBrushMode, GUILayout.Width(120));
        GUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制可用的色块设置
    /// </summary>
    private void DrawAvailableColorBlockSettings()
    {
        EditorGUILayout.LabelField("随机色块列表", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.HelpBox("此列表定义创建随机色块时会出现的色块", MessageType.Info);

        // 使用滚动视图确保列表在空间不足时可以滚动查看
        colorBlocksScrollPos = EditorGUILayout.BeginScrollView(colorBlocksScrollPos, GUILayout.Height(200));
        
        if (availableColorBlocksList != null)
        {
            availableColorBlocksList.DoLayoutList();
        }
        
        EditorGUILayout.EndScrollView();

        EditorGUIUtility.labelWidth = 120;
  
        EditorGUIUtility.labelWidth = 90;

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制地图编辑区域
    /// </summary>
    private void DrawMapEditor()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (prevWidth != currentLevel.width || prevHeight != currentLevel.height)
        {
            // 备份现有方块
            List<LevelTile> oldTiles = new List<LevelTile>(currentLevel.tiles);
            
            // 创建新地图
            currentLevel.tiles = new List<LevelTile>(currentLevel.width * currentLevel.height);
            
            // 填充新地图，尽可能保留原有内容
            for (var i = 0; i < currentLevel.height; i++)
            {
                for (var j = 0; j < currentLevel.width; j++)
                {
                    int oldIndex = (prevWidth > 0 && prevHeight > 0) ? (j + (i * prevWidth)) : -1;
                    if (oldIndex >= 0 && oldIndex < oldTiles.Count && j < prevWidth && i < prevHeight)
                    {
                        // 保留原有方块
                        currentLevel.tiles.Add(oldTiles[oldIndex]);
                    }
                    else
                    {
                        // 添加新方块
                        currentLevel.tiles.Add(new BlockTile() { type = BlockType.RandomBlock });
                    }
                }
            }
            
            // 更新先前尺寸
            prevWidth = currentLevel.width;
            prevHeight = currentLevel.height;
        }
        
        // 检查确保tiles列表的大小与宽度*高度一致
        if (currentLevel.tiles.Count != currentLevel.width * currentLevel.height)
        {
            Debug.LogWarning($"检测到tiles列表大小({currentLevel.tiles.Count})与实际需要大小({currentLevel.width * currentLevel.height})不一致，正在修复...");
            
            // 如果列表过小，添加块
            while (currentLevel.tiles.Count < currentLevel.width * currentLevel.height)
            {
                currentLevel.tiles.Add(new BlockTile() { type = BlockType.RandomBlock });
            }
            
            // 如果列表过大，移除多余的块
            if (currentLevel.tiles.Count > currentLevel.width * currentLevel.height)
            {
                currentLevel.tiles.RemoveRange(currentLevel.width * currentLevel.height, 
                    currentLevel.tiles.Count - currentLevel.width * currentLevel.height);
            }
            
            //Debug.Log($"tiles列表大小已修复：{currentLevel.tiles.Count}");
        }

        // 计算单个方块的尺寸，当地图较大时自动缩小
        float availableWidth = EditorGUIUtility.currentViewWidth * 0.7f - 40; // 左侧区域宽度减去边距
        float tileSize = Mathf.Min(60f, availableWidth / Mathf.Max(currentLevel.width, 1));
        
        // 添加地图尺寸信息
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"地图尺寸: {currentLevel.width}x{currentLevel.height}", EditorStyles.boldLabel);
        
        // 添加缩放控制
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("缩放:", GUILayout.Width(40));
        tileScale = EditorGUILayout.Slider(tileScale, 0.5f, 2.0f, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();
        
        // 应用自定义缩放
        tileSize *= tileScale;
        
        // 添加缩放比例提示
        if (tileSize < 60f)
        {
            EditorGUILayout.HelpBox($"地图较大，显示比例已缩小。可以使用上方滑块调整缩放比例。", MessageType.Info);
        }
        
        // 计算地图总宽度和高度
        float totalMapWidth = currentLevel.width * tileSize;
        float totalMapHeight = currentLevel.height * tileSize;
        
        // 添加滚动视图用于大地图的显示，限制最大高度
        mapScrollPos = EditorGUILayout.BeginScrollView(mapScrollPos, 
            GUILayout.Height(Mathf.Min(totalMapHeight + 40, 600)));
        
        // 居中显示地图
        EditorGUILayout.BeginHorizontal();
        float centerMargin = Mathf.Max(0, (availableWidth - totalMapWidth) / 2);
        if (centerMargin > 0)
        {
            GUILayout.Space(centerMargin);
        }
        
        EditorGUILayout.BeginVertical();
        
        for (var i = 0; i < currentLevel.height; i++)
        {
            GUILayout.BeginHorizontal();
            
            for (var j = 0; j < currentLevel.width; j++)
            {
                var tileIndex = (currentLevel.width * i) + j;
                CreateButton(tileIndex, tileSize);
            }
            
            GUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 初始化新创建的关卡
    /// </summary>
    private void InitializeNewLevel()
    {
        // 设置默认值
        currentLevel.id = 1;
        currentLevel.width = 9;
        currentLevel.height = 9;
        currentLevel.tiles = new List<LevelTile>(currentLevel.width * currentLevel.height);
        
        // 初始化地图块
        for (var i = 0; i < currentLevel.height; i++)
        {
            for (var j = 0; j < currentLevel.width; j++)
            {
                currentLevel.tiles.Add(new BlockTile() { type = BlockType.RandomBlock });
            }
        }
        
        // 初始化可用色块
        foreach (var type in Enum.GetValues(typeof(ColorBlockType)))
        {
            currentLevel.availableColors.Add((ColorBlockType)type);
        }
    }

    /// <summary>
    /// 创建此关卡的随机色块列表
    /// </summary>
    private void CreateAvailableColorBlocksList()
    {
        availableColorBlocksList = SetupReorderableList("地图色块列表", currentLevel.availableColors, ref currentColorBlock, (rect, x) =>
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                x.ToString());
        },
            (x) =>
            {
                currentColorBlock = x;
            },
            () =>
            {
                var menu = new GenericMenu();
                foreach (var type in Enum.GetValues(typeof(ColorBlockType)))
                {
                    menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(type.ToString())), false,
                        CreateColorBlockTypeCallback, type);
                }
                menu.ShowAsContext();
            },
            (x) =>
            {
                currentColorBlock = ColorBlockType.Number1;
            });
        availableColorBlocksList.onRemoveCallback = l =>
        {
            if (currentLevel.availableColors.Count == 1)
            {
                EditorUtility.DisplayDialog("警告", "至少需要一个色块类型", "确定");
            }
            else
            {
                if (!EditorUtility.DisplayDialog("警告",
                    "确定要删除此项目？", "是", "否"))
                {
                    return;
                }
                currentColorBlock = ColorBlockType.Number1;
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            }
        };
    }

    /// <summary>
    /// 创建新的色块类型时的回调
    /// </summary>
    /// <param name="obj">要创建的对象类型</param>
    private void CreateColorBlockTypeCallback(object obj)
    {
        var color = (ColorBlockType)obj;
        if (currentLevel.availableColors.Contains(color))
        {
            EditorUtility.DisplayDialog("警告", "色块类型已存在列表中。", "确定");
        }
        else
        {
            currentLevel.availableColors.Add(color);
        }
    }

    /// <summary>
    /// 创建地图中的各类型画块图标
    /// </summary>
    /// <param name="tileIndex">方块索引</param>
    /// <param name="tileSize">方块尺寸</param>
    private void CreateButton(int tileIndex, float tileSize = 60f)
    {
        var tileTypeName = string.Empty;
        if (currentLevel.tiles[tileIndex] is BlockTile)
        {
            var blockTile = (BlockTile)currentLevel.tiles[tileIndex];
            tileTypeName = blockTile.type.ToString();
        }
        else if (currentLevel.tiles[tileIndex] is BoosterTile)
        {
            var boosterTile = (BoosterTile)currentLevel.tiles[tileIndex];
            tileTypeName = boosterTile.type.ToString();
        }
        else if (currentLevel.tiles[tileIndex] is PlayerTile)
        {
            var playerTile = (PlayerTile)currentLevel.tiles[tileIndex];
            tileTypeName = playerTile.type.ToString();
        }
        if (tileTextures.ContainsKey(tileTypeName))
        {
            if (GUILayout.Button(tileTextures[tileTypeName], GUILayout.Width(tileSize), GUILayout.Height(tileSize)))
            {
                DrawTile(tileIndex);
                // 强制重绘
                EditorUtility.SetDirty(parentEditor);
                parentEditor.Repaint();
            }
        }
        else
        {
            if (GUILayout.Button(tileTypeName, GUILayout.Width(tileSize), GUILayout.Height(tileSize)))
            {
                DrawTile(tileIndex);
                // 强制重绘
                EditorUtility.SetDirty(parentEditor);
                parentEditor.Repaint();
            }
        }
    }

    /// <summary>
    /// 在指定索引处绘制图块
    /// </summary>
    /// <param name="tileIndex">方块索引</param>
    private void DrawTile(int tileIndex)
    {
        var x = tileIndex % currentLevel.width;
        var y = tileIndex / currentLevel.width;
        if (currentBrushType == BrushType.Block)
        {
            switch (currentBrushMode)
            {
                case BrushMode.Tile:
                    currentLevel.tiles[tileIndex] = new BlockTile { type = currentBlockType };
                    break;

                case BrushMode.Row:
                    for (var i = 0; i < currentLevel.width; i++)
                    {
                        var idx = i + (y * currentLevel.width);
                        currentLevel.tiles[idx] = new BlockTile { type = currentBlockType };
                    }
                    break;

                case BrushMode.Column:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        var idx = x + (j * currentLevel.width);
                        currentLevel.tiles[idx] = new BlockTile { type = currentBlockType };
                    }
                    break;

                case BrushMode.Fill:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new BlockTile { type = currentBlockType };
                        }
                    }
                    break;
            }
        }
        else if (currentBrushType == BrushType.Booster)
        {
            switch (currentBrushMode)
            {
                case BrushMode.Tile:
                    currentLevel.tiles[tileIndex] = new BoosterTile { type = currentBoosterType };
                    break;

                case BrushMode.Row:
                    for (var i = 0; i < currentLevel.width; i++)
                    {
                        var idx = i + (y * currentLevel.width);
                        currentLevel.tiles[idx] = new BoosterTile { type = currentBoosterType };
                    }
                    break;

                case BrushMode.Column:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        var idx = x + (j * currentLevel.width);
                        currentLevel.tiles[idx] = new BoosterTile { type = currentBoosterType };
                    }
                    break;

                case BrushMode.Fill:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new BoosterTile { type = currentBoosterType };
                        }
                    }
                    break;
            }
        }
        else if (currentBrushType == BrushType.Player)
        {
            switch (currentBrushMode)
            {
                case BrushMode.Tile:
                    currentLevel.tiles[tileIndex] = new PlayerTile { type = currentPlayerType };
                    break;

                case BrushMode.Row:
                    for (var i = 0; i < currentLevel.width; i++)
                    {
                        var idx = i + (y * currentLevel.width);
                        currentLevel.tiles[idx] = new PlayerTile { type = currentPlayerType };
                    }
                    break;

                case BrushMode.Column:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        var idx = x + (j * currentLevel.width);
                        currentLevel.tiles[idx] = new PlayerTile { type = currentPlayerType };
                    }
                    break;

                case BrushMode.Fill:
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new PlayerTile { type = currentPlayerType };
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 保存当前关卡设置到指定位置
    /// </summary>
    /// <param name="path">要保存的路径</param>
    public void SaveLevel(string path)
    {
#if UNITY_EDITOR
        SaveJsonFile(path + "/GameLevel/" + currentLevel.id + ".json", currentLevel);
        AssetDatabase.Refresh();
#endif
    }
}
