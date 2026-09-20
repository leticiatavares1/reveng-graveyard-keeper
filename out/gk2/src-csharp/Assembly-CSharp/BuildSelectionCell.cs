using UnityEngine;

public class BuildSelectionCell : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[Space]
	[SerializeField]
	private Color availableColor = Color.green;

	[SerializeField]
	private Color unavailableColor = Color.red;

	[Space]
	[SerializeField]
	private Sprite[] spriteVariations = new Sprite[13];

	[SerializeField]
	private Vector2Int[] spriteXZPixelShifts = new Vector2Int[13];

	private Vector3 localPos;

	private bool isAvailableForBuild;

	public bool IsAvailableForBuild
	{
		get
		{
			return isAvailableForBuild;
		}
		set
		{
			isAvailableForBuild = value;
			spriteRenderer.color = (isAvailableForBuild ? availableColor : unavailableColor);
		}
	}

	public Bounds SpriteBounds => spriteRenderer.bounds;

	public int OverlapBoxNonAlloc(Collider[] overlapColliders, int mask)
	{
		return Physics.OverlapBoxNonAlloc(base.transform.position, BuildConsts.CASTING_BOX_HALF_EXTENTS, overlapColliders, Quaternion.identity, mask);
	}

	public void SetVariation(BuildSelectionCellVariation variation)
	{
		spriteRenderer.sprite = spriteVariations[(int)variation];
		spriteRenderer.transform.localPosition = Vector3.Scale(new Vector3(spriteXZPixelShifts[(int)variation].x, 0f, spriteXZPixelShifts[(int)variation].y), VisualConsts.XYZ_STEP);
	}
}
