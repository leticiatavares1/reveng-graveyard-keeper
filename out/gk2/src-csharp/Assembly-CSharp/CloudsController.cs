using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[DefaultExecutionOrder(20)]
public class CloudsController : MonoBehaviour
{
	private const float CAPTURE_MARGIN = 5f;

	private const float MaxWindDeltaTime = 0.1f;

	private static readonly int idCloudsRT = Shader.PropertyToID("_CloudsRT");

	private static readonly int idCloudsRTViewProjMatrix = Shader.PropertyToID("_CloudsRTViewProjMatrix");

	private static readonly int idCloudsRTIntensity = Shader.PropertyToID("_CloudsRTIntensity");

	private static readonly int idCloudsShadowStrength = Shader.PropertyToID("_CloudsShadowStrength");

	private static readonly int idCloudsDensity = Shader.PropertyToID("_CloudsDensity");

	private static readonly int idCloudWindSpeed = Shader.PropertyToID("_CloudWindSpeed");

	private static readonly int idCloudWindAccel = Shader.PropertyToID("_CloudWindAccel");

	private static readonly int idCloudWindOffset = Shader.PropertyToID("_CloudWindOffset");

	[Tooltip("Default-mode ASE \"Clouds Shadow\" quad.")]
	[SerializeField]
	private GameObject defaultCloudsQuad;

	[Tooltip("Replaced-mode procedural quad. Prefab starts inactive.")]
	[SerializeField]
	private GameObject replacedCloudsQuad;

	[Tooltip("Procedural material used only for RenderTexture capture. Assign \"Clouds Shadow Procedural\". Falls back to GlobalResources.cloudsMaterial when empty.")]
	[SerializeField]
	private Material rtShadowMaterial;

	[Tooltip("Procedural material for Replaced real-time shadows. Assign \"Clouds Shadow Procedural Replaced\". Falls back to the replaced quad's sharedMaterial.")]
	[SerializeField]
	private Material replacedShadowMaterial;

