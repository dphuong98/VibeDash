using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    //TODO encapsulation
    public Stage currentStage;

    public Stage GetCurrentStage()
    {
        return currentStage;
    }
}
