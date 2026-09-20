using UnityEngine;

public class MultiAnswerOptionGUI : MonoBehaviour
{
	[HideInInspector]
	public UILabel label;

	[HideInInspector]
	public UI2DSprite ld;

	[HideInInspector]
	public UI2DSprite rd;

	[HideInInspector]
	public UI2DSprite lu;

	[HideInInspector]
	public UI2DSprite ru;

	[HideInInspector]
	public UI2DSprite back;

	private UIButton _button;

	private GamepadNavigationItem _gamepad_item;

	private UIWidget _widget;

	[Header("Mouse settings")]
	public int mouse_delta_size = 4;

	public Color mouse_focus_color;

	[Header("Gamepad settings")]
	public int gamepad_delta_size = 8;

	public Color gamepad_focus_color;

	private string _answer_id;

	private MultiAnswerGUI _answer_gui;

	private int _label_width;

	private int _focus_delta_size;

	private int _start_size;

	private Color _default_color;

	public UILabel label_2;

	public UIWidget button_widget;

	[Space(10f)]
	public UI2DSprite price_icon;

	public UIWidget price_widget;

	public UILabel price_label_n;

	public UI2DSprite price_quality_icon;

	public UIWidget price_lock_available;

	public UIWidget price_lock_locked;

	[Space(10f)]
	public UI2DSprite reward_icon;

	public UIWidget reward_widget;

	public UILabel reward_label;

	public UILabel reward_label_n;

	public UI2DSprite reward_quality_icon;

	[Space(10f)]
	public UIWidget lock_widget;

	public UIWidget lock_widget_locked;

	public UIWidget lock_widget_available;

	public UIWidget lock_widget_arrow;

	public UILabel lock_label;

	public UI2DSprite lock_quality_icon;

	[Space(10f)]
	public Color color_disabled;

	public Color color_icon_normal;

	public Color color_icon_not_enough;

	public Color color_text_normal;

	public Color color_text_not_enough;

	public const int DETAILED_OPTION_WIDTH = 150;

	public const int DETAILED_OPTION_HEIGHT = 28;

	private bool _can_be_picked = true;

	private AnswerVisualData _answer_data;

	public Color inside_color_normal;

	public Color inside_color_not_enough;

	private bool _inited;

	private bool _animating;

	private UIWidget _wgt;

	[Header("Multiple Price Lock")]
	public Transform multiple_price_content;

	public Transform multiple_lock_content;

	public MultiAnswerLock multiAnswerLockPrefab;

	public MultiAnswerPrice multiAnswerPricePrefab;

	public UITable table;

	public void Init()
	{
		_inited = true;
		label = GetComponent<UILabel>();
		_wgt = GetComponent<UIWidget>();
		UI2DSprite[] componentsInChildren = GetComponentsInChildren<UI2DSprite>(includeInactive: true);
		foreach (UI2DSprite uI2DSprite in componentsInChildren)
		{
			switch (uI2DSprite.name)
			{
			case "ld":
			case "left down":
				ld = uI2DSprite;
				break;
			case "rd":
			case "right down":
				rd = uI2DSprite;
				break;
			case "lu":
			case "left up":
				lu = uI2DSprite;
				break;
			case "ru":
			case "right up":
				ru = uI2DSprite;
				break;
			case "back":
				back = uI2DSprite;
				break;
			}
		}
		_button = GetComponentInChildren<UIButton>(includeInactive: true);
		_gamepad_item = GetComponent<GamepadNavigationItem>();
		_widget = back.GetComponent<UIWidget>();
	}

	private void Update()
	{
		if (_inited)
		{
			UI2DSprite uI2DSprite = ld;
			UI2DSprite uI2DSprite2 = rd;
			UI2DSprite uI2DSprite3 = lu;
			Color color2 = (ru.color = back.color);
			Color color4 = (uI2DSprite3.color = color2);
			Color color6 = (uI2DSprite2.color = color4);
			uI2DSprite.color = color6;
			if (_animating)
			{
				_wgt.UpdateVisibility(visibleByAlpha: true, visibleByPanel: true);
			}
		}
	}

