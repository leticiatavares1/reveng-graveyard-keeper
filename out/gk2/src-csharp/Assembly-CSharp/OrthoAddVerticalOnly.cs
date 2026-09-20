using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class OrthoAddVerticalOnly : MonoBehaviour
{
	[Tooltip(">1 renders more vertical content. 1 = off.")]
	[Range(1f, 3f)]
	public float verticalFactor = 1f;

	[Tooltip("Material with the shader below (auto-created if null).")]
	public Material cropMat;

	private Camera cam;

	private float baseOrthoSize;

	private bool cached;

	public Shader shader;

	public bool autoSqueeze = true;

	public float defaultCameraXAngle = 53.13f;

	public float autoSqueezeFactor = 1.1f;

	private void OnEnable()
	{
		if (shader == null)
		{
			shader = Shader.Find("Hidden/Ortho/AddVerticalOnlyCropX");
		}
		cam = GetComponent<Camera>();
		CacheBase();
		EnsureMat();
	}

	private void OnDisable()
	{
		RestoreBase();
	}

	private void OnDestroy()
	{
		RestoreBase();
	}

	private void CacheBase()
	{
		if (!cached)
		{
			baseOrthoSize = cam.orthographicSize;
			cached = true;
		}
	}

	private void RestoreBase()
	{
		if (cached && !(cam == null))
		{
			cam.orthographicSize = baseOrthoSize;
		}
	}

	private void EnsureMat()
	{
		if (cropMat == null)
		{
			cropMat = new Material(shader)
			{
				hideFlags = HideFlags.DontSave
			};
		}
	}

	private float CalculateVerticalFactor()
	{
		float num = Mathf.Max(1f, verticalFactor);
		if (autoSqueeze)
		{
			float x = cam.transform.eulerAngles.x;
			if (x < defaultCameraXAngle)
			{
				float num2 = defaultCameraXAngle - x;
				float num3 = 1f + num2 / defaultCameraXAngle * autoSqueezeFactor;
				num *= num3;
			}
		}
		return num;
	}

	private void OnPreCull()
	{
		float num = CalculateVerticalFactor();
		cam.orthographicSize = baseOrthoSize * num;
	}

	private void OnPostRender()
	{
		RestoreBase();
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if (cropMat == null)
		{
			Graphics.Blit(src, dst);
			return;
		}
		float value = CalculateVerticalFactor();
		cropMat.SetFloat("_CropXF", value);
		Graphics.Blit(src, dst, cropMat, 0);
	}
}
