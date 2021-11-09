using System;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public enum GMode {
        Exploration,
        Puzzle
    }

    public Action OnPuzzleMode;
    public Action OnExplorationMode;

    public GMode gMode = GMode.Exploration;

    public static GameManager instance;

    public GameObject player;
    public Camera playerCamera;
    public PipeGrid pipeGrid;

    private void Awake() {
        instance = this;

        playerCamera = player.GetComponentInChildren<Camera>();
    }


    public static bool IsPuzzleMode() {
        return instance.gMode == GMode.Puzzle;
    }

    public static bool IsExplorationMode() {
        return instance.gMode == GMode.Exploration;
    }

    public static void SetToPuzzleMode(PipeGrid _pipeGrid) {
        if (_pipeGrid == null) {
            throw new Exception("Missing pipe grid");
        }

        instance.gMode = GMode.Puzzle;

        instance.OnPuzzleMode?.Invoke();

        instance.pipeGrid = _pipeGrid;
    }

    public static void SetToExplorationMode() {
        instance.gMode = GMode.Exploration;
        instance.pipeGrid = null;

        instance.OnExplorationMode?.Invoke();
    }
    
    
}