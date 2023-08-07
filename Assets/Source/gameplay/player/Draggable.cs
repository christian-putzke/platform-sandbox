using UnityEngine;

namespace plasa.gameplay.player
{
	public class Draggable : MonoBehaviour
	{
		[SerializeField]
		private float _dragForce = 1f;

		[SerializeField]
		private Rigidbody _rigidbody;

		public void Drag(Vector2 direction)
		{
			direction.Normalize();
			var dragForce = new Vector3(direction.x * _dragForce, 0f, 0f);
			Debug.Log(dragForce);
			//_rigidbody.AddForce(, ForceMode.Force);
			_rigidbody.AddForceAtPosition(dragForce, _rigidbody.transform.position, ForceMode.VelocityChange);
		}
	}
}
