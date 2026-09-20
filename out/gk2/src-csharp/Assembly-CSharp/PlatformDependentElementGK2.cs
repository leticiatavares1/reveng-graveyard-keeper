using LazyBearTechnology;

public class PlatformDependentElementGK2 : LazyPlatformDependentElement
{
	public bool rtLightInUse = true;

	public bool rtLightNotInUse = true;

	public override bool Init()
	{
		bool flag = base.Init();
		flag = ((!SwitchLightPolicy.UseLightRT) ? (flag & rtLightNotInUse) : (flag & rtLightInUse));
		base.gameObject.SetActive(flag);
		base.IsActive = flag;
		return flag;
	}
}
