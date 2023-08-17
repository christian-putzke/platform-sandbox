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
		private float _rotationSpeed2;

		[SerializeField]
		private float _transformRotationLookLeft;

		[SerializeField]
		private float _transformRotationLookRight;

		[SerializeField]
		private bool _isViewDirectionLeft;


		private float _rotationTime;
		private Vector3? _rotationStart = null;
		private Vector3? _rotationTarget = null;

		public bool IsRotating => _rotationStart != null;

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


		public void Animate(Vector3 velocity, float maxVelocityX)
		{
			UpdateVelocity(velocity, maxVelocityX);

			if (_rotationTarget.HasValue && _rotationStart.HasValue) {
				Rotate(_rotationTarget.Value, _rotationStart.Value);
			} else if (_rotationTarget.HasValue) {
				Rotate2(_rotationTarget.Value);
			}
		}

		// slow rotation
		private void Rotate(Vector3 rotationTarget, Vector3 rotationStart)
		{
			var lerpedRotation = Vector3.Lerp(rotationStart, rotationTarget, _rotationTime);
			transform.rotation = Quaternion.Euler(lerpedRotation);

			if (_rotationTime >= 1f) {
				_rotationTarget = null;
				_rotationStart = null;
			}

			_rotationTime += _rotationSpeed * Time.deltaTime;
		}

		// run rotation
		private void Rotate2(Vector3 rotationTarget)
		{
			if (_rotationTime >= 1f) {
				
				_rotationTarget = null;
				_rotationStart = null;
				transform.rotation = Quaternion.Euler(rotationTarget);
			}

			_rotationTime += _rotationSpeed2 * Time.deltaTime;
		}

		public void StartRotationIfNeeded(float directionX, float velocityX, float currentMaxVelcoityX)
		{
			var velocityPercentageX = Mathf.Abs(velocityX) / currentMaxVelcoityX;

			if (_isViewDirectionLeft && directionX > 0f) {
				if (velocityX >= 0f) { // slow rotation right
					_isViewDirectionLeft = false;
					_rotationTime = 0;
					_rotationStart = transform.rotation.eulerAngles;
					_rotationTarget = new Vector3(_rotationStart.Value.x, _transformRotationLookRight, _rotationStart.Value.z);
					_animator.SetTrigger(AnimatorTriggerHash.TurnRight);
				} else if (velocityPercentageX > 0.6) { // run rotation right
					_isViewDirectionLeft = false;
					_rotationTime = 0;
					_rotationStart = null;
					var rotationStart = transform.rotation.eulerAngles;
					_rotationTarget = new Vector3(rotationStart.x, _transformRotationLookRight, rotationStart.z);
					_animator.SetTrigger(AnimatorTriggerHash.RunningTurnRight);
				}
			} else if (!_isViewDirectionLeft && directionX < 0f) {
				if (velocityX <= 0f) { // slow rotation left
					_isViewDirectionLeft = true;
					_rotationTime = 0;
					_rotationStart = transform.rotation.eulerAngles;
					_rotationTarget = new Vector3(_rotationStart.Value.x, _transformRotationLookLeft, _rotationStart.Value.z);
					_animator.SetTrigger(AnimatorTriggerHash.TurnLeft);
				} else if (velocityPercentageX > 0.6) { // run rotation left
					_isViewDirectionLeft = true;
					_rotationTime = 0;
					_rotationStart = null;
					var rotationStart = transform.rotation.eulerAngles;
					_rotationTarget = new Vector3(rotationStart.x, _transformRotationLookLeft, rotationStart.z);
					_animator.SetTrigger(AnimatorTriggerHash.RunningTurnLeft);
				}
			}
		}

		private void UpdateVelocity(Vector3 velocity, float maxVelocityX)
		{
			var velocityPercentageX = Mathf.Abs(velocity.x) / maxVelocityX;
			_animator.SetFloat(AnimatorTriggerHash.VelocityX, velocityPercentageX);
		}
	}
}