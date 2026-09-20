using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Fighting/Damage Effect Settings", fileName = "DamageEffectSettings")]
public class DamageEffectSettings : ScriptableObject
{
	[Header("Visual Effect")]
	public string fxName;

	public bool useAdditiveTintColor;

	public Gradient colorGradient;

	[Header("Animation")]
	[Range(0.1f, 2f)]
	public float blinkDuration = 0.3f;

	public Ease blinkEase = Ease.OutBounce;

	[Header("Decals")]
	public bool spawnBloodPaddle;

	public bool spawnGutsDecals;

	public bool spawnBonesDecals;
}
