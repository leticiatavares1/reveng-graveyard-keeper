using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class CraftNeedsBubbleGUIOLD : TooltipBubbleGUI
{
	public static CraftDefinition last_definition;

	public static CraftNeedsBubbleGUIOLD current;

	[HideInInspector]
	public UITable content_table;

	[HideInInspector]
	public List<BaseItemCellGUI> ingredients;

	public BaseItemCellGUI single_ingredient;

	public UILabel time_label;

	public UILabel output_item_label;

	public UILabel description;

	public UILabel ingredients_label;

	public override void Init()
	{
		ingredients = GetComponentsInChildren<BaseItemCellGUI>(includeInactive: true).ToList();
		ingredients.Remove(single_ingredient);
		content_table = GetComponentInChildren<UITable>(includeInactive: true);
		base.Init();
	}

	protected void ShowDefinition(CraftDefinition craft_definition)
	{
		last_definition = craft_definition;
		MultiInventory multiInventoryForInteraction = MainGame.me.player.GetMultiInventoryForInteraction();
		List<Item> needs = craft_definition.needs;
		if (needs.Count == 1 && needs[0].definition.is_big)
		{
			single_ingredient.Activate();
			single_ingredient.DrawIngredient(needs[0], multiInventoryForInteraction);
			foreach (BaseItemCellGUI ingredient2 in ingredients)
			{
				ingredient2.DrawIngredient(null, multiInventoryForInteraction);
			}
		}
		else
		{
			single_ingredient.Deactivate();
			for (int i = 0; i < 4; i++)
			{
				Item item = ((i < needs.Count) ? needs[i] : null);
				BaseItemCellGUI baseItemCellGUI = ingredients[i];
				if (baseItemCellGUI == null)
				{
					Debug.LogError("CraftIngredientGUI is null, i = " + i, this);
				}
				else
				{
					baseItemCellGUI.DrawIngredient(item, multiInventoryForInteraction);
				}
			}
		}
		ingredients_label.SetActive(needs.Count > 0);
		string text = craft_definition.id;
		if (text.Contains("p:"))
		{
			while (text.Contains(":"))
			{
				text = text.Substring(text.IndexOf(":") + 1);
			}
		}
		output_item_label.text = GJL.L(text);
		ingredients_label.text = GJL.L("ingredients") + GJL.L(":");
		if (craft_definition.craft_time.EvaluateFloat().EqualsTo(0f))
		{
			time_label.Deactivate();
		}
		else
		{
			time_label.text = GJL.L("craft time") + GJL.L(":") + " " + craft_definition.craft_time;
		}
		description.Deactivate();
		base.gameObject.Activate();
		content_table.Reposition();
		content_table.repositionNow = true;
		try_show_down = true;
		OnContentChanged();
		widget = GetComponent<UIWidget>();
		Update();
		if (for_gamepad)
		{
			widget.alpha = 0f;
		}
		GJTimer.AddTimer(0.01f, delegate
		{
			foreach (BaseItemCellGUI ingredient3 in ingredients)
			{
				ReactivateIfNecessary(ingredient3);
			}
			ReactivateIfNecessary(single_ingredient);
		});
	}

	private void ReactivateIfNecessary(BaseItemCellGUI ingredient)
	{
		if (!(ingredient == null) && ingredient.isActiveAndEnabled)
		{
			ingredient.Deactivate();
			ingredient.Activate();
		}
	}

	public override void DestroyBubble()
	{
		if (current == this)
		{
			current = null;
		}
		Object.Destroy(base.gameObject);
	}

	public static TooltipBubbleGUI ShowMessage(CraftDefinition craft_definition, bool for_gamepad, Collider2D tooltip_collider)
	{
		return null;
	}
}
