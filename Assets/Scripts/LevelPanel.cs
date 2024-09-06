using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour
{
    public static LevelPanel Instance { get; set; }

	[SerializeField] private Button _nextLevelButton = null;
	[SerializeField] private Button _resetButton = null;
	[SerializeField] private Button _undoButton = null;
	[SerializeField] private Button _redoButton = null;
	[SerializeField] private TextMeshProUGUI _clicksCounterText = null;
	[SerializeField] private Level _level = null;

	private void Awake()
	{
		Instance = this;
	}

	public void UpdateNextLevelButton(bool toggle)
	{
		_nextLevelButton.gameObject.SetActive(toggle);
		_nextLevelButton.interactable = toggle;
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
		_level.GenerateLevel();
	}
}
