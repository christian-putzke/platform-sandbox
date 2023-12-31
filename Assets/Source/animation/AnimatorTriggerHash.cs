﻿using UnityEngine;

namespace plasa.animation
{
	public static class AnimatorTriggerHash
	{
		public static readonly int TurnRight = Animator.StringToHash("TurnRight");
		public static readonly int TurnLeft = Animator.StringToHash("TurnLeft");
		public static readonly int RunningTurnRight = Animator.StringToHash("RunningTurnRight");
		public static readonly int RunningTurnLeft = Animator.StringToHash("RunningTurnLeft");
		public static readonly int VelocityX = Animator.StringToHash("VelocityX");
	}
}