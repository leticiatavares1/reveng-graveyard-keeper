using UnityEngine;

public class SpriteText : MonoBehaviour
{
	public SpriteRenderer prefab;

	public float padding = 5f / 48f;

	public string sprite_prefix = "sprfont_";

	public void SetText(string txt)
	{
		SpriteRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer != prefab)
			{
				Object.Destroy(spriteRenderer.gameObject);
			}
		}
		prefab.gameObject.SetActive(value: false);
		float num = 0f;
		for (int num2 = txt.Length - 1; num2 >= 0; num2--)
		{
			char c = txt[num2];
			SpriteRenderer spriteRenderer2 = Object.Instantiate(prefab, prefab.transform.parent);
			spriteRenderer2.gameObject.SetActive(value: true);
			spriteRenderer2.transform.localPosition = new Vector3(num, 0f);
			num -= padding;
			spriteRenderer2.sprite = EasySpritesCollection.GetSprite(sprite_prefix + c);
		}
	}
}
