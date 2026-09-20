using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Graphic))]
[AddComponentMenu("UI/Progress Cell Atlas Size")]
public class ProgressCellAtlasSize : BaseMeshEffect
{
	private const float SingleCellXOffsetPx = 1f;

	[SerializeField]
	private int cellCount;

	public void SetCellCount(int count)
	{
		if (cellCount != count)
		{
			cellCount = count;
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		EnableCanvasTexCoord1();
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		EnableCanvasTexCoord1();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		if (base.graphic != null)
		{
			base.graphic.SetVerticesDirty();
		}
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		if (base.graphic != null)
		{
			base.graphic.SetVerticesDirty();
		}
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (IsActive() && vh != null)
		{
			Vector2 size = ((RectTransform)base.transform).rect.size;
			float w = ((cellCount == 1) ? 1f : 0f);
			UIVertex vertex = default(UIVertex);
			int currentVertCount = vh.currentVertCount;
			for (int i = 0; i < currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref vertex, i);
				vertex.uv1 = new Vector4(size.x, size.y, cellCount, w);
				vh.SetUIVertex(vertex, i);
			}
		}
	}

	private void EnableCanvasTexCoord1()
	{
		if (!(base.graphic == null))
		{
			Canvas canvas = base.graphic.canvas;
			if (!(canvas == null))
			{
				canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
			}
		}
	}
}
