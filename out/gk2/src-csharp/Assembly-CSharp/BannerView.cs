using System;
using System.Collections.Generic;
using UnityEngine;

public class BannerView : MonoBehaviour
{
	[Serializable]
	private class BannerVariation
	{
		[SerializeField]
		public string id = "";

		[SerializeField]
		public GameObject gameObject;

		private Cloth cloth;

		public Cloth Cloth => cloth ?? (cloth = gameObject.GetComponentInChildren<Cloth>());
	}

	[SerializeField]
	private List<BannerVariation> variations = new List<BannerVariation>();

	private BannerVariation currentVariation;

	public bool IsVisible => base.gameObject.activeSelf;

	public string GetCurVariationId()
	{
		return variations.Find((BannerVariation x) => x.gameObject.activeSelf).id;
	}

	public void Show(bool isEnabled, string id = "")
	{
		if (!isEnabled)
		{
			base.gameObject.SetActive(value: false);
			currentVariation = null;
			return;
		}
		foreach (BannerVariation variation in variations)
		{
			if (variation.id == id)
			{
				variation.gameObject.SetActive(value: true);
				currentVariation = variation;
			}
			else
			{
				variation.gameObject.SetActive(value: false);
			}
		}
		base.gameObject.SetActive(value: true);
	}

	public void SetEnabledClothFading(bool isEnabled)
	{
		if (currentVariation != null)
		{
			currentVariation.Cloth.SetEnabledFading(isEnabled);
		}
	}
}
