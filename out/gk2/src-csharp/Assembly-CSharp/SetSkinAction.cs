using System;
using UnityEngine;

[Serializable]
public class SetSkinAction : ConditionalDrawerActionBase
{
	[Tooltip("Skin preset applying if condition is true")]
	public SkinPresetGK2 skinOnTrue;

	[Tooltip("Skin preset applying if condition is false")]
	public SkinPresetGK2 skinOnFalse;

	public bool isOnTrueDefault;

	public override void Execute(ConditionalDrawerContext context, bool conditionMet)
	{
		AnimationComponent obj = ((context.WgoPart.AnimationComponent == null) ? context.WgoPart.GetComponentInChildren<AnimationComponent>() : (context.WgoPart.AnimationComponent as AnimationComponent));
		if (obj == null)
		{
			Debug.LogError("AnimationComponent not found on WGO");
		}
		obj?.ChangeSkinPreset(conditionMet ? skinOnTrue : skinOnFalse);
	}

	public override void Reset(ConditionalDrawerContext context)
	{
		((context.WgoPart.AnimationComponent == null) ? context.WgoPart.GetComponentInChildren<AnimationComponent>() : (context.WgoPart.AnimationComponent as AnimationComponent))?.ChangeSkinPreset(isOnTrueDefault ? skinOnTrue : skinOnFalse);
	}
}
