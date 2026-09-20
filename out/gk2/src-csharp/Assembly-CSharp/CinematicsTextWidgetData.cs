using System;
using LazyBearTechnology;
using UnityEngine;

public class CinematicsTextWidgetData : LazyWidgetDataBase
{
	public Action<string> OnTextChanged { get; set; }

	public Action<bool> OnVisibleChanged { get; set; }

	public Action<Color> OnColorChanged { get; set; }

	public Vector2 Position { get; set; }

	public Vector2 Size { get; set; }

	public Color Color { get; set; } = Color.white;


	public Vector2 Pivot { get; set; } = new Vector2(0.5f, 0.5f);


	public bool ShowBackground { get; set; }

	public CinematicsTextWidgetData(Vector2 position, Vector2 size)
	{
		Position = position;
		Size = size;
	}
}
