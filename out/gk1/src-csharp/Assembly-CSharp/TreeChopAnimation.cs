using DG.Tweening;
using UnityEngine;

public class TreeChopAnimation : InteractionAnimation
{
	public float duration = 0.5f;

	public float z_rotation = 4f;

	public override void DoAction()
	{
		base.transform.DOShakeRotation(duration, new Vector3(0f, 0f, z_rotation));
	}
}
