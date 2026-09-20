using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "GK2/Player Physics Config", fileName = "PlayerPhysicsConfig")]
public class PlayerPhysicsConfig : ScriptableObject
{
	public float speed = 5f;

	public float jumpForce = 100f;

	public float groundRaycastLength = 0.5f;

	public float gravityFallScale = 1f;

	public float maxGroundAngle = 60f;

	public float contactForceMult = 1f;

	public float stickForceMult = 10f;

	[Range(0f, 10f)]
	public float slopeGravityMovementSlowdown = 1f;

	[Space]
	[Header("Attack Dash Settings")]
	public float attackDashForce = 15f;

	public float attackDashDuration = 0.2f;

	public float attackDashCooldown = 0.5f;
}
