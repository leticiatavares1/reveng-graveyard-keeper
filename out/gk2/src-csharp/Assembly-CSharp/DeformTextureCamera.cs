using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class DeformTextureCamera : MonoBehaviour
{
	public float pixelSize = 1f;

	[SerializeField]
	private Camera camera;

	private RenderTexture renderTexture;

	private RenderTexture secondRenderTexture;

	public Material blitMaterial;

	private Vector2 prevCameraPos = Vector2.negativeInfinity;

	private static readonly int idOffset = Shader.PropertyToID("_Offset");

	private static readonly int idCameraViewMatrix = Shader.PropertyToID("_CameraViewMatrix");

	private static readonly int idCameraProjectionMatrix = Shader.PropertyToID("_CameraProjectionMatrix");

	private static readonly int idCameraVpMatrix = Shader.PropertyToID("_CameraVPMatrix");

	private static readonly int idCameraProjectionParams = Shader.PropertyToID("_CameraProjectionParams");

	private Camera Camera
	{
		get
		{
			if (!camera)
			{
				camera = GetComponent<Camera>();
			}
			return camera;
		}
	}

	private void Awake()
	{
		SetRenderTexture();
	}

	private void SetRenderTexture()
	{
		if ((bool)renderTexture)
		{
			Shader.SetGlobalTexture("_ScreenDeformTex", renderTexture);
		}
	}

	private void OnPreRender()
	{
		Shader.EnableKeyword("DEFORM_RENDERING");
		if (!(renderTexture == null))
		{
			Graphics.Blit(renderTexture, secondRenderTexture);
			CalculateCameraOffset();
			CalculateCameraMatrices();
			Graphics.Blit(secondRenderTexture, renderTexture, blitMaterial);
		}
	}

	private void CalculateCameraOffset()
	{
		Vector2 cameraXY = GetCameraXY();
		Vector2 vector = prevCameraPos - cameraXY;
		blitMaterial.SetVector(idOffset, vector);
		prevCameraPos = cameraXY;
	}

	private void CalculateCameraMatrices()
	{
		Camera camera = this.camera;
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true);
		Matrix4x4 value = gPUProjectionMatrix * worldToCameraMatrix;
		Shader.SetGlobalMatrix(idCameraViewMatrix, worldToCameraMatrix);
		Shader.SetGlobalMatrix(idCameraProjectionMatrix, gPUProjectionMatrix);
		Shader.SetGlobalMatrix(idCameraVpMatrix, value);
		Vector4 value2 = new Vector4(1f / camera.projectionMatrix[1, 1], 1f / camera.projectionMatrix[0, 0], camera.nearClipPlane, camera.farClipPlane);
		Shader.SetGlobalVector(idCameraProjectionParams, value2);
	}

	private void OnPostRender()
	{
		Shader.DisableKeyword("DEFORM_RENDERING");
	}

	private Vector2 GetCameraXY()
	{
		Vector3 position = camera.transform.position;
		return new Vector2(position.x / 0.01f / pixelSize, position.z / 0.0125f / pixelSize);
	}

	public void ChangeOrthographicSize(int screenW, int screenH, float mainCameraOrthoSize)
	{
		int textureWidth = Mathf.RoundToInt((float)screenW / pixelSize);
		int textureHeight = Mathf.RoundToInt((float)screenH / pixelSize);
		ChangeRenderTargetSize(textureWidth, textureHeight, mainCameraOrthoSize);
	}

	public void ChangeRenderTargetSize(int textureWidth, int textureHeight, float mainCameraOrthoSize)
	{
		camera.orthographicSize = mainCameraOrthoSize;
		pixelSize = ResolutionConfig.PixelSize;
		prevCameraPos = Vector2.negativeInfinity;
		if ((bool)renderTexture)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		if ((bool)secondRenderTexture)
		{
			RenderTexture.ReleaseTemporary(secondRenderTexture);
		}
		Debug.Log($"DeformTexture: Texture: {textureWidth}x{textureHeight}, PixelSize: {pixelSize}");
		renderTexture = RenderTexture.GetTemporary(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		renderTexture.filterMode = FilterMode.Trilinear;
		secondRenderTexture = RenderTexture.GetTemporary(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		secondRenderTexture.filterMode = FilterMode.Trilinear;
		camera.targetTexture = renderTexture;
		SetRenderTexture();
		Shader.SetGlobalFloat("_DeformCameraPixelScale", 1f / pixelSize);
		CalculateCameraMatrices();
		LightRTManager.NotifyWorldRenderTargetChanged();
	}
}