	public void OnChosen()
	{
		if (!_can_be_picked || _widget.alpha < 0.5f)
		{
			Debug.Log("Reply option can't be clicked, _can_be_picked = " + _can_be_picked);
			return;
		}
		LazyInput.ClearAllKeysDown();
		_can_be_picked = false;
		if (_answer_data != null && _answer_data.link_to_answer_data != null)
		{
			if (_answer_data is MultipleAnswerVisualData)
			{
				for (int i = 0; i < _answer_data.answer_visual_datas.Count; i++)
				{
					MainGame.me.player.RemoveSmartRes(_answer_data.answer_visual_datas[i].link_to_answer_data.d_price);
				}
			}
			else
			{
				MainGame.me.player.RemoveSmartRes(_answer_data.link_to_answer_data.d_price);
			}
			if (_answer_data.link_to_answer_data.HasAnyReward())
			{
				MainGame.me.player.ReceiveSmartRes(_answer_data.link_to_answer_data.d_reward);
			}
		}
		_answer_gui.OnChosen(_answer_id);
		Sounds.OnGUIClick();
	}

	public void OnFocused()
	{
		_button.defaultColor = _button.hover;
		_widget.ChangeSize(_start_size + _focus_delta_size, _widget.height, 0.1f);
		Sounds.OnGUIHover();
	}

	public void OnUnfocused()
	{
		_button.defaultColor = _default_color;
		_widget.ChangeSize(_start_size, _widget.height, 0.1f);
	}

