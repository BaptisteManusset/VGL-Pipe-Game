using TMPro;
using UnityEngine;

public class ControlesUi : MonoBehaviour {
    [SerializeField] private TMP_Text text;

    private void Awake() {
        text = GetComponentInChildren<TMP_Text>();
    }

    private void FixedUpdate() {
        text.text = "";
        if (GameManager.IsPuzzleMode()) {
            text.text += Place("Echap", "Exit Puzzle mode");
            text.text += Place("Space", "Rotate Pipe");
            if (Inventory.instance.inventory != null) text.text += Place("P", "Place Pipe");
        }
        else {
            text.text += Place("Enter", "Enter Puzzle mode");
            text.text += Place("E", "add Pipe to inventory");
        }
    }

    private string Place(string _key, string _msg) {
        return $"<color=red>{_key}</color> {_msg}\n";
    }
}