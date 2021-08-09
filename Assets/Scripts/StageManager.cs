using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public Stage currentStage;

    public Stage GetCurrentStage()
    {
        return currentStage;
    }
}
