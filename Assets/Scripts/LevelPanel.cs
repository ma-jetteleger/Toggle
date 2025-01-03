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
	//[SerializeField] private Button _previousLevelButton = null;
	[SerializeField] private Button _resetButton = null;
	[SerializeField] private Button _undoButton = null;
	[SerializeField] private Button _redoButton = null;
	[SerializeField] private TextMeshProUGUI _clicksCounterText = null;
	[SerializeField] private Image _nextLevelButtonOverlay = null;
	[SerializeField] private TextMeshProUGUI _levelsClearedText = null;

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
	[SerializeField] private Vector3 _clicksCounterPunchScale = Vector3.zero;
	[SerializeField] private float _clicksCounterPunchTime = 0f;
	[SerializeField] private int _clicksCounterPunchVibrato = 0;
	[SerializeField] private float _clicksCounterPunchElasticity = 0f;
	[SerializeField] private Vector3 _levelsClearedPunchScale = Vector3.zero;
	[SerializeField] private float _levelsClearedPunchTime = 0f;
	[SerializeField] private int _levelsClearedPunchVibrato = 0;
	[SerializeField] private float _levelsClearedPunchElasticity = 0f;
	[SerializeField] private float _levelsClearedDelay = 0f;

	private RectTransform _nextLevelButtonRectTransform;
	private Tweener _clicksCounterPunch;
	private CanvasGroup _levelsClearedBox;
	private Tweener _levelsClearedPunch;
	private Tweener _clicksCounterShake;
	private Tweener _nextLevelButtonShake;
	private Tweener _clicksCounterColorChange;
	private Tweener _nextLevelButtonColorChange;
	private Vector3 _clicksCounterNormalPosition;
	private Vector3 _nextLevelButtonNormalPosition;
	private Color _clicksCounterNormalColor;
	private Color _nextLevelButtonNormalOverlayColor;
	private Image _resetArrow;
	private Image _undoArrow;
	private Image _redoArrow;
	private GameObject _leftCornerButtons;

	private void Awake()
	{
		Instance = this;

		_nextLevelButtonRectTransform = _nextLevelButton.GetComponent<RectTransform>();

		_clicksCounterNormalPosition = _clicksCounterText.transform.position;
		_clicksCounterNormalColor = _clicksCounterText.color;

		_nextLevelButtonNormalPosition = _nextLevelButtonRectTransform.position;
		_nextLevelButtonNormalOverlayColor = _nextLevelButtonOverlay.color;

		_solutionClicksBoxTemplate.SetActive(false);

		_resetArrow = _resetButton.transform.GetChild(0).GetComponent<Image>();
		_undoArrow = _undoButton.transform.GetChild(0).GetComponent<Image>();
		_redoArrow = _redoButton.transform.GetChild(0).GetComponent<Image>();

		_levelsClearedBox = _levelsClearedText.GetComponentInParent<CanvasGroup>();
		_levelsClearedBox.alpha = 0f;

		_levelsClearedText.text = "0";

		_leftCornerButtons = _resetButton.transform.parent.gameObject;
		_leftCornerButtons.SetActive(false);
	}

	public void SetupSolutionBoxes(List<Solution> solutions)
	{
		if (_level.SolutionType == SolutionType.SingleSolution)
		{
			//_clicksCounterText.gameObject.SetActive(true);
		}
		else if(_level.SolutionType == SolutionType.MultipleSolutions)
		{
			//_clicksCounterText.gameObject.SetActive(false);

			for (var i = 0; i < solutions.Count; i++)
			{
				var newSolutionClicksBox = Instantiate(_solutionClicksBoxTemplate, _solutionClicksBoxTemplate.transform.parent).GetComponentInChildren<SolutionClicksBox>();
				newSolutionClicksBox.ClicksText.text = solutions[i].Sequence.Length.ToString();
				newSolutionClicksBox.gameObject.SetActive(true);

				solutions[i].SolutionClicksBox = newSolutionClicksBox;
			}
		}
	}

	public void UpdateLevelsClearedText(int levelsCleared, bool animate, bool delayed)
	{
		if(_levelsClearedBox.alpha < 0.5f && levelsCleared > 0)
		{
			_levelsClearedBox.alpha = 1f;
		}
		
		/*if(!active)
		{
			return;
		}*/

		var coroutine = DoUpdateLevelsClearedText(levelsCleared, animate, delayed);

		StartCoroutine(coroutine);
	}

	private IEnumerator DoUpdateLevelsClearedText(int levelsCleared, bool animate, bool delayed)
	{
		if(delayed)
		{
			yield return new WaitForSeconds(_levelsClearedDelay);
		}
		
		_levelsClearedText.text = levelsCleared.ToString();

		if (animate)
		{
			PunchLevelsClearedText();
		}

		yield return null;
	}

	public void UpdateNextLevelButton(bool toggle)
	{
		_nextLevelButton.gameObject.SetActive(toggle);
		_nextLevelButton.interactable = toggle;
	}

	/*public void UpdatePreviousLevelButton(bool toggle)
	{
		_previousLevelButton.gameObject.SetActive(toggle);
		_previousLevelButton.interactable = toggle;
	}*/

	public void ShakeNextLevelButton()
	{
		if (_nextLevelButtonShake != null)
		{
			_nextLevelButtonShake.Kill(true);
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
			_nextLevelButtonColorChange.Kill(true);
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
			_clicksCounterShake.Kill(true);
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
			_clicksCounterColorChange.Kill(true);
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

	public void PunchClicksCounter()
	{
		if (_clicksCounterPunch != null)
		{
			_clicksCounterPunch.Kill(true);
		}

		_clicksCounterPunch = _clicksCounterText.transform.DOPunchScale(_clicksCounterPunchScale, _clicksCounterPunchTime, _clicksCounterPunchVibrato, _clicksCounterPunchElasticity).OnComplete(() =>
		{
			_clicksCounterText.transform.localScale = Vector3.one;

			_clicksCounterPunch = null;
		});
	}

	public void PunchLevelsClearedText()
	{
		if (_levelsClearedPunch != null)
		{
			_levelsClearedPunch.Kill(true);
		}

		_levelsClearedPunch = _levelsClearedText.transform.DOPunchScale(_levelsClearedPunchScale, _levelsClearedPunchTime, _levelsClearedPunchVibrato, _levelsClearedPunchElasticity).OnComplete(() =>
		{
			_levelsClearedText.transform.localScale = Vector3.one;

			_levelsClearedPunch = null;
		});
	}

	public void UpdateClicksCounter(bool animate)
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
				break;
		}

		if (!_leftCornerButtons.activeSelf && animate)
		{
			_leftCornerButtons.SetActive(true);
		}

		_clicksCounterText.text = $"{numberToDisplay}";

		if(animate)
		{
			PunchClicksCounter();
		}
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

	/*public void UI_PreviousLevel()
	{
		_level.Previouslevel();
	}*/
}
