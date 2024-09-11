using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelPanel : MonoBehaviour
{
    public static LevelPanel Instance { get; set; }

	[SerializeField] private Button _nextLevelButton = null;
	[SerializeField] private Button _resetButton = null;
	[SerializeField] private Button _undoButton = null;
	[SerializeField] private Button _redoButton = null;
	[SerializeField] private TextMeshProUGUI _clicksCounterText = null;
	[SerializeField] private Image _nextLevelButtonOverlay = null;
	[SerializeField] private Level _level = null;
	[SerializeField] private Gradient _clicksCounterShakeGradient = null;
	[SerializeField] private Gradient _nextLevelButtonShakeGradient = null;
	[SerializeField] private float _clicksCounterShakeTime = 0f;
	[SerializeField] private float _clicksCounterShakeStrength = 0f;
	[SerializeField] private int _clicksCounterShakeVibrato = 0;
	[SerializeField] private float _clicksCounterShakeRandomness = 0f;
	[SerializeField] private bool _clicksCounterShakeSnapping = false;
	[SerializeField] private bool _clicksCounterShakeFadeOut = false;

	private RectTransform _nextLevelButtonRectTransform;
	private Image _nextLevelButtonArrowImage;
	private Tweener _clicksCounterShake;
	private Tweener _nextLevelButtonShake;
	private Tweener _clicksCounterColorChange;
	private Tweener _nextLevelButtonColorChange;
	private Vector3 _clicksCounterNormalPosition;
	private Vector3 _nextLevelButtonNormalPosition;
	private Color _clicksCounterNormalColor;
	private Color _nextLevelButtonNormalOverlayColor;

	private void Awake()
	{
		Instance = this;

		_nextLevelButtonRectTransform = _nextLevelButton.GetComponent<RectTransform>();
		_nextLevelButtonArrowImage = _nextLevelButton.transform.GetChild(0).GetComponent<Image>();

		_clicksCounterNormalPosition = _clicksCounterText.transform.position;
		_clicksCounterNormalColor = _clicksCounterText.color;

		_nextLevelButtonNormalPosition = _nextLevelButtonRectTransform.position;
		_nextLevelButtonNormalOverlayColor = _nextLevelButtonOverlay.color;
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
		_clicksCounterText.text = $"{_level.ClicksLeft}";
	}

	public void UpdateHistoryButtons(bool? forcedValue = null)
	{
		_resetButton.interactable = forcedValue != null ? forcedValue.Value : (!_level.EmptyHistory && !_level.BottomOfHistory);
		_undoButton.interactable = forcedValue != null ? forcedValue.Value : (!_level.EmptyHistory && !_level.BottomOfHistory);
		_redoButton.interactable = forcedValue != null ? forcedValue.Value : (!_level.EmptyHistory && !_level.TopOfHistory);
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
