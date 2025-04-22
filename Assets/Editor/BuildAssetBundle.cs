using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundle
{
    [MenuItem("Tool/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string dst = Application.streamingAssetsPath + "/AssetBundles";
        if (!Directory.Exists(dst))
        {
            Directory.CreateDirectory(dst);
        }
        BuildPipeline.BuildAssetBundles(dst, BuildAssetBundleOptions.AppendHashToAssetBundleName | BuildAssetBundleOptions.ChunkBasedCompression | UnityEditor.BuildAssetBundleOptions.DisableWriteTypeTree | BuildAssetBundleOptions.None, BuildTarget.WebGL);
    }
}
