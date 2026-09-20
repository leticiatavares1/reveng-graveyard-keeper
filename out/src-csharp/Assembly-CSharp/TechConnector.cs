using SmartPools;
using UnityEngine;

public class TechConnector : MonoBehaviour
{
	public TechTreeGUIItem tech_1;

	public TechTreeGUIItem tech_2;

	public GameObject go_horiz;

	public GameObject go_diag_up;

	public GameObject go_diag_down;

	public Vector2 p1;

	public Vector2 p2;

	public UIWidget w1;

	public UIWidget w2;

	public static TechConnector Create(TechTreeGUIItem t1, TechTreeGUIItem t2)
	{
		TechConnector techConnector = SmartPooler.CreateObject<TechConnector>();
		techConnector.name = t1.tech_id + " -> " + t2.tech_id;
		techConnector.tech_1 = t1;
		techConnector.tech_2 = t2;
		if (t1 == null || t2 == null)
		{
			return techConnector;
		}
		techConnector.transform.SetParent(MainGame.me.gui_elements.tech_tree.content.gameObject.transform, worldPositionStays: true);
		techConnector.transform.localPosition = Vector3.zero;
		techConnector.transform.localScale = Vector3.one;
		techConnector.p1 = t1.gameObject.transform.localPosition + t1.pos2.localPosition;
		techConnector.p2 = t2.gameObject.transform.localPosition + t2.pos1.localPosition;
		techConnector.w1.transform.localPosition = techConnector.p1;
		techConnector.w2.transform.localPosition = techConnector.p2;
		bool flag = Mathf.Abs(techConnector.p1.y - techConnector.p2.y) < 3f;
		techConnector.go_horiz.SetActive(flag);
		if (flag)
		{
			techConnector.go_diag_up.SetActive(value: false);
			techConnector.go_diag_down.SetActive(value: false);
		}
		else
		{
			bool flag2 = techConnector.p1.y < techConnector.p2.y;
			techConnector.go_diag_up.SetActive(flag2);
			techConnector.go_diag_down.SetActive(!flag2);
		}
		return techConnector;
	}

	public void SetState(TechDefinition.TechState state)
	{
		UI2DSprite[] componentsInChildren = GetComponentsInChildren<UI2DSprite>(includeInactive: true);
		foreach (UI2DSprite uI2DSprite in componentsInChildren)
		{
			string text = uI2DSprite.sprite2D.name.Replace("_act", "");
			if (uI2DSprite.depth <= -150)
			{
				uI2DSprite.depth += 50;
			}
			if (state == TechDefinition.TechState.Purchased)
			{
				text += "_act";
			}
			else
			{
				uI2DSprite.depth -= 50;
			}
			uI2DSprite.sprite2D = EasySpritesCollection.GetSprite(text);
		}
	}

	public void Hide()
	{
		SmartPooler.DestroyObject(this);
	}
}
