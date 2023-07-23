using plasa.input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace plasa.gameplay.player
{
	[RequireComponent(typeof(Rigidbody))]
	public class Player : MonoBehaviour
	{
		[SerializeField]
		private float _movementSpeed = 1f;

		[SerializeField]
		private float _jumpForce = 1f;

		[SerializeField]
		private Collider _collider;

		private Rigidbody _rigidbody;
		private InputActions _inputActions;

		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();

			_inputActions = new InputActions();
			_inputActions.Enable(); // be careful, this enables ALL control schemes (keyboard AND gamepad)
			_inputActions.Player.Jump.performed += Jump;

		}

		private void FixedUpdate()
		{
			if (_inputActions.Player.Move.IsInProgress()) {
				var direction = _inputActions.Player.Move.ReadValue<Vector2>();
				Move(direction);
			}
		}

		private void Move(Vector2 direction)
		{
			var currentPosition = transform.position;
			currentPosition.x += direction.x * _movementSpeed * Time.deltaTime;

			_rigidbody.MovePosition(currentPosition);
			//_rigidbody.AddForce(new Vector3(direction.x, 0f, 0f) * _movementSpeed, ForceMode.Force);
		}

		public void Jump(InputAction.CallbackContext context)
		{
			if (!context.performed) {
				return;
			}

			_rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
		}
	}
}
