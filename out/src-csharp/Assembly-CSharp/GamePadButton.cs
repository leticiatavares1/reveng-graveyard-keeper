using System;

[Flags]
public enum GamePadButton
{
	None = 0,
	B = 1,
	A = 2,
	X = 4,
	Y = 8,
	Left = 0x10,
	Right = 0x20,
	Up = 0x40,
	Down = 0x80,
	LB = 0x100,
	RB = 0x200,
	Back = 0x400,
	Start = 0x800,
	DUp = 0x1000,
	DDown = 0x2000,
	DLeft = 0x4000,
	DRight = 0x8000,
	LT = 0x10000,
	RT = 0x20000
}
