using System;

[Serializable]
public struct UIMouseTooltipEdges
{
	public float left;

	public float right;

	public float top;

	public float bottom;

	public bool IsZero
	{
		get
		{
			if (left == 0f && right == 0f && top == 0f)
			{
				return bottom == 0f;
			}
			return false;
		}
	}

	public UIMouseTooltipEdges(float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
	{
		this.left = left;
		this.right = right;
		this.top = top;
		this.bottom = bottom;
	}

	public static UIMouseTooltipEdges All(float value)
	{
		return new UIMouseTooltipEdges(value, value, value, value);
	}
}
