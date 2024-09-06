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
	[SerializeField] private Level _level = null;
	[SerializeField] private float _clicksCounterShakeTime = 0f;
	[SerializeField] private float _clicksCounterShakeStrength = 0f;
	[SerializeField] private int _clicksCounterShakeVibrato = 0;
	[SerializeField] private float _clicksCounterShakeRandomness = 0f;
	[SerializeField] private bool _clicksCounterShakeSnapping = false;
	[SerializeField] private bool _clicksCounterShakeFadeOut = false;

	private RectTransform _nextLevelButtonRectTransform;
	private Image _nextLevelButtonArrowImage;

	private void Awake()
	{
		Instance = this;

		_nextLevelButtonRectTransform = _nextLevelButton.GetComponent<RectTransform>();
		_nextLevelButtonArrowImage = _nextLevelButton.transform.GetChild(0).GetComponent<Image>();
	}

	public void UpdateNextLevelButton(bool toggle)
	{
		_nextLevelButton.gameObject.SetActive(toggle);
		_nextLevelButton.interactable = toggle;
	}

	public void ShakeNextLevelButton()
	{
		_nextLevelButtonRectTransform.DOShakePosition(
			_clicksCounterShakeTime,
			_clicksCounterShakeStrength,
			_clicksCounterShakeVibrato,
			_clicksCounterShakeRandomness,
			_clicksCounterShakeSnapping,
			_clicksCounterShakeFadeOut
		);

		var originalColor = _nextLevelButtonArrowImage.color;

		_nextLevelButtonArrowImage.DOColor(Color.red, _clicksCounterShakeTime / 2f).OnComplete(() =>
		{
			_nextLevelButtonArrowImage.DOColor(originalColor, _clicksCounterShakeTime / 2f);
		});
	}

	public void ShakeClicksCounter()
	{
		_clicksCounterText.rectTransform.DOShakePosition(
			_clicksCounterShakeTime, 
			_clicksCounterShakeStrength, 
			_clicksCounterShakeVibrato, 
			_clicksCounterShakeRandomness, 
			_clicksCounterShakeSnapping, 
			_clicksCounterShakeFadeOut
		);

		var originalColor = _clicksCounterText.color;

		_clicksCounterText.DOColor(Color.red, _clicksCounterShakeTime / 2f).OnComplete(() =>
		{
			_clicksCounterText.DOColor(originalColor, _clicksCounterShakeTime / 2f);
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
