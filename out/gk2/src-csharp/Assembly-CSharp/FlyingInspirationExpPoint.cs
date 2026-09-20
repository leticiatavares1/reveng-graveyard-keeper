using System;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class FlyingInspirationExpPoint : MonoBehaviour, IPoolable
{
	[SerializeField]
	private TextMeshProUGUI label;

	private Pool pool;

	private Tween moveTween;

	private Action onReachedDestination;

	private bool isReleased;

	public void Fly(Vector3 from, Vector3 to, string icon, Pool pool, float duration, Action onReachedDestination)
	{
		this.pool = pool;
		this.onReachedDestination = onReachedDestination;
		isReleased = false;
		EnsureLabel();
		if (!(label == null))
		{
			label.raycastTarget = false;
			label.tintAllSprites = false;
			label.text = EnsureSpriteUntinted(icon);
			base.transform.position = from;
			base.transform.localScale = Vector3.one;
			if (base.transform is RectTransform rectTransform)
			{
				rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
				rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
				rectTransform.pivot = new Vector2(0.5f, 0.5f);
				rectTransform.position = from;
			}
			base.gameObject.SetActive(value: true);
			label.color = Color.white;
			float delay = UnityEngine.Random.Range(0f, 0.12f);
			EnsureTweenStopped();
			Sequence sequence = DOTween.Sequence().SetLink(base.gameObject, LinkBehaviour.KillOnDisable);
			sequence.Append(base.transform.DOMove(to, duration).SetEase(Ease.InOutCubic));
			sequence.Join(base.transform.DOScale(1.1f, duration * 0.4f).SetEase(Ease.OutCubic));
			sequence.Insert(duration * 0.4f, base.transform.DOScale(0.8f, duration * 0.6f).SetEase(Ease.InCubic));
			sequence.SetDelay(delay);
			sequence.OnComplete(OnReachedDestination);
			moveTween = sequence;
		}
	}

	public void StopAndRelease()
	{
		if (!isReleased)
		{
			onReachedDestination = null;
			EnsureTweenStopped();
			ReleaseToPool();
		}
	}

	public void OnPoolableObjReleased()
	{
		EnsureTweenStopped();
		onReachedDestination = null;
	}

	private void OnReachedDestination()
	{
		EnsureTweenStopped();
		Action action = onReachedDestination;
		onReachedDestination = null;
		ReleaseToPool();
		action?.Invoke();
	}

	private void OnDisable()
	{
		onReachedDestination = null;
		EnsureTweenStopped();
	}

	private void EnsureLabel()
	{
		if (label == null)
		{
			label = GetComponent<TextMeshProUGUI>();
		}
	}

	private static string EnsureSpriteUntinted(string icon)
	{
		if (string.IsNullOrEmpty(icon) || icon.Contains("tint="))
		{
			return icon;
		}
		return icon.Replace("\">", "\" tint=0>");
	}

	private void ReleaseToPool()
	{
		if (!isReleased)
		{
			isReleased = true;
			pool?.ReleaseObject(this);
		}
	}

	private void EnsureTweenStopped()
	{
		Tween tween = moveTween;
		moveTween = null;
		if (tween != null && tween.IsActive())
		{
			tween.Kill();
		}
	}
}
