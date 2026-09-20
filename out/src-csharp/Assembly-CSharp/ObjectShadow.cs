using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class ObjectShadow : MonoBehaviour
{
	protected Material mat;

	private SpriteRenderer _spr;

	public void Update()
	{
		_spr = base.gameObject.GetComponent<SpriteRenderer>();
		if (mat == null)
		{
			mat = _spr.sharedMaterial;
		}
		if (!(mat == null) && !(TimeOfDay.me == null))
		{
			mat.SetFloat("_HSkew", TimeOfDay.me.time_of_day * 1.25f);
			mat.SetFloat("_Rotation", TimeOfDay.me.time_of_day / 3f);
			Color color = _spr.color;
			color.a = TimeOfDay.me.shadow_alpha.Evaluate((TimeOfDay.me.time_of_day + 1f) / 2f);
			_spr.color = color;
		}
	}
}
