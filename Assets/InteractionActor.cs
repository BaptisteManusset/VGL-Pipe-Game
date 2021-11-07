using System;
using CMF;
using UnityEngine;
using UnityEngine.Events;

namespace ItsBaptiste.Interaction.Core {
    /// <summary>
    ///     Class permettant de detecter les objets interactifs,
    ///     emet un raycast depuis la camera enfante
    /// </summary>
    public class InteractionActor : MonoBehaviour {
        public Camera _camera;

        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float distanceMax = 10;
        [SerializeField] private InteractableBase currentInteractable;

        public AdvancedWalkerController advancedWalkerController;

        private void FixedUpdate() {
            RaycastFromCamera();
        }

        private void RaycastFromCamera() {
            RaycastHit hit;
            if (Physics.Raycast(_camera.transform.position, _camera.transform.TransformDirection(Vector3.forward), out hit, distanceMax, layerMask) && advancedWalkerController.IsInteract()) {
                Debug.DrawRay(_camera.transform.position, _camera.transform.TransformDirection(Vector3.forward) * hit.distance, Color.cyan);
                InteractableBase hitInteractable = hit.collider.gameObject.GetComponent<InteractableBase>();
                if (hitInteractable != null) {
                    // interactable deja selectionné
                    if (hitInteractable == currentInteractable) {
                        float distance = Vector3.Distance(transform.position, GetComponent<Camera>().transform.position);
                        if (distance >= distanceMax) {
                            currentInteractable.OnExit();
                            currentInteractable = null;
                            return;
                        }

                        return;
                    }

                    // aucun interactable definie, entre dans le nouveau
                    if (currentInteractable == null) {
                        currentInteractable = hitInteractable;
                        currentInteractable.OnEnter();
                        return;
                    }

                    // il y a deja un interactable, on le quitte puis on entre dans le nouveau
                    if (hitInteractable != currentInteractable) {
                        currentInteractable.OnExit();
                        currentInteractable = hitInteractable;
                        currentInteractable.OnEnter();
                    }
                }
                else {
                    // on quitte le current interactable
                    if (currentInteractable != null) {
                        currentInteractable.OnExit();
                        currentInteractable = null;
                    }
                }
            }
            else {
                if (currentInteractable != null) {
                    currentInteractable.OnExit();
                    currentInteractable = null;
                }
            }
        }

        public interface IInteractable {
            public void OnEnter();
            public void OnExit();
        }
    }
}