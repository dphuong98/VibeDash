using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class BuilderEditor<T> : Editor where T : ScriptableObject, IInit, ICopiable<T>
{
    protected Builder<T> builder;

    protected void OnEnable()
    {
        builder = target as Builder<T>;
    }

    public virtual void NewItem()
    {
        builder.NewItem();
    }

    public virtual void Open()
    {
        var path = EditorUtility.OpenFilePanel("Open", Builder<T>.DefaultFolder, "asset");
        builder.Open(FileUtil.GetProjectRelativePath(path));
    }

    public virtual void Save()
    {
        if (!builder.Save())
            SaveAs();
    }

    public virtual void SaveAs()
    {
        var rx = new Regex(@"(\d+)");
        var d = new DirectoryInfo(Builder<T>.DefaultFolder);
        var number = 0;
        if (d.GetFiles(Builder<T>.DefaultName+"?.asset") is var fileInfos && fileInfos.Count() != 0)
        {
            number = fileInfos.Select(s => rx.Match(s.Name)).Where(s => s.Success).Max(s =>
            {
                int.TryParse(s.Value, out var num);
                return num;
            });
        }
        
        var path = EditorUtility.SaveFilePanel("Save As", Builder<T>.DefaultFolder, Builder<T>.DefaultName+(number+1), "asset");
        builder.Save(FileUtil.GetProjectRelativePath(path));
    }

    public virtual void Reload()
    {
        builder.Reload();
    }
}
