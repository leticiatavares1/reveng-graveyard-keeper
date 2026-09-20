using System;
using UnityEngine;

public class ProjectileObjectPart : MonoBehaviour
{
	public const string PATH = "Projectiles/";

	public Collider2D attack_collider;

	public ProjectileColliderController collider_controller;

	public Animator animator;

	public Action on_start;

	public Action on_hit_combat;

	public Action on_hit_non_combat;

	public Action on_out_of_screen;

	public Action on_max_dist_reached;

	public void Init()
	{
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogError("Not found any collider in projectile \"" + base.name + "\"!");
		}
		else
		{
			Collider2D[] array = componentsInChildren;
			foreach (Collider2D collider2D in array)
			{
				if (collider2D.gameObject.layer == 0 && collider2D.isTrigger)
				{
					attack_collider = collider2D;
					break;
				}
			}
		}
		if (attack_collider == null)
		{
			Debug.LogError("Not found attack collider for projectile \"" + base.name + "\"!");
		}
		collider_controller = GetComponentInChildren<ProjectileColliderController>();
		if (collider_controller == null)
		{
			Debug.LogError("Collider controller not found for projectile \"" + base.name + "\"!");
		}
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			Debug.LogError("Not found animator for projectile \"" + base.name + "\"!");
		}
	}

	public static ProjectileObjectPart Load(string filename)
	{
		ProjectileObjectPart projectileObjectPart = Resources.Load<ProjectileObjectPart>("Projectiles/" + filename);
		if (projectileObjectPart == null)
		{
			Debug.LogError("Error loading: Projectiles/" + filename);
		}
		return projectileObjectPart;
	}
}
