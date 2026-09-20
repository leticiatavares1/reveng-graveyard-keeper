using System;

public static class Compare
{
	public enum CompareType
	{
		Equal,
		More,
		Less,
		MoreEqual,
		LessEqual,
		NotEqual
	}

	public static bool Floats(CompareType comp, float v1, float v2)
	{
		return comp switch
		{
			CompareType.Equal => Math.Abs(v1 - v2) < 1E-05f, 
			CompareType.Less => v1 < v2, 
			CompareType.LessEqual => v1 <= v2, 
			CompareType.More => v1 > v2, 
			CompareType.MoreEqual => v1 >= v2, 
			CompareType.NotEqual => !Floats(CompareType.Equal, v1, v2), 
			_ => throw new Exception("Unknown compare type: " + comp), 
		};
	}

	public static bool Integers(CompareType comp, int v1, int v2)
	{
		return comp switch
		{
			CompareType.Equal => v1 == v2, 
			CompareType.Less => v1 < v2, 
			CompareType.LessEqual => v1 <= v2, 
			CompareType.More => v1 > v2, 
			CompareType.MoreEqual => v1 >= v2, 
			CompareType.NotEqual => v1 != v2, 
			_ => throw new Exception("Unknown compare type: " + comp), 
		};
	}
}
