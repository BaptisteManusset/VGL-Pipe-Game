using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF {
    //This character movement input class is an example of how to get input from a keyboard to control the character;
    public class CharacterKeyboardInput : CharacterInput {
        public string horizontalInputAxis = "Horizontal";
        public string verticalInputAxis = "Vertical";
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;
        public KeyCode interactionKey = KeyCode.E;

        //If this is enabled, Unity's internal input smoothing is bypassed;
        public bool useRawInput = true;

        public override float GetHorizontalMovementInput() {
            if (useRawInput) {
                return Input.GetAxisRaw(horizontalInputAxis);
            }

            return Input.GetAxis(horizontalInputAxis);
        }

        public override float GetVerticalMovementInput() {
            if (useRawInput) {
                return Input.GetAxisRaw(verticalInputAxis);
            }

            return Input.GetAxis(verticalInputAxis);
        }

        public override bool IsJumpKeyPressed() {
            return Input.GetKey(jumpKey);
        }

        public override bool IsSprintKeyPressed() {
            return Input.GetKey(sprintKey);
        }

        public override bool IsInteractionKeyPressed() {
            return Input.GetKey(interactionKey);
        }
    }
}