using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class HitZone : MonoBehaviour
{
	[SerializeField]
	private HitZoneType zoneType;

	[SerializeField]
	[Tooltip("Optional shared reaction presets. Looked up in order; first matching sourceType wins.")]
	private List<HitZoneReactionsPreset> reactionPresets = new List<HitZoneReactionsPreset>();

	[SerializeField]
	[Tooltip("Local reactions used when presets have no match (or when no presets are assigned).")]
	private List<HitZoneReaction> reactions = new List<HitZoneReaction>();

	private AnimationComponentBase animationComponent;

	public AnimationComponentBase AnimationComponent => animationComponent;

	public HitZoneType ZoneType => zoneType;

	private void Start()
	{
		animationComponent = GetComponentInParent<AnimationComponentBase>();
	}

	public (HitResult result, HitZoneReaction reaction) ProcessHit(DamageSourceType sourceType, AttackContext context, Vector3 hitPosition)
	{
		HitZoneReaction hitZoneReaction = FindReaction(sourceType);
		if (hitZoneReaction == null)
		{
			return (result: HitResult.Pass(), reaction: null);
		}
		return (result: hitZoneReaction.Execute(context, hitPosition), reaction: hitZoneReaction);
	}

	[CanBeNull]
	private HitZoneReaction FindReaction(DamageSourceType sourceType)
	{
		for (int i = 0; i < reactionPresets.Count; i++)
		{
			HitZoneReactionsPreset hitZoneReactionsPreset = reactionPresets[i];
			if (!(hitZoneReactionsPreset == null))
			{
				HitZoneReaction hitZoneReaction = hitZoneReactionsPreset.FindReaction(sourceType);
				if (hitZoneReaction != null)
				{
					return hitZoneReaction;
				}
			}
		}
		HitZoneReaction hitZoneReaction2 = null;
		for (int j = 0; j < reactions.Count; j++)
		{
			HitZoneReaction hitZoneReaction3 = reactions[j];
			if (hitZoneReaction3 != null)
			{
				if (hitZoneReaction3.sourceType == sourceType)
				{
					return hitZoneReaction3;
				}
				if (hitZoneReaction2 == null && hitZoneReaction3.sourceType == DamageSourceType.Any)
				{
					hitZoneReaction2 = hitZoneReaction3;
				}
			}
		}
		return hitZoneReaction2;
	}

	public bool CheckDoorDirectionBlock(AnimationComponentBase attackerAnimation)
	{
		if (!animationComponent || !attackerAnimation)
		{
			return false;
		}
		if (zoneType != HitZoneType.Door || zoneType != HitZoneType.DoorFrontArea)
		{
			return false;
		}
		Direction dir = animationComponent.GetDirection().ConvertFromVector2();
		Direction direction = attackerAnimation.GetDirection().ConvertFromVector2();
		return dir.OppositeDir() == direction;
	}

	public void PlayHitEffect(HitResult hitResult, HitZoneReaction reaction, Vector3 position)
	{
		if (reaction != null)
		{
			HitEffectConfig hitEffectConfig = reaction.effects.Find((HitEffectConfig e) => e.resultType == hitResult.type);
			if (hitEffectConfig != null)
			{
				reaction.PlayEffect(hitEffectConfig, position);
			}
		}
	}
}
