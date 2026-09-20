using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class HitZoneReaction
{
	public DamageSourceType sourceType;

	public HitReactionType reactionType;

	public float damageMultiplier = 1f;

	[Header("Effects")]
	public List<HitEffectConfig> effects = new List<HitEffectConfig>();

	public HitResult Execute(AttackContext context, Vector3 hitPosition)
	{
		return reactionType switch
		{
			HitReactionType.Block => HitResult.Blocked(), 
			HitReactionType.Absorb => HitResult.Absorbed(), 
			HitReactionType.Deflect => HitResult.Deflected(), 
			HitReactionType.Pass => HitResult.Pass(damageMultiplier), 
			HitReactionType.MarkZone => HitResult.ZoneMarked(), 
			_ => HitResult.Pass(), 
		};
	}

	public void PlayEffect(HitEffectConfig effectConfig, Vector3 position)
	{
		if (!string.IsNullOrEmpty(effectConfig.fxName))
		{
			WorldFX.Spawn(position, effectConfig.fxName);
		}
		if (!string.IsNullOrEmpty(effectConfig.soundId))
		{
			LazyAudio.PlayAtPos(effectConfig.soundId, position);
		}
	}
}
