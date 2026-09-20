using System;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeConnector : MonoBehaviour
{
	[SerializeField]
	private TechTreeConnectorElement angleDownUp;

	[SerializeField]
	private TechTreeConnectorElement angleUpDown;

	[SerializeField]
	private TechTreeConnectorElement horizontal;

	[SerializeField]
	private TechTreeConnectorElement verticalDown;

	[SerializeField]
	private TechTreeConnectorElement verticalUp;

	private LazyScrollableElement parent;

	private LazyScrollableElement child;

	private bool isConnectorActive;

	private bool isBackground;

	private bool useRepWidgetSourceSprites;

	private TreeElementBaseWidgetData ParentData => parent.Data as TreeElementBaseWidgetData;

	private TreeElementBaseWidgetData ChildData => child.Data as TreeElementBaseWidgetData;

	public bool IsConnectorActive => isConnectorActive;

	public bool IsBackground => isBackground;

	private void CalculateAndDisplayLine()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		Vector2 rightConnectorPos = ParentData.rightConnectorPos;
		Vector2 leftConnectorPos = ChildData.leftConnectorPos;
		if (rightConnectorPos.x == leftConnectorPos.x)
		{
			if (rightConnectorPos.y < leftConnectorPos.y)
			{
				rightConnectorPos = ParentData.upConnectorPos;
				leftConnectorPos = ChildData.downConnectorPos;
				flag3 = true;
				float y = leftConnectorPos.y - rightConnectorPos.y;
				verticalUp.RectTransform.sizeDelta = new Vector2(verticalUp.RectTransform.sizeDelta.x, y) + verticalUp.extraSize;
				base.transform.localPosition = ParentData.upConnectorPos;
			}
			else if (rightConnectorPos.y > leftConnectorPos.y)
			{
				rightConnectorPos = ParentData.downConnectorPos;
				leftConnectorPos = ChildData.upConnectorPos;
				flag4 = true;
				float y2 = rightConnectorPos.y - leftConnectorPos.y;
				verticalDown.RectTransform.sizeDelta = new Vector2(verticalDown.RectTransform.sizeDelta.x, y2) + verticalDown.extraSize;
				base.transform.localPosition = ParentData.downConnectorPos;
			}
		}
		else
		{
			if (rightConnectorPos.y == leftConnectorPos.y)
			{
				flag5 = true;
				horizontal.RectTransform.sizeDelta = new Vector2(leftConnectorPos.x - rightConnectorPos.x, horizontal.RectTransform.sizeDelta.y);
			}
			else if (rightConnectorPos.y < leftConnectorPos.y)
			{
				flag = true;
				float y3 = Mathf.Abs(leftConnectorPos.y - rightConnectorPos.y);
				float x = leftConnectorPos.x - rightConnectorPos.x;
				angleDownUp.RectTransform.sizeDelta = new Vector2(x, y3) + angleDownUp.extraSize;
			}
			else if (rightConnectorPos.y > leftConnectorPos.y)
			{
				flag2 = true;
				float y4 = Mathf.Abs(rightConnectorPos.y - leftConnectorPos.y);
				float x2 = leftConnectorPos.x - rightConnectorPos.x;
				angleUpDown.RectTransform.sizeDelta = new Vector2(x2, y4) + angleUpDown.extraSize;
			}
			base.transform.localPosition = ParentData.rightConnectorPos;
		}
		if (angleDownUp.gameObject.activeSelf != flag)
		{
			angleDownUp.gameObject.SetActive(flag);
		}
		if (verticalUp.gameObject.activeSelf != flag3)
		{
			verticalUp.gameObject.SetActive(flag3);
		}
		if (verticalDown.gameObject.activeSelf != flag4)
		{
			verticalDown.gameObject.SetActive(flag4);
		}
		if (angleUpDown.gameObject.activeSelf != flag2)
		{
			angleUpDown.gameObject.SetActive(flag2);
		}
		if (horizontal.gameObject.activeSelf != flag5)
		{
			horizontal.gameObject.SetActive(flag5);
		}
	}

	public void Draw(LazyScrollableElement parent, LazyScrollableElement child, bool isBackground)
	{
		this.isBackground = isBackground;
		this.parent = parent;
		this.child = child;
		useRepWidgetSourceSprites = parent.Data is TechTreeCharReputationWidgetData;
		UpdateState();
		CalculateAndDisplayLine();
		base.gameObject.SetActive(value: true);
	}

	public void UpdateState()
	{
		if (ParentData.techDef != null)
		{
			UpdateTechState();
		}
		else
		{
			UpdateQuestState();
		}
	}

	public void UpdateTechState()
	{
		switch (ParentData.techDef.TechState)
		{
		case TechState.Hidden:
			angleDownUp.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			angleUpDown.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			horizontal.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			verticalUp.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			verticalDown.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			isConnectorActive = false;
			break;
		case TechState.Visible:
			angleDownUp.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			angleUpDown.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			horizontal.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			verticalUp.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			verticalDown.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			isConnectorActive = false;
			break;
		case TechState.Available:
			angleDownUp.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			angleUpDown.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			horizontal.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			verticalUp.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			verticalDown.SetupAsInactive(isBackground, useRepWidgetSourceSprites);
			isConnectorActive = false;
			break;
		case TechState.Unlocked:
			angleDownUp.SetupAsActive(isBackground, useRepWidgetSourceSprites);
			angleUpDown.SetupAsActive(isBackground, useRepWidgetSourceSprites);
			horizontal.SetupAsActive(isBackground, useRepWidgetSourceSprites);
			verticalUp.SetupAsActive(isBackground, useRepWidgetSourceSprites);
			verticalDown.SetupAsActive(isBackground, useRepWidgetSourceSprites);
			isConnectorActive = true;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		angleDownUp.image.type = Image.Type.Sliced;
		angleUpDown.image.type = Image.Type.Sliced;
		horizontal.image.type = Image.Type.Sliced;
		verticalUp.image.type = Image.Type.Sliced;
		verticalDown.image.type = Image.Type.Sliced;
	}

	public void UpdateQuestState()
	{
		switch (ParentData.questData.ViewStatus)
		{
		case QuestViewStatus.Visible:
			angleDownUp.SetupAsActive(isBackground);
			angleUpDown.SetupAsActive(isBackground);
			horizontal.SetupAsActive(isBackground);
			verticalUp.SetupAsActive(isBackground);
			verticalDown.SetupAsActive(isBackground);
			isConnectorActive = false;
			break;
		case QuestViewStatus.Revealed:
			angleDownUp.SetupAsActive(isBackground);
			angleUpDown.SetupAsActive(isBackground);
			horizontal.SetupAsActive(isBackground);
			verticalUp.SetupAsActive(isBackground);
			verticalDown.SetupAsActive(isBackground);
			isConnectorActive = false;
			break;
		case QuestViewStatus.Completed:
			angleDownUp.SetupAsActive(isBackground);
			angleUpDown.SetupAsActive(isBackground);
			horizontal.SetupAsActive(isBackground);
			verticalUp.SetupAsActive(isBackground);
			verticalDown.SetupAsActive(isBackground);
			isConnectorActive = true;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		angleDownUp.image.type = Image.Type.Sliced;
		angleUpDown.image.type = Image.Type.Sliced;
		horizontal.image.type = Image.Type.Sliced;
		verticalUp.image.type = Image.Type.Sliced;
		verticalDown.image.type = Image.Type.Sliced;
	}
}
