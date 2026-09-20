using LazyBearTechnology;
using UnityEngine;

[ExecuteInEditMode]
public class FishingContainer : MonoBehaviour
{
	[SerializeField]
	private RopeRenderer rope;

	[SerializeField]
	private Transform castStartPoint;

	[SerializeField]
	private RopePoint bob;

	[SerializeField]
	private AnimationComponent animationComponent;

	[SerializeField]
	private FishUnderwaterGfx fishUnderwaterGfx;

	[SerializeField]
	private GameObject signGgameObject;

	public FishUnderwaterGfx FishUnderwaterGfx => fishUnderwaterGfx;

	public RopeRenderer Rope => rope;

	public RopePoint Bob => bob;

	public Transform CastStartPoint => castStartPoint;

	public AnimationComponent AnimationComponent => animationComponent;

	public GameObject SignGameObject => signGgameObject;

	private void OnEnable()
	{
		if (signGgameObject != null)
		{
			signGgameObject.SetActive(value: false);
		}
	}

	public void ResetColor()
	{
		SetRopeColor(LazySingletonSerializedSO<FishingSettings>.Instance.ropeGradient.Evaluate(0f));
	}

	public void SetRopeColor(Color newColor)
	{
		rope.SetColor(newColor);
	}

	public void SetRopeEmission(float intensity)
	{
		rope.SetEmission(intensity);
	}
}
