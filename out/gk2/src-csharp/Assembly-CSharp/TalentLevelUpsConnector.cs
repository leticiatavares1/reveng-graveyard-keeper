using System;
using UnityEngine;
using UnityEngine.UI;

public class TalentLevelUpsConnector : MonoBehaviour
{
	[SerializeField]
	private TalentLevelUpsConnectorElement angleDownLeft;

	[SerializeField]
	private TalentLevelUpsConnectorElement angleDownRight;

	[SerializeField]
	private TalentLevelUpsConnectorElement verticalDown;

	[SerializeField]
	private TalentLevelUpsConnectorElement horizontalLeft;

	[SerializeField]
	private TalentLevelUpsConnectorElement horizontalRight;

	private TalentLevelUpWidget parent;

	private TalentLevelUpWidget child;

	private TalentConnectorType connectorType;

	private bool isBackground;

	private TalentLevelUpWidgetData ParentData => parent.Data;

	private TalentLevelUpWidgetData ChildData => child.Data;

	public TalentConnectorType TalentConnectorType => connectorType;

	private void CalculateAndDisplayLine()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		Vector2 downConnectorPos = ParentData.downConnectorPos;
		Vector2 upConnectorPos = ChildData.upConnectorPos;
		if (downConnectorPos.x < upConnectorPos.x)
		{
			if (downConnectorPos.y == upConnectorPos.y)
			{
				downConnectorPos = ParentData.rightConnectorPos;
				upConnectorPos = ChildData.leftConnectorPos;
				flag4 = true;
				float y = horizontalRight.RectTransform.sizeDelta.y;
				float x = upConnectorPos.x - downConnectorPos.x;
				horizontalRight.RectTransform.sizeDelta = new Vector2(x, y) + horizontalRight.extraSize;
				base.transform.localPosition = ParentData.rightConnectorPos;
			}
			else
			{
				downConnectorPos = ParentData.downConnectorPos;
				upConnectorPos = ChildData.upConnectorPos;
				flag3 = true;
				float y2 = downConnectorPos.y - upConnectorPos.y;
				float x2 = upConnectorPos.x - downConnectorPos.x;
				angleDownRight.RectTransform.sizeDelta = new Vector2(x2, y2) + angleDownRight.extraSize;
				base.transform.localPosition = ParentData.downConnectorPos;
			}
		}
		else if (downConnectorPos.x > upConnectorPos.x)
		{
			if (downConnectorPos.y == upConnectorPos.y)
			{
				downConnectorPos = ParentData.leftConnectorPos;
				upConnectorPos = ChildData.rightConnectorPos;
				flag5 = true;
				float y3 = horizontalLeft.RectTransform.sizeDelta.y;
				float x3 = downConnectorPos.x - upConnectorPos.x;
				horizontalLeft.RectTransform.sizeDelta = new Vector2(x3, y3) + horizontalLeft.extraSize;
				base.transform.localPosition = ParentData.leftConnectorPos;
			}
			else
			{
				downConnectorPos = ParentData.downConnectorPos;
				upConnectorPos = ChildData.upConnectorPos;
				flag2 = true;
				float y4 = downConnectorPos.y - upConnectorPos.y;
				float x4 = downConnectorPos.x - upConnectorPos.x;
				angleDownLeft.RectTransform.sizeDelta = new Vector2(x4, y4) + angleDownLeft.extraSize;
				base.transform.localPosition = ParentData.downConnectorPos;
			}
		}
		else
		{
			downConnectorPos = ParentData.downConnectorPos;
			upConnectorPos = ChildData.upConnectorPos;
			flag = true;
			float y5 = downConnectorPos.y - upConnectorPos.y;
			verticalDown.RectTransform.sizeDelta = new Vector2(verticalDown.RectTransform.sizeDelta.x, y5) + verticalDown.extraSize;
			base.transform.localPosition = ParentData.downConnectorPos;
		}
		if (angleDownLeft.gameObject.activeSelf != flag2)
		{
			angleDownLeft.gameObject.SetActive(flag2);
		}
		if (angleDownRight.gameObject.activeSelf != flag3)
		{
			angleDownRight.gameObject.SetActive(flag3);
		}
		if (verticalDown.gameObject.activeSelf != flag)
		{
			verticalDown.gameObject.SetActive(flag);
		}
		if (horizontalLeft.gameObject.activeSelf != flag5)
		{
			horizontalLeft.gameObject.SetActive(flag5);
		}
		if (horizontalRight.gameObject.activeSelf != flag4)
		{
			horizontalRight.gameObject.SetActive(flag4);
		}
	}

	public void Draw(TalentLevelUpWidget parent, TalentLevelUpWidget child, bool isBackground)
	{
		this.parent = parent;
		this.child = child;
		this.isBackground = isBackground;
		UpdateState();
		CalculateAndDisplayLine();
		base.gameObject.SetActive(value: true);
	}

	public void UpdateState()
	{
		UpdateTechState();
	}

	public void UpdateTechState()
	{
		switch (ParentData.State)
		{
		case TalentLevelUpDef.State.Unknown:
			angleDownLeft.SetupAsInactive(isBackground);
			angleDownRight.SetupAsInactive(isBackground);
			verticalDown.SetupAsInactive(isBackground);
			connectorType = TalentConnectorType.Inactive;
			break;
		case TalentLevelUpDef.State.Visible:
			angleDownLeft.SetupAsInactive(isBackground);
			angleDownRight.SetupAsInactive(isBackground);
			verticalDown.SetupAsInactive(isBackground);
			connectorType = TalentConnectorType.Inactive;
			break;
		case TalentLevelUpDef.State.Available:
			angleDownLeft.SetupAsInactive(isBackground);
			angleDownRight.SetupAsInactive(isBackground);
			verticalDown.SetupAsInactive(isBackground);
			connectorType = TalentConnectorType.Inactive;
			break;
		case TalentLevelUpDef.State.Unlocked:
			angleDownLeft.SetupAsActive(isBackground);
			angleDownRight.SetupAsActive(isBackground);
			verticalDown.SetupAsActive(isBackground);
			connectorType = TalentConnectorType.Active;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case TalentLevelUpDef.State.Hidden:
			break;
		}
		angleDownLeft.image.type = Image.Type.Sliced;
		angleDownRight.image.type = Image.Type.Sliced;
		verticalDown.image.type = Image.Type.Sliced;
	}
}
