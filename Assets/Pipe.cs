using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[SelectionBase]
public class Pipe : MonoBehaviour {
    public PipeData data;

    private PipeVisual visual;

    private GridPerso grid;


    private void OnMouseDown() {
        RotateClock();
    }


    private void Awake() {
        visual = GetComponentInChildren<PipeVisual>();
        data.isFilled = data.isInput;
        grid = GetComponentInParent<GridPerso>();


        data.pipe = this;
        data.name = gameObject.name;
        data.position = new Vector2Int((int)transform.localPosition.x, (int)transform.localPosition.y);

        // data.position = new Vector2Int((int)transform.localPosition.x, (int)transform.localPosition.y);

        grid.GetArray()[data.position.x, data.position.y] = data;
    }

    private void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = new Color(1f, 0.46f, 0.03f);
        if (data.directionLinks.up) Gizmos.DrawSphere(Vector3.up / 3, .05f);
        if (data.directionLinks.right) Gizmos.DrawSphere(Vector3.left / 3, .05f);
        if (data.directionLinks.left) Gizmos.DrawSphere(Vector3.right / 3, .05f);
        if (data.directionLinks.down) Gizmos.DrawSphere(Vector3.down / 3, .05f);


        Gizmos.color = Color.cyan;
        if (data.isFilled) Gizmos.DrawSphere(Vector3.zero, .3f);


        Gizmos.color = Color.green;
        if (data.isInput)
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(.7f, .7f, .7f));


        Gizmos.color = Color.red;
        if (data.isOutput)
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(.7f, .7f, .7f));
    }


    [Button("Rotate")]
    public void RotateClock() {
        PipeData.DirectionLinks _links = new PipeData.DirectionLinks();
        _links.right = data.directionLinks.up;
        _links.down = data.directionLinks.right;
        _links.left = data.directionLinks.down;
        _links.up = data.directionLinks.left;

        data.directionLinks = _links;

        grid.ResetAll();
        visual.UpdateVisual(data);

    }

    [Button()]
    private void PipeCross() {
        SetPipe("Cross", new PipeData.DirectionLinks() {
            up = true,
            right = true,
            down = true,
            left = true
        });
    }

    [Button()]
    private void PipeT() {
        SetPipe("T", new PipeData.DirectionLinks() {
            up = false,
            right = true,
            down = true,
            left = true
        });
    }

    [Button()]
    private void PipeI() {
        SetPipe("I", new PipeData.DirectionLinks() {
            up = true,
            right = false,
            down = true,
            left = false
        });
    }

    [Button()]
    private void PipeAngle() {
        SetPipe("Angle", new PipeData.DirectionLinks() {
            up = true,
            right = true,
            down = false,
            left = false
        });
    }

    private void SetPipe(string name, PipeData.DirectionLinks _directionLinks) {
        data.directionLinks = _directionLinks;
        visual.UpdateVisual(data);

        gameObject.name = "Pipe " + name;
    }


#if UNITY_EDITOR
    private void OnValidate() {
        if (visual == null) visual = GetComponentInChildren<PipeVisual>();
        visual.UpdateVisual(data);
    }
#endif


    private void Reset() {
        if (visual == null) visual = GetComponentInChildren<PipeVisual>();
        visual.UpdateVisual(data);
    }
}