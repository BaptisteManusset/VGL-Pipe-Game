using System;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour {
    public GameObject uiRange;
    public TMP_Text uiMode;
    public TMP_Text uiInfos;

    public static UiManager instance;

    private void Awake() {
        instance = this;
    }


    void UpdateUi() {
        if (GameManager.instance == null) return;

        PipeData pipe = GameManager.instance.pipeGrid.currentPipe;
        if (pipe == null) {
            uiInfos.text = "<size=150%>Empty</size>";
            return;
        }

        uiInfos.text = $"<size=150%>Pipe</size>\n";
        if (pipe.isOutput)
            uiInfos.text = $"<size=150%>Output</size>\n";

        if (pipe.isInput)
            uiInfos.text = $"<size=150%>Input</size>\n";
        uiInfos.text += $"Locked : {pipe.isLocked}\n";
        uiInfos.text += $"Filled : {pipe.isFilled}\n";
    }

    private void FixedUpdate() {
        uiInfos.transform.parent.gameObject.SetActive(GameManager.IsPuzzleMode());
        uiMode.text = $"{GameManager.instance.gMode}";

        if (GameManager.IsPuzzleMode()) {
            UpdateUi();
        }
    }
}