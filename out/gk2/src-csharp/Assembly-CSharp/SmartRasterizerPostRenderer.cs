using LazyBearTechnology;
using UnityEngine.Rendering.PostProcessing;

public sealed class SmartRasterizerPostRenderer : PostProcessEffectRenderer<SmartRasterizerPostEffect>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(LazySingletonSO<GlobalResources>.Instance.smartRasterizerShader);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
