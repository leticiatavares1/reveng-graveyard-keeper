using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Fighting/HitZone Reactions Preset", fileName = "HitZoneReactionsPreset")]
public class HitZoneReactionsPreset : ScriptableObject
{
	[SerializeField]
	private List<HitZoneReaction> reactions = new List<HitZoneReaction>();

	public IReadOnlyList<HitZoneReaction> Reactions => reactions;

	[CanBeNull]
	public HitZoneReaction FindReaction(DamageSourceType sourceType)
	{
		HitZoneReaction hitZoneReaction = null;
		for (int i = 0; i < reactions.Count; i++)
		{
			HitZoneReaction hitZoneReaction2 = reactions[i];
			if (hitZoneReaction2 != null)
			{
				if (hitZoneReaction2.sourceType == sourceType)
				{
					return hitZoneReaction2;
				}
				if (hitZoneReaction == null && hitZoneReaction2.sourceType == DamageSourceType.Any)
				{
					hitZoneReaction = hitZoneReaction2;
				}
			}
		}
		return hitZoneReaction;
	}
}
