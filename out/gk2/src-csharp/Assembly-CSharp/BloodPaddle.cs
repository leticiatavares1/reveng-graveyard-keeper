using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;

public class BloodPaddle : MonoBehaviour, IFightDecal
{
	private const float X_SCALE = 1.25f;

	private const float Z_SCALE = 0.8f;

	public const float MAX_FOOTPRINT_SIZE = 1.76f;

	public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float scaleDuration = 10f;

	private float scalingProgress;

	public float Lifetime { get; set; }

	public float TimeToLive { get; private set; }

	public IFightDecal SpawnDecal(Vector3 position, Direction orientation, string customDeathEffectId = "")
	{
		Vector3 position2 = RaycastUtils.TrySnapToTheGround(position, 1f, 10f);
		position2 += VisualConsts.GetLayerOffset(2);
		base.transform.position = position2;
		Rotate(orientation);
		Lifetime = 0f;
		scalingProgress = 0f;
		DOTween.To(() => scalingProgress, delegate(float x)
		{
			scalingProgress = x;
			base.transform.localScale = Vector3.one * scaleCurve.Evaluate(scalingProgress);
		}, scaleDuration * 0.1f, scaleDuration);
		return this;
	}

	private void Rotate(Direction direction)
	{
		Vector2 vector = direction.ConvertToVector2XZ();
		base.transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(vector.y, vector.x) * 57.29578f, 0f);
		switch (direction)
		{
		case Direction.Up:
		case Direction.Down:
			base.transform.localScale = Vector3.one;
			break;
		case Direction.Right:
		case Direction.Left:
			base.transform.localScale = new Vector3(1.25f, 1f, 0.8f);
			break;
		}
	}

	private void Awake()
	{
		TimeToLive = LazySingletonSO<GlobalResources>.Instance.fighting.decalsLifeTime;
	}
}
