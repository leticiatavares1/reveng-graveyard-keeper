using System;

public static class LazyTerrainMath
{
	public static uint FloatToUint(float value)
	{
		return BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
	}

	public static float UintToFloat(uint value)
	{
		return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
	}

	public static float PackShaderValue(int x, int y, int textureId)
	{
		return (float)x + (float)y * 100f + (float)textureId * 10000f + 1f;
	}

	public static void UnpackShaderValue(float v, out int x, out int y, out int textureId)
	{
		textureId = (int)(v / 10000f);
		y = (int)(v % 10000f / 100f);
		x = (int)(v % 100f);
	}
}
