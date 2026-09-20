using TheraBytes.BetterUi;
using UnityEngine;

public class SetAsAnchorTarget : MonoBehaviour
{
	[SerializeField]
	private AnchorOverride affectedObject;

	public void SetTarget(RectTransform target)
	{
		affectedObject.CurrentAnchors.Elements[0].Reference = target;
	}
}
