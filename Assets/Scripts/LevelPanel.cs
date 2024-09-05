using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour
{
    public static LevelPanel Instance { get; set; }

	[SerializeField] private Button _nextLevelButton = null;
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

	public void UI_NextLevel()
	{
		_level.GenerateLevel();
	}
}
