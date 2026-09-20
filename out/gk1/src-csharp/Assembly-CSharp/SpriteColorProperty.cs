using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteColorProperty : MaterialPropertyModifier
{
	public Color color2 = Color.black;

	private SpriteRenderer _spr_renderer;

	private Color _last_color = Color.black;

	public void Awake()
	{
		_spr_renderer = GetComponent<SpriteRenderer>();
	}

	public void Start()
	{
		UpdateRenderer();
	}

	public void Update()
	{
		if (color2 != _last_color)
		{
			UpdateRenderer();
		}
	}

	public override void UpdateRenderer()
	{
		DoUpdateRenderer(_spr_renderer, delegate(MaterialPropertyBlock prop)
		{
			prop.SetColor("_Color2", color2);
		});
		_last_color = color2;
	}
}
