using UnityEngine;

public class BakedAnimationComponent : AnimationComponentBase
{
	[SerializeField]
	protected SkinPresetGK2 bakeSkinPreset;

	private SkinChangerGK2 skinChanger;

	protected override void InitInternal(SkinPresetGK2 skinPreset)
	{
		skinChanger = new SkinChangerGK2(base.gameObject);
		skinChanger.ApplySkin(bakeSkinPreset);
	}
}
