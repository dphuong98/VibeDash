using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

public class SetPlayerModelButton : MonoBehaviour
{
    private GameObject playerModel;

    [SerializeField] private AnimatorController idlePose;
    [SerializeField] private Transform modelPivot;
    [SerializeField] private Transform checkIcon;

    public void SetPlayerPref()
    {
        if (!playerModel) return;
        
        PlayerPrefs.SetString(SaveDataKeys.PlayerModelName, playerModel.name);
    }

    public void SetPlayerModel(GameObject model)
    {
        playerModel = model;
        var playerAnim = Instantiate(playerModel, modelPivot); 
        playerAnim.transform.SetLayer(LayerMask.NameToLayer("UI"));
        playerAnim.GetComponent<Animator>().runtimeAnimatorController = idlePose;
    }

    public void SetSelected(bool selected)
    {
        checkIcon.gameObject.SetActive(selected);
    }
}
