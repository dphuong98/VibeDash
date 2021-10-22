using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : Singleton<DebugUI>
{
    [SerializeField] private Text text;

    private void Update()
    {
        text.text = "";
    }

    public void AddText(string text)
    {
        this.text.text += text;
    }
}
