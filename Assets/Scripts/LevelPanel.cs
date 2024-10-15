using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelPanel : MonoBehaviour
{
    public static LevelPanel Instance { get; set; }

	// Components
	[SerializeField] private GameObject _solutionClicksBoxTemplate = null;
	[SerializeField] private Button _nextLevelButton = null;
	[SerializeField] private Button _resetButton = null;
	[SerializeField] private Button _undoButton = null;
	[SerializeField] private Button _redoButton = null;
	[SerializeField] private TextMeshProUGUI _clicksCounterText = null;
	[SerializeField] private Image _nextLevelButtonOverlay = null;

	// References
	[SerializeField] private Level _level = null;

	// Animation/visual parameters
	[SerializeField] private Gradient _clicksCounterShakeGradient = null;
	[SerializeField] private Gradient _nextLevelButtonShakeGradient = null;
	[SerializeField] private float _clicksCounterShakeTime = 0f;
	[SerializeField] private float _clicksCounterShakeStrength = 0f;
	[SerializeField] private int _clicksCounterShakeVibrato = 0;
	[SerializeField] private float _clicksCounterShakeRandomness = 0f;
	[SerializeField] private bool _clicksCounterShakeSnapping = false;
	[SerializeField] private bool _clicksCounterShakeFadeOut = false;

	private RectTransform _nextLevelButtonRectTransform;
	private Tweener _clicksCounterShake;
	private Tweener _nextLevelButtonShake;
	private Tweener _clicksCounterColorChange;
	private Tweener _nextLevelButtonColorChange;
	private Vector3 _clicksCounterNormalPosition;
	private Vector3 _nextLevelButtonNormalPosition;
	private Color _clicksCounterNormalColor;
	private Color _nextLevelButtonNormalOverlayColor;
	private Dictionary<Solution, TextMeshProUGUI> _solutionClicksTexts;
	private Image _resetArrow;
	private Image _undoArrow;
	private Image _redoArrow;

	private void Awake()
	{
		Instance = this;

		_nextLevelButtonRectTransform = _nextLevelButton.GetComponent<RectTransform>();

		_clicksCounterNormalPosition = _clicksCounterText.transform.position;
		_clicksCounterNormalColor = _clicksCounterText.color;

		_nextLevelButtonNormalPosition = _nextLevelButtonRectTransform.position;
		_nextLevelButtonNormalOverlayColor = _nextLevelButtonOverlay.color;

		_solutionClicksBoxTemplate.SetActive(false);

		_solutionClicksTexts = new Dictionary<Solution, TextMeshProUGUI>();

		_resetArrow = _resetButton.transform.GetChild(0).GetComponent<Image>();
		_undoArrow = _undoButton.transform.GetChild(0).GetComponent<Image>();
		_redoArrow = _redoButton.transform.GetChild(0).GetComponent<Image>();
	}

	public void SetupSolutionBoxes(List<Solution> solutions)
	{
		if (_solutionClicksTexts.Count != 0)
		{
			foreach (var solutionClicksText in _solutionClicksTexts)
			{
				Destroy(solutionClicksText.Value.transform.parent.gameObject);
			}
		}

		_solutionClicksTexts.Clear();

		if (_level.SolutionType == SolutionType.SingleSolution)
		{
			//_clicksCounterText.gameObject.SetActive(true);
		}
		else if(_level.SolutionType == SolutionType.MultipleSolutions)
		{
			//_clicksCounterText.gameObject.SetActive(false);

			for (var i = 0; i < solutions.Count; i++)
			{
				var newSolutionClicksText = Instantiate(_solutionClicksBoxTemplate, _solutionClicksBoxTemplate.transform.parent).GetComponentInChildren<TextMeshProUGUI>();
				newSolutionClicksText.text = solutions[i].Sequence.Length.ToString();
				newSolutionClicksText.transform.parent.gameObject.SetActive(true);

				_solutionClicksTexts.Add(solutions[i], newSolutionClicksText);
			}
		}
	}

	public void UpdateNextLevelButton(bool toggle)
	{
		_nextLevelButton.gameObject.SetActive(toggle);
		_nextLevelButton.interactable = toggle;
	}

	public void ShakeNextLevelButton()
	{
		if (_nextLevelButtonShake != null)
		{
			DOTween.Kill(_nextLevelButtonShake, true);

			_nextLevelButtonRectTransform.position = _nextLevelButtonNormalPosition;
		}

		_nextLevelButtonShake = _nextLevelButtonRectTransform.DOShakePosition(
			_clicksCounterShakeTime,
			_clicksCounterShakeStrength,
			_clicksCounterShakeVibrato,
			_clicksCounterShakeRandomness,
			_clicksCounterShakeSnapping,
			_clicksCounterShakeFadeOut
		).OnComplete(() =>
		{
			_nextLevelButtonRectTransform.position = _nextLevelButtonNormalPosition;

			_nextLevelButtonShake = null;
		});

		if (_nextLevelButtonColorChange != null)
		{
			DOTween.Kill(_nextLevelButtonColorChange, true);

			_nextLevelButtonOverlay.color = _nextLevelButtonNormalOverlayColor;
		}

		var value = 0f;

		_nextLevelButtonColorChange = DOTween.To(() => value, x =>
		{
			value = x;

			_nextLevelButtonOverlay.color = _nextLevelButtonShakeGradient.Evaluate(value);
		},
		1f,
		_clicksCounterShakeTime).OnComplete(() =>
		{
			_nextLevelButtonColorChange = null;

			_nextLevelButtonOverlay.color = _nextLevelButtonNormalOverlayColor;
		});
	}

	public void ShakeClicksCounter()
	{
		if (_clicksCounterShake != null)
		{
			DOTween.Kill(_clicksCounterShake, true);

			_clicksCounterText.transform.position = _clicksCounterNormalPosition;
		}

		_clicksCounterShake = _clicksCounterText.rectTransform.DOShakePosition(
			_clicksCounterShakeTime,
			_clicksCounterShakeStrength,
			_clicksCounterShakeVibrato,
			_clicksCounterShakeRandomness,
			_clicksCounterShakeSnapping,
			_clicksCounterShakeFadeOut
		).OnComplete(() =>
		{
			_clicksCounterText.transform.position = _clicksCounterNormalPosition;

			_clicksCounterShake = null;
		});

		if (_clicksCounterColorChange != null)
		{
			DOTween.Kill(_clicksCounterColorChange, true);

			_clicksCounterText.color = _clicksCounterNormalColor;
		}

		var value = 0f;

		_clicksCounterColorChange = DOTween.To(() => value, x =>
		{
			value = x;

			_clicksCounterText.color = _clicksCounterShakeGradient.Evaluate(value);
		},
		1f,
		_clicksCounterShakeTime).OnComplete(() =>
		{
			_clicksCounterColorChange = null;

			_clicksCounterText.color = _clicksCounterNormalColor;
		});
	}

	public void UpdateClicksCounter()
	{
		var numberToDisplay = 0;

		switch (_level.SolutionType)
		{
			case SolutionType.SingleSolution:
				numberToDisplay = _level.ClicksLeft;
				break;
			case SolutionType.MultipleSolutions:
				//_clicksCounterText.gameObject.SetActive(_level.Clicks > 0);
				numberToDisplay = _level.Clicks;

				foreach(var solutionClicksText in _solutionClicksTexts)
				{
					if(solutionClicksText.Key.Solved)
					{
						continue;
					}

					var solutionClicks = int.Parse(solutionClicksText.Value.text);
					var levelComplete = _level.GetLevelCompletion();

					var solutionParent = solutionClicksText.Value.transform.parent;

					solutionParent.GetChild(3).gameObject.SetActive(_level.Clicks > solutionClicks 
						|| _level.Clicks == solutionClicks && !levelComplete);

					solutionClicksText.Key.Solved = _level.Clicks == solutionClicks && levelComplete;

					solutionParent.GetChild(1).gameObject.SetActive(solutionClicksText.Key.Solved);
				}

				break;
		}

		_clicksCounterText.text = $"{numberToDisplay}";
	}

	public void UpdateHistoryButtons(bool? forcedValue = null)
	{
		_resetButton.interactable = forcedValue != null ? forcedValue.Value : (!_level.EmptyHistory && !_level.BottomOfHistory);
		_resetArrow.color = new Color(_resetArrow.color.r, _resetArrow.color.g, _resetArrow.color.b, _resetButton.interactable ? 1f : 0.5f);

		_undoButton.interactable = forcedValue != null ? forcedValue.Value : (!_level.EmptyHistory && !_level.BottomOfHistory);
		_undoArrow.color = new Color(_undoArrow.color.r, _undoArrow.color.g, _undoArrow.color.b, _undoButton.interactable ? 1f : 0.5f);

		_redoButton.interactable = forcedValue != null ? forcedValue.Value : (!_level.EmptyHistory && !_level.TopOfHistory);
		_redoArrow.color = new Color(_redoArrow.color.r, _redoArrow.color.g, _redoArrow.color.b, _redoButton.interactable ? 1f : 0.5f);
	}

	public void UI_Reset()
	{
		_level.ResetLevel();
	}

	public void UI_Undo()
	{
		_level.Undo();
	}

	public void UI_Redo()
	{
		_level.Redo();
	}

	public void UI_NextLevel()
	{
		_level.NextLevel();
	}
}
