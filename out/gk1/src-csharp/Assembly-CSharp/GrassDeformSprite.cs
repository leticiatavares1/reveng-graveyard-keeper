using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class GrassDeformSprite : MaterialPropertyModifier
{
	private SpriteRenderer _spr_renderer;

	public float skew;

	private float _wind_phase;

	public float wind_coef = 1f;

	public void Awake()
	{
		_spr_renderer = GetComponent<SpriteRenderer>();
		_wind_phase = Random.Range(0f, 3f);
	}

	public override void UpdateRenderer()
	{
		DoUpdateRenderer(_spr_renderer, delegate(MaterialPropertyBlock prop)
		{
			prop.SetFloat("_Skew", skew);
			prop.SetFloat("_WindPhase", _wind_phase);
			prop.SetFloat("_WindK", wind_coef);
		});
	}
}
