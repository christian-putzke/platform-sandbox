using plasa.input;
using UnityEngine;

namespace plasa.gameplay.player
{
	[RequireComponent(typeof(CharacterController))]
	public class Player3 : MonoBehaviour
	{
		[SerializeField]
		private CharacterController _characterController;
        
		[SerializeField]
		private float _movementSpeed;


        private InputActions _inputActions;

        private void Awake() {
			_characterController = GetComponent<CharacterController>();

			_inputActions = new InputActions();
			_inputActions.Enable(); // be careful, this enables ALL control schemes (keyboard AND gamepad)
			// _inputActions.Player.Jump.performed += Jump;
		}

		private void FixedUpdate()
		{
			if (_inputActions.Player.Move.IsInProgress()) {
				var direction = _inputActions.Player.Move.ReadValue<Vector2>();
				Move(direction);
			}
		}

        private void Move(Vector3 direction)
        {
			var movement = new Vector3(direction.x * _movementSpeed, 0f, 0f);
			_characterController.Move(movement);
        }
    }
}
