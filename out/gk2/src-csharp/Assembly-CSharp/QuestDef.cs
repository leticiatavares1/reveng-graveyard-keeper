using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class QuestDef : BalanceBaseObject
{
	public bool isHidden;

	[AutoParse("is_unknown")]
	public bool isUnknown;

	[AutoParse("custom_triggers")]
	public List<ExpressionGameRes> customTriggers = new List<ExpressionGameRes>();

	[AutoParse("execute_start")]
	public List<LazyExpression> execExpressionsStart = new List<LazyExpression>();

	[AutoParse("execute_start_fail")]
	public List<LazyExpression> execExpressionsStartFail = new List<LazyExpression>();

	[AutoParse("execute_finish")]
	public List<LazyExpression> execExpressionsFinish = new List<LazyExpression>();

	[AutoParse("parents")]
	public List<string> parents = new List<string>();

	[AutoParse("brother_ids")]
	public List<string> brotherIds = new List<string>();

	[AutoParse("xpos")]
	[SerializeField]
	private int posX;

	[AutoParse("ypos")]
	[SerializeField]
	private int posY;

	[AutoParse("phase")]
	public int phase;

	public bool hasPosInBalance;

	[AutoParse("icon_id")]
	public string iconId;

	[AutoParse("wgo_npc_id")]
	public string wgoNpcId;

	[AutoParse("rep_visualisation")]
	public GameRes repVisualisationRes;

	public QuestCheck startCheck;

	public QuestFinishCheck finishCheck;

	[NonSerialized]
	public List<QuestDef> childDefinitionList = new List<QuestDef>();

	[NonSerialized]
	public List<QuestDef> parentDefinitionList = new List<QuestDef>();

	public Sprite Icon => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(iconId, "default_quest_icon");

	public Sprite Portrait
	{
		get
		{
			if (LinkedWgo != null)
			{
				return LinkedWgo.Portrait;
			}
			return LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("default_quest_icon");
		}
	}

	private WGODef LinkedWgo => GameBalance.Me.GetData<WGODef>(wgoNpcId);

	public Vector2Int TreePos => new Vector2Int(posX, posY);

	public static void LinkQuests()
	{
		foreach (QuestDef questDef in GameBalance.Me.questDefs)
		{
			questDef.InitParentsAndChildren();
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
			QuestDef data = GameBalance.Me.GetData<QuestDef>(text);
			if (data == null)
			{
				Debug.LogError("no quest definition with id: [" + text + "]");
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
