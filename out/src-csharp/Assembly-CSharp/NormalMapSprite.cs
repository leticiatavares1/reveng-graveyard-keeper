using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class NormalMapSprite : MaterialPropertyModifier
{
	public Texture2D normal_map;

	private SpriteRenderer _spr_renderer;

	private Texture2D _last_normal_map;

	private Sprite _last_spr;

	public void Awake()
	{
		_spr_renderer = GetComponent<SpriteRenderer>();
	}

	public override void UpdateRenderer()
	{
		DoUpdateRenderer(_spr_renderer, delegate(MaterialPropertyBlock prop)
		{
			if (normal_map != null)
			{
				prop.SetTexture("_NormalDepth", normal_map);
			}
		});
		_last_normal_map = normal_map;
	}
}
