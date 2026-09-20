using System;
using LazyBearTechnology;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class UITutorialArrow : LazySingleton<UITutorialArrow>, ILazyGUIElement
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Transform insideObj;

	[SerializeField]
	private float timeK = 7f;

	[SerializeField]
	private float timeK2 = 4f;

	[SerializeField]
	private float timeK3 = 4f;

	[SerializeField]
	private float widthScreenDivider = 6.6f;

	[SerializeField]
	private float heightScreenDivider = 6.4f;

	private float width;

	private float height;

	private WgoData wgoData;

	public void Init()
	{
		base.gameObject.SetActive(value: false);
		MainGame.OnGoToMainMenu = (Action)Delegate.Combine(MainGame.OnGoToMainMenu, new Action(UnAttach));
		canvas.overrideSorting = true;
		canvas.sortingOrder = 51;
	}

	public void Attach(WgoData wgo)
	{
		if (wgo != null)
		{
			wgoData = wgo;
			base.gameObject.SetActive(value: true);
			MainGame.PlayerData.tutorialArrowWgoId = wgoData.UniqueId;
		}
	}

	public void Attach(string wgoCustomTag)
	{
		Attach(MainGame.Instance.GameSave.WorldData.GetWgoDataByCustomTag(wgoCustomTag));
	}

	public void UnAttach()
	{
		MainGame.PlayerData.tutorialArrowWgoId = null;
		wgoData = null;
		base.gameObject.SetActive(value: false);
	}

	private void LateUpdate()
	{
		Vector3 bubblePos = wgoData.BubblePos;
		base.transform.position = CameraSystem.WorldToScreenPoint(bubblePos);
		Vector2 vector = base.transform.localPosition;
		Vector2 vector2 = vector;
		width = (float)Screen.width / widthScreenDivider;
		height = (float)Screen.height / heightScreenDivider;
		bool flag = false;
		if (vector.x > width)
		{
			vector.x = width;
			flag = true;
		}
		else if (vector.x < 0f - width)
		{
			vector.x = 0f - width;
			flag = true;
		}
		if (vector.y > height)
		{
			vector.y = height;
			flag = true;
		}
		else if (vector.y < 0f - height)
		{
			vector.y = 0f - height;
			flag = true;
		}
		if (flag)
		{
			base.transform.localPosition = vector;
			base.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vector2.y, vector2.x) * 57.29578f + 90f);
		}
		else
		{
			base.transform.localRotation = Quaternion.identity;
		}
		insideObj.localPosition = new Vector3(0f, Mathf.Sin(Time.time * timeK) * timeK2 + timeK3);
	}
}
