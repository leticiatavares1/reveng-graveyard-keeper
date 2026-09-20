using UnityEngine;

[ExecuteAlways]
public class PerpendicularVerticalSprite : VerticalSprite
{
	[SerializeField]
	[Range(-1f, 1f)]
	protected float shadowDepthCorrection;

	private MaterialPropertyBlock matProp;

	private static readonly int idWorldPos = Shader.PropertyToID("_ObjectWorldPos");

	private Vector3 lastPosition;

	private Vector3 depthOffset;

	protected override void ApplyMaterial()
	{
		matProp = new MaterialPropertyBlock();
		depthOffset = Vector3.forward * shadowDepthCorrection;
		base.ApplyMaterial();
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		if (position != lastPosition)
		{
			lastPosition = position;
			base.SpriteRenderer.GetPropertyBlock(matProp);
			matProp.SetVector(idWorldPos, position + depthOffset);
			base.SpriteRenderer.SetPropertyBlock(matProp);
		}
	}
}
