using System;

namespace LazyBearTechnology;

[Serializable]
public class LazyBuildType : Enumeration
{
	public static LazyBuildType Release = new LazyBuildType(0);

	public static LazyBuildType Demo = new LazyBuildType(1);

	public static LazyBuildType PS4_Release_US = new LazyBuildType(2);

	public static LazyBuildType PS4_Release_EU = new LazyBuildType(3);

	public static LazyBuildType PS4_Patch_US = new LazyBuildType(4);

	public static LazyBuildType PS4_Patch_EU = new LazyBuildType(5);

	public static LazyBuildType PS5_Release = new LazyBuildType(6);

	public static LazyBuildType PS4_Release_JP = new LazyBuildType(7);

	public static LazyBuildType PS4_Release_ASIA = new LazyBuildType(8);

	public static LazyBuildType PS4_Patch_JP = new LazyBuildType(9);

	public static LazyBuildType PS4_Patch_ASIA = new LazyBuildType(10);

	public static LazyBuildType PS5_Demo = new LazyBuildType(11);

	public static LazyBuildType PS4_Demo_US = new LazyBuildType(12);

	public static LazyBuildType PS4_Demo_Patch_US = new LazyBuildType(13);

	public LazyBuildType(int value)
		: base(value)
	{
	}
}
