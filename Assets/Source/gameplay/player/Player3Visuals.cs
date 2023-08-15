using plasa.animation;
using UnityEngine;

namespace plasa.gameplay.player
{
	public class Player3Visuals : MonoBehaviour
	{
		[SerializeField]
		private Animator _animator;

		[Header("View Direction")]
		[SerializeField]
		private float _rotationSpeed;

		[SerializeField]
		private float _transformRotationLookLeft;

		[SerializeField]
		private float _transformRotationLookRight;

		[SerializeField]
		private bool _isViewDirectionLeft;


		private float _rotationTime;
		private Vector3 _rotationStart;
		private Vector3? _rotationTarget = null;

		public bool IsRotating => _rotationTarget != null;

		public void Setup()
		{
			float characterRotationY;
			if (_isViewDirectionLeft) {
				characterRotationY = _transformRotationLookLeft;
			} else {
				characterRotationY = _transformRotationLookRight;
			}

			var characterRotation = transform.eulerAngles;
			transform.rotation = Quaternion.Euler(characterRotation.x, characterRotationY, characterRotation.z);
		}


		public void Animate(Vector3 velocity, Vector3 maxVelocity)
		{
			UpdateVelocity(velocity, maxVelocity);

			if (_rotationTarget.HasValue) {
				Rotate(_rotationTarget.Value);
			}
		}

		private void Rotate(Vector3 rotationTarget)
		{
			var lerpedRotation = Vector3.Lerp(_rotationStart, rotationTarget, _rotationTime);
			transform.rotation = Quaternion.Euler(lerpedRotation);

			if (_rotationTime >= 1f) {
				_rotationTarget = null;
			}

			_rotationTime += _rotationSpeed * Time.deltaTime;
		}

		public void StartRotationIfNeeded(float directionX, float velocityX)
		{
			if (_isViewDirectionLeft && directionX > 0f && velocityX >= 0f) {
				_isViewDirectionLeft = false;
				_rotationTime = 0;
				_rotationStart = transform.rotation.eulerAngles;
				_rotationTarget = new Vector3(_rotationStart.x, _transformRotationLookRight, _rotationStart.z);
				_animator.SetTrigger(AnimatorTriggerHash.TurnRight);
			} else if (!_isViewDirectionLeft && directionX < 0f && velocityX <= 0f) {
				_isViewDirectionLeft = true;
				_rotationTime = 0;
				_rotationStart = transform.rotation.eulerAngles;
				_rotationTarget = new Vector3(_rotationStart.x, _transformRotationLookLeft, _rotationStart.z);
				_animator.SetTrigger(AnimatorTriggerHash.TurnLeft);
			}
		}

		private void UpdateVelocity(Vector3 velocity, Vector3 maxVelocity)
		{
			var velocityPercentageX = Mathf.Abs(velocity.x) / maxVelocity.x;
			_animator.SetFloat(AnimatorTriggerHash.VelocityX, velocityPercentageX);
		}
	}
}