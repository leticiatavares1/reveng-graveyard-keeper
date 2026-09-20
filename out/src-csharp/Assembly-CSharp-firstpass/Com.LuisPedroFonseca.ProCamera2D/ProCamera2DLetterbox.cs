using System.Collections;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D;

[ExecuteInEditMode]
public class ProCamera2DLetterbox : MonoBehaviour
{
	[Range(0f, 0.5f)]
	public float Amount;

	public Color Color;

	private Material _material;

	private Material material
	{
		get
		{
			if (_material == null)
			{
				_material = new Material(Shader.Find("Hidden/ProCamera2D/Letterbox"));
				_material.hideFlags = HideFlags.HideAndDontSave;
			}
			return _material;
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if (Mathf.Approximately(Amount, 0f) || material == null)
		{
			Graphics.Blit(sourceTexture, destTexture);
			return;
		}
		Amount = Mathf.Clamp01(Amount);
		material.SetFloat("_Top", 1f - Amount);
		material.SetFloat("_Bottom", Amount);
		material.SetColor("_Color", Color);
		Graphics.Blit(sourceTexture, destTexture, material);
	}

	private void OnDisable()
	{
		if ((bool)_material)
		{
			Object.DestroyImmediate(_material);
		}
	}

	public void TweenTo(float targetAmount, float duration)
	{
		StopAllCoroutines();
		StartCoroutine(TweenToRoutine(targetAmount, duration));
	}

	private IEnumerator TweenToRoutine(float targetAmount, float duration)
	{
		float initialAmount = Amount;
		float t = 0f;
		while (t <= 1f)
		{
			t += Time.deltaTime / duration;
			Amount = Utils.EaseFromTo(initialAmount, targetAmount, t, EaseType.EaseOut);
			yield return null;
		}
		Amount = targetAmount;
		yield return null;
	}
}
