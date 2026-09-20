using System;

namespace LazyBearTechnology;

[Serializable]
public class VoiceID : Enumeration
{
	public static VoiceID None = new VoiceID(0);

	public static VoiceID Larry = new VoiceID(1);

	public static VoiceID Donkey = new VoiceID(2);

	public static VoiceID Feather = new VoiceID(3);

	public static VoiceID AlbertCured = new VoiceID(4);

	public static VoiceID AlbertSick = new VoiceID(5);

	public static VoiceID Captain = new VoiceID(6);

	public static VoiceID Carpenter2 = new VoiceID(7);

	public static VoiceID Dig = new VoiceID(8);

	public static VoiceID Fairy = new VoiceID(9);

	public static VoiceID Fisherman = new VoiceID(10);

	public static VoiceID Garry = new VoiceID(11);

	public static VoiceID GoddessNature = new VoiceID(12);

	public static VoiceID Gunter = new VoiceID(13);

	public static VoiceID Hans = new VoiceID(14);

	public static VoiceID Heffry = new VoiceID(15);

	public static VoiceID HerbertHeadGuards = new VoiceID(16);

	public static VoiceID JackWorkshopForeman = new VoiceID(17);

	public static VoiceID Januarius = new VoiceID(18);

	public static VoiceID Jeffry = new VoiceID(19);

	public static VoiceID Jully = new VoiceID(20);

	public static VoiceID Linda = new VoiceID(22);

	public static VoiceID Mirror = new VoiceID(23);

	public static VoiceID OldGod = new VoiceID(24);

	public static VoiceID Refugee2 = new VoiceID(25);

	public static VoiceID Scout1 = new VoiceID(27);

	public static VoiceID Soldier1 = new VoiceID(28);

	public static VoiceID Soldier2 = new VoiceID(29);

	public static VoiceID Sven = new VoiceID(30);

	public static VoiceID VillageGuard = new VoiceID(31);

	public static VoiceID VillageGuard2 = new VoiceID(32);

	public static VoiceID Wife = new VoiceID(33);

	public static VoiceID Woodcarver = new VoiceID(34);

	public static VoiceID Bishop = new VoiceID(35);

	public static VoiceID Agatha = new VoiceID(36);

	public static VoiceID Carpenter1 = new VoiceID(37);

	public static VoiceID Homobonus = new VoiceID(38);

	public static VoiceID LootersLeader = new VoiceID(39);

	public static VoiceID MercLeader = new VoiceID(40);

	public static VoiceID NobelKnight = new VoiceID(41);

	public static VoiceID Refugee1 = new VoiceID(42);

	public static VoiceID Scout2 = new VoiceID(43);

	public static VoiceID Trademaster = new VoiceID(44);

	public static VoiceID RoyalMailBox = new VoiceID(45);

	public VoiceID(int value)
		: base(value)
	{
	}
}
