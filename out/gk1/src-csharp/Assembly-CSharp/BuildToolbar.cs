using DG.Tweening;
using UnityEngine;

public class BuildToolbar : MonoBehaviour
{
	public enum BuildMode
	{
		Build,
		PickUp,
		Remove
	}

	private const int MAX_BUILD_MODE = 2;

	private const int NORMAL_Y_POS = 20;

	private const int HIDDEN_Y_POS = -30;

	private BuildMode _build_mode;

	private Sequence _seq;

	public void AnimateAppear()
	{
		base.gameObject.SetActive(value: true);
		Init();
	}

	public void AnimateDisppear()
	{
	}

	private void AnimateToolbarsSwap(UIRect.AnchorPoint show, UIRect.AnchorPoint hide)
	{
		show.absolute = -30;
		hide.absolute = 20;
		if (_seq != null)
		{
			_seq.Kill();
		}
		_seq = DOTween.Sequence();
		_seq.Append(DOTween.To(() => hide.absolute, delegate(int x)
		{
			hide.absolute = x;
		}, -30, 0.2f));
		_seq.Append(DOTween.To(() => show.absolute, delegate(int x)
		{
			show.absolute = x;
		}, 20, 0.2f));
	}

	private void Init()
	{
		Redraw();
	}

	private void Redraw()
	{
	}

	public void SwitchTool(int dir)
	{
		int num = (int)(_build_mode + dir);
		if (num < 0)
		{
			num = 2;
		}
		if (num > 2)
		{
			num = 0;
		}
		_build_mode = (BuildMode)num;
		Redraw();
	}

	public BuildMode GetCurrentBuildMode()
	{
		return _build_mode;
	}
}
