using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class StageBuilderScene : MonoBehaviour
{
    public static string loadStageUponEnable;
    public StageBuilder StageBuilder;
    
    //Currently on the bottom of Script Execution Order
    private void OnEnable()
    {
        if (loadStageUponEnable == null) return;
        
        StageBuilder.Open(loadStageUponEnable);
        loadStageUponEnable = null;
    }
}
