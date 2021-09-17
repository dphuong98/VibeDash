using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public abstract class Builder<T> : MonoBehaviour where T : ScriptableObject, IInit, ICopiable<T>
{
    private string defaultName = typeof(T).ToString();
    
    //Path
    private static string defaultFolder;
    public static string DefaultFolder => defaultFolder;
    
    public T LoadedItem { get; private set; }
    public T EditingItem { get; private set; }
    
    public void Init(string defaultFolder)
    {
        Builder<T>.defaultFolder = defaultFolder;
    }
    
    public void NewItem()
    {
        LoadedItem = default;
        EditingItem = ScriptableObject.CreateInstance<T>();
        EditingItem.Init();
        AssetDatabase.CreateAsset(EditingItem, Path.Combine(defaultFolder, "_tmp_.asset"));
        AssetDatabase.SaveAssets();
        OnReload();
    }
    
    public bool Open()
    {
        var path = EditorUtility.OpenFilePanel("Open", StageBuilder.StageFolder, "asset");
        if (string.IsNullOrEmpty(path)) return false;
        
        try
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(UnityEditor.FileUtil.GetProjectRelativePath(path));
            if (asset == null)
            {
                Debug.LogErrorFormat("Cannot load {0} asset at {1}", defaultName, path);
                return false;
            }
            LoadedItem = asset;
            EditingItem.CopyFrom(asset);
            OnReload();

            gameObject.name = Path.GetFileNameWithoutExtension(path);
            Debug.LogFormat("Opened {0} from {1}", defaultName, path);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Exception when open asset {0} {1} {2}", path, ex.Message, ex.StackTrace);
        }

        return true;
    }
    
    public bool Save()
    {
        if (LoadedItem == null) return SaveAs();

        LoadedItem.CopyFrom(EditingItem);

        EditorUtility.SetDirty(LoadedItem);
        EditorUtility.SetDirty(EditingItem);
        AssetDatabase.SaveAssets();
        
        Debug.LogFormat("Saved {0} to {1}", defaultName, LoadedItem);
        return true;
    }
    
    public bool SaveAs()
    {
        var rx = new Regex(@"(\d+)");
        var d = new DirectoryInfo(StageBuilder.StageFolder);
        var number = 0;
        if (d.GetFiles(defaultName+"?.asset") is var fileInfos && fileInfos.Count() != 0)
        {
            number = fileInfos.Select(s => rx.Match(s.Name)).Where(s => s.Success).Max(s =>
            {
                int.TryParse(s.Value, out var num);
                return num;
            });
        }
        
        var path = EditorUtility.SaveFilePanel("Save As", StageBuilder.StageFolder, defaultName+(number+1), "asset");

        if (string.IsNullOrEmpty(path)) return false;
        
        try
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.Init();
            asset.CopyFrom(EditingItem);
            AssetDatabase.CreateAsset(asset, UnityEditor.FileUtil.GetProjectRelativePath(path));
            AssetDatabase.SaveAssets();
            LoadedItem = asset;

            gameObject.name = Path.GetFileNameWithoutExtension(path);
            Debug.LogFormat("Saved {0} to {1}", defaultName, path);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("{0} {1}", ex.Message, ex.StackTrace);
            return false;
        }
    }
    
    public void Reload()
    {
        if (LoadedItem != null)
        {
            EditingItem.CopyFrom(LoadedItem);
            EditorUtility.SetDirty(EditingItem);
            OnReload();
            Debug.LogFormat("Reloaded {0}", defaultName);
        }
    }

    protected virtual void OnReload() {}
}
