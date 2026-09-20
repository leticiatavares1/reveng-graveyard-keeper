using System;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(SmartRasterizerPostRenderer), PostProcessEvent.BeforeTransparent, "Custom/Smart Rasterizer", true)]
public sealed class SmartRasterizerPostEffect : PostProcessEffectSettings
{
	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (base.IsEnabledAndSupported(context))
		{
			return PlatformFeatures.Current.renderMode != PlatformRenderMode.Lightweight;
		}
		return false;
	}
}
