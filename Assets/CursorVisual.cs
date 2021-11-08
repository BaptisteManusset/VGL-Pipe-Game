using UnityEngine;
using TMPro;

public class CursorVisual : MonoBehaviour {
    private TMP_Text textElem;


    private void Awake() {
        textElem = GetComponentInChildren<TMP_Text>();
    }

    public void UpdateVisual(PipeData _pipe) {
        textElem.text = "";

        if (_pipe == null) return;
        if (_pipe.isOutput)
            textElem.text = "Output";
        if (_pipe.isInput)
            textElem.text = "Input";
    }
}