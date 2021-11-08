using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF {
    /// <summary>
    ///Advanced walker controller script;
    ///This controller is used as a basis for other controller types ('SidescrollerController');
    ///Custom movement input can be implemented by creating a new script that inherits 'AdvancedWalkerController' and overriding the 'CalculateMovementDirection' function;
    /// </summary>
    public class AdvancedWalkerController : Controller {
        //References to attached components;
        protected Transform TR;
        protected Mover Mover;
        protected CharacterInput CharacterInput;
        protected CeilingDetector CeilingDetector;

        //Jump key variables;
        bool _jumpInputIsLocked = false;
        bool _jumpKeyWasPressed = false;
        bool _jumpKeyWasLetGo = false;
        bool _jumpKeyIsPressed = false;

        [Header("Movement speed")] public float movementSpeed = 7;
        public float sprintSpeed = 10;

        /// <summary>
        /// How fast the controller can change direction while in the air;
        /// Higher values result in more air control;
        /// </summary>
        [Space] public float airControlRate = 2;

        /// <summary>
        /// Jump speed
        /// </summary>
        public float jumpSpeed = 10;

        /// <summary>
        /// Jump duration variables;
        /// </summary>
        public float jumpDuration = 0.2f;

        float _currentJumpStartTime = 0;

        /// <summary>
        /// 'AirFriction' determines how fast the controller loses its momentum while in the air;
        /// 'GroundFriction' is used instead, if the controller is grounded;
        /// </summary>
        public float airFriction = 0.5f;

        public float groundFriction = 100;

        /// <summary>
        /// Current momentum;
        /// </summary>
        protected Vector3 Momentum = Vector3.zero;

        /// <summary>
        /// Saved velocity from last frame;
        /// </summary>
        Vector3 _savedVelocity = Vector3.zero;

        /// <summary>
        /// Saved horizontal movement velocity from last frame;
        /// </summary>
        Vector3 _savedMovementVelocity = Vector3.zero;

        /// <summary>
        /// Amount of downward gravity;
        /// </summary>
        public float gravity = 30f;

        [Tooltip("How fast the character will slide down steep slopes.")] public float slideGravity = 5f;

        /// <summary>
        /// Acceptable slope angle limit;
        /// </summary>
        public float slopeLimit = 80f;

        [Tooltip("Whether to calculate and apply momentum relative to the controller's transform.")]
        public bool useLocalMomentum = false;

        /// <summary>
        /// Enum describing basic controller states; 
        /// </summary>
        public enum ControllerState {
            Grounded,
            Sliding,
            Falling,
            Rising,
            Jumping
        }

        ControllerState _currentControllerState = ControllerState.Falling;

        [Tooltip("Optional camera transform used for calculating movement direction. If assigned, character movement will take camera view into account.")]
        public Transform cameraTransform;

        /// <summary>
        /// Get references to all necessary components;
        /// </summary>
        void Awake() {
            Mover = GetComponent<Mover>();
            TR = transform;
            CharacterInput = GetComponent<CharacterInput>();
            CeilingDetector = GetComponent<CeilingDetector>();

            if (CharacterInput == null)
                Debug.LogWarning("No character input script has been attached to this gameobject", this.gameObject);

            Setup();
        }

        /// <summary>
        /// This function is called right after Awake(); It can be overridden by inheriting scripts;
        /// </summary>
        protected virtual void Setup() { }

        void Update() {
            HandleJumpKeyInput();
        }

        /// <summary>
        /// Handle jump booleans for later use in FixedUpdate;
        /// </summary>
        void HandleJumpKeyInput() {
            bool newJumpKeyPressedState = IsJumpKeyPressed();

            if (_jumpKeyIsPressed == false && newJumpKeyPressedState == true)
                _jumpKeyWasPressed = true;

            if (_jumpKeyIsPressed == true && newJumpKeyPressedState == false) {
                _jumpKeyWasLetGo = true;
                _jumpInputIsLocked = false;
            }

            _jumpKeyIsPressed = newJumpKeyPressedState;
        }

        void FixedUpdate() {
            ControllerUpdate();
        }


        /// <summary>
        ///Update controller;
        ///This function must be called every fixed update, in order for the controller to work correctly;
        /// </summary>
        void ControllerUpdate() {
            if (GameManager.instance.gMode == GameManager.GMode.Puzzle) return;

            //Check if mover is grounded;
            Mover.CheckForGround();

            //Determine controller state;
            _currentControllerState = DetermineControllerState();

            //Apply friction and gravity to 'momentum';
            HandleMomentum();

            //Check if the player has initiated a jump;
            HandleJumping();

            //Calculate movement velocity;
            Vector3 velocity = Vector3.zero;
            if (_currentControllerState == ControllerState.Grounded)
                velocity = CalculateMovementVelocity();

            //If local momentum is used, transform momentum into world space first;
            Vector3 worldMomentum = Momentum;
            if (useLocalMomentum)
                worldMomentum = TR.localToWorldMatrix * Momentum;

            //Add current momentum to velocity;
            velocity += worldMomentum;

            //If player is grounded or sliding on a slope, extend mover's sensor range;
            //This enables the player to walk up/down stairs and slopes without losing ground contact;
            Mover.SetExtendSensorRange(IsGrounded());

            //Set mover velocity;		
            Mover.SetVelocity(velocity);

            //Store velocity for next frame;
            _savedVelocity = velocity;

            //Save controller movement velocity;
            _savedMovementVelocity = CalculateMovementVelocity();

            //Reset jump key booleans;
            _jumpKeyWasLetGo = false;
            _jumpKeyWasPressed = false;

            //Reset ceiling detector, if one is attached to this gameobject;
            if (CeilingDetector != null)
                CeilingDetector.ResetFlags();
        }

        /// <summary>
        /// Calculate and return movement direction based on player input;
        /// This function can be overridden by inheriting scripts to implement different player controls;
        /// </summary>
        /// <returns></returns>
        protected virtual Vector3 CalculateMovementDirection() {
            //If no character input script is attached to this object, return;
            if (CharacterInput == null)
                return Vector3.zero;

            Vector3 velocity = Vector3.zero;

            //If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
            if (cameraTransform == null) {
                velocity += TR.right * CharacterInput.GetHorizontalMovementInput();
                velocity += TR.forward * CharacterInput.GetVerticalMovementInput();
            }
            else {
                //If a camera transform has been assigned, use the assigned transform's axes for movement direction;
                //Project movement direction so movement stays parallel to the ground;
                velocity += Vector3.ProjectOnPlane(cameraTransform.right, TR.up).normalized * CharacterInput.GetHorizontalMovementInput();
                velocity += Vector3.ProjectOnPlane(cameraTransform.forward, TR.up).normalized * CharacterInput.GetVerticalMovementInput();
            }

            //If necessary, clamp movement vector to magnitude of 1f;
            if (velocity.magnitude > 1f)
                velocity.Normalize();

            return velocity;
        }

        /// <summary>
        /// Calculate and return movement velocity based on player input, controller state, ground normal [...];
        /// </summary>
        /// <returns></returns>
        protected virtual Vector3 CalculateMovementVelocity() {
            //Calculate (normalized) movement direction;
            Vector3 velocity = CalculateMovementDirection();

            //Multiply (normalized) velocity with movement speed;
            velocity *= MovementSpeed();

            return velocity;
        }

        private float MovementSpeed() {
            return CharacterInput.IsSprintKeyPressed() ? sprintSpeed : movementSpeed;
        }


        /// <summary>
        /// Returns 'true' if the player presses the jump key;
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsJumpKeyPressed() {
            //If no character input script is attached to this object, return;
            if (CharacterInput == null)
                return false;

            return CharacterInput.IsJumpKeyPressed();
        }

        /// <summary>
        /// Determine current controller state based on current momentum and whether the controller is grounded (or not);
        /// Handle state transitions;
        /// </summary>
        /// <returns></returns>
        ControllerState DetermineControllerState() {
            //Check if vertical momentum is pointing upwards;
            bool isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), TR.up) > 0f);
            //Check if controller is sliding;
            bool isSliding = Mover.IsGrounded() && IsGroundTooSteep();

            //Grounded;
            if (_currentControllerState == ControllerState.Grounded) {
                if (isRising) {
                    OnGroundContactLost();
                    return ControllerState.Rising;
                }

                if (!Mover.IsGrounded()) {
                    OnGroundContactLost();
                    return ControllerState.Falling;
                }

                if (isSliding) {
                    OnGroundContactLost();
                    return ControllerState.Sliding;
                }

                return ControllerState.Grounded;
            }

            //Falling;
            if (_currentControllerState == ControllerState.Falling) {
                if (isRising) {
                    return ControllerState.Rising;
                }

                if (Mover.IsGrounded() && !isSliding) {
                    OnGroundContactRegained();
                    return ControllerState.Grounded;
                }

                if (isSliding) {
                    return ControllerState.Sliding;
                }

                return ControllerState.Falling;
            }

            //Sliding;
            if (_currentControllerState == ControllerState.Sliding) {
                if (isRising) {
                    OnGroundContactLost();
                    return ControllerState.Rising;
                }

                if (!Mover.IsGrounded()) {
                    OnGroundContactLost();
                    return ControllerState.Falling;
                }

                if (Mover.IsGrounded() && !isSliding) {
                    OnGroundContactRegained();
                    return ControllerState.Grounded;
                }

                return ControllerState.Sliding;
            }

            //Rising;
            if (_currentControllerState == ControllerState.Rising) {
                if (!isRising) {
                    if (Mover.IsGrounded() && !isSliding) {
                        OnGroundContactRegained();
                        return ControllerState.Grounded;
                    }

                    if (isSliding) {
                        return ControllerState.Sliding;
                    }

                    if (!Mover.IsGrounded()) {
                        return ControllerState.Falling;
                    }
                }

                //If a ceiling detector has been attached to this gameobject, check for ceiling hits;
                if (CeilingDetector != null) {
                    if (CeilingDetector.HitCeiling()) {
                        OnCeilingContact();
                        return ControllerState.Falling;
                    }
                }

                return ControllerState.Rising;
            }

            //Jumping;
            if (_currentControllerState == ControllerState.Jumping) {
                //Check for jump timeout;
                if ((Time.time - _currentJumpStartTime) > jumpDuration)
                    return ControllerState.Rising;

                //Check if jump key was let go;
                if (_jumpKeyWasLetGo)
                    return ControllerState.Rising;

                //If a ceiling detector has been attached to this gameobject, check for ceiling hits;
                if (CeilingDetector != null) {
                    if (CeilingDetector.HitCeiling()) {
                        OnCeilingContact();
                        return ControllerState.Falling;
                    }
                }

                return ControllerState.Jumping;
            }

            return ControllerState.Falling;
        }

        /// <summary>
        /// Check if player has initiated a jump;
        /// </summary>
        void HandleJumping() {
            if (_currentControllerState == ControllerState.Grounded) {
                if ((_jumpKeyIsPressed == true || _jumpKeyWasPressed) && !_jumpInputIsLocked) {
                    //Call events;
                    OnGroundContactLost();
                    OnJumpStart();

                    _currentControllerState = ControllerState.Jumping;
                }
            }
        }

        /// <summary>
        /// Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
        /// Handle movement in the air;
        /// Handle sliding down steep slopes;
        /// </summary>
        void HandleMomentum() {
            //If local momentum is used, transform momentum into world coordinates first;
            if (useLocalMomentum)
                Momentum = TR.localToWorldMatrix * Momentum;

            Vector3 verticalMomentum = Vector3.zero;
            Vector3 horizontalMomentum = Vector3.zero;

            //Split momentum into vertical and horizontal components;
            if (Momentum != Vector3.zero) {
                verticalMomentum = VectorMath.ExtractDotVector(Momentum, TR.up);
                horizontalMomentum = Momentum - verticalMomentum;
            }

            //Add gravity to vertical momentum;
            verticalMomentum -= TR.up * gravity * Time.deltaTime;

            //Remove any downward force if the controller is grounded;
            if (_currentControllerState == ControllerState.Grounded && VectorMath.GetDotProduct(verticalMomentum, TR.up) < 0f)
                verticalMomentum = Vector3.zero;

            //Manipulate momentum to steer controller in the air (if controller is not grounded or sliding);
            if (!IsGrounded()) {
                Vector3 movementVelocity = CalculateMovementVelocity();

                //If controller has received additional momentum from somewhere else;
                if (horizontalMomentum.magnitude > MovementSpeed()) {
                    //Prevent unwanted accumulation of speed in the direction of the current momentum;
                    if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0f)
                        movementVelocity = VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);

                    //Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller;
                    float airControlMultiplier = 0.25f;
                    horizontalMomentum += movementVelocity * Time.deltaTime * airControlRate * airControlMultiplier;
                }
                //If controller has not received additional momentum;
                else {
                    //Clamp _horizontal velocity to prevent accumulation of speed;
                    horizontalMomentum += movementVelocity * Time.deltaTime * airControlRate;
                    horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, MovementSpeed());
                }
            }

            //Steer controller on slopes;
            if (_currentControllerState == ControllerState.Sliding) {
                //Calculate vector pointing away from slope;
                Vector3 pointDownVector = Vector3.ProjectOnPlane(Mover.GetGroundNormal(), TR.up).normalized;

                //Calculate movement velocity;
                Vector3 slopeMovementVelocity = CalculateMovementVelocity();
                //Remove all velocity that is pointing up the slope;
                slopeMovementVelocity = VectorMath.RemoveDotVector(slopeMovementVelocity, pointDownVector);

                //Add movement velocity to momentum;
                horizontalMomentum += slopeMovementVelocity * Time.fixedDeltaTime;
            }

            //Apply friction to horizontal momentum based on whether the controller is grounded;
            if (_currentControllerState == ControllerState.Grounded)
                horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(horizontalMomentum, groundFriction, Time.deltaTime, Vector3.zero);
            else
                horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(horizontalMomentum, airFriction, Time.deltaTime, Vector3.zero);

            //Add horizontal and vertical momentum back together;
            Momentum = horizontalMomentum + verticalMomentum;

            //Additional momentum calculations for sliding;
            if (_currentControllerState == ControllerState.Sliding) {
                //Project the current momentum onto the current ground normal if the controller is sliding down a slope;
                Momentum = Vector3.ProjectOnPlane(Momentum, Mover.GetGroundNormal());

                //Remove any upwards momentum when sliding;
                if (VectorMath.GetDotProduct(Momentum, TR.up) > 0f)
                    Momentum = VectorMath.RemoveDotVector(Momentum, TR.up);

                //Apply additional slide gravity;
                Vector3 slideDirection = Vector3.ProjectOnPlane(-TR.up, Mover.GetGroundNormal()).normalized;
                Momentum += slideDirection * slideGravity * Time.deltaTime;
            }

            //If controller is jumping, override vertical velocity with jumpSpeed;
            if (_currentControllerState == ControllerState.Jumping) {
                Momentum = VectorMath.RemoveDotVector(Momentum, TR.up);
                Momentum += TR.up * jumpSpeed;
            }

            if (useLocalMomentum)
                Momentum = TR.worldToLocalMatrix * Momentum;
        }

        //Events;

        /// <summary>
        /// This function is called when the player has initiated a jump;
        /// </summary>
        void OnJumpStart() {
            //If local momentum is used, transform momentum into world coordinates first;
            if (useLocalMomentum)
                Momentum = TR.localToWorldMatrix * Momentum;

            //Add jump force to momentum;
            Momentum += TR.up * jumpSpeed;

            //Set jump start time;
            _currentJumpStartTime = Time.time;

            //Lock jump input until jump key is released again;
            _jumpInputIsLocked = true;

            //Call event;
            if (OnJump != null)
                OnJump(Momentum);

            if (useLocalMomentum)
                Momentum = TR.worldToLocalMatrix * Momentum;
        }

        /// <summary>
        /// This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
        /// </summary>
        void OnGroundContactLost() {
            //If local momentum is used, transform momentum into world coordinates first;
            if (useLocalMomentum)
                Momentum = TR.localToWorldMatrix * Momentum;

            //Get current movement velocity;
            Vector3 velocity = GetMovementVelocity();

            //Check if the controller has both momentum and a current movement velocity;
            if (velocity.sqrMagnitude >= 0f && Momentum.sqrMagnitude > 0f) {
                //Project momentum onto movement direction;
                Vector3 projectedMomentum = Vector3.Project(Momentum, velocity.normalized);
                //Calculate dot product to determine whether momentum and movement are aligned;
                float dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);

                //If current momentum is already pointing in the same direction as movement velocity,
                //Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation;
                if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f)
                    velocity = Vector3.zero;
                else if (dot > 0f)
                    velocity -= projectedMomentum;
            }

            //Add movement velocity to momentum;
            Momentum += velocity;

            if (useLocalMomentum)
                Momentum = TR.worldToLocalMatrix * Momentum;
        }

        /// <summary>
        /// This function is called when the controller has landed on a surface after being in the air;
        /// </summary>
        void OnGroundContactRegained() {
            //Call 'OnLand' event;
            if (OnLand != null) {
                Vector3 collisionVelocity = Momentum;
                //If local momentum is used, transform momentum into world coordinates first;
                if (useLocalMomentum)
                    collisionVelocity = TR.localToWorldMatrix * collisionVelocity;

                OnLand(collisionVelocity);
            }
        }

        /// <summary>
        /// This function is called when the controller has collided with a ceiling while jumping or moving upwards;
        /// </summary>
        void OnCeilingContact() {
            //If local momentum is used, transform momentum into world coordinates first;
            if (useLocalMomentum)
                Momentum = TR.localToWorldMatrix * Momentum;

            //Remove all vertical parts of momentum;
            Momentum = VectorMath.RemoveDotVector(Momentum, TR.up);

            if (useLocalMomentum)
                Momentum = TR.worldToLocalMatrix * Momentum;
        }

        //Helper functions;

        /// <summary>
        /// Returns 'true' if vertical momentum is above a small threshold;
        /// </summary>
        /// <returns></returns>
        private bool IsRisingOrFalling() {
            //Calculate current vertical momentum;
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), TR.up);

            //Setup threshold to check against;
            //For most applications, a value of '0.001f' is recommended;
            float limit = 0.001f;

            //Return true if vertical momentum is above '_limit';
            return (verticalMomentum.magnitude > limit);
        }

        /// <summary>
        /// Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
        /// </summary>
        /// <returns></returns>
        private bool IsGroundTooSteep() {
            if (!Mover.IsGrounded())
                return true;

            return (Vector3.Angle(Mover.GetGroundNormal(), TR.up) > slopeLimit);
        }

        //Getters;

        /// <summary>
        /// Get last frame's velocity;
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetVelocity() {
            return _savedVelocity;
        }

        /// <summary>
        /// Get last frame's movement velocity (momentum is ignored);
        /// </summary>
        /// <returns></returns>
        public override Vector3 GetMovementVelocity() {
            return _savedMovementVelocity;
        }

        /// <summary>
        /// Get current momentum;
        /// </summary>
        /// <returns></returns>
        public Vector3 GetMomentum() {
            Vector3 worldMomentum = Momentum;
            if (useLocalMomentum)
                worldMomentum = TR.localToWorldMatrix * Momentum;

            return worldMomentum;
        }

        /// <summary>
        /// Returns 'true' if controller is grounded (or sliding down a slope);
        /// </summary>
        /// <returns></returns>
        public override bool IsGrounded() {
            return (_currentControllerState == ControllerState.Grounded || _currentControllerState == ControllerState.Sliding);
        }

        public bool IsSprinting() {
            return CharacterInput.IsSprintKeyPressed();
        }

        /// <summary>
        /// Returns 'true' if controller is sliding;
        /// </summary>
        /// <returns></returns>
        public bool IsSliding() {
            return (_currentControllerState == ControllerState.Sliding);
        }

        public bool IsInteract() {
            return CharacterInput.IsInteractionKeyPressed();
        }


        public CharacterInput GetCharacterInput() {
            return CharacterInput;
        }

        /// <summary>
        /// Add momentum to controller;
        /// </summary>
        /// <param name="momentum"></param>
        public void AddMomentum(Vector3 momentum) {
            if (useLocalMomentum)
                Momentum = TR.localToWorldMatrix * Momentum;

            Momentum += momentum;

            if (useLocalMomentum)
                Momentum = TR.worldToLocalMatrix * Momentum;
        }

        /// <summary>
        /// Set controller momentum directly;
        /// </summary>
        /// <param name="newMomentum"></param>
        public void SetMomentum(Vector3 newMomentum) {
            if (useLocalMomentum)
                Momentum = TR.worldToLocalMatrix * newMomentum;
            else
                Momentum = newMomentum;
        }
    }
}