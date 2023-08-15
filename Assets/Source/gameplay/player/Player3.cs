using System;
using plasa.input;
using UnityEngine;

namespace plasa.gameplay.player
{
	[RequireComponent(typeof(CharacterController))]
	public class Player3 : MonoBehaviour
	{
		[SerializeField]
		private Player3Visuals _visuals;

		[SerializeField]
		private CharacterController _characterController;

		[Header("Horizontal Velocity")]
		[SerializeField]
		private float _horizontalWalkingMaxVelocityWhileGrounded;

		[SerializeField]
		private float _horizontalRunningMaxVelocityWhileGrounded;

		[SerializeField]
		private float _horizontalDecelerationWhileGrounded;

		[SerializeField]
		private float _horizontalAccelerationWhileGrounded;

		[SerializeField]
		private float _horizontalCounterAccelerationWhileGrounded;

		[Range(0, 1)]
		[SerializeField]
		private float _horizontalSpeedReductionWhileJumping;


		[Header("Vertical Speed")]
		[SerializeField]
		private float _jumpForce = 1f;

		[Header("Gravity")]
		[SerializeField]
		private float _minFallSpeed = 8f;

		[SerializeField]
		private float _maxFallSpeed = 15f;

		[SerializeField]
		private float _timeMaxFallSpeed = 0.5f;

		[Header("Ground Check")]
		[SerializeField]
		private LayerMask _groundCheckLayer;

		[SerializeField]
		private Transform _groundCheckPosition;

		[SerializeField]
		private Vector3 _groundCheckSize;

		private InputActions _inputActions;
		private Vector3 _velocity = Vector3.zero;
		private Vector3 _velocityWhenJumpStarted;

		private void Awake()
		{
			_characterController = GetComponent<CharacterController>();

			_inputActions = new InputActions();
			_inputActions.Enable(); // be careful, this enables ALL control schemes (keyboard AND gamepad)

			_visuals.Setup();
		}

		public bool IsGrounded()
		{
			var result = Physics.OverlapBox(_groundCheckPosition.position, _groundCheckSize / 2, Quaternion.identity, _groundCheckLayer);
			return result.Length > 0;
		}

		private void Update()
		{
			var isGrounded = IsGrounded();

			debug.ui.WriteText($"_velocity: {_velocity}", true);
			//debug.ui.WriteText($"_velocityWhenJumpStarted: {_velocityWhenJumpStarted}");
			debug.ui.WriteText($"isGrounded: {isGrounded}");

			if (isGrounded && _inputActions.Player.Jump.WasPerformedThisFrame()) {
				_velocityWhenJumpStarted = _velocity;
				_velocity.y = 5f;
			} else if (!isGrounded) {
				_velocity.y -= 0.01f;
			} else if (_velocity.y <= 0.01f && isGrounded) {
				_velocity.y = 0f;
			}

			if (isGrounded) {
				_velocity = HorizontalMovementWhileGrounded(_velocity);
			} else {
				_velocity = HorizontalMovementWhileJumping(_velocity);
			}

			_velocity = SanitizeVelocity(_velocity);

			Move(_velocity);
			_visuals.Animate(_velocity, _horizontalRunningMaxVelocityWhileGrounded);
		}

		private Vector3 SanitizeVelocity(Vector3 velocity)
		{
			return new Vector3(
				Mathf.Clamp(velocity.x, _horizontalRunningMaxVelocityWhileGrounded * -1, _horizontalRunningMaxVelocityWhileGrounded),
				velocity.y,
				velocity.z
			);
		}

		private Vector3 HorizontalMovementWhileGrounded(Vector3 velocity)
		{
			var isMoveInProgress = _inputActions.Player.Move.IsInProgress();

			if (isMoveInProgress) {
				float velocityX;
				var direction = _inputActions.Player.Move.ReadValue<Vector2>();
				var currentMaxVelcoityX = GetCurrentMaxVelcoityX();

				_visuals.StartRotationIfNeeded(direction.x, velocity.x);

				var acceleration = _horizontalAccelerationWhileGrounded;
				if (velocity.x < 0 && direction.x > 0 || velocity.x > 0 && direction.x < 0) {
					acceleration = _horizontalCounterAccelerationWhileGrounded;
				}

				if (direction.x > 0f) {
					if (velocity.x > currentMaxVelcoityX) {
						velocityX = Mathf.Max(velocity.x - (_horizontalDecelerationWhileGrounded * Time.deltaTime), currentMaxVelcoityX);
					} else {
						velocityX = velocity.x + (acceleration * Time.deltaTime);
					}
				} else {
					if (velocity.x < currentMaxVelcoityX * -1) {
						velocityX = Mathf.Min(velocity.x + (_horizontalDecelerationWhileGrounded * Time.deltaTime), currentMaxVelcoityX * -1);
					} else {
						velocityX = velocity.x - (acceleration * Time.deltaTime);
					}
				}

				velocityX *= Mathf.Abs(direction.x);

				if (!_visuals.IsRotating) {
					velocity.x = velocityX;
				} else {
					// TODO: Do we need this?
					velocity.x = 0f;
				}
			} else {
				if (velocity.x > 0f) {
					velocity.x = Mathf.Max(velocity.x - (_horizontalDecelerationWhileGrounded * Time.deltaTime), 0f);
				} else {
					velocity.x = Mathf.Min(velocity.x + (_horizontalDecelerationWhileGrounded * Time.deltaTime), 0f);
				}
			}

			return velocity;
		}

		private float GetCurrentMaxVelcoityX()
		{
			if (_inputActions.Player.Run.IsInProgress()) {
				return _horizontalRunningMaxVelocityWhileGrounded;
			}

			return _horizontalWalkingMaxVelocityWhileGrounded;
		}

		private Vector3 HorizontalMovementWhileJumping(Vector3 velocity)
		{
			if (_inputActions.Player.Move.IsInProgress()) {
				var direction = _inputActions.Player.Move.ReadValue<Vector2>();

				if (direction.x > 0f && velocity.x < 0f || direction.x < 0f && velocity.x > 0f) {
					velocity.x = _velocityWhenJumpStarted.x * _horizontalSpeedReductionWhileJumping;
				}
			}

			return velocity;
		}

		private void Move(Vector3 velocity)
		{
			var motion = new Vector3(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime, 0f);
			_characterController.Move(motion);
		}

		private void OnDrawGizmos()
		{
			// Show ground check overlap box
			Gizmos.color = Color.red;
			Gizmos.DrawCube(_groundCheckPosition.position, _groundCheckSize);
		}
	}
}
