using UnityEngine;

public class NGUIAnimCurves : MonoBehaviour
{
	private static NGUIAnimCurves _instance;

	public AnimationCurve color;

	public AnimationCurve alpha;

	public AnimationCurve size;

	public AnimationCurve position;

	public static NGUIAnimCurves me => _instance;

	private void Awake()
	{
		_instance = this;
	}
}
