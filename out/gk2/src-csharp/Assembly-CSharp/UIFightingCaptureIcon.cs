using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIFightingCaptureIcon : MonoBehaviour, IPoolable
{
	[SerializeField]
	private Image image;

	private bool isSubscribed;

	private FightingCapturePoint capturePoint;

	public FightingCapturePoint CapturePoint => capturePoint;

	public void Draw(FightingCapturePoint capturePoint)
	{
		if (isSubscribed)
		{
			Unsubscribe();
		}
		this.capturePoint = capturePoint;
		Subscribe();
		DrawCurrentPoint();
	}

	private void DrawCurrentPoint()
	{
		if (capturePoint.OwnedByTeam == LazyConsts.Fighting.TeamType.Player)
		{
			if (capturePoint.isBasePoint)
			{
				image.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("icon-flag_main-blue");
			}
			else
			{
				image.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("icon-flag_secondary-blue");
			}
		}
		else if (capturePoint.isBasePoint)
		{
			image.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("icon-flag_main-red");
		}
		else
		{
			image.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("icon-flag_secondary-red");
		}
		image.SetNativeSize();
	}

	private void HandleStateChanged(FightingCapturePoint capturePoint)
	{
		DrawCurrentPoint();
	}

	private void Subscribe()
	{
		if (!isSubscribed && capturePoint != null)
		{
			capturePoint.OnCapturedByTeam += HandleStateChanged;
			isSubscribed = true;
		}
	}

	private void Unsubscribe()
	{
		if (isSubscribed && capturePoint != null)
		{
			capturePoint.OnCapturedByTeam -= HandleStateChanged;
		}
		isSubscribed = false;
	}

	public void OnPoolableObjReleased()
	{
		Unsubscribe();
	}
}
