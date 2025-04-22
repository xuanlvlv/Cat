using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Assertions;

public class FileUtils : MonoBehaviour
{
    /// <summary>
    /// 加载指定的json文件。
    /// </summary>
    /// <param name="serializer">The FullSerializer serializer to use.</param>
    /// <param name="path">The json file path.</param>
    /// <typeparam name="T">The type of the data to load.</typeparam>
    /// <returns>The loaded json data.</returns>
    public static T LoadJsonFile<T>(fsSerializer serializer, string path) where T : class
    {
        var textAsset = Resources.Load<TextAsset>(path);
        Assert.IsNotNull((textAsset));
        var data = fsJsonParser.Parse(textAsset.text);
        object deserialized = null;
        serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
        return deserialized as T;
    }

    /// <summary>
    /// 如果指定的路径存在，则返回true，否则返回false。
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>True if the specified path exists; false otherwise.</returns>
    public static bool FileExists(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        return textAsset != null;
    }
}
