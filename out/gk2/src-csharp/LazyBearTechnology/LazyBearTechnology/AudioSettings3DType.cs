using System;

namespace LazyBearTechnology;

[Serializable]
public class AudioSettings3DType : Enumeration
{
	public static AudioSettings3DType Default = new AudioSettings3DType(0);

	public static AudioSettings3DType Default3D = new AudioSettings3DType(1);

	public static AudioSettings3DType SoundZone3D = new AudioSettings3DType(2);

	public static AudioSettings3DType Fight3D3D = new AudioSettings3DType(3);

	public static AudioSettings3DType Workbench3D = new AudioSettings3DType(4);

	public AudioSettings3DType(int value)
		: base(value)
	{
	}
}
