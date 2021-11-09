using UnityEngine;

public class Door : MonoBehaviour {
    public PipeGrid[] pipeGrids;

    public GameObject door;

    void FixedUpdate() {
        bool close = true;

        foreach (PipeGrid _pipeGrid in pipeGrids) {
            if (_pipeGrid.completed) close = false;
        }

        door.SetActive(close);
    }
}