using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipHandler : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("renderer")]
	private SpriteRenderer rend;

	private void Awake()
	{
		if (rend == null)
		{
			rend = GetComponent<SpriteRenderer>();
		}
	}

	private void LateUpdate()
	{
		if (base.transform.lossyScale.x < 0f)
		{
			base.transform.localScale = new Vector3(0f - base.transform.localScale.x, base.transform.localScale.y, base.transform.localScale.z);
			rend.flipX = !rend.flipX;
		}
	}
}
