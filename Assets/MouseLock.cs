using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItsBaptiste {
    public class MouseLock : MonoBehaviour {
        private void Start() {
            SetCursorLock(false);
        }

        private void SetCursorLock(bool cursorLock) {
            if (cursorLock) {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        void DisplayCursor() {
            Cursor.visible = true;
        }

        void HideCursor() {
            Cursor.visible = false;
        }
    }
}