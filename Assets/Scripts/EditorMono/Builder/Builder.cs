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
    public static string DefaultName { get; } = typeof(T).ToString();
    
    //Path
    //TODO This static variable is shared between derived
    public static string DefaultFolder { get; private set; }

    public T LoadedItem { get; private set; }
    public T EditingItem { get; private set; }
    
    public void Init(string defaultFolder)
    {
        DefaultFolder = defaultFolder;
        if (EditingItem == null) NewItem();
    }
    
    public virtual void NewItem()
    {
        LoadedItem = default;
        EditingItem = ScriptableObject.CreateInstance<T>();
        EditingItem.Init();
        AssetDatabase.CreateAsset(EditingItem, Path.Combine(DefaultFolder, "_tmp_.asset"));
        AssetDatabase.SaveAssets();
        OnReload();
    }
    
    public virtual bool Open(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        
        try
        {
            //TODO decoupling
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Debug.LogErrorFormat("Cannot load {0} asset at {1}", DefaultName, path);
                return false;
            }
            LoadedItem = asset;
            EditingItem.CopyFrom(asset);
            OnReload();

            gameObject.name = Path.GetFileNameWithoutExtension(path);
            Debug.LogFormat("Opened {0} from {1}", DefaultName, path);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Exception when open asset {0} {1} {2}", path, ex.Message, ex.StackTrace);
        }

        return true;
    }
    
    public virtual bool Save()
    {
        if (LoadedItem == null) return false;

        LoadedItem.CopyFrom(EditingItem);

        EditorUtility.SetDirty(LoadedItem);
        EditorUtility.SetDirty(EditingItem);
        AssetDatabase.SaveAssets();
        
        Debug.LogFormat("Saved {0} to {1}", DefaultName, LoadedItem);
        return true;
    }
    
    public virtual bool Save(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        
        try
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.Init();
            asset.CopyFrom(EditingItem);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            LoadedItem = asset;

            gameObject.name = Path.GetFileNameWithoutExtension(path);
            Debug.LogFormat("Saved {0} to {1}", DefaultName, path);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("{0} {1}", ex.Message, ex.StackTrace);
        }

        return false;
    }
    
    public virtual void Reload()
    {
        if (LoadedItem != null)
        {
            EditingItem.CopyFrom(LoadedItem);
            EditorUtility.SetDirty(EditingItem);
            OnReload();
            Debug.LogFormat("Reloaded {0}", DefaultName);
        }
    }

    protected virtual void OnReload() {}
}
