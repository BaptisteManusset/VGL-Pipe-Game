using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emplacement : MonoBehaviour {
    private MeshRenderer render;

    private void Awake() {
        render = GetComponent<MeshRenderer>();
    }

    private void OnMouseEnter() {
        render.material.color = Color.red;
    }

    private void OnMouseExit() {
        render.material.color = Color.white;
    }
}