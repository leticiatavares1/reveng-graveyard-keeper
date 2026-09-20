using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileStats", menuName = "GK2/Fighting/ProjectileStats")]
public class ProjectileStats : ScriptableObject
{
	public float speed = 40f;

	public float maxLifeTime = 8f;

	[Tooltip("World FX spawned by the projectile at the hit point when damage is dealt.")]
	public string onDamageHitFxName;

	public DamageEffectSettings damageEffectOverride;
}
