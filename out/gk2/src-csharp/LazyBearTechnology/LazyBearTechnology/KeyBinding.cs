using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class KeyBinding : BaseBinding
{
	public KeyCode keyCode;

	public KeyCode[] additionalKeyCodes;
}
