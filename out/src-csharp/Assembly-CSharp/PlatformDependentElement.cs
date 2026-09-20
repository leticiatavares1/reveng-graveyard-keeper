using UnityEngine;

public class PlatformDependentElement : MonoBehaviour
{
	public bool ignore;

	[Space]
	[Space]
	[Header("Show when:")]
	public bool controller_gamepad = true;

	public bool controller_mouse = true;

	[Space]
	public bool platform_pc = true;

	public bool platform_any_console = true;

	public bool platform_xbox = true;

	public bool platform_switch = true;

	public bool platform_ps = true;

	public void Init(bool for_gamepad)
	{
		if (!ignore)
		{
			bool flag = true;
			flag &= platform_pc;
			flag = ((!for_gamepad) ? (flag & controller_mouse) : (flag & controller_gamepad));
			base.gameObject.SetActive(flag);
		}
	}
}
