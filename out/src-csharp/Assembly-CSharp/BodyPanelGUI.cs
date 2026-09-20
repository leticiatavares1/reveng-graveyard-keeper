using UnityEngine;

public class BodyPanelGUI : MonoBehaviour
{
	public UILabel txt_body;

	public UILabel txt_description;

	public UILabel white_skulls;

	public UILabel red_skulls;

	public GameObject skull_labels_back;

	public UI2DSprite spr_body;

	public BodyPanelSkullBarGUI skull_bar;

	public GameObject btn_remove_body;

	public GameObject no_body_go;

	public GamepadNavigationItem button_item;

	public void Draw(Item body)
	{
		spr_body.enabled = true;
		skull_bar.gameObject.SetActive(body != null);
		btn_remove_body.GetComponent<Collider2D>().enabled = body != null && !GlobalCraftControlGUI.is_global_control_active;
		if (txt_description != null)
		{
			txt_description.text = "";
		}
		UIButton[] componentsInChildren = btn_remove_body.GetComponentsInChildren<UIButton>(includeInactive: true);
		foreach (UIButton uIButton in componentsInChildren)
		{
			if (GlobalCraftControlGUI.is_global_control_active)
			{
				uIButton.SetState(UIButtonColor.State.Disabled, immediate: true);
			}
			else
			{
				uIButton.SetState((body == null) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, immediate: true);
			}
		}
		if (body == null)
		{
			if (skull_labels_back != null)
			{
				skull_labels_back.gameObject.SetActive(value: false);
			}
			if (txt_body != null)
			{
				txt_body.text = GJL.L("txt_no_body");
				txt_body.gameObject.SetActive(value: true);
			}
			if (spr_body != null)
			{
				spr_body.sprite2D = EasySpritesCollection.GetSprite("i_body");
			}
			skull_bar.NoBodyRedraw();
			return;
		}
		if (body.is_worker)
		{
			spr_body.sprite2D = EasySpritesCollection.GetSprite(body.worker.GetOnGroundItem().GetIcon());
			if (txt_body != null)
			{
				txt_body.text = body.worker.GetWorkerEfficiencyText();
				txt_body.gameObject.SetActive(!string.IsNullOrEmpty(txt_body.text));
			}
		}
		else
		{
			spr_body.sprite2D = EasySpritesCollection.GetSprite("i_body");
			txt_body.text = GJL.L("body");
			txt_body.gameObject.SetActive(value: true);
		}
		if (!string.IsNullOrEmpty(body.sub_name))
		{
			txt_body.text = body.sub_name;
		}
		body.GetBodySkulls(out skull_bar.negative, out skull_bar.positive, out var _);
		if (skull_labels_back != null)
		{
			skull_labels_back.gameObject.SetActive(value: true);
			red_skulls.text = skull_bar.negative.ToString();
			white_skulls.text = skull_bar.positive.ToString();
		}
		skull_bar.durability = body.durability;
		skull_bar.Redraw();
	}

	public void DrawWorker(WorldGameObject worker, string no_worker_string = "txt_no_linked_worker")
	{
		if (worker != null && !worker.IsWorker())
		{
			worker = null;
		}
		Draw((worker == null) ? null : worker.worker.GetOnGroundItem());
		if (worker == null)
		{
			txt_body.text = "\n\n" + GJL.L(no_worker_string);
		}
		if (no_body_go != null)
		{
			no_body_go.SetActive(worker == null);
		}
	}
}
