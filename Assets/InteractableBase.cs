using System;
using ItsBaptiste.Interaction.Core;
using UnityEngine;
using UnityEngine.Events;

public class InteractableBase : MonoBehaviour, InteractionActor.IInteractable {
    [Serializable]
    public class Events {
        public UnityEvent onHover;
        public UnityEvent onEnter;
        public UnityEvent onExit;
    }

    [SerializeField] public Events events;

    // public bool automaticInteract = false;
    public virtual void OnHover() {
        events.onHover?.Invoke();
        _interact = true;
    }

    public virtual void OnEnter() {
        events.onEnter?.Invoke();
        _interact = true;
    }

    public virtual void OnExit() {
        events.onExit?.Invoke();
        _interact = false;
    }

    private bool _interact = false;


    private void OnDrawGizmos() {
        if (_interact)
            Gizmos.DrawCube(transform.position, Vector3.one * 1.001f);
    }
}