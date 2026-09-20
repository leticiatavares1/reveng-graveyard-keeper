using System.Collections.Generic;
using UnityEngine;

public class AOEDamageSource : MonoBehaviour
{
	public enum ColliderType
	{
		Box,
		Sphere,
		Capsule
	}

	public Collider col;

	public ColliderType colliderType;

	public LazyConsts.Fighting.TeamType damagingTeamType;

	public int damage;

	private Collider[] colliders = new Collider[50];

	private void OnEnable()
	{
		Activate();
	}

	private void Activate()
	{
		HashSet<ICombatEntity> hashSet = new HashSet<ICombatEntity>();
		int num = colliderType switch
		{
			ColliderType.Box => Physics.OverlapBoxNonAlloc(col.bounds.center, col.bounds.size, colliders, col.transform.rotation, -1, QueryTriggerInteraction.Ignore), 
			ColliderType.Sphere => Physics.OverlapSphereNonAlloc(col.bounds.center, Mathf.Max(col.bounds.extents.x, col.bounds.extents.y, col.bounds.extents.z), colliders, -1, QueryTriggerInteraction.Ignore), 
			ColliderType.Capsule => GetCapsuleOverlapCount(), 
			_ => 0, 
		};
		for (int i = 0; i < num; i++)
		{
			ICombatEntity componentInParent = colliders[i].GetComponentInParent<ICombatEntity>();
			if (componentInParent != null && componentInParent.IsActiveCombatant && componentInParent.TeamType == damagingTeamType && !hashSet.Contains(componentInParent))
			{
				Debug.Log($"Applying damage to {componentInParent.CombatEntityUID}, damage: {damage}");
				componentInParent.CombatEntityHpComponent.ApplyDamage(damage);
				hashSet.Add(componentInParent);
			}
		}
	}

	private int GetCapsuleOverlapCount()
	{
		CapsuleCollider capsuleCollider = (CapsuleCollider)col;
		Vector3 vector = capsuleCollider.direction switch
		{
			0 => Vector3.right, 
			1 => Vector3.up, 
			_ => Vector3.forward, 
		};
		float num = capsuleCollider.height * 0.5f - capsuleCollider.radius;
		Vector3 point = col.transform.TransformPoint(capsuleCollider.center - vector * num);
		Vector3 point2 = col.transform.TransformPoint(capsuleCollider.center + vector * num);
		Vector3 lossyScale = col.transform.lossyScale;
		float num2 = capsuleCollider.direction switch
		{
			0 => Mathf.Max(lossyScale.y, lossyScale.z), 
			1 => Mathf.Max(lossyScale.x, lossyScale.z), 
			_ => Mathf.Max(lossyScale.x, lossyScale.y), 
		};
		return Physics.OverlapCapsuleNonAlloc(point, point2, capsuleCollider.radius * num2, colliders, -1, QueryTriggerInteraction.Ignore);
	}
}
