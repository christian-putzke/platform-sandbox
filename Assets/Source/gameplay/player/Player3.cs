using plasa.input;
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
		private Transform _characerVisuals;

		[SerializeField]
		private CharacterController _characterController;

		[SerializeField]
		private Animator _animator;

		[Header("Horizontal Speed")]
		[SerializeField]
		private float _horizontalSpeed;

		[Range(0, 1)]
		[SerializeField]
		private float _horizontalSpeedReductionWhileJumping;

		[SerializeField]
		private float _horizontalSpeedReductionWhileGrounded;

		[SerializeField]
		private float _rotationSpeed;

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

		private bool _isViewingLeft;
		private float _rotationTime;
		private Vector3 _rotationStart;
		private Vector3? _rotationTarget = null;
		private InputActions _inputActions;
		private Vector3 _speed = Vector3.zero;
		private Vector3 _speedWhenJumpStarted;

		private int _animatorHorizontalMovement;


		private void Awake()
		{
			_animatorHorizontalMovement = Animator.StringToHash("HorizontalMovement");

			_characterController = GetComponent<CharacterController>();

			_inputActions = new InputActions();
			_inputActions.Enable(); // be careful, this enables ALL control schemes (keyboard AND gamepad)
		}

		public bool IsGrounded()
		{
			var result = Physics.OverlapBox(_groundCheckPosition.position, _groundCheckSize / 2, Quaternion.identity, _groundCheckLayer);
			return result.Length > 0;
		}

		private void Update()
		{
			var isGrounded = IsGrounded();

			debug.ui.WriteText($"_speed: {_speed}", true);
			debug.ui.WriteText($"_speedWhenJumpStarted: {_speedWhenJumpStarted}");
			debug.ui.WriteText($"isGrounded: {isGrounded}");

			if (isGrounded && _inputActions.Player.Jump.WasPerformedThisFrame()) {
				_speedWhenJumpStarted = _speed;
				_speed.y = 5f;
			} else if (!isGrounded) {
				_speed.y -= 0.01f;
			} else if (_speed.y <= 0.01f && isGrounded) {
				_speed.y = 0f;
			}

			if (isGrounded) {
				_speed = HorizontalMovementWhileGrounded(_speed);
			} else {
				_speed = HorizontalMovementWhileJumping(_speed);
			}

			Move(_speed);
			AnimationTest();
		}

		private void AnimationTest()
		{
			if (!_rotationTarget.HasValue) {
				return;
			}

			var targetRotation = Vector3.Lerp(_rotationStart, _rotationTarget.Value, _rotationTime);
			_characerVisuals.transform.rotation = Quaternion.Euler(targetRotation);

			if (_rotationTime >= 1f) {
				_rotationTarget = null;
			}

			_rotationTime += _rotationSpeed * Time.deltaTime;
		}

		private Vector3 HorizontalMovementWhileGrounded(Vector3 speed)
		{
			if (_inputActions.Player.Move.IsInProgress()) {
				var direction = _inputActions.Player.Move.ReadValue<Vector2>();

				if (_isViewingLeft && direction.x > 0f) {
					_isViewingLeft = false;
					_rotationTime = 0;
					_rotationStart = _characerVisuals.transform.rotation.eulerAngles;
					_rotationTarget = new Vector3(_rotationStart.x, 90, _rotationStart.z);
					_animator.SetTrigger("TurnRight");
				} else if (!_isViewingLeft && direction.x < 0f) {
					_isViewingLeft = true;
					_rotationTime = 0;
					_rotationStart = _characerVisuals.transform.rotation.eulerAngles;
					_rotationTarget = new Vector3(_rotationStart.x, 270, _rotationStart.z);
					_animator.SetTrigger("TurnLeft");
				} else if (_rotationTarget == null) {
					speed.x = direction.x * _horizontalSpeed;
				}
			} else {
				// TODO: Improve by adding lerp
				if (speed.x > 0f) {
					speed.x = Mathf.Max(_speed.x - _horizontalSpeedReductionWhileGrounded, 0f);
				} else {
					speed.x = Mathf.Min(_speed.x + _horizontalSpeedReductionWhileGrounded, 0f);
				}
			}

			debug.ui.WriteText($"speed: {speed}");

			return speed;
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
