using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GrassAnimation : MonoBehaviour, ICustomUpdateMonoBehaviour
{
	public List<GrassDeformSprite> sprs;

	private bool _animated;

	private Vector3 _shake_target = Vector3.zero;

	public float duration = 2.5f;

	public float strength = 0.3f;

	public int vibrato = 5;

	public float rnd = 10f;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		StartAnimation();
	}

	public void StartAnimation()
	{
		if (!_animated)
		{
			_shake_target = Vector3.zero;
			_animated = true;
			CustomUpdateManager.updates.Add(this);
			DOTween.Shake(() => _shake_target, delegate(Vector3 x)
			{
				_shake_target = x;
			}, duration, new Vector3(strength, strength), vibrato, rnd).OnComplete(delegate
			{
				_animated = false;
				CustomUpdateManager.updates.Remove(this);
			});
		}
	}

	public void CustomUpdate()
	{
		if (!_animated || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		foreach (GrassDeformSprite spr in sprs)
		{
			if (spr.gameObject.activeInHierarchy)
			{
				spr.skew = _shake_target.x;
				spr.UpdateRenderer();
			}
		}
	}

	public void OnDestroy()
	{
		CustomUpdateManager.updates.Remove(this);
	}
}