	public void Show(AnswerVisualData answer, bool first_answer, bool last_answer, int width, MultiAnswerGUI answer_gui, float anim_time = 0.3f, float anim_delay = 0f)
	{
		if (!_inited)
		{
			Init();
		}
		if (!first_answer)
		{
			DeactivateCorners(top: true);
		}
		if (!last_answer)
		{
			DeactivateCorners(top: false);
		}
		_answer_id = answer.id;
		_answer_gui = answer_gui;
		_answer_data = answer;
		_animating = false;
		lock_widget_locked.gameObject.SetActive(value: false);
		price_widget.gameObject.SetActive(value: false);
		reward_widget.gameObject.SetActive(value: false);
		multiple_lock_content.gameObject.SetActive(value: false);
		multiple_price_content.gameObject.SetActive(value: false);
		for (int i = 1; i < multiple_lock_content.childCount; i++)
		{
			Object.Destroy(multiple_lock_content.GetChild(i).gameObject);
		}
		for (int j = 1; j < multiple_price_content.childCount; j++)
		{
			Object.Destroy(multiple_price_content.GetChild(j).gameObject);
		}
		lock_label.color = color_text_normal;
		label.width = (_label_width = width);
		label.overflowMethod = UILabel.Overflow.ResizeHeight;
		UILabel uILabel = label_2;
		string text = (label.text = answer.translation);
		uILabel.text = text;
		int height = label.height;
		_can_be_picked = answer.can_be_picked;
		if (answer.IsDetailed())
		{
			label.width = (_label_width = 150);
			bool flag = false;
			label_2.gameObject.SetActive(value: true);
			label.text = "";
			label.ProcessText();
			_wgt.UpdateAnchors();
			label_2.overflowMethod = UILabel.Overflow.ResizeHeight;
			float y = LabelSizeCalculator.Calc(label_2, label_2.text).y;
			int num = 0;
			while ((float)(label.height + 3) < y)
			{
				label.text += "\n";
				label.ProcessText();
				if (++num > 10)
				{
					break;
				}
			}
			bool flag2 = false;
			if (answer is MultipleAnswerVisualData)
			{
				MultipleAnswerVisualData multipleAnswerVisualData = (MultipleAnswerVisualData)answer;
				for (int k = 0; k < multipleAnswerVisualData.answer_visual_datas.Count; k++)
				{
					AnswerVisualData answerVisualData = multipleAnswerVisualData.answer_visual_datas[k];
					MultiAnswerLock multiAnswerLock = Object.Instantiate(multiAnswerLockPrefab, multiple_lock_content);
					MultiAnswerPrice multiAnswerPrice = Object.Instantiate(multiAnswerPricePrefab, multiple_price_content);
					multiAnswerPrice.price_widget.gameObject.SetActive(value: true);
					multiAnswerLock.lock_widget.gameObject.SetActive(value: true);
					if (!string.IsNullOrEmpty(answerVisualData.icon_price) && answerVisualData.icon_price.StartsWith(":"))
					{
						multiAnswerLock.lock_widget_arrow.gameObject.SetActive(value: true);
						multiAnswerLock.lock_widget_locked.gameObject.SetActive(value: false);
						multiAnswerLock.lock_widget_available.gameObject.SetActive(value: false);
						multiple_lock_content.gameObject.SetActive(value: true);
						multiAnswerLock.lock_label.text = answerVisualData.icon_price.Substring(2);
						if (answerVisualData.inside_price_is_red)
						{
							multiAnswerLock.lock_label.color = color_text_not_enough;
						}
					}
					else
					{
						multiAnswerPrice.price_label_n.gameObject.SetActive(value: true);
						if (EasySpritesCollection.SetSpriteOrDisableGameObject(multiAnswerPrice.price_icon, FixIconName(answerVisualData.icon_price)))
						{
							if (answerVisualData.inside_price_is_red && !string.IsNullOrEmpty(answerVisualData.price_txt))
							{
								multiAnswerPrice.price_label_n.text = answerVisualData.price_txt;
							}
							else
							{
								multiAnswerPrice.price_label_n.text = ((answerVisualData.n_price > 1) ? (answerVisualData.n_price.ToString() ?? "") : "");
							}
							if (answerVisualData.n_price == 0)
							{
								Debug.LogError("Price n = 0 for item = " + answerVisualData.icon_price);
								multiAnswerPrice.price_label_n.text = "???";
							}
							multiAnswerPrice.price_label_n.color = (answerVisualData.inside_price_is_red ? inside_color_not_enough : inside_color_normal);
							flag = true;
							if (string.IsNullOrEmpty(answerVisualData.icon_price_quality))
							{
								multiAnswerPrice.price_quality_icon.gameObject.SetActive(value: false);
							}
							else
							{
								multiAnswerPrice.price_quality_icon.gameObject.SetActive(value: true);
								multiAnswerPrice.price_quality_icon.sprite2D = EasySpritesCollection.GetSprite(answerVisualData.icon_price_quality);
							}
							multiple_price_content.gameObject.SetActive(value: true);
							multiAnswerPrice.price_lock_available.gameObject.SetActive(value: false);
							multiAnswerPrice.price_lock_locked.gameObject.SetActive(value: false);
						}
						else
						{
							multiAnswerPrice.price_label_n.text = "";
							multiAnswerPrice.price_widget.SetActive(active: false);
						}
						multiAnswerLock.lock_widget_arrow.gameObject.SetActive(value: false);
						multiAnswerLock.lock_widget_locked.gameObject.SetActive(!_can_be_picked && !flag2);
						multiAnswerLock.lock_widget_available.gameObject.SetActive(_can_be_picked && !flag2);
						if (!string.IsNullOrEmpty(answerVisualData.icon_lock) && answerVisualData.icon_lock.StartsWith(":"))
						{
							multiple_lock_content.gameObject.SetActive(value: true);
							multiAnswerLock.lock_label.text = answerVisualData.icon_lock.Substring(2);
						}
						else
						{
							if (!string.IsNullOrEmpty(answerVisualData.icon_lock))
							{
								Debug.Log("lock icon = " + answerVisualData.icon_lock);
								if (EasySpritesCollection.SetSpriteOrDisableGameObject(multiAnswerPrice.price_icon, FixIconName(answerVisualData.icon_lock)))
								{
									multiAnswerPrice.price_lock_locked.gameObject.SetActive(!_can_be_picked);
									multiAnswerPrice.price_lock_available.gameObject.SetActive(_can_be_picked);
									flag2 = true;
									multiple_price_content.gameObject.SetActive(value: true);
									multiAnswerPrice.price_widget.gameObject.SetActive(value: true);
									multiAnswerPrice.price_label_n.text = ((answerVisualData.n_lock <= 1) ? "" : (answerVisualData.n_lock.ToString() ?? ""));
									flag = true;
									if (string.IsNullOrEmpty(answerVisualData.icon_lock_quality))
									{
										multiAnswerPrice.price_quality_icon.gameObject.SetActive(value: false);
									}
									else
									{
										multiAnswerPrice.price_quality_icon.gameObject.SetActive(value: true);
										multiAnswerPrice.price_quality_icon.sprite2D = EasySpritesCollection.GetSprite(answerVisualData.icon_lock_quality);
									}
								}
							}
							multiAnswerLock.lock_widget.gameObject.SetActive(value: false);
						}
					}
					multiAnswerPrice.SetBack(k == multipleAnswerVisualData.answer_visual_datas.Count - 1);
				}
			}
			else if (!string.IsNullOrEmpty(answer.icon_price) && answer.icon_price.StartsWith(":"))
			{
				lock_widget_arrow.gameObject.SetActive(value: true);
				lock_widget_locked.gameObject.SetActive(value: false);
				lock_widget_available.gameObject.SetActive(value: false);
				lock_widget.gameObject.SetActive(value: true);
				lock_label.text = answer.icon_price.Substring(2);
				if (answer.inside_price_is_red)
				{
					lock_label.color = color_text_not_enough;
				}
			}
			else
			{
				price_label_n.gameObject.SetActive(value: true);
				if (EasySpritesCollection.SetSpriteOrDisableGameObject(price_icon, FixIconName(answer.icon_price)))
				{
					if (answer.inside_price_is_red && !string.IsNullOrEmpty(answer.price_txt))
					{
						price_label_n.text = answer.price_txt;
					}
					else
					{
						price_label_n.text = ((answer.n_price > 1) ? (answer.n_price.ToString() ?? "") : "");
					}
					if (answer.n_price == 0)
					{
						Debug.LogError("Price n = 0 for item = " + answer.icon_price);
						price_label_n.text = "???";
					}
					price_label_n.color = (answer.inside_price_is_red ? inside_color_not_enough : inside_color_normal);
					flag = true;
					if (string.IsNullOrEmpty(answer.icon_price_quality))
					{
						price_quality_icon.gameObject.SetActive(value: false);
					}
					else
					{
						price_quality_icon.gameObject.SetActive(value: true);
						price_quality_icon.sprite2D = EasySpritesCollection.GetSprite(answer.icon_price_quality);
					}
					price_widget.gameObject.SetActive(value: true);
					price_lock_available.gameObject.SetActive(value: false);
					price_lock_locked.gameObject.SetActive(value: false);
				}
				else
				{
					price_label_n.text = "";
					price_widget.gameObject.SetActive(value: false);
				}
				lock_widget_arrow.gameObject.SetActive(value: false);
				lock_widget_locked.gameObject.SetActive(!_can_be_picked);
				lock_widget_available.gameObject.SetActive(_can_be_picked);
				if (!string.IsNullOrEmpty(answer.icon_lock) && answer.icon_lock.StartsWith(":"))
				{
					lock_widget.gameObject.SetActive(value: true);
					lock_label.text = answer.icon_lock.Substring(2);
				}
				else
				{
					if (!string.IsNullOrEmpty(answer.icon_lock))
					{
						Debug.Log("lock icon = " + answer.icon_lock);
						if (EasySpritesCollection.SetSpriteOrDisableGameObject(price_icon, FixIconName(answer.icon_lock)))
						{
							price_lock_locked.gameObject.SetActive(!_can_be_picked);
							price_lock_available.gameObject.SetActive(_can_be_picked);
							price_widget.gameObject.SetActive(value: true);
							price_label_n.text = ((answer.n_lock <= 1) ? "" : (answer.n_lock.ToString() ?? ""));
							flag = true;
							if (string.IsNullOrEmpty(answer.icon_lock_quality))
							{
								price_quality_icon.gameObject.SetActive(value: false);
							}
							else
							{
								price_quality_icon.gameObject.SetActive(value: true);
								price_quality_icon.sprite2D = EasySpritesCollection.GetSprite(answer.icon_lock_quality);
							}
						}
					}
					lock_widget.gameObject.SetActive(value: false);
				}
			}
			if (!string.IsNullOrEmpty(answer.icon_reward) && answer.icon_reward.StartsWith(":"))
			{
				if (height >= 28)
				{
					flag = true;
				}
				reward_widget.gameObject.SetActive(value: true);
				reward_icon.gameObject.SetActive(value: false);
				reward_label.text = answer.icon_reward.Substring(1);
			}
			else
			{
				reward_label.text = "";
				if (EasySpritesCollection.SetSpriteOrDisableGameObject(reward_icon, FixIconName(answer.icon_reward)))
				{
					reward_widget.gameObject.SetActive(value: true);
					reward_label_n.text = ((answer.n_reward == 1) ? "" : ("×" + answer.n_reward));
					flag = true;
				}
				else
				{
					reward_widget.gameObject.SetActive(value: false);
					reward_label_n.text = "";
				}
				if (string.IsNullOrEmpty(answer.icon_reward_quality))
				{
					reward_quality_icon.gameObject.SetActive(value: false);
				}
				else
				{
					reward_quality_icon.gameObject.SetActive(value: true);
					reward_quality_icon.sprite2D = EasySpritesCollection.GetSprite(answer.icon_reward_quality);
				}
			}
			Debug.Log(label_2.text + ", fh = " + flag);
			if (flag)
			{
				if (label.height < 28)
				{
					label.overflowMethod = UILabel.Overflow.ClampContent;
				}
				GetComponent<UIWidget>().height = ((28 >= height) ? 28 : height);
			}
		}
		else
		{
			label_2.gameObject.SetActive(value: false);
			reward_widget.gameObject.SetActive(value: false);
			lock_widget.gameObject.SetActive(value: false);
			price_widget.gameObject.SetActive(value: false);
			multiple_lock_content.gameObject.SetActive(value: false);
			multiple_price_content.gameObject.SetActive(value: false);
		}
		_widget.Update();
		_start_size = _widget.width;
		_focus_delta_size = (BaseGUI.for_gamepad ? gamepad_delta_size : mouse_delta_size);
		_button.hover = (BaseGUI.for_gamepad ? gamepad_focus_color : mouse_focus_color);
		_default_color = _button.defaultColor;
		_animating = true;
		label.color = (_can_be_picked ? Color.black : color_disabled);
		label_2.color = label.color;
		_wgt.ChangeAlpha(0.01f, 1f, anim_time, delegate
		{
			_animating = false;
		}, anim_delay);
		table.repositionNow = true;
		_gamepad_item.SetCallbacks(OnFocused, OnUnfocused, OnChosen);
	}

	private string FixIconName(string s)
	{
		return s;
	}

	public void FinishAnimation()
	{
		_widget.GetComponent<TweenAlpha>().DestroyComponent();
		label.GetComponent<TweenAlpha>().DestroyComponent();
		_widget.alpha = 1f;
		label.alpha = 1f;
	}

	private void DeactivateCorners(bool top)
	{
		if (top)
		{
			lu.Deactivate();
			ru.Deactivate();
		}
		else
		{
			ld.Deactivate();
			rd.Deactivate();
		}
	}
}
