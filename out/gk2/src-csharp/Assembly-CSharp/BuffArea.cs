using UnityEngine;

public class BuffArea : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	private readonly int shaderIdDataTex = Shader.PropertyToID("_DataTex");

	private readonly int shaderIdObjectScale = Shader.PropertyToID("_ObjectScale");

	private MaterialPropertyBlock propertyBlock;

	public void Draw(Texture2D texture, Vector3 position, Vector2 size)
	{
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		meshRenderer.transform.localScale = new Vector3(size.x / base.transform.parent.localScale.x, size.y / base.transform.parent.localScale.y, 1f);
		meshRenderer.transform.position = position;
		meshRenderer.transform.localPosition = new Vector3(meshRenderer.transform.localPosition.x, meshRenderer.transform.localPosition.y, 0f);
		propertyBlock.SetTexture(shaderIdDataTex, texture);
		propertyBlock.SetVector(shaderIdObjectScale, meshRenderer.transform.lossyScale);
		meshRenderer.SetPropertyBlock(propertyBlock);
	}
}
