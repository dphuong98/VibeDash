using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeSetPlayerModelButtons : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;

    private SetPlayerModelButton selectedButton;
    
    private void Awake()
    {
        var playerModels = Resources.LoadAll<GameObject>(ResourcePaths.PlayerModelFolder);
        foreach (var playerModel in playerModels)
        {
            var button = Instantiate(buttonPrefab, transform);
            button.name = playerModel.name;
            button.GetComponent<SetPlayerModelButton>().SetPlayerModel(playerModel);
        }
    }

    private void Update()
    {
        var selectedModelName = PlayerPrefs.GetString(SaveDataKeys.PlayerModelName);
        
        if (selectedButton && selectedButton.name == selectedModelName) return;
        
        selectedButton?.SetSelected(false);
        selectedButton = transform.FindInChildren(selectedModelName).GetComponent<SetPlayerModelButton>();
        selectedButton.SetSelected(true);
    }
}