	[Tooltip("Global RT overlay intensity (multiplies sampled RT alpha). Independent of material _CloudOpacity.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float shadowStrength = 0.35f;

	[Tooltip("Only one mesh is drawn into this RT, so raising the resolution is cheap.")]
	[SerializeField]
	private int resolution = 1024;

	private CommandBuffer commandBuffer;

	private MaterialPropertyBlock propertyBlock;

	private RenderTexture cloudsRT;

	private Matrix4x4 viewProjMatrix;

	private int forwardPassIndex = -1;

	private bool keywordApplied;

	private PlatformCloudAppearance lastAppearance = (PlatformCloudAppearance)(-1);

	private float windScrollOffset;

	private float lastWindIntegrateTime = -1f;

	private MeshFilter defaultMeshFilter;

	private MeshRenderer replacedMeshRenderer;

	private bool loggedMissingRtPathRefs;

	private bool loggedMissingReplacedPathRefs;

	private void OnEnable()
	{
		EnsureRuntimeState();
		lastAppearance = (PlatformCloudAppearance)(-1);
		lastWindIntegrateTime = -1f;
		loggedMissingRtPathRefs = false;
		loggedMissingReplacedPathRefs = false;
	}

	private void OnDisable()
	{
		ApplyQuadVisibility(defaultActive: true, replacedActive: false);
		ClearReplacedPropertyBlock();
		ClearGlobals();
		ReleaseRT();
		lastAppearance = (PlatformCloudAppearance)(-1);
		lastWindIntegrateTime = -1f;
	}

	private void OnDestroy()
	{
		commandBuffer?.Release();
		commandBuffer = null;
	}

	private void EnsureRuntimeState()
	{
		if (commandBuffer == null)
		{
			commandBuffer = new CommandBuffer
			{
				name = "Clouds Shadow RT Capture"
			};
		}
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
	}

	private void LateUpdate()
	{
		EnsureRuntimeState();
		PlatformCloudAppearance activeAppearance = CloudsShadowPolicy.ActiveAppearance;
		if (activeAppearance != lastAppearance)
		{
			OnAppearanceChanged(lastAppearance, activeAppearance);
			lastAppearance = activeAppearance;
		}
		if (CloudsShadowPolicy.UseCloudsRTOverlay)
		{
			UpdateRenderTexturePath();
			return;
		}
		ClearGlobals();
		if (CloudsShadowPolicy.UseReplacedCloudMaterial)
		{
			UpdateReplacedPath();
			return;
		}
		ApplyQuadVisibility(defaultActive: true, replacedActive: false);
		ClearReplacedPropertyBlock();
	}

	private void OnAppearanceChanged(PlatformCloudAppearance previous, PlatformCloudAppearance next)
	{
		forwardPassIndex = -1;
		if (previous == PlatformCloudAppearance.RenderTexture && next != PlatformCloudAppearance.RenderTexture)
		{
			ClearGlobals();
		}
		if (next != PlatformCloudAppearance.Replaced)
		{
			ClearReplacedPropertyBlock();
		}
	}

	private void UpdateRenderTexturePath()
	{
		MeshFilter meshFilter = GetDefaultMeshFilter();
		if (meshFilter == null || meshFilter.sharedMesh == null)
		{
			LogMissingConfigOnce(ref loggedMissingRtPathRefs, "RenderTexture mode needs defaultCloudsQuad (or a child MeshFilter) with a sharedMesh.");
			FallbackFromFailedRtPath();
			return;
		}
		Material material = ResolveRtMaterial();
		if (material == null)
		{
			LogMissingConfigOnce(ref loggedMissingRtPathRefs, "RenderTexture mode needs rtShadowMaterial assigned, or GlobalResources.cloudsMaterial.");
			FallbackFromFailedRtPath();
			return;
		}
		if (forwardPassIndex < 0)
		{
			forwardPassIndex = Mathf.Max(0, material.FindPass("FORWARD"));
		}
		ApplyQuadVisibility(defaultActive: false, replacedActive: false);
		ClearReplacedPropertyBlock();
		BuildPropertyBlock(material);
		EnsureRT();
		CaptureCloudsRT(meshFilter, material);
		PublishGlobals();
	}

	private void FallbackFromFailedRtPath()
	{
		ClearGlobals();
		ApplyQuadVisibility(defaultActive: true, replacedActive: false);
		ClearReplacedPropertyBlock();
	}

	private void UpdateReplacedPath()
	{
		if (replacedCloudsQuad == null)
		{
			LogMissingConfigOnce(ref loggedMissingReplacedPathRefs, "Replaced mode requires replacedCloudsQuad assigned on World Clouds.");
			FallbackFromFailedReplacedPath();
			return;
		}
		Material material = ResolveReplacedMaterial();
		MeshRenderer meshRenderer = GetReplacedMeshRenderer();
		if (material == null || meshRenderer == null)
		{
			LogMissingConfigOnce(ref loggedMissingReplacedPathRefs, "Replaced mode needs a MeshRenderer and material on replacedCloudsQuad (or replacedShadowMaterial).");
			FallbackFromFailedReplacedPath();
			return;
		}
		ApplyQuadVisibility(defaultActive: false, replacedActive: true);
		if (meshRenderer.sharedMaterial != material)
		{
			meshRenderer.sharedMaterial = material;
		}
		BuildPropertyBlock(material);
		meshRenderer.SetPropertyBlock(propertyBlock);
	}

	private void FallbackFromFailedReplacedPath()
	{
		ApplyQuadVisibility(defaultActive: true, replacedActive: false);
		ClearReplacedPropertyBlock();
	}

	private void ApplyQuadVisibility(bool defaultActive, bool replacedActive)
	{
		SetActiveIfNeeded(defaultCloudsQuad, defaultActive);
		SetActiveIfNeeded(replacedCloudsQuad, replacedActive);
	}

	private static void SetActiveIfNeeded(GameObject go, bool active)
	{
		if (go != null && go.activeSelf != active)
		{
			go.SetActive(active);
		}
	}

	private void ClearReplacedPropertyBlock()
	{
		MeshRenderer meshRenderer = GetReplacedMeshRenderer();
		if (meshRenderer != null)
		{
			meshRenderer.SetPropertyBlock(null);
		}
	}

	private MeshFilter GetDefaultMeshFilter()
	{
		if (defaultMeshFilter != null)
		{
			return defaultMeshFilter;
		}
		if (defaultCloudsQuad != null)
		{
			defaultMeshFilter = defaultCloudsQuad.GetComponent<MeshFilter>();
			if (defaultMeshFilter != null)
			{
				return defaultMeshFilter;
			}
		}
		defaultMeshFilter = GetComponentInChildren<MeshFilter>(includeInactive: true);
		if (defaultMeshFilter != null && defaultCloudsQuad == null)
		{
			defaultCloudsQuad = defaultMeshFilter.gameObject;
		}
		return defaultMeshFilter;
	}

	private void LogMissingConfigOnce(ref bool logged, string message)
	{
		if (!logged)
		{
			logged = true;
			Debug.LogError("[CloudsController] " + message, this);
		}
	}

	private MeshRenderer GetReplacedMeshRenderer()
	{
		if (replacedMeshRenderer == null && replacedCloudsQuad != null)
		{
			replacedMeshRenderer = replacedCloudsQuad.GetComponent<MeshRenderer>();
		}
		return replacedMeshRenderer;
	}

	private Material ResolveRtMaterial()
	{
		if (rtShadowMaterial != null)
		{
			return rtShadowMaterial;
		}
		if (!(LazySingletonSO<GlobalResources>.Instance != null))
		{
			return null;
		}
		return LazySingletonSO<GlobalResources>.Instance.cloudsMaterial;
	}

	private Material ResolveReplacedMaterial()
	{
		if (replacedShadowMaterial != null)
		{
			return replacedShadowMaterial;
		}
		MeshRenderer meshRenderer = GetReplacedMeshRenderer();
		if (!(meshRenderer != null))
		{
			return null;
		}
		return meshRenderer.sharedMaterial;
	}

	private void BuildPropertyBlock(Material material)
	{
		propertyBlock.Clear();
		propertyBlock.SetFloat(idCloudsDensity, CPCloudsDensity.ResolveDensityForClouds());
		AdvanceWindScroll(material);
		propertyBlock.SetFloat(idCloudWindOffset, windScrollOffset);
	}

	private void AdvanceWindScroll(Material material)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = 0f;
		if (lastWindIntegrateTime >= 0f)
		{
			num = Mathf.Min(0.1f, Mathf.Max(0f, realtimeSinceStartup - lastWindIntegrateTime));
		}
		lastWindIntegrateTime = realtimeSinceStartup;
		float num2 = ((material != null && material.HasProperty(idCloudWindSpeed)) ? material.GetFloat(idCloudWindSpeed) : 0f);
		float num3 = ((material != null && material.HasProperty(idCloudWindAccel)) ? material.GetFloat(idCloudWindAccel) : 0f);
		float num4 = num2 + ReadWindValue() * num3;
		windScrollOffset += num4 * num;
	}

	private static float ReadWindValue()
	{
		if (WeatherSystem.Instance != null)
		{
			return Mathf.Clamp01(WeatherSystem.Instance.WindValue);
		}
		return Mathf.Clamp01(Shader.GetGlobalFloat(GlobalShaderParameters.idWindValue));
	}

	private void EnsureRT()
	{
		int num = Mathf.Max(1, resolution);
		if (!(cloudsRT != null) || cloudsRT.width != num || cloudsRT.height != num)
		{
			ReleaseRT();
			cloudsRT = new RenderTexture(num, num, 0, RenderTextureFormat.ARGB32)
			{
				name = "_CloudsRT",
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp,
				useMipMap = false
			};
			cloudsRT.Create();
		}
	}

	private void CaptureCloudsRT(MeshFilter meshFilter, Material material)
	{
		Bounds bounds = TransformBounds(meshFilter.transform, meshFilter.sharedMesh.bounds);
		Vector3 center = bounds.center;
		float num = Mathf.Max(0.01f, bounds.extents.x);
		float num2 = Mathf.Max(0.01f, bounds.extents.z);
		float num3 = bounds.max.y + 5f;
		float num4 = bounds.min.y - 5f;
		float zFar = Mathf.Max(1f, num3 - num4);
		Matrix4x4 inverse = Matrix4x4.TRS(new Vector3(center.x, num3, center.z), Quaternion.LookRotation(Vector3.down, Vector3.forward), new Vector3(1f, 1f, -1f)).inverse;
		Matrix4x4 proj = Matrix4x4.Ortho(0f - num, num, 0f - num2, num2, 0.01f, zFar);
		Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(proj, renderIntoTexture: true);
		bool num5 = proj.m11 * gPUProjectionMatrix.m11 < 0f;
		commandBuffer.Clear();
		commandBuffer.SetRenderTarget(cloudsRT);
		commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		commandBuffer.SetViewProjectionMatrices(inverse, gPUProjectionMatrix);
		if (num5)
		{
			commandBuffer.SetInvertCulling(invertCulling: true);
		}
		commandBuffer.DrawMesh(meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, material, 0, forwardPassIndex, propertyBlock);
		if (num5)
		{
			commandBuffer.SetInvertCulling(invertCulling: false);
		}
		Graphics.ExecuteCommandBuffer(commandBuffer);
		viewProjMatrix = gPUProjectionMatrix * inverse;
	}

	private static Bounds TransformBounds(Transform t, Bounds localBounds)
	{
		Vector3 center = t.TransformPoint(localBounds.center);
		Vector3 extents = localBounds.extents;
		Vector3 vector = t.TransformVector(extents.x, 0f, 0f);
		Vector3 vector2 = t.TransformVector(0f, extents.y, 0f);
		Vector3 vector3 = t.TransformVector(0f, 0f, extents.z);
		Vector3 vector4 = new Vector3(Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x), Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y), Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z));
		return new Bounds(center, vector4 * 2f);
	}

	private void PublishGlobals()
	{
		Shader.SetGlobalTexture(idCloudsRT, cloudsRT);
		Shader.SetGlobalMatrix(idCloudsRTViewProjMatrix, viewProjMatrix);
		Shader.SetGlobalFloat(idCloudsRTIntensity, 1f);
		Shader.SetGlobalFloat(idCloudsShadowStrength, shadowStrength);
		SetKeyword(enable: true);
	}

	private void ClearGlobals()
	{
		Shader.SetGlobalTexture(idCloudsRT, Texture2D.blackTexture);
		Shader.SetGlobalFloat(idCloudsRTIntensity, 0f);
		SetKeyword(enable: false);
	}

	private void SetKeyword(bool enable)
	{
		if (keywordApplied != enable)
		{
			keywordApplied = enable;
			if (enable)
			{
				Shader.EnableKeyword("USE_CLOUDS_RT");
			}
			else
			{
				Shader.DisableKeyword("USE_CLOUDS_RT");
			}
		}
	}

	private void ReleaseRT()
	{
		if (!(cloudsRT == null))
		{
			cloudsRT.Release();
			Object.Destroy(cloudsRT);
			cloudsRT = null;
		}
	}
}
