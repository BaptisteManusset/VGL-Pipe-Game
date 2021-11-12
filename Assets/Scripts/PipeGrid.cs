using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class PipeGrid : MonoBehaviour {
    public Vector2Int dimension = new Vector2Int(6, 3);
    public Vector2Int cursor = new Vector2Int(0, 0);

    public Pipe input;
    public Pipe output;

    public Renderer background;

    public Transform cursorObject;

    public bool isSelected = false;
    public bool inRange = false;

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

    // private void OnTriggerEnter(Collider _other) {
    //     distance = true;
    // }
    //
    // private void OnTriggerExit(Collider _other) {
    //     distance = false;
    // }

    private bool distance = false;

    private void Update() {
        // if (isSelected == false) {
        if (GameManager.IsExplorationMode()) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                float distance = Vector3.Distance(GameManager.instance.player.transform.position, Center);
                if (distance <= maxDistance && isWorking) {
                    inRange = true;

                    GameManager.SetToPuzzleMode(this);
                    isSelected = true;
                }
            }
            else {
                inRange = false;
            }

            return;
        }

        //     return;
        // }

        #region input

        if (GameManager.IsPuzzleMode() && isSelected) {
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


            if (Input.GetKeyDown(Keys.Interaction)) RotatePipe();
            if (Input.GetKeyDown(Keys.Interaction)) PlacePipe();
            if (Input.GetKeyDown(Keys.InteractionSecondary)) GetPipe();
        }

        #endregion
    }

    private void GetPipe() {
        Debug.Log("GetPipe");
        var _d = pipeArray[cursor.x, cursor.y];
        if (_d.isEmpty) return;
        if (_d.isLocked || _d.isOutput || _d.isInput) return;
        if (Inventory.instance.isEmpty == false) return;

        Inventory.instance.Slot = _d;
        ResetAll();
    }

    private void PlacePipe() {
        var _d = pipeArray[cursor.x, cursor.y];
        if (_d.isEmpty) {
            if (Inventory.instance.isEmpty) return;
            pipeArray[cursor.x, cursor.y].directionLinks = Inventory.instance.Slot.directionLinks;
            pipeArray[cursor.x, cursor.y].pipe.visual.UpdateVisual(Inventory.instance.Slot);
            Inventory.instance.RemoveItem();
            _d.isEmpty = false;

            ResetAll();
        }
    }


#if UNITY_EDITOR
    [Button]
    private void RandomPosition() {
        foreach (Pipe _pipe in GetComponentsInChildren<Pipe>()) {
            _pipe.RandomPipe();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.matrix = transform.localToWorldMatrix;
        for (int _i = 0; _i < dimension.x; _i++) {
            for (int _j = 0; _j < dimension.y; _j++) {
                Gizmos.DrawWireCube(new Vector3(_i, _j), Vector3.one * .8f);
            }
        }

        Gizmos.color = Color.green;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(cursor.x, cursor.y), Vector3.one * .9f);

        Gizmos.DrawWireSphere(new Vector3(dimension.x, dimension.y, 0) / 2, maxDistance);
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
        for (int _y = 0; _y < pipeArray.GetLength(0); _y++) {
            for (int _j = 0; _j < pipeArray.GetLength(1); _j++) {
                if (pipeArray[_y, _j] != null) {
                    pipeArray[_y, _j].isFilled = pipeArray[_y, _j].isInput;

                    pipeArray[_y, _j].pipe.visual.UpdateVisual(pipeArray[_y, _j]);
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
        completed = true;
        onSuccess.Invoke();
        // GameManager.SetToExplorationMode();
        UpdateWire();
    }


    [SerializeField] private Material sucessMaterial;
    public bool completed = false;

    private void UpdateWire() {
        Transform _child = spline.transform.GetChild(0);
        for (int _i = 0; _i < _child.childCount; _i++) {
            MeshRenderer _renderer = _child.GetChild(_i).GetComponent<MeshRenderer>();
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
        Vector3 _position = transform.position + new Vector3(dimension.x, dimension.y, 0) / 2;
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