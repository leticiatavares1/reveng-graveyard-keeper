using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TalentLevelUpDef : BalanceBaseObject
{
	public enum State
	{
		Unknown,
		Hidden,
		Visible,
		Available,
		Unlocked
	}

	public enum LockType
	{
		All,
		Any
	}

	[AutoParse("icon")]
	public string icon;

	[AutoParse("talent")]
	public string talentId;

	[AutoParse("add_level")]
	public int talentValueAdd;

	[AutoParse("talent_exp_points_price")]
	public int talentExpPointsPrice;

	[AutoParse("perk")]
	public string linkedPerk;

	[AutoParse("is_zombie_perk")]
	public bool isZombiePerk;

	[AutoParse("is_unknown")]
	public bool isUnknown;

	[AutoParse("is_hidden")]
	public bool isHidden;

	[AutoParse("available_at_start")]
	public bool availableAtStart;

	[AutoParse("is_free")]
	public bool isFreeCoordinates;

	[AutoParse("parents")]
	public List<string> parents = new List<string>();

	[AutoParse("tech_r")]
	public int techRed;

	[AutoParse("tech_g")]
	public int techGreen;

	[AutoParse("tech_b")]
	public int techBlue;

	[AutoParse("expression_on_buy")]
	public List<LazyExpression> expressionsOnBuy = new List<LazyExpression>();

	[AutoParse("xpos")]
	[SerializeField]
	private float posX;

	[AutoParse("ypos")]
	[SerializeField]
	private float posY;

	public LockType lockType;

	[NonSerialized]
	public List<TalentLevelUpDef> childDefinitionList = new List<TalentLevelUpDef>();

	[NonSerialized]
	public List<TalentLevelUpDef> parentDefinitionList = new List<TalentLevelUpDef>();

	public Vector2 TreePos => new Vector2(posX, posY);

	public Sprite Icon => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(icon);

	public int ZombieTechPoints
	{
		get
		{
			if (techRed > 0)
			{
				return techRed;
			}
			if (techBlue > 0)
			{
				return techBlue;
			}
			if (techGreen > 0)
			{
				return techGreen;
			}
			return 1;
		}
	}

	public string ZombieTechPointsIcon
	{
		get
		{
			if (techRed > 0)
			{
				return "tech_red".FontIcon();
			}
			if (techBlue > 0)
			{
				return "tech_blue".FontIcon();
			}
			if (techGreen > 0)
			{
				return "tech_green".FontIcon();
			}
			return string.Empty;
		}
	}

	public bool ParentsUnlocked
	{
		get
		{
			if (isZombiePerk)
			{
				Debug.LogError("Do not request ParentsUnlocked for TalentLevelUpDef with id:[" + id + "], it is for zombie!!!");
				return false;
			}
			bool result = false;
			switch (lockType)
			{
			case LockType.All:
				result = true;
				foreach (string parent in parents)
				{
					if (!MainGame.Instance.GameSave.talentSystemData.GetTalentBranch(talentId).studiedLevelUps.Contains(parent))
					{
						result = false;
						break;
					}
				}
				break;
			case LockType.Any:
				foreach (string parent2 in parents)
				{
					if (MainGame.Instance.GameSave.talentSystemData.GetTalentBranch(talentId).studiedLevelUps.Contains(parent2))
					{
						result = true;
					}
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return result;
		}
	}

	public static void Link()
	{
		foreach (TalentLevelUpDef talentLevelUpDef in GameBalance.Me.talentLevelUpDefs)
		{
			talentLevelUpDef.InitParentsAndChildren();
		}
	}

	private void InitParentsAndChildren()
	{
		for (int i = 0; i < parents.Count; i++)
		{
			string text = parents[i];
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			TalentLevelUpDef data = GameBalance.Me.GetData<TalentLevelUpDef>(text);
			if (data == null)
			{
				Debug.LogError("no TalentLevelUpDef with id: [" + text + "]");
				continue;
			}
			if (!data.childDefinitionList.Contains(this))
			{
				data.childDefinitionList.Add(this);
			}
			if (!parentDefinitionList.Contains(data))
			{
				parentDefinitionList.Add(data);
			}
		}
	}
}
