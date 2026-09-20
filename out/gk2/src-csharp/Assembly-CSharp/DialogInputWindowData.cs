using System;
using LazyBearTechnology;

public class DialogInputWindowData : LazyWidgetDataBase
{
	public Action<string, float> OnButtonPressed { get; private set; }

	public string HeaderText { get; private set; }

	public string ButtonText { get; private set; }

	public DialogInputWindowData(string headerText, string buttonText, Action<string, float> onButtonPressed)
	{
		HeaderText = headerText;
		ButtonText = buttonText;
		OnButtonPressed = onButtonPressed;
	}
}
