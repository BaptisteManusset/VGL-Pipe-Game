using UnityEngine;

class DragTransform : MonoBehaviour {
    private Color mouseOverColor = Color.blue;
    private Color originalColor = Color.yellow;
    private bool dragging = false;
    private float distance;

    public new Renderer renderer;
    private Vector3 starDist;

    void OnMouseEnter() {
        renderer.material.color = mouseOverColor;
    }

    void OnMouseExit() {
        renderer.material.color = originalColor;
    }

    void OnMouseDown() {
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        dragging = true;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 rayPoint = ray.GetPoint(distance);
        starDist = transform.position - rayPoint;
    }

    void OnMouseUp() {
        dragging = false;
    }

    void Update() {
        if (dragging) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(distance);
            transform.position = rayPoint + starDist;
        }
    }
}