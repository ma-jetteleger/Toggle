using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;

public class SettingsPanel : MonoBehaviour
{
	public static SettingsPanel Instance { get; set; }

	public const string kSolutionTypeSelection = "SolutionTypeSelection";
	
	[SerializeField] private SolutionType _defaultSolutionType = SolutionType.MultipleSolutions;
	[SerializeField] private Color _inactiveButtonColor = Color.black;
	[SerializeField] private SettingsPanelOption[] _solutionTypeOptionButtons = null;
	[SerializeField] private CanvasGroup _clearProgressionButton = null;
	[SerializeField] private CanvasGroup _confirmClearProgressPanel = null;
	[SerializeField] private float _panelTransitionTime = 0f;

	public int SolutionTypeSelection
	{
		get
		{
			if (_solutionTypeSelection == 0)
			{
				var savedSolutionTypeSelection = PlayerPrefs.GetInt(kSolutionTypeSelection);

				if (savedSolutionTypeSelection != 0)
				{
					_solutionTypeSelection = savedSolutionTypeSelection;
				}
				else
				{
					_solutionTypeSelection = (int)_defaultSolutionType;

					PlayerPrefs.SetInt(kSolutionTypeSelection, _solutionTypeSelection);
				}
			}

			return _solutionTypeSelection;
		}
		set
		{
			_solutionTypeSelection = value;

			PlayerPrefs.SetInt(kSolutionTypeSelection, _solutionTypeSelection);
		}
	}

	public CanvasGroup Panel { get; set; }

	private int _solutionTypeSelection;
	private CanvasGroup _panel;
	private bool _fromCustom;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}

		Panel = GetComponent<CanvasGroup>();
	}

	private void Start()
	{
		SelectSolutionTypeOption(SolutionTypeSelection - 1);

		ForceLayoutRebuilding(GetComponent<RectTransform>());
	}
	
	public void Show(bool fromCustom)
	{
		ToggleSubPanel(Panel, true, true);
		ToggleSubPanel(SettingsPanel.Instance.Panel, false, true);
		ToggleSubPanel(_confirmClearProgressPanel, false, false);

		/*_fromCustom = fromCustom;

		var currentProgressionGrid = GamePanel.Instance.CurrentProgressionGrid;
		var isContinuing = !string.IsNullOrEmpty(currentProgressionGrid) && !string.IsNullOrEmpty(currentProgressionGrid.Split(';')[6]);

		ToggleSubPanel(_clearProgressionButton, !_fromCustom && (isContinuing || MenuPanel.Instance.ProgressionIndex > 0), false);*/
	}
	
	public void SelectSolutionTypeOption(int solutionTypeOptionIndex)
	{
		for (var i = 0; i < _solutionTypeOptionButtons.Length; i++)
		{
			var active = i == solutionTypeOptionIndex;

			_solutionTypeOptionButtons[i].Outline.color = active ? Color.white : _inactiveButtonColor;
			_solutionTypeOptionButtons[i].Check.enabled = active;
		}
	}

	public void ForceLayoutRebuilding(RectTransform transformToRebuild)
	{
		var verticalLayoutGroup = transformToRebuild.GetComponent<VerticalLayoutGroup>();
		var horizontalLayoutGroup = transformToRebuild.GetComponent<HorizontalLayoutGroup>();

		LayoutRebuilder.ForceRebuildLayoutImmediate(transformToRebuild);
		LayoutRebuilder.MarkLayoutForRebuild(transformToRebuild);

		if (verticalLayoutGroup != null)
		{
			verticalLayoutGroup.CalculateLayoutInputVertical();
			verticalLayoutGroup.SetLayoutVertical();
		}

		if (horizontalLayoutGroup != null)
		{
			horizontalLayoutGroup.CalculateLayoutInputHorizontal();
			horizontalLayoutGroup.SetLayoutHorizontal();
		}
	}

	public void UI_SaveSolutionTypeSelection(int inputTypeOptionIndex)
	{
		SolutionTypeSelection = inputTypeOptionIndex;

		SelectSolutionTypeOption(inputTypeOptionIndex - 1);
	}

	public void UI_OpenConfirmClearProgress()
	{
		ToggleSubPanel(_confirmClearProgressPanel, true, true);
	}

	public void UI_ConfirmClearProgress()
	{
		/*MenuPanel.Instance.ProgressionIndex = 0;

		GamePanel.Instance.CurrentProgressionGrid = null;*/
		
		ToggleSubPanel(_clearProgressionButton, false, false);
		ToggleSubPanel(_confirmClearProgressPanel, false, true);
	}

	public void UI_CancelClearProgress()
	{
		ToggleSubPanel(_confirmClearProgressPanel, false, true);
	}

	public void UI_Close()
	{
		//MenuPanel.Instance.Show(_fromCustom, false);
	}

	private void ToggleSubPanel(CanvasGroup panel, bool toggle, bool animate)
	{
		panel.interactable = toggle;
		panel.blocksRaycasts = toggle;

		if (animate)
		{
			panel.DOFade(toggle ? 1f : 0f, _panelTransitionTime);
		}
		else
		{
			panel.alpha = toggle ? 1f : 0f;
		}
	}
}
