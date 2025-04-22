using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// 编辑器选项卡的基类
/// </summary>
public class EditorTab : MonoBehaviour
{
    protected GameEditor parentEditor;

    public EditorTab(GameEditor editor)
    {
        parentEditor = editor;
    }

    /// <summary>
    /// 当用户选择此选项卡时调用
    /// </summary>
    public virtual void OnTabSelected()
    {
    }

    /// <summary>
    /// 绘制此选项卡时调用。
    /// </summary>
    public virtual void Draw()
    {
    }


    /// <summary>
    /// 创建和初始化可重新排序列表的实用方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="headerText"></param>
    /// <param name="elements"></param>
    /// <param name="currentElement"></param>
    /// <param name="drawElement"></param>
    /// <param name="selectElement"></param>
    /// <param name="createElement"></param>
    /// <param name="removeElement"></param>
    /// <returns></returns>
    public static ReorderableList SetupReorderableList<T>(
            string headerText,
            List<T> elements,
            ref T currentElement,
            Action<Rect, T> drawElement,
            Action<T> selectElement,
            Action createElement,
            Action<T> removeElement)
    {
        var list = new ReorderableList(elements, typeof(T), true, true, true, true)
        {
            drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, headerText); },
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = elements[index];
                drawElement(rect, element);
            }
        };

        list.onSelectCallback = l =>
        {
            var selectedElement = elements[list.index];
            selectElement(selectedElement);
        };

        if (createElement != null)
        {
            list.onAddDropdownCallback = (buttonRect, l) =>
            {
                createElement();
            };
        }

        list.onRemoveCallback = l =>
        {
            if (!EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this item?", "Yes", "No")
            )
            {
                return;
            }
            var element = elements[l.index];
            removeElement(element);
            ReorderableList.defaultBehaviours.DoRemoveButton(l);
        };

        return list;
    }


    /// <summary>
    /// 加载json文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    protected T LoadJsonFile<T>(string path) where T : class
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var file = new StreamReader(path);
        var fileContents = file.ReadToEnd();
        var data = fsJsonParser.Parse(fileContents);
        object deserialized = null;
        var serializer = new fsSerializer();
        serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
        file.Close();
        return deserialized as T;
    }


    /// <summary>
    /// 保存为json文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="data"></param>
    protected void SaveJsonFile<T>(string path, T data) where T : class
    {
        fsData serializedData;
        var serializer = new fsSerializer();
        serializer.TrySerialize(data, out serializedData).AssertSuccessWithoutWarnings();
        var file = new StreamWriter(path);
        var json = fsJsonPrinter.PrettyJson(serializedData);
        file.WriteLine(json);
        file.Close();
    }
}
