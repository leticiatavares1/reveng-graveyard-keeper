using System.Collections;
using DG.Tweening;
using UnityEngine;

public class NewBodyArrivedGUI : MonoBehaviour
{
	[SerializeField]
	private float _appear_time = 0.4f;

	[SerializeField]
	private float _display_time = 1f;

	[SerializeField]
	private float _hide_time = 0.2f;

	[SerializeField]
	[Space]
	private float _visible_point_y = 30f;

	[SerializeField]
	private float _default_point_y = 40f;

	[SerializeField]
	private GameObject _parent_object;

	public void Display()
	{
		if (!base.gameObject.activeSelf)
		{
			if (!_parent_object.activeSelf)
			{
				_parent_object.gameObject.SetActive(value: true);
			}
			base.gameObject.SetActive(value: true);
			StartCoroutine(RunAppearCoroutine());
		}
	}

	private IEnumerator RunAppearCoroutine()
	{
		bool is_done = false;
		base.transform.DOLocalMoveY((float)Screen.height / 4f - _visible_point_y, _appear_time).OnComplete(delegate
		{
			is_done = true;
		});
		yield return new WaitUntil(() => is_done);
		yield return new WaitForSeconds(_display_time);
		base.transform.DOLocalMoveY((float)Screen.height / 4f + _default_point_y, _hide_time).OnComplete(OnAnimationComplete);
	}

	private void OnAnimationComplete()
	{
		base.gameObject.SetActive(value: false);
	}
}
