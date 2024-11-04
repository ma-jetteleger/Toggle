using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SolutionClicksBox : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _clicksText = null;
	[SerializeField] private GameObject _solvedOverlay = null;
	[SerializeField] private GameObject _bustedOverlay = null;

	public TextMeshProUGUI ClicksText => _clicksText;
	public GameObject SolvedOverlay => _solvedOverlay;
	public GameObject BustedOverlay => _bustedOverlay;
}
