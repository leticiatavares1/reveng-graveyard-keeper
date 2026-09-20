using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProjectileColliderController : MonoBehaviour
{
	public ProjectileObject father;

	private List<Collider2D> _skip_colliders = new List<Collider2D>();

	public void AddSkipColliders(List<Collider2D> colliders_for_skip)
	{
		if (colliders_for_skip != null && colliders_for_skip.Count != 0)
		{
			if (_skip_colliders == null)
			{
				_skip_colliders = new List<Collider2D>();
			}
			_skip_colliders.AddRange(colliders_for_skip);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (_skip_colliders.Contains(other))
		{
			return;
		}
		GameObject gameObject = other.gameObject;
		if (gameObject.layer != 0 && gameObject.layer != 9)
		{
			return;
		}
		WorldGameObject componentInParent = other.GetComponentInParent<WorldGameObject>();
		if (componentInParent != null)
		{
			if (father.father != null && componentInParent.unique_id == father.father.unique_id)
			{
				_skip_colliders.Add(other);
			}
			else if (other.isTrigger)
			{
				Debug.Log("Projectile " + father.name + "[" + father.id + "] is hit to trigger " + componentInParent.name + "[" + componentInParent.obj_id + "]. Doing nothing.", componentInParent);
			}
			else if (!father.definition.can_damage_mobs && componentInParent.obj_def.IsMob())
			{
				Debug.Log("Projectile " + father.name + "[" + father.id + "] is hit to mob " + componentInParent.name + "[" + componentInParent.obj_id + "]. Can not attack mobs => doing nothing", componentInParent);
			}
			else if (componentInParent.components.combat.enabled)
			{
				Debug.Log("Projectile " + father.name + "[" + father.id + "] is hit to " + componentInParent.name + "[" + componentInParent.obj_id + "] combat component.", componentInParent);
				father.OnHitCombat(componentInParent.components.combat);
				_skip_colliders.Add(other);
			}
			else if (componentInParent.components.hp.enabled)
			{
				Debug.Log("Projectile " + father.name + "[" + father.id + "] is hit to " + componentInParent.name + "[" + componentInParent.obj_id + "] hp component.", componentInParent);
				father.OnHitNonCombat(componentInParent);
			}
			else
			{
				Debug.Log("Projectile " + father.name + "[" + father.id + "] is hit to just WGO " + componentInParent.name + "[" + componentInParent.obj_id + "].", componentInParent);
				father.OnHitNonCombat(componentInParent);
			}
			return;
		}
		WorldSimpleObject worldSimpleObject = other.GetComponent<WorldSimpleObject>();
		if (worldSimpleObject == null)
		{
			worldSimpleObject = other.GetComponentInParent<WorldSimpleObject>();
		}
		if (worldSimpleObject != null)
		{
			if (other.isTrigger)
			{
				Debug.Log("Projectile " + father.name + "[" + father.id + "] is hit to trigger wso " + worldSimpleObject.name + ".Doing nothing.", worldSimpleObject);
			}
			else
			{
				Debug.Log("Projectile " + father.name + "[" + father.id + "] is hit to WSO. Destroying.", worldSimpleObject);
				father.OnHitNonCombat(null);
			}
		}
		else
		{
			ProjectileColliderController component = other.GetComponent<ProjectileColliderController>();
			if (component != null && component.father != null && component.father.id == father.id)
			{
				_skip_colliders.Add(other);
				return;
			}
			Debug.Log("Projectile " + father.name + "[" + father.id + "] is hit to unknown collider. Destroying.", other);
			father.OnHitNonCombat(null);
		}
	}
}
