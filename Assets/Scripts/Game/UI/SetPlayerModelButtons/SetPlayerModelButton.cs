using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerModelButton : MonoBehaviour
{
    public GameObject playerPrefab;
    
    public void SetPlayerModel()
    {
        if (!playerPrefab) return;
        
        PlayerPrefs.SetString(SaveDataKeys.PlayerModelName, playerPrefab.name);
    }
}
