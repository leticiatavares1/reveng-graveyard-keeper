using DG.Tweening;
using UnityEngine;

public class FallingPart : MonoBehaviour
{
	public Transform target_point;

	public InteractionAnimationPreset preset;

	private SpriteRenderer _spr;

	public void StartAnimation(GJCommons.VoidDelegate on_complete)
	{
		if (target_point == null || preset == null)
		{
			return;
		}
		_spr = GetComponent<SpriteRenderer>();
		float duration = preset.duration + Random.Range(0f, preset.duration_random);
		Tweener t = base.transform.DOMoveY(target_point.transform.position.y, duration).OnComplete(delegate
		{
			if (on_complete != null)
			{
				on_complete();
			}
		});
		base.transform.DORotate(new Vector3(0f, 0f, Random.Range(0f - preset.rotation_a, preset.rotation_a)), duration);
		t.OnUpdate(delegate
		{
			if (preset.alpha_curve != null && _spr != null)
			{
				Color color = _spr.color;
				color.a = preset.alpha_curve.Evaluate(t.ElapsedPercentage());
				_spr.color = color;
			}
		});
	}
}
