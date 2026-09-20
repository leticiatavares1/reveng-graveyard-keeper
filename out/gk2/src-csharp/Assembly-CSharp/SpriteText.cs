using LazyBearTechnology;
using UnityEngine;

public class SpriteText : MonoBehaviour
{
	private const string SPRITE_PREFIX = "sprfont_";

	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private float padding = 5f / 48f;

	public void SetText(string txt)
	{
		SpriteRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer != sprite)
			{
				Object.Destroy(spriteRenderer.gameObject);
			}
		}
		sprite.gameObject.SetActive(value: false);
		float num = 0f;
		for (int num2 = txt.Length - 1; num2 >= 0; num2--)
		{
			char c = txt[num2];
			SpriteRenderer spriteRenderer2 = Object.Instantiate(sprite, sprite.transform.parent);
			spriteRenderer2.gameObject.SetActive(value: true);
			spriteRenderer2.transform.localPosition = new Vector3(num, 0f);
			num -= padding;
			spriteRenderer2.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("sprfont_" + c);
		}
	}
}
