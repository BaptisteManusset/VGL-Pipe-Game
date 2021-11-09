using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

public class PipeGrid : MonoBehaviour {
    public Vector2Int dimension = new Vector2Int(6, 3);
    public Vector2Int cursor = new Vector2Int(0, 0);

    public Pipe input;
    public Pipe output;

    public Renderer background;

    public Transform cursorObject;

    public bool isSelected = false;
    public bool inRange = false;


    public Transform cameraPoint;

    private CursorVisual cursorVisual;

    private float maxDistance = 5;


    public UnityEvent onSuccess = new UnityEvent();

    private PipeData[,] pipeArray;
    public PipeData currentPipe;

    public bool isWorking = false;
    public GameObject lamp;
    public Vector3 Center => transform.position + new Vector3(dimension.x, dimension.y, 0) / 2;

    [SerializeField] private GameObject spline;


    private void Awake() {
        cursorVisual = GetComponentInChildren<CursorVisual>();
    }


    private void Start() {
        ResetAll();
    }


    public void ToggleIsWorking(bool _b) {
        isWorking = _b;
    }

    private void OnTriggerEnter(Collider other) {
        _distance = true;
    }

    private void OnTriggerExit(Collider other) {
        _distance = false;
    }

    private bool _distance = false;

    private void Update() {
        if (isSelected == false) {
            if (GameManager.IsExplorationMode()) {
                // float _distance = Vector3.Distance(GameManager.instance.player.transform.position, Center);
                if (_distance && isWorking) {
                    inRange = true;

                    if (Input.GetKeyDown(KeyCode.Return)) {
                        GameManager.SetToPuzzleMode(this);
                        isSelected = true;
                    }
                }
                else {
                    inRange = false;
                }

                return;
            }

            return;
        }

        #region input

        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            cursor.x++;
            UpdateCusorPosition();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            cursor.x--;
            UpdateCusorPosition();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            cursor.y++;
            UpdateCusorPosition();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            cursor.y--;
            UpdateCusorPosition();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            GameManager.SetToExplorationMode();
            isSelected = false;
        }


        if (Input.GetKeyDown(KeyCode.Space)) RotatePipe();
        if (Input.GetKeyDown(KeyCode.P)) PlacePipe();

        #endregion
    }

    private void PlacePipe() {
        var d = pipeArray[cursor.x, cursor.y];
        if (d.isEmpty) {
            if (Inventory.instance.inventory == null) return;
            pipeArray[cursor.x, cursor.y].directionLinks = Inventory.instance.inventory.data.directionLinks;
            pipeArray[cursor.x, cursor.y].pipe.visual.UpdateVisual(Inventory.instance.inventory.data);
            Inventory.instance.RemoveItem();
            d.isEmpty = false;

            ResetAll();
        }
    }


#if UNITY_EDITOR
    [Button]
    private void RandomPosition() {
        foreach (Pipe pipe in GetComponentsInChildren<Pipe>()) {
            pipe.RandomPipe();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.matrix = transform.localToWorldMatrix;
        for (int i = 0; i < dimension.x; i++) {
            for (int j = 0; j < dimension.y; j++) {
                Gizmos.DrawWireCube(new Vector3(i, j), Vector3.one * .8f);
            }
        }

        Gizmos.color = Color.green;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(cursor.x, cursor.y), Vector3.one * .9f);

        Gizmos.DrawWireSphere(new Vector3(dimension.x, dimension.y, 0) / 2, maxDistance);

        Gizmos.DrawSphere(cameraPoint.localPosition, .5f);

        // if (Application.isPlaying) Gizmos.DrawFrustum(cursorObject.localPosition, GameManager.instance.playerCamera.fieldOfView, 10, 0, GameManager.instance.playerCamera.aspect);
    }
