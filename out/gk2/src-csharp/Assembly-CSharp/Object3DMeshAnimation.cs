using UnityEngine;

[ExecuteAlways]
public class Object3DMeshAnimation : MonoBehaviour
{
	public Object3DMesh targetMesh;

	public int submeshIndex;

	public Texture2D texture;

	private Texture2D prevTexture;

	private Texture2D cachedStaticTexture;

	private bool isAnimatable;

	public void SetAnimatableState(bool isAnimatable)
	{
		this.isAnimatable = isAnimatable;
		base.enabled = isAnimatable;
	}

	public void ReturnToStaticState()
	{
		if (!isAnimatable)
		{
			if (cachedStaticTexture == null)
			{
				Debug.LogWarning("Object 3D Mesh Animation: Static texture is null [" + base.gameObject.name + "]");
				return;
			}
			targetMesh.GetSubMesh(submeshIndex).SetTextureOnly(cachedStaticTexture);
			cachedStaticTexture = null;
			prevTexture = null;
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (!isAnimatable || texture == null || texture == prevTexture)
		{
			return;
		}
		prevTexture = texture;
		Object3DSubMesh subMesh = targetMesh.GetSubMesh(submeshIndex);
		if (subMesh != null)
		{
			if (!cachedStaticTexture)
			{
				cachedStaticTexture = subMesh.Texture;
			}
			subMesh.SetTextureOnly(texture);
		}
	}
}
