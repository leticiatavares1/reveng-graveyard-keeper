using System.Collections.Generic;
using UnityEngine;

public class FishUnderwaterGfx : MonoBehaviour
{
	public enum FishGfxState
	{
		None,
		Losing,
		Pulling
	}

	[SerializeField]
	private GameObject bobGameObject;

	[SerializeField]
	private List<GameObject> fishGfxObjects = new List<GameObject>();

	[SerializeField]
	private FishGfxUnderwaterType fishGfxType;

	private Animator curFishAnimator;

	private static readonly int fishStateAnimator = Animator.StringToHash("fish_state");

	public FishGfxState FishState { get; private set; }

	public void UpdateGfx(FishGfxUnderwaterType type = FishGfxUnderwaterType.None)
	{
		fishGfxType = type;
		fishGfxObjects.ForEach(delegate(GameObject obj)
		{
			obj.SetActive(value: false);
		});
		int num = -1;
		switch (type)
		{
		case FishGfxUnderwaterType.Small:
			num = 0;
			break;
		case FishGfxUnderwaterType.Big:
			num = 1;
			break;
		case FishGfxUnderwaterType.Snake:
			num = 2;
			break;
		case FishGfxUnderwaterType.Squid:
			num = 3;
			break;
		case FishGfxUnderwaterType.Frog:
			num = 4;
			break;
		}
		if (num != -1)
		{
			GameObject obj2 = fishGfxObjects[num];
			obj2.SetActive(value: true);
			if (obj2.TryGetComponent<Animator>(out var component))
			{
				curFishAnimator = component;
			}
		}
		else
		{
			curFishAnimator = null;
		}
	}

	public void SetFishState(FishGfxState state)
	{
		if ((bool)curFishAnimator)
		{
			curFishAnimator.SetInteger(fishStateAnimator, (int)state);
		}
	}

	private void Update()
	{
		bobGameObject.transform.position = base.transform.position;
	}
}