#endif

    public PipeData[,] GetArray() {
        return pipeArray ??= new PipeData[dimension.x, dimension.y];
    }

    private void RotatePipe() {
        if (pipeArray[cursor.x, cursor.y] != null) {
            PipeData _pipe = pipeArray[cursor.x, cursor.y];

            _pipe.pipe.Rotate();
        }
    }

    public void ResetAll() {
        for (int y = 0; y < pipeArray.GetLength(0); y++) {
            for (int j = 0; j < pipeArray.GetLength(1); j++) {
                if (pipeArray[y, j] != null) {
                    pipeArray[y, j].isFilled = pipeArray[y, j].isInput;

                    pipeArray[y, j].pipe.visual.UpdateVisual(pipeArray[y, j]);
                }
            }
        }

        SearchAdjacents(input.data);
    }

    #region Search System

    private void SearchAdjacents(PipeData _input) {
        _input.isFilled = true;

        if (_input.isOutput) OnSuccess();

        _input.pipe.visual.UpdateVisual(_input);
        if (_input.directionLinks.down) SearchAdjacent(_input, Vector2Int.down);
        if (_input.directionLinks.up) SearchAdjacent(_input, Vector2Int.up);
        if (_input.directionLinks.right) SearchAdjacent(_input, Vector2Int.left);
        if (_input.directionLinks.left) SearchAdjacent(_input, Vector2Int.right);
    }

    private void OnSuccess() {
        Debug.Log("Sucess");
        onSuccess.Invoke();
        GameManager.SetToExplorationMode();
        UpdateWire();
    }


    [SerializeField] private Material sucessMaterial;

    private void UpdateWire() {
        Transform _child = spline.transform.GetChild(0);
        for (int i = 0; i < _child.childCount; i++) {
            MeshRenderer _renderer = _child.GetChild(i).GetComponent<MeshRenderer>();
            _renderer.material = sucessMaterial;
        }
    }


    public PipeData GetCaseAtCusor() {
        return pipeArray[cursor.x, cursor.y];
    }

    private void SearchAdjacent(PipeData _current, Vector2Int _pos) {
        Vector2Int _position = _current.position + _pos;

        //si la position est pas dans le tableau continuer 
        if ((_position.x < 0 || _position.x >= dimension.x) || (_position.y < 0 || _position.y >= dimension.y)) return;

        //il y a t'il un pipe dans la case selectionnée ?
        if (pipeArray[_position.x, _position.y] == null) return;


        PipeData _pipeToTest = pipeArray[_position.x, _position.y];
        if (_pipeToTest.isFilled) return;
        if (!IsAdjacent(_current, _pipeToTest)) return;

        StartCoroutine(nameof(SearchWithDelay), _pipeToTest);
    }

    public IEnumerator SearchWithDelay(PipeData _pipe) {
        yield return new WaitForSeconds(.1f);
        SearchAdjacents(_pipe);
    }

    private bool IsAdjacent(PipeData _current, PipeData _next) {
        //si c'est la meme ligne
        if (_current.position.x - _next.position.x == 0) {
            int _verti = _current.position.y - _next.position.y;
            if (_verti == 1 && _next.directionLinks.up && _current.directionLinks.down) {
                return true;
            }

            if (_verti == -1 && _next.directionLinks.down && _current.directionLinks.up) {
                return true;
            }

            return false;
        }

        //si c'est la meme colonne
        if (_current.position.y - _next.position.y == 0) {
            int _hori = _current.position.x - _next.position.x;
            if (_hori == 1 && _next.directionLinks.left && _current.directionLinks.right) {
                return true;
            }

            if (_hori == -1 && _next.directionLinks.right && _current.directionLinks.left) {
                return true;
            }

            return false;
        }

        return false;
    }

    #endregion


    #region cursor

    private void UpdateCusorPosition() {
        cursor.x = Mathf.Min(dimension.x - 1, cursor.x);
        cursor.x = Mathf.Max(0, cursor.x);

        cursor.y = Mathf.Min(dimension.y - 1, cursor.y);
        cursor.y = Mathf.Max(0, cursor.y);

        currentPipe = pipeArray[cursor.x, cursor.y];
        UpdateCursorVisual();
    }

    private void UpdateCursorVisual() {
        cursorObject.position = transform.position + (Vector3)(transform.localToWorldMatrix * new Vector3(cursor.x, cursor.y, 0));
        cursorVisual.UpdateVisual(pipeArray[cursor.x, cursor.y]);
    }

    #endregion

    [Button]
    private void SetCameraPoint() {
        Vector3 position = transform.position + new Vector3(dimension.x, dimension.y, 0) / 2;
    }

    private void OnBecameVisible() {
        Debug.Log("visible");
    }


    private void FixedUpdate() {
        Color _color = Color.black;


        if (isWorking) {
            if (isSelected) {
                _color = Color.white;
            }
            else {
                _color = Color.gray;
            }

            if (inRange) _color = new Color(0.96f, 0.98f, 0.79f);

            if (output.data.isFilled) {
                _color = new Color(0.18f, 0.66f, 0.07f);
            }
        }

        lamp.SetActive(isWorking);

        background.material.SetColor("_Color", _color);
    }
}