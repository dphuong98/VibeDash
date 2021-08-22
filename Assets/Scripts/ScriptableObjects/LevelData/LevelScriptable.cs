using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Scriptables/Level", order = 2)]
public class LevelScriptable : ScriptableObject
{
    public List<StageScriptable> stages;
}
