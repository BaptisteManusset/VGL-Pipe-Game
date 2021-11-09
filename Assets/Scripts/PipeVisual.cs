using System;
using UnityEngine;

public class PipeVisual : MonoBehaviour {
    [SerializeField] private GameObject up;
    [SerializeField] private GameObject down;
    [SerializeField] private GameObject right;
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject body;

    [SerializeField] private SpriteRenderer fill;

    public void UpdateVisual(PipeData _data) {
        if (_data == null) return;

        up.SetActive(_data.directionLinks.up && !_data.isEmpty);
        down.SetActive(_data.directionLinks.down && !_data.isEmpty);
        right.SetActive(_data.directionLinks.right && !_data.isEmpty);
        left.SetActive(_data.directionLinks.left && !_data.isEmpty);
        body.SetActive(!_data.isEmpty);

        fill.gameObject.SetActive(_data.isFilled && !_data.isEmpty);
    }
}