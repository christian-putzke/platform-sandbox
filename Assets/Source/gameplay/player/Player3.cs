using plasa.input;
using plasa.animation;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace plasa.gameplay.player
{
	[RequireComponent(typeof(CharacterController))]
	public class Player3 : MonoBehaviour
	{
		[SerializeField]
		private Player3Visuals _visuals;

		[SerializeField]
		private CharacterController _characterController;

		[Header("Horizontal Speed")]
		[SerializeField]
		private float _horizontalSpeed;

		[Range(0, 1)]
		[SerializeField]
		private float _horizontalSpeedReductionWhileJumping;

		[SerializeField]
		private float _horizontalSpeedReductionWhileGrounded;


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
		private Vector3 _speedWhenJumpStarted;

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

			debug.ui.WriteText($"_speed: {_velocity}", true);
			debug.ui.WriteText($"_speedWhenJumpStarted: {_speedWhenJumpStarted}");
			debug.ui.WriteText($"isGrounded: {isGrounded}");

			if (isGrounded && _inputActions.Player.Jump.WasPerformedThisFrame()) {
				_speedWhenJumpStarted = _velocity;
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

			Move(_velocity);
			_visuals.Animate();
		}

		private Vector3 HorizontalMovementWhileGrounded(Vector3 velocity)
		{
			if (_inputActions.Player.Move.IsInProgress()) {
				var direction = _inputActions.Player.Move.ReadValue<Vector2>();
				var velocityX = direction.x * _horizontalSpeed;

				_visuals.StartRotationIfNeeded(velocityX);

				if (!_visuals.IsRotating) {
					velocity.x = velocityX;
				}
			} else {
				// TODO: Improve by adding lerp
				if (velocity.x > 0f) {
					velocity.x = Mathf.Max(_velocity.x - _horizontalSpeedReductionWhileGrounded, 0f);
				} else {
					velocity.x = Mathf.Min(_velocity.x + _horizontalSpeedReductionWhileGrounded, 0f);
				}
			}

			debug.ui.WriteText($"velocity: {velocity}");

			return velocity;
		}

		private Vector3 HorizontalMovementWhileJumping(Vector3 speed)
		{
			if (_inputActions.Player.Move.IsInProgress()) {
				var direction = _inputActions.Player.Move.ReadValue<Vector2>();

				if (direction.x > 0f && speed.x < 0f || direction.x < 0f && speed.x > 0f) {
					speed.x = _speedWhenJumpStarted.x * _horizontalSpeedReductionWhileJumping;
				}
			}

			return speed;
		}

		private void Move(Vector3 speed)
		{
			var movement = new Vector3(speed.x * Time.deltaTime, speed.y * Time.deltaTime, 0f);
			debug.ui.WriteText($"movement: {movement}");
			_characterController.Move(movement);
		}

		private void OnDrawGizmos()
		{
			// Show ground check overlap box
			Gizmos.color = Color.red;
			Gizmos.DrawCube(_groundCheckPosition.position, _groundCheckSize);
		}
	}
}
