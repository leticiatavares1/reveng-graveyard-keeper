using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class RopeMeshView : MonoBehaviour
{
	private MeshRenderer meshRenderer;

	private MaterialPropertyBlock materialPropertyBlock;

	private readonly int startPointProp = Shader.PropertyToID("_StartPoint");

	private readonly int endPointProp = Shader.PropertyToID("_EndPoint");

	private readonly int loosenessProp = Shader.PropertyToID("_Looseness");

	private readonly int thicknessProp = Shader.PropertyToID("_Thickness");

	private readonly int ropeColorProp = Shader.PropertyToID("_RopeColor");

	private readonly int emissionIntensityProp = Shader.PropertyToID("_EmissionIntensity");

	private readonly int gravityStrengthProp = Shader.PropertyToID("_GravityStrength");

	private readonly int resolutionProp = Shader.PropertyToID("_Resolution");

	private readonly int antiAliasingProp = Shader.PropertyToID("_AntiAliasing");

	private readonly int pixelScaleProp = Shader.PropertyToID("_PixelScale");

	public MeshRenderer MeshRenderer => meshRenderer;

	public Vector3 MeshScale => base.transform.localScale;

	private void Awake()
	{
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		materialPropertyBlock = new MaterialPropertyBlock();
	}

	public void EnsureInitialized()
	{
		if (meshRenderer == null || materialPropertyBlock == null)
		{
			InitializeComponents();
		}
	}

	public void AlignToEndPoint(Vector3 localEndPoint)
	{
		Vector3 vector = new Vector3(localEndPoint.x, 0f, localEndPoint.z);
		if (vector.sqrMagnitude < 0.0001f)
		{
			base.transform.localRotation = Quaternion.identity;
			return;
		}
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.left, vector.normalized);
		base.transform.localRotation = Quaternion.Euler(0f, quaternion.eulerAngles.y, 0f);
	}

	public void UpdateRopeVisuals(Vector2 startPos2D, Vector2 endPos2D, float looseness, float thickness, Color ropeColor, float emissionIntensity, float gravityStrength, int textureResolution, bool antiAliasing)
	{
		if (!(meshRenderer == null) && materialPropertyBlock != null)
		{
			materialPropertyBlock.SetVector(startPointProp, startPos2D);
			materialPropertyBlock.SetVector(endPointProp, endPos2D);
			materialPropertyBlock.SetFloat(loosenessProp, looseness);
			materialPropertyBlock.SetFloat(thicknessProp, thickness);
			materialPropertyBlock.SetColor(ropeColorProp, ropeColor);
			materialPropertyBlock.SetFloat(emissionIntensityProp, emissionIntensity);
			materialPropertyBlock.SetFloat(gravityStrengthProp, gravityStrength);
			materialPropertyBlock.SetFloat(resolutionProp, textureResolution);
			materialPropertyBlock.SetInt(antiAliasingProp, antiAliasing ? 1 : 0);
			materialPropertyBlock.SetFloat(pixelScaleProp, 2f);
			meshRenderer.SetPropertyBlock(materialPropertyBlock);
		}
	}

	public void UpdateRopeParameters(float looseness, float thickness, Color ropeColor, float emissionIntensity, float gravityStrength)
	{
		if (!(meshRenderer == null) && materialPropertyBlock != null)
		{
			materialPropertyBlock.SetFloat(loosenessProp, looseness);
			materialPropertyBlock.SetFloat(thicknessProp, thickness);
			materialPropertyBlock.SetColor(ropeColorProp, ropeColor);
			materialPropertyBlock.SetFloat(emissionIntensityProp, emissionIntensity);
			materialPropertyBlock.SetFloat(gravityStrengthProp, gravityStrength);
			meshRenderer.SetPropertyBlock(materialPropertyBlock);
		}
	}

	public void UpdateQualitySettings(int textureResolution, bool antiAliasing)
	{
		if (!(meshRenderer == null) && materialPropertyBlock != null)
		{
			materialPropertyBlock.SetFloat(resolutionProp, textureResolution);
			materialPropertyBlock.SetInt(antiAliasingProp, antiAliasing ? 1 : 0);
			materialPropertyBlock.SetFloat(pixelScaleProp, 2f);
			meshRenderer.SetPropertyBlock(materialPropertyBlock);
		}
	}
}
