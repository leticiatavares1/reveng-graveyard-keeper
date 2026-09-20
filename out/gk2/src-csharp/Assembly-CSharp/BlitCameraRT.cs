using UnityEngine;

[ExecuteAlways]
public class BlitCameraRT : MonoBehaviour
{
	[SerializeField]
	private MainCamera mainCamera;

	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (mainCamera != null && mainCamera.RenderTexture != null)
		{
			Graphics.Blit(mainCamera.RenderTexture, destination);
		}
	}
}
