using UnityEngine;
using UnityEngine.Serialization;

public class PipeVisual : MonoBehaviour {
    [SerializeField] private GameObject up;
    [SerializeField] private GameObject down;
    [SerializeField] private GameObject right;
    [SerializeField] private GameObject left;

    public void UpdateVisual(PipeData _data) {
        up.SetActive(_data.directionLinks.up);
        down.SetActive(_data.directionLinks.down);
        right.SetActive(_data.directionLinks.right);
        left.SetActive(_data.directionLinks.left);
    }
}