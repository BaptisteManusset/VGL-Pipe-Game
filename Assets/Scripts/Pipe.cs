using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[SelectionBase]
public class Pipe : MonoBehaviour {
    public PipeData data;

    public PipeVisual visual;

    private PipeGrid pipeGrid;

    //
    // private void OnMouseDown() {
    //     Rotate();
    // }


    private void Awake() {
        pipeGrid = GetComponentInParent<PipeGrid>();
        visual = GetComponentInChildren<PipeVisual>();


        data.isFilled = data.isInput;

        //si c'est un input ou un output alors on block la rotation
        data.isLocked = (data.isInput || data.isOutput) ? true : data.isLocked;

        data.pipe = this;
        data.name = gameObject.name;
        data.position = new Vector2Int((int)transform.localPosition.x, (int)transform.localPosition.y);

        pipeGrid.GetArray()[data.position.x, data.position.y] = data;
    }


    [Button("Rotate")]
    public void Rotate() {
        if (data.isLocked) return;


        PipeData.DirectionLinks _links = new PipeData.DirectionLinks();
        _links.right = data.directionLinks.up;
        _links.down = data.directionLinks.right;
        _links.left = data.directionLinks.down;
        _links.up = data.directionLinks.left;

        data.directionLinks = _links;

        if (pipeGrid == null) pipeGrid.GetComponentInParent<PipeGrid>();
        pipeGrid.ResetAll();
        visual.UpdateVisual(data);
    }

    private void SetPipe(string name, PipeData.DirectionLinks _directionLinks) {
        data.directionLinks = _directionLinks;
        visual.UpdateVisual(data);

        gameObject.name = "Pipe " + name;
    }




    private void Reset() {
        if (visual == null) visual = GetComponentInChildren<PipeVisual>();
        visual.UpdateVisual(data);
    }


    #region editor buttons

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

    #endregion
    
    
#if UNITY_EDITOR
    private void OnValidate() {
        if (visual == null) visual = GetComponentInChildren<PipeVisual>();
        visual.UpdateVisual(data);
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
#endif
}