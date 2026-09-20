using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class ChangingLightSprite : MonoBehaviour
{
	private SpriteRenderer _spr;

	private bool _spr_set;

	protected SpriteRenderer spr
	{
		get
		{
			if (!_spr_set)
			{
				_spr_set = true;
				_spr = GetComponent<SpriteRenderer>();
			}
			return _spr;
		}
	}

	public void Update()
	{
		spr.color = TimeOfDay.light_sprites_color;
	}
}
