using UnityEngine;

[ExecuteAlways]
public class AlphaGradientController : MonoBehaviour
{
	private static readonly int GradientStart = Shader.PropertyToID("_GradientStart");

	private static readonly int GradientEnd = Shader.PropertyToID("_GradientEnd");

	[SerializeField]
	private Renderer targetRenderer;

	[SerializeField]
	[Range(0f, 1f)]
	private float gradientStart;

	[SerializeField]
	[Range(0f, 1f)]
	private float gradientEnd = 1f;

	private MaterialPropertyBlock propertyBlock;

	private void OnValidate()
	{
		if (targetRenderer == null)
		{
			targetRenderer = GetComponent<Renderer>();
		}
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		UpdateMaterialProperties();
	}

	private void OnEnable()
	{
		propertyBlock = new MaterialPropertyBlock();
		UpdateMaterialProperties();
	}

	private void UpdateMaterialProperties()
	{
		if (!(targetRenderer == null))
		{
			targetRenderer.GetPropertyBlock(propertyBlock);
			propertyBlock.SetFloat(GradientStart, gradientStart);
			propertyBlock.SetFloat(GradientEnd, gradientEnd);
			targetRenderer.SetPropertyBlock(propertyBlock);
		}
	}
}
