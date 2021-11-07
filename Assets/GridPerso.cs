using System.Collections;
using UnityEngine;

public class GridPerso : MonoBehaviour {
    public Vector2Int dimension = new Vector2Int(6, 3);
    public Vector2Int cursor = new Vector2Int(0, 0);

    // public Vector2Int outputPosition;
    // public Vector2Int inputPosition;


    private PipeData[,] pipeArray;


    public PipeData input;
    public PipeData output;

    public PipeData[,] GetArray() {
        return pipeArray ??= new PipeData[dimension.x, dimension.y];
    }


    private void Awake() {
        foreach (PipeData _pipeData in pipeArray) {
            if (_pipeData.isInput) input = _pipeData;
            if (_pipeData.isOutput) output = _pipeData;
        }
    }


    private void Start() {
        SearchAdjacents(input);
    }

    private void SearchAdjacents(PipeData _input) {
        _input.isFilled = true;
        if (_input.directionLinks.down) SearchAdjacent(_input, Vector2Int.down);
        if (_input.directionLinks.up) SearchAdjacent(_input, Vector2Int.up);
        if (_input.directionLinks.right) SearchAdjacent(_input, Vector2Int.left);
        if (_input.directionLinks.left) SearchAdjacent(_input, Vector2Int.right);
    }

    private void SearchAdjacent(PipeData _current, Vector2Int _pos) {
        Vector2Int _position = _current.position + _pos;

        //si la position est pas dans le tableau continuer 
        if ((_position.x < 0 || _position.x >= dimension.x) || (_position.y < 0 || _position.y >= dimension.y)) return;

        //il y a t'il un pipe dans la case selectionnée ?
        if (pipeArray[_position.x, _position.y] == null) return;


        PipeData _pipeToTest = pipeArray[_position.x, _position.y];
        if (_pipeToTest.isFilled) return;
        if (!CanLink(_current, _pipeToTest)) return;


        StartCoroutine(nameof(Search), _pipeToTest);
    }

    public IEnumerator Search(PipeData _pipe) {
        yield return new WaitForSeconds(.1f);
        SearchAdjacents(_pipe);
    }

    private bool CanLink(PipeData _a, PipeData _b) {
        //si c'est la meme ligne
        if (_a.position.x - _b.position.x == 0) {
            if (Mathf.Abs(_a.position.y - _b.position.y) == 1) {
                return true;
            }
        }
        //si c'est la meme colonne
        else if (_a.position.y - _b.position.y == 0) {
            if (Mathf.Abs(_a.position.x - _b.position.x) == 1) {
                return true;
            }
        }

        return false;
    }


    private void Update() {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) cursor.x++;
        if (Input.GetKeyDown(KeyCode.RightArrow)) cursor.x--;

        if (Input.GetKeyDown(KeyCode.UpArrow)) cursor.y++;
        if (Input.GetKeyDown(KeyCode.DownArrow)) cursor.y--;

        if (Input.GetKeyDown(KeyCode.Space)) SelectPipe();

        cursor.x = Mathf.Min(dimension.x - 1, cursor.x);
        cursor.x = Mathf.Max(0, cursor.x);

        cursor.y = Mathf.Min(dimension.y - 1, cursor.y);
        cursor.y = Mathf.Max(0, cursor.y);
    }

    private void SelectPipe() { }

    private void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        for (int i = 0; i < dimension.x; i++) {
            for (int j = 0; j < dimension.y; j++) {
                Gizmos.DrawWireCube(new Vector3(i, j), Vector3.one * .8f);
            }
        }

        Gizmos.color = Color.green;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(cursor.x, cursor.y), Vector3.one * .9f);
    }
}