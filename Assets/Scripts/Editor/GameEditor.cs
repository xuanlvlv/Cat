using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static PlasticGui.Configuration.OAuth.OAuthSignIn;

public class GameEditor : EditorWindow
{
    //public GameConfiguration gameConfig; //游戏设置配置表 ToDo

    private readonly List<EditorTab> tabs = new List<EditorTab>();

    private int selectedTabIndex = -1;
    private int prevSelectedTabIndex = -1;

    /// <summary>
    /// 编辑器窗口的静态初始化
    /// </summary>
    [MenuItem("Tool/GameEditor", false, 0)]
    private static void Init()
    {
        var window = GetWindow(typeof(GameEditor));
        window.titleContent = new GUIContent("Game Editor");
    }

    private void OnEnable()
    {
        tabs.Add(new LevelEditorTab(this));
        selectedTabIndex = 0;
    }

    private void OnGUI()
    {
        selectedTabIndex = GUILayout.Toolbar(selectedTabIndex,
            new[] { "Level editor", "Other editor", });
        if (selectedTabIndex >= 0 && selectedTabIndex < tabs.Count)
        {
            var selectedEditor = tabs[selectedTabIndex];
            if (selectedTabIndex != prevSelectedTabIndex)
            {
                selectedEditor.OnTabSelected();
                GUI.FocusControl(null);
            }
            selectedEditor.Draw();
            prevSelectedTabIndex = selectedTabIndex;
        }
    }
}
