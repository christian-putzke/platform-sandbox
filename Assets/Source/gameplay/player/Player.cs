using plasa.input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace plasa.gameplay.player
{
	[RequireComponent(typeof(Rigidbody))]
	public class Player : MonoBehaviour
	{
		[Header("Speed")]
		[SerializeField]
		private float _movementSpeed = 1f;

		[SerializeField]
		private float _jumpForce = 1f;


		[Header("Collision")]
		[SerializeField]
		private Collider _collider;


		[Header("Ground Check")]
		[SerializeField]
		private LayerMask _groundCheckLayer;

		[SerializeField]
		private Transform _groundCheckPosition;

		[SerializeField]
		private Vector3 _groundCheckSize;


		private Rigidbody _rigidbody;
		private InputActions _inputActions;
		private Draggable _draggable;

		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();

			_inputActions = new InputActions();
			_inputActions.Enable(); // be careful, this enables ALL control schemes (keyboard AND gamepad)
			_inputActions.Player.Jump.performed += Jump;

		}

		private void FixedUpdate()
		{
			debug.ui.WriteText($"velocity: {_rigidbody.velocity}", true);
			debug.ui.WriteText($"angularVelocity: {_rigidbody.angularVelocity}");

			if (_inputActions.Player.Move.IsInProgress()) {
				var direction = _inputActions.Player.Move.ReadValue<Vector2>();
				Move(direction);
			}

			if (_draggable != null && _inputActions.Player.Grab.IsInProgress()) {
				_draggable.Drag(_rigidbody.velocity);
			}
		}

		private void Move(Vector2 direction)
		{
			_rigidbody.AddForce(new Vector3(direction.x * _movementSpeed, 0f, 0f), ForceMode.Force);

			//_rigidbody.velocity = new Vector3(direction.x, _rigidbody.velocity.y, 0f) * _movementSpeed;
			//var currentPosition = transform.position;
			//currentPosition.x += direction.x * _movementSpeed * Time.deltaTime;

			//_rigidbody.MovePosition(currentPosition);
			//_rigidbody.AddForce(new Vector3(direction.x, 0f, 0f) * _movementSpeed, ForceMode.Force);
		}

		public bool IsGrounded()
		{
			var result = Physics.OverlapBox(_groundCheckPosition.position, _groundCheckSize / 2, Quaternion.identity, _groundCheckLayer);

			return result.Length > 0;
			//return _rigidbody.velocity.y >= 0.01f || _rigidbody.velocity.y <= -0.01f;
		}

		public void Jump(InputAction.CallbackContext context)
		{
			if (!context.performed || !IsGrounded()) {
				return;
			}

			_rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out _draggable)) {
				Debug.Log($"OnTriggerEnter drag enabled: {other.gameObject.name}");
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.TryGetComponent<Draggable>(out _)) {
				_draggable = null;
				Debug.Log($"OnTriggerExit drag disabled: {other.gameObject.name}");
			}
		}

		private void OnDrawGizmos()
		{
			// Show ground check overlap box
			Gizmos.color = Color.red;
			Gizmos.DrawCube(_groundCheckPosition.position, _groundCheckSize);
		}
	}
}
