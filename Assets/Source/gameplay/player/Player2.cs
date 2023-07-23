using plasa.input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace plasa.gameplay.player
{
	public class Player2 : MonoBehaviour
	{
		[SerializeField]
		private float _movementSpeed = 1f;

		[SerializeField]
		private float _jumpForce = 1f;

		[SerializeField]
		private Collider _collider;

		[SerializeField]
		private LayerMask _groundLayer;

		[SerializeField]
		private float _groundedCastOffeset = 0.01f;

		[Header("Gravity")]
		[SerializeField]
		private float _minFallSpeed = 8f;

		[SerializeField]
		private float _maxFallSpeed = 15f;

		[SerializeField]
		private float _timeMaxFallSpeed = 0.5f;

		private InputActions _inputActions;

		private Vector3 _groundCastPosition;
		private Vector3 _hitPosition;
		private float _currentVerticalSpeed;

		private bool _isGrounded = false;
		private float _fallSpeedLerpTime;
		private Vector3 _velocity;
		private Vector3 _lastPosition;

		private void Awake()
		{
			_inputActions = new InputActions();
			_inputActions.Enable(); // be careful, this enables ALL control schemes (keyboard AND gamepad)
			_inputActions.Player.Jump.performed += Jump;
		}

		private void Update()
		{
			_velocity = (transform.position - _lastPosition) / Time.deltaTime;
			_lastPosition = transform.position;

			Debug.Log($"_velocity: {_velocity}");

			var currentPosition = transform.position;
			_groundCastPosition = currentPosition;
			_groundCastPosition.y -= (_collider.bounds.extents.y - _groundedCastOffeset);

			if (Physics.Raycast(_groundCastPosition, Vector3.down, out var hitInfo, Mathf.Infinity, _groundLayer)) {
				_hitPosition = hitInfo.point;

				var distance = hitInfo.distance - _groundedCastOffeset;

				if (_currentVerticalSpeed <= 0f && distance <= 0.01f) {
					_isGrounded = true;
					_currentVerticalSpeed = 0;
					_fallSpeedLerpTime = 0;
					var groundedYPosition = transform.position;
					groundedYPosition.y = _hitPosition.y + _collider.bounds.extents.y;
					transform.position = groundedYPosition;
				} else {
					_fallSpeedLerpTime += Time.deltaTime * _timeMaxFallSpeed;
					_fallSpeedLerpTime = Mathf.Clamp(_fallSpeedLerpTime, 0f, 1f);

					var fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, _fallSpeedLerpTime);
					_currentVerticalSpeed -= fallSpeed * Time.deltaTime;
					_isGrounded = false;
				}
			} else {
				_currentVerticalSpeed = 0;
				_fallSpeedLerpTime = 0;
				_isGrounded = true;
			}

			if (!_isGrounded) {
				var newPosition = transform.position;
				newPosition.y += _currentVerticalSpeed * Time.deltaTime;
				transform.position = newPosition;
			}

			if (_inputActions.Player.Move.IsInProgress()) {
				var direction = _inputActions.Player.Move.ReadValue<Vector2>();
				Move(direction);
			}
		}

		private void Move(Vector2 direction)
		{
			var currentPosition = transform.position;
			var positionX = currentPosition.x + (direction.x * _movementSpeed * Time.deltaTime);
			var newPosition = new Vector3(positionX, currentPosition.y, currentPosition.z);

			transform.position = newPosition;
		}


		public void Jump(InputAction.CallbackContext context)
		{
			if (!context.performed || !_isGrounded) {
				return;
			}

			_currentVerticalSpeed = _jumpForce;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(_groundCastPosition, _hitPosition);
		}
	}
}
