using System;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UICreditsWindow : LazyWindow<LazyWidgetDataBase>
{
	private const int EndGameSortingOrder = 30000;

	private static (string, string)[] groupTeams = new(string, string)[2]
	{
		("Game by", "Lazy Bear Games"),
		("Published by", "tinyBuild")
	};

	private static string lazyBearTeamString = "Lazy Bear Games team:";

	private static (string, string)[] groupLazyBear = new(string, string)[31]
	{
		("Nikita Kulaga", "Creative Director\nScriptwriter"),
		("Slava Cherkasov", "Technical Director"),
		("Bulat Zaripov", "Producer\nLead Game Designer"),
		("Roman Belikov", "Senior Game Designer"),
		("Evgenii Emelianov", "Senior Game Designer\nSound Designer"),
		("Aleksander Mukha", "Junior Game Designer"),
		("", ""),
		("Artur Akhmadeev", "Technical Lead"),
		("Ilia Filippov", "Senior Programmer\nConsole Programmer"),
		("Vladislav Kornienko", "Senior Programmer"),
		("Vladislav Fomenko", "Senior Technical Game Designer"),
		("Andrei Skriabin", "Technical Game Designer\nProgrammer"),
		("", ""),
		("Pavel Malakhov", "Lead Artist"),
		("Aleksandr Minichev", "UI Lead\nArt Style Heritage Holder\n3D Modeler"),
		("Aleksei Zaikin", "Animation Lead"),
		("Aleksei Nikolaev", "Artist"),
		("Daniil Yamsya", "Artist"),
		("Vadim Minnebaev", "Artist\n3D Modeler"),
		("Timur Bulatov", "Artist\n3D Modeler"),
		("Dmitry Vetrov", "Artist\n3D Modeler"),
		("Ekaterina Gorbunova", "Artist\nAnimator"),
		("", ""),
		("Justas Gabrusenas", "Junior QA"),
		("Stanislav Rozhdestvenskiy", "Senior QA"),
		("", ""),
		("Alexey Nechaev", "Sound Designer"),
		("Igor Chernyshev", "Sound Designer"),
		("Hamza El Hamri", "Music Composer"),
		("Lilija Kulaga", "Financial Director"),
		("", "")
	};

	private static string translationTeamString = "Localization:";

	private static (string, string)[] groupTranslation = new(string, string)[9]
	{
		("Steve Breslin", "English Proofreading"),
		("Thomas Faust", "German Localization"),
		("Words of Magic", "French Localization"),
		("Letícia Araujo", "Brazilian Portuguese Localization"),
		("Ramón Méndez", "Spanish Localization"),
		("Alba Calvo", "Spanish Localization"),
		("Ainhoa García", "Spanish Localization"),
		("Javier Llópiz", "Spanish Localization"),
		("", "")
	};

	private static string translationTeamAkebonoString = "Akebono Translation Service:";

	private static (string, string)[] groupTranslationAkebono = new(string, string)[7]
	{
		("Loek van Kooten", "CJK Localization Project Management"),
		("Niu Pengfei", "Chinese Translation"),
		("Aya Pickard", "Japanese Translation"),
		("Rumi Tasaki", "Japanese Translation"),
		("Cindy Kim", "Korean Translation"),
		("Lois Yang", "Korean Translation"),
		("", "")
	};

	private static string fromTheVoidString = "From the Void:";

	private static (string, string)[] groupFromTheVoid = new(string, string)[13]
	{
		("Marc Eybert-Guillon", "Director & Project Manager"),
		("Alicja Kaniecka", "Polish Localization"),
		("Agnieszka Chęcińska", "Polish Localization"),
		("Wojciech Brudziński", "Polish Localization"),
		("Natalia Paterek", "Polish Localization"),
		("Ebru Yılmaz Akca", "Turkish Localization"),
		("Nazaret Poyraz", "Turkish Localization"),
		("Nehir Durmuşoğlu", "Turkish Localization"),
		("Onur Küçük", "Turkish Localization"),
		("Dmitry Kornyukhov", "Russian Localization"),
		("Arty Ra", "Russian Localization"),
		("Indy", "Team Mascot"),
		("", "")
	};

	private static string omukString = "Cast and Recorded at OMUK:";

	private static (string, string)[] groupOmuk = new(string, string)[19]
	{
		("Thomas Mitchells", "Voice Director"),
		("Freda D'Souza", "Casting Director"),
		("Mia Coffield", "Casting Assistant"),
		("Lukas Jakubenas", "Dialogue Recording"),
		("James Hazel", "Dialogue Recording"),
		("Josh Hayward", "Dialogue Recording"),
		("Lukas Jakubenas", "Dialogue Editing"),
		("James Hazel", "Dialogue Editing"),
		("Tom Murton", "Dialogue Editing"),
		("Joel Douglas", "Dialogue Editing"),
		("Marcy Jensen", "Dialogue Editing"),
		("Tabby Griffiths", "Dialogue Editing"),
		("Joshua Hayward", "Dialogue Editing"),
		("Josh Hayward", "Dialogue Mastering"),
		("Josh Hayward", "Audio Manager"),
		("Freda D'Souza", "Production Manager"),
		("Mia Coffield", "Production Assistant"),
		("Creative Dialogue Tools", "Dialogue Production tech"),
		("", "")
	};

	private static string voiceOverString = "Voiceover:";

	private static (string, string)[] groupVoiceOver = new(string, string)[18]
	{
		("Adam Longworth", "Workshop Foreman, Jack"),
		("Alex Jordan", "Looters leader, Davy Dagger\nScout_2"),
		("Anna Cass", "Goddess of Nature"),
		("Beth Robb Adams", "Port Club owner, Linda"),
		("Billie Fulford Brown", "Nun, Aghata"),
		("Chris Tester", "Head of the Guards, Herbert\nTrademaster\nMonk\nSoldier_1\nCarpenter_1\nVilage trader, Herm\nHomobonus"),
		("Joseph Capp", "Old God\nBlack screen\nZombie\nBrother Bishop\nAstrologer, Gunter"),
		("Lisa Graydon", "Fairy, Soul\nKeepers Wife"),
		("Matthew Biddulph", "Plague Doctor, Albert\nMirror\nWoodcarver"),
		("Neil Roberts", "Tavern owner, Januarius\nHead of the Vilage, Jully"),
		("Peter Warnock", "Village guard_2\nRefugee_2\nSome Guy"),
		("Phil Rowe", "Comrad Donkey\nMerc Leader"),
		("Shaun Mendum", "Heffry the Twin\nJeffry the Twin\nVillage guard_1"),
		("Shogo Miyakita", "Bandit, Sven\nSoldier_2\nCarpenter_2"),
		("Stu McLoughlin", "Larry\nGarry\nCaptain\nScout_1"),
		("Barry McStay", "Refugee_1"),
		("Tobias Weatherburn", "Dig\nBandit, Hans\nFisherman\nMain Hero (The Keeper)\nAlter Keeper\nNobel Knight"),
		("", "")
	};

	private static string tinyBuildTeamString = "tinyBuild Publishing:";

	private static (string, string)[] groupTinyBuild = new(string, string)[47]
	{
		("Alex Nichiporchik", "CEO"),
		("Giasone Salati", "Chief Financial Officer"),
		("Annette Patent", "Executive Assistant"),
		("Corey Caplan", "Director of Business Development and Partner Licensing"),
		("Carla Woo", "Director, Contract Management"),
		("Michael Kuzmin", "Sales Director"),
		("Artem Bochkarev", "Head of Publishing"),
		("Mike Rafiienko", "Executive Producer"),
		("Anton Pavlov", "Executive Producer"),
		("Dmitry Yashanov", "Executive Producer"),
		("Vladimir Tolmachev", "Producer"),
		("Aleksei Glebov", "Producer"),
		("Artyom Safarov", "Producer"),
		("Nadya Zhuk", "Associate Producer"),
		("Anton Daty", "Senior Marketing Manager"),
		("Sergey Smirnov", "Marketing Manager"),
		("Arnaud Richard", "Marketing Manager"),
		("Koen Rebel", "Head of Influencer Management"),
		("Vera Lubbers", "Senior Influencer Manager"),
		("George Kulko", "Senior Community Manager"),
		("James Croucher", "Senior Community Manager"),
		("Tina Benoit", "Community Manager"),
		("Shunise Wise", "Social Media Marketing Manager"),
		("Francesca Falcini", "Social Media Marketing Manager"),
		("Tori Gerbeshi", "Social Media Marketing Manager"),
		("Anna Bienek", "Social Media Marketing Manager"),
		("Artem Peganov", "User Acquisition Manager"),
		("Aleksandra Akimova", "Porting Producer"),
		("Alex Leoveanu", "LiveOps Producer"),
		("Artem Chernyshev", "Head of Development Services"),
		("Artem Pozhilenkov", "Release Manager"),
		("Bradley Manning", "Release Manager"),
		("Lisa Sidorova", "Localization Manager"),
		("Alina Aliabieva", "Localization Manager"),
		("Jacky Motta", "Discord Manager"),
		("Kimon Sklavounos", "Customer Support Manager"),
		("Oleksandr Striuk", "Customer Support Manager"),
		("Tiberiu Cristea", "QA Manager"),
		("George Popa", "QA Manager"),
		("Igor Surov", "Head of Media Production"),
		("Christina Osipova", "Media Project Manager"),
		("Nikita Varnakov", "Trailer Producer"),
		("Alexander Isaev", "Trailer Producer"),
		("Eugene Chataev", "Video Editor"),
		("Tatiana Kurguzova", "Graphic Designer"),
		("Olga Barlet", "Head of HR"),
		("Valerii Zotov", "System Administrator\nDevOps Engineer")
	};

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private UICreditsWindowElement prefab;

	[SerializeField]
	private GameObject spacePrefab;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private UIDialogWindowButton lazyButton;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private float scrollSpeed = 12f;

	[SerializeField]
	private float fadeTime = 0.5f;

	private float tweenTime = 165f;

	private Tween tween;

	private UIDialogWindowData.ButtonData btnData;

	private bool isContentCreated;

	private bool shouldGoToMenuOnReturn;

	private bool openAfterGameComplete;

	private bool creditsCameraEnabled;

	private bool isSortingOrderCached;

	private bool cachedOverrideSorting;

	private bool isBackButtonShown;

	private int cachedSortingOrder;

	private bool isScrollRectInsetsCached;

	private bool endGameScrollRectInsetsApplied;

	private Vector2 cachedScrollOffsetMin;

	private Vector2 cachedScrollOffsetMax;

	private RectOffset cachedContentPadding;

	private RectTransform firstElementRect;

	public RectTransform CameraTrackRect => firstElementRect;

	protected override void TestDraw()
	{
	}

	public override void Open(LazyWidgetDataBase data)
	{
		base.Open(data);
		RestoreEndGameScrollRectInsets();
		InitializeBackButton();
		canvasGroup.alpha = 0f;
		canvasGroup.DOKill();
		canvasGroup.DOFade(1f, fadeTime);
		if (CreateContentIfNeeded())
		{
			if (tween != null && tween.active)
			{
				tween.Kill();
			}
			scrollRect.StopMovement();
			scrollRect.verticalNormalizedPosition = 1f;
			tweenTime = CalculateTweenTime();
			if (tweenTime <= 0f)
			{
				Debug.LogWarning("UICreditsWindow scroll distance is zero. Credits auto-scroll was not started.");
				return;
			}
			tween = scrollRect.DOVerticalNormalizedPos(0f, tweenTime);
			tween.SetEase(Ease.Linear);
			tween.OnComplete(ReturnToMainMenu);
		}
	}

	public void OpenAfterGameComplete(bool shouldGoToMenuOnReturn, Action onClosed)
	{
		Open(null, delegate
		{
			onClosed?.Invoke();
		});
		openAfterGameComplete = true;
		this.shouldGoToMenuOnReturn = shouldGoToMenuOnReturn;
		ApplyEndGameSortingOrder();
		ApplyEndGameScrollRectInsets();
		InitializeBackButton(showImmediately: false);
		backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, shouldGoToMenuOnReturn ? 0f : 0.5f);
		canvasGroup.alpha = 0f;
		canvasGroup.DOKill();
		canvasGroup.DOFade(1f, fadeTime);
		if (CreateContentIfNeeded())
		{
			ApplyEndGameContentStartFromViewportBottom();
			if (tween != null && tween.active)
			{
				tween.Kill();
			}
			scrollRect.StopMovement();
			scrollRect.verticalNormalizedPosition = 1f;
			tweenTime = CalculateTweenTime();
			EnableCreditsCamera();
			if (tweenTime <= 0f)
			{
				Debug.LogWarning("UICreditsWindow scroll distance is zero. Credits auto-scroll was not started.");
				return;
			}
			tween = scrollRect.DOVerticalNormalizedPos(0f, tweenTime);
			tween.SetEase(Ease.Linear);
			tween.OnComplete(ReturnToMainMenu);
		}
	}

	public override void Close()
	{
		DisableCreditsCamera();
		RestoreEndGameSortingOrder();
		RestoreEndGameScrollRectInsets();
		base.Close();
	}

	public void ShowBackButton()
	{
		if (!isBackButtonShown)
		{
			lazyButton.gameObject.SetActive(value: true);
			btnData = new UIDialogWindowData.ButtonData(OnPressBtn, LLBase.L("tip_back"), null, replaceForGamepad: true, GameKey.Back);
			lazyButton.Draw(btnData);
			isBackButtonShown = true;
		}
	}

	public void HideBackButton()
	{
		if (isBackButtonShown)
		{
			lazyButton.gameObject.SetActive(value: false);
			isBackButtonShown = false;
		}
	}

	private void InitializeBackButton(bool showImmediately = true)
	{
		lazyButton.gameObject.SetActive(showImmediately);
		if (showImmediately)
		{
			btnData = new UIDialogWindowData.ButtonData(OnPressBtn, LLBase.L("tip_back"), null, replaceForGamepad: true, GameKey.Back);
			lazyButton.Draw(btnData);
			isBackButtonShown = true;
		}
		else
		{
			isBackButtonShown = false;
		}
	}

	private void EnableCreditsCamera()
	{
		CreditsCameraController.TryEnable(this);
		creditsCameraEnabled = true;
	}

	private void DisableCreditsCamera()
	{
		if (creditsCameraEnabled)
		{
			creditsCameraEnabled = false;
			CreditsCameraController.TryDisable();
		}
	}

	private void ApplyEndGameSortingOrder()
	{
		if (!(base.Canvas == null))
		{
			if (!isSortingOrderCached)
			{
				cachedOverrideSorting = base.Canvas.overrideSorting;
				cachedSortingOrder = base.Canvas.sortingOrder;
				isSortingOrderCached = true;
			}
			base.Canvas.overrideSorting = true;
			base.Canvas.sortingOrder = 30000;
		}
	}

	private void RestoreEndGameSortingOrder()
	{
		if (isSortingOrderCached)
		{
			base.Canvas.overrideSorting = cachedOverrideSorting;
			base.Canvas.sortingOrder = cachedSortingOrder;
			isSortingOrderCached = false;
		}
	}

	private void ApplyEndGameScrollRectInsets()
	{
		RectTransform scrollRectTransform = GetScrollRectTransform();
		if (!(scrollRectTransform == null))
		{
			CacheScrollRectInsetsIfNeeded(scrollRectTransform);
			scrollRectTransform.offsetMin = Vector2.zero;
			scrollRectTransform.offsetMax = Vector2.zero;
			endGameScrollRectInsetsApplied = true;
		}
	}

	private void ApplyEndGameContentStartFromViewportBottom()
	{
		VerticalLayoutGroup contentLayoutGroup = GetContentLayoutGroup();
		RectTransform viewport = GetViewport();
		if (!(contentLayoutGroup == null) && !(content == null) && !(viewport == null))
		{
			Canvas.ForceUpdateCanvases();
			int num = Mathf.RoundToInt(viewport.rect.height);
			contentLayoutGroup.padding = new RectOffset(0, 0, num, num);
			content.RefreshContentFitterAndDisable();
		}
	}

	private void RestoreEndGameScrollRectInsets()
	{
		if (endGameScrollRectInsetsApplied)
		{
			RectTransform scrollRectTransform = GetScrollRectTransform();
			if (scrollRectTransform != null && isScrollRectInsetsCached)
			{
				scrollRectTransform.offsetMin = cachedScrollOffsetMin;
				scrollRectTransform.offsetMax = cachedScrollOffsetMax;
			}
			VerticalLayoutGroup contentLayoutGroup = GetContentLayoutGroup();
			if (contentLayoutGroup != null && cachedContentPadding != null)
			{
				contentLayoutGroup.padding = new RectOffset(cachedContentPadding.left, cachedContentPadding.right, cachedContentPadding.top, cachedContentPadding.bottom);
			}
			if (isContentCreated && content != null)
			{
				content.RefreshContentFitterAndDisable();
			}
			endGameScrollRectInsetsApplied = false;
		}
	}

	private RectTransform GetViewport()
	{
		if (scrollRect != null && scrollRect.viewport != null)
		{
			return scrollRect.viewport;
		}
		return GetScrollRectTransform();
	}

	private void CacheScrollRectInsetsIfNeeded(RectTransform scrollRectTransform)
	{
		if (!isScrollRectInsetsCached)
		{
			cachedScrollOffsetMin = scrollRectTransform.offsetMin;
			cachedScrollOffsetMax = scrollRectTransform.offsetMax;
			VerticalLayoutGroup contentLayoutGroup = GetContentLayoutGroup();
			if (contentLayoutGroup != null)
			{
				RectOffset padding = contentLayoutGroup.padding;
				cachedContentPadding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
			}
			isScrollRectInsetsCached = true;
		}
	}

	private RectTransform GetScrollRectTransform()
	{
		if (scrollRect == null)
		{
			return null;
		}
		return (RectTransform)scrollRect.transform;
	}

	private VerticalLayoutGroup GetContentLayoutGroup()
	{
		if (content == null && scrollRect != null)
		{
			content = scrollRect.content;
		}
		if (content == null)
		{
			return null;
		}
		return content.GetComponent<VerticalLayoutGroup>();
	}

	private bool CreateContentIfNeeded()
	{
		if (isContentCreated)
		{
			return true;
		}
		if (content == null && scrollRect != null)
		{
			content = scrollRect.content;
		}
		if (content == null)
		{
			Debug.LogError("UICreditsWindow content is not set.");
			return false;
		}
		if (prefab == null)
		{
			Debug.LogError("UICreditsWindow element prefab is not set.");
			return false;
		}
		if (spacePrefab == null)
		{
			Debug.LogError("UICreditsWindow space prefab is not set.");
			return false;
		}
		ClearContent();
		AddRows(groupTeams);
		AddSpace();
		AddText(lazyBearTeamString);
		AddSpace();
		AddRows(groupLazyBear);
		AddSpace();
		AddText(translationTeamString);
		AddSpace();
		AddRows(groupTranslation);
		AddSpace();
		AddText(translationTeamAkebonoString);
		AddSpace();
		AddRows(groupTranslationAkebono);
		AddSpace();
		AddText(fromTheVoidString);
		AddSpace();
		AddRows(groupFromTheVoid);
		AddSpace();
		AddText(omukString);
		AddSpace();
		AddRows(groupOmuk);
		AddSpace();
		AddText(voiceOverString);
		AddSpace();
		AddRows(groupVoiceOver);
		AddSpace();
		AddText(tinyBuildTeamString);
		AddSpace();
		AddRows(groupTinyBuild);
		content.RefreshContentFitterAndDisable();
		isContentCreated = true;
		return true;
	}

	private void ClearContent()
	{
		firstElementRect = null;
		for (int num = content.childCount - 1; num >= 0; num--)
		{
			Transform child = content.GetChild(num);
			child.SetParent(null, worldPositionStays: false);
			UnityEngine.Object.Destroy(child.gameObject);
		}
	}

	private void AddRows((string leftText, string rightText)[] rows)
	{
		foreach ((string, string) row in rows)
		{
			AddRow(row);
		}
	}

	private void AddRow((string leftText, string rightText) row)
	{
		if (string.IsNullOrEmpty(row.leftText) && string.IsNullOrEmpty(row.rightText))
		{
			AddSpace();
			return;
		}
		UICreditsWindowElement uICreditsWindowElement = AddElement();
		if (!(uICreditsWindowElement == null))
		{
			uICreditsWindowElement.DrawRow(row.leftText, row.rightText);
		}
	}

	private void AddText(string text)
	{
		UICreditsWindowElement uICreditsWindowElement = AddElement();
		if (!(uICreditsWindowElement == null))
		{
			uICreditsWindowElement.DrawCenter(text);
		}
	}

	private UICreditsWindowElement AddElement()
	{
		UICreditsWindowElement uICreditsWindowElement = UnityEngine.Object.Instantiate(prefab, content);
		uICreditsWindowElement.gameObject.SetActive(value: true);
		if (firstElementRect == null)
		{
			firstElementRect = (RectTransform)uICreditsWindowElement.transform;
		}
		return uICreditsWindowElement;
	}

	private void AddSpace()
	{
		UnityEngine.Object.Instantiate(spacePrefab, content).SetActive(value: true);
	}

	private float CalculateTweenTime()
	{
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(content);
		RectTransform viewport = GetViewport();
		float num = Mathf.Max(content.rect.height, LayoutUtility.GetPreferredHeight(content));
		float num2 = Mathf.Max(0f, num - viewport.rect.height);
		if (num2 <= 0f)
		{
			return 0f;
		}
		return num2 / Mathf.Max(0.01f, scrollSpeed);
	}

	private void OnPressBtn()
	{
		OnPressedBack();
	}

	protected override bool OnPressedBack()
	{
		if (!lazyButton.gameObject.activeSelf || !isBackButtonShown)
		{
			return false;
		}
		if (tween != null && tween.active)
		{
			tween.Kill();
		}
		ReturnToMainMenu();
		return true;
	}

	private void ReturnToMainMenu()
	{
		if (openAfterGameComplete)
		{
			openAfterGameComplete = false;
			Close();
			if (shouldGoToMenuOnReturn)
			{
				shouldGoToMenuOnReturn = false;
				MainGame.Instance.GoToMenu(null, skipFadeIn: true, FadeFlag.All);
			}
		}
		else
		{
			MainGame.Instance.SetMainMenuInfoPanelEnabled(isEnabled: true);
			LazyUI.GetWindow<UIMainMenuWindow>().Open(null);
			Close();
		}
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Clear();
	}
}
