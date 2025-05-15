using Shapes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;
using System.IO;
using UnityEditor;
using System;

[Serializable]
public class ProgressionEntry
{
	public int Squares;                         // Fixed int value, -1: random from inspector range
	public int LeftRightTargets;                // At least the int value, -1: random including none
	public int SelfLeftTargets;                 // At least the int value, -1: random including none
	public int SelfRightTargets;                // At least the int value, -1: random including none
	public int SelfLeftRightTargets;            // At least the int value, -1: random including none
	public int WrapAroundToggles;               // At least the int value, -1: random including none
	public int CascadingToggles;                // Fixed int value, -1: random including none
	public bool AdjacentCascadingToggles;       // Allowed or not 
												// This is more complex than originally thought of
												// as it only impacts gameplay/complexity if two adjacent 
												// cascading squares actually interact with each other through
												// their targeting schemes. I'll ignore the adjacent cascading 
												// toggle issue for now and think back on it later. 
												// (the issue is that "chained" cascading 
												// toggles are more complex but also _maybe_ more interesting so
												// it'd be nice if we were able to include them progressively 
												// and control their introduction through the progression)
												// I decided to include them in the progression but also chose
												// to ignore the issue of "how many" to include and how
												// progressively they are introduced
}

public enum SolutionType
{
	SingleSolution,
	MultipleSolutions
}

public enum ClicksCountToNextLevelRestriction
{
	HardRestriction,
	SoftRestriction,
	NoRestriction
}

public enum CompletedSolutionsToNextLevelRestriction
{
	AllSolutions,
	AtLeastOneSolution
}

public class Level : MonoBehaviour
{
	[SerializeField] private LevelPanel _levelPanel = null;

	// Components
	[SerializeField] private GameObject _squareTemplate = null;
	[SerializeField] private GameObject _solutionSquareTemplate = null;
	[SerializeField] private Rectangle _rectangle = null;
	[SerializeField] private Rectangle _solutionRectangle = null;
	[SerializeField] private Rectangle _levelCompletionFeedback = null;

	[HorizontalLine(1)]

	// Generation parameters
	[SerializeField] private Vector2Int _squaresRange = Vector2Int.zero;
	[SerializeField] private int _maxClicksBufferForSolution = 0;
	[SerializeField] private int _solutionGenerationAttempts = 0;

	[HorizontalLine(1)]

	// Animation/visual parameters
	[SerializeField] private float _squaresDistance = 0f;
	[SerializeField] private float _solutionSquaresDistance = 0f;
	[SerializeField] private Color _trueLevelCompletionColor = Color.black;
	[SerializeField] private float _levelCompletionTime = 0f;
	[SerializeField] private AnimationCurve _levelCompletionCurve = null;
	[SerializeField] private AnimationCurve _levelCompletionThicknessCurve = null;
	[SerializeField] private AnimationCurve _levelCompletionAlphaCurve = null;
	[SerializeField] private Vector3 _solutionRectanglePunchScale = Vector3.zero;
	[SerializeField] private float _solutionRectanglePunchTime = 0f;
	[SerializeField] private int _solutionRectanglePunchVibrato = 0;
	[SerializeField] private float _solutionRectanglePunchElasticity = 0f;
	[SerializeField] private float[] _levelCompleteSequenceDelays = null;

	[HorizontalLine(1)]

	// Features
	[SerializeField] private bool _progression = false;
	[SerializeField] [ShowIf(nameof(_progression))] private string _progressionFile = string.Empty;
	[SerializeField] [HideIf(nameof(_progression))] private int _squares = -1;
	[SerializeField] [HideIf(nameof(_progression))] private int _wrapAroundToggles = -1;
	[SerializeField] [HideIf(nameof(_progression))] private int _leftRightTargets = -1;
	[SerializeField] [HideIf(nameof(_progression))] private int _selfRightTargets = -1;
	[SerializeField] [HideIf(nameof(_progression))] private int _selfLeftTargets = -1;
	[SerializeField] [HideIf(nameof(_progression))] private int _selfLeftRightTargets = -1;
	[SerializeField] [HideIf(nameof(_progression))] private int _cascadingToggles = -1;
	[SerializeField] [HideIf(nameof(_progression))] private bool _adjacentCascadingToggles = false;
	[SerializeField] private SolutionType _solutionType = SolutionType.SingleSolution;
	[SerializeField] [ShowIf(nameof(_solutionType), SolutionType.SingleSolution)] private ClicksCountToNextLevelRestriction _clicksCountRestriction = ClicksCountToNextLevelRestriction.HardRestriction;
	[SerializeField] [ShowIf(nameof(_solutionType), SolutionType.SingleSolution)] private bool _forceSingleSolution = false;
	[SerializeField] [ShowIf(nameof(_solutionType), SolutionType.MultipleSolutions)] private CompletedSolutionsToNextLevelRestriction _completedSolutionsToNextLevelRestriction = CompletedSolutionsToNextLevelRestriction.AllSolutions;
	[SerializeField] [ShowIf(nameof(_solutionType), SolutionType.MultipleSolutions)] private bool _forceMultipleSolution = false;
	[SerializeField] private bool _binaryStateSquares = false;
	[SerializeField] private bool _unclickableToggledSquares = false;
	[SerializeField] private bool _distanceToggles = false;

	[HorizontalLine(1)]

	// File stuff
	[SerializeField] private bool _saveLevelsToFile = false;
	[SerializeField] private string _singleSolutionLevelsFile = string.Empty;
	[SerializeField] private string _multiSolutionsLevelsFile = string.Empty;
	[SerializeField] private int _playedLevelQueueSize = 0;

	// Debug
	[HorizontalLine(1)]
	[SerializeField] private bool _printSolutions = false;

	public Square[] Squares { get; set; }
	public Square[] SolutionSquares { get; set; }
	public List<Solution> Solutions { get; set; }
	public int Clicks { get; set; }
	public int TogglesThisClickSequence { get; set; }
	public List<Square> SquaresToggledLastClick { get; set; }
	public Square LastSquareClicked { get; set; }
	public bool CanClick { get; set; }
	public int Quadrant { get; set; }

	public int ClicksLeft => Solutions[0].Sequence.Count - Clicks;
	public bool UnclickableToggledSquares => _unclickableToggledSquares;
	public bool EmptyHistory => _squareHistory.Count == 1;
	public bool TopOfHistory => Clicks == _squareHistory.Count - 1;
	public bool BottomOfHistory => Clicks == 0;
	public SolutionType SolutionType => _solutionType;
	public LevelPanel LevelPanel => _levelPanel;
	public bool BinaryStateSquares => _binaryStateSquares;
	public bool DistanceToggles => _distanceToggles;

	private string _levelsFileName => _solutionType == SolutionType.SingleSolution ? _singleSolutionLevelsFile : _multiSolutionsLevelsFile;
	private string _levelsFilePath => Application.persistentDataPath + "/" + _levelsFileName;
	private ProgressionEntry _currentProgressionEntry => _progression
		? _progressionIndex <= _progressionEntries.Count - 1
			? _progressionEntries[_progressionIndex]
			: _progressionEntries[_progressionEntries.Count - 1]
		: _nonProgressionFakeProgressionEntry;

	private Square _previousHoveredSquare;
	private Rectangle _squareTemplateRectangle;
	private Rectangle _solutionSquareTemplateRectangle;
	private Square _lastSquareClickedDown;
	private Vector2 _levelCompletionFeedbackFinalSize;
	private Color _levelCompletionFeedbackBaseColor;
	private int _progressionIndex;
	//private int _progressionIndexBuffer;
	private List<HistorySquare[]> _squareHistory;
	private TestSquare[] _testSquares;
	private Queue<string> _playedLevels;
	//private List<string> _generatedLevels;
	private string _levelCode;
	private bool _trulyCompleted;
	private List<ProgressionEntry> _progressionEntries;
	private ProgressionEntry _nonProgressionFakeProgressionEntry;
	private Tweener _levelCompleteHeightScale;
	private Tweener _levelCompleteWidthScale;
	private Tweener _levelCompleteThicknessScale;
	private Tweener _solutionRectangleScale;
	private Tweener _levelRectangleScale;

	private void Awake()
	{
		_squareTemplateRectangle = _squareTemplate.GetComponent<Rectangle>();
		_solutionSquareTemplateRectangle = _solutionSquareTemplate.GetComponent<Rectangle>();

		_squareTemplate.SetActive(false);
		_solutionSquareTemplate.SetActive(false);

		_squareHistory = new List<HistorySquare[]>();

		_playedLevels = new Queue<string>();
		//_generatedLevels = new List<string>();

		_progressionEntries = new List<ProgressionEntry>();

		SquaresToggledLastClick = new List<Square>();
	}

	private void Start()
	{
		_levelCompletionFeedbackFinalSize = new Vector2(Screen.width, Screen.height) / 50f;
		_levelCompletionFeedback.Width = _levelCompletionFeedbackFinalSize.x / 100f;
		_levelCompletionFeedback.Height = _levelCompletionFeedbackFinalSize.y / 100f;
		_levelCompletionFeedbackBaseColor = _levelCompletionFeedback.Color;

		LoadProgressionEntries();
		GenerateLevel();

		_levelPanel.UpdateLevelsClearedText(_progressionIndex, false);

		CanClick = true;
	}

	private void Update()
	{
		if (_lastSquareClickedDown == null && CanClick)
		{
			if (Input.GetKey(KeyCode.D))
			{
				var mouseIsInQuadrant = GetQuadrant(Input.mousePosition) == Quadrant;

				if (Input.GetKeyDown(KeyCode.Return) || (mouseIsInQuadrant && Input.GetKeyDown(KeyCode.N)))
				{
					/*if (_unclickableToggledSquares || _distanceToggles || _binaryStateSquares)
					{
						_playedLevels.Enqueue(_levelCode);

						if (_playedLevels.Count > _playedLevelQueueSize)
						{
							_playedLevels.Dequeue();
						}
					}*/

					GenerateLevel();

					return;
				}
				if (Input.GetKeyDown(KeyCode.LeftArrow) || (mouseIsInQuadrant && Input.GetKeyDown(KeyCode.DownArrow)))
				{
					_progressionIndex--;

					_levelPanel.UpdateLevelsClearedText(_progressionIndex, false);
				}
				if (Input.GetKeyDown(KeyCode.RightArrow) || (mouseIsInQuadrant && Input.GetKeyDown(KeyCode.UpArrow)))
				{
					_progressionIndex++;

					_levelPanel.UpdateLevelsClearedText(_progressionIndex, true);
				}
				if (Input.GetKeyDown(KeyCode.Backspace) || (mouseIsInQuadrant && Input.GetKeyDown(KeyCode.Minus)))
				{
					_progressionIndex = 0;

					_levelPanel.UpdateLevelsClearedText(_progressionIndex, false);
				}
				if (Input.GetKeyDown(KeyCode.Space) || (mouseIsInQuadrant && Input.GetKeyDown(KeyCode.S)))
				{
					PrintSolutions();
				}
			}

			var squareHovered = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero).collider?.GetComponent<Square>();

			if (squareHovered != null)
			{
				if (squareHovered.SolutionSquare || !squareHovered.Interactable || squareHovered.Level != this
					/*|| (squareHovered.Toggled && _unclickableToggledSquares)*/)
				{
					squareHovered = null;

				}
				else if (squareHovered != _previousHoveredSquare && !squareHovered.Highlighted)
				{
					squareHovered.OnMouseOverEnter(true);
				}
			}

			if (_previousHoveredSquare != null && _previousHoveredSquare.Highlighted && squareHovered != _previousHoveredSquare)
			{
				_previousHoveredSquare.OnMouseOverExit();
			}

			_previousHoveredSquare = squareHovered;

			if (squareHovered != null)
			{
				if (Input.GetMouseButtonDown(0))
				{
					squareHovered.OnMouseClickDown();

					_lastSquareClickedDown = squareHovered;
				}
			}
		}

		if (Input.GetMouseButtonUp(0) && _lastSquareClickedDown != null)
		{
			var squareClickedUp = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero).collider?.GetComponent<Square>();

			var sameSquare = squareClickedUp != null && squareClickedUp == _lastSquareClickedDown;

			if (sameSquare)
			{
				if (_solutionType == SolutionType.SingleSolution
					&& _clicksCountRestriction == ClicksCountToNextLevelRestriction.HardRestriction
					&& ClicksLeft <= 0)
				{
					_lastSquareClickedDown.Shake();

					_levelPanel.ShakeClicksCounter();

					if (_lastSquareClickedDown != null)
					{
						_lastSquareClickedDown.OnMouseClickUp();
						_lastSquareClickedDown.OnMouseOverExit();

						_lastSquareClickedDown = null;
					}

					if (_previousHoveredSquare != null)
					{
						_previousHoveredSquare = null;
					}
				}
				else
				{
					TogglesThisClickSequence = 0;

					LastSquareClicked = _lastSquareClickedDown;

					_lastSquareClickedDown.Click(true, false);

					//_lastSquareClickedDown.HideTargetPredictions();
					//_lastSquareClickedDown.Interactable = false;

					if (_solutionType == SolutionType.MultipleSolutions)
					{
						_lastSquareClickedDown.Interactable = false;
					}

					Clicks++;

					CanClick = false;
				}
			}

			if (_lastSquareClickedDown != null)
			{
				_lastSquareClickedDown.OnMouseClickUp();
				_lastSquareClickedDown.OnMouseOverExit();

				_lastSquareClickedDown = null;
			}

			if (_previousHoveredSquare != null)
			{
				_previousHoveredSquare = null;
			}
		}
	}

	public int GetQuadrant(Vector2 screenPos)
	{
		var w = Screen.width;
		var h = Screen.height;

		if (screenPos.x < w / 2f)
			return (screenPos.y < h / 2f) ? 3 : 1; // 1 = Top Left, 3 = Bottom Left
		else
			return (screenPos.y < h / 2f) ? 4 : 2; // 2 = Top Right, 4 = Bottom Right
	}

	public void OnSquareClicked()
	{
		GetLevelCompletion(true);
	}

	public void EndClickSequence(bool fromPlayer)
	{
		if (!fromPlayer)
		{
			return;
		}

		AddNewHistorySnapshot();

		_levelPanel.UpdateHistoryButtons(true);

		CheckLevelCompletion(true);
	}

	private void OverwriteLevel(string levelCode)
	{
		var splitLevelCode = levelCode.Split(';');
		var splitSquaresCode = splitLevelCode[0].Split(',');

		if (splitSquaresCode.Length != Squares.Length)
		{
			Debug.Log("Can't overwrite a level with a different amount of squares than in the level code");

			return;
		}

		var splitGoalCode = splitLevelCode[1].Split(',');
		var splitSolutionsCode = splitLevelCode[2].Split(',');

		for (var i = 0; i < Squares.Length; i++)
		{
			var squareCodeToggle = splitSquaresCode[i][0] == 't';
			var squareCodeTarget = (Square.TargetingScheme)int.Parse(splitSquaresCode[i][1].ToString());
			var squareCodeCascading = splitSquaresCode[i].Length > 2 && splitSquaresCode[i][2] == 'c';

			Squares[i].Overwrite(squareCodeToggle ? Square.PossibleToggleState.Two : Square.PossibleToggleState.Zero, squareCodeTarget, squareCodeCascading);
		}

		var firstHistorySnapshot = _squareHistory[0];

		for (var i = 0; i < Squares.Length; i++)
		{
			Squares[i].SetupTargets(Squares);

			firstHistorySnapshot[i].ToggledState = Squares[i].ToggledState;
			firstHistorySnapshot[i].Interactable = UnclickableToggledSquares ? Squares[i].ToggledState == Square.PossibleToggleState.Zero : true;
			firstHistorySnapshot[i].Cascading = Squares[i].Cascading;
		}

		for (var i = 0; i < Squares.Length; i++)
		{
			Squares[i].SetupPredictions(true);
		}

		_squareHistory.Clear();
		_squareHistory.Add(firstHistorySnapshot);

		GenerateSolution(splitGoalCode, splitSolutionsCode);
	}

	public void GenerateLevel(string levelCode = null)
	{
		StopAllCoroutines();

		if (_levelCompleteHeightScale != null)
		{
			_levelCompleteHeightScale.Kill(true);
			_levelCompleteHeightScale = null;
		}

		if (_levelCompleteWidthScale != null)
		{
			_levelCompleteWidthScale.Kill(true);
			_levelCompleteWidthScale = null;
		}

		if (_levelCompleteThicknessScale != null)
		{
			_levelCompleteThicknessScale.Kill(true);
			_levelCompleteThicknessScale = null;
		}

		if (_solutionRectangleScale != null)
		{
			_solutionRectangleScale.Kill(true);
			_solutionRectangleScale = null;
		}

		if (_levelRectangleScale != null)
		{
			_levelRectangleScale.Kill(true);
			_levelRectangleScale = null;
		}

		_trulyCompleted = false;

		// Clean up

		if (Squares != null && Squares.Length != 0)
		{
			foreach (var square in Squares)
			{
				Destroy(square.gameObject);
			}

			foreach (var solutionSquare in SolutionSquares)
			{
				Destroy(solutionSquare.gameObject);
			}
		}

		if (Solutions != null)
		{
			foreach (var solution in Solutions)
			{
				if (solution.SolutionClicksBox != null)
				{
					Destroy(solution.SolutionClicksBox.gameObject);
				}
			}

			Solutions.Clear();
		}

		_levelPanel.UpdateInvalidLevelX(false);

		var splitLevelCode = levelCode != null ? levelCode.Split(';') : null;
		var splitSquaresCode = levelCode != null ? splitLevelCode[0].Split(',') : null;
		var splitGoalCode = levelCode != null ? splitLevelCode[1].Split(',') : null;
		var splitSolutionsCode = levelCode != null ? splitLevelCode[2].Split(',') : null;

		// Generation

		_nonProgressionFakeProgressionEntry = new ProgressionEntry()
		{
			Squares = _squares,
			LeftRightTargets = _leftRightTargets,
			SelfRightTargets = _selfRightTargets,
			SelfLeftTargets = _selfLeftTargets,
			SelfLeftRightTargets = _selfLeftRightTargets,
			WrapAroundToggles = _wrapAroundToggles,
			CascadingToggles = _cascadingToggles,
			AdjacentCascadingToggles = _adjacentCascadingToggles
		};

		var squares = levelCode != null
			? splitSquaresCode.Length
			: _currentProgressionEntry.Squares > 0
				? _currentProgressionEntry.Squares
				: UnityEngine.Random.Range(_squaresRange.x, _squaresRange.y + 1);

		var indices = new int[squares];

		Squares = new Square[squares];
		SolutionSquares = new Square[squares];

		for (var i = 0; i < squares; i++)
		{
			var newSquare = Instantiate(_squareTemplate, _squareTemplate.transform.parent).GetComponent<Square>();
			newSquare.transform.localPosition = -(Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * (squares - 1)) / 2f
				+ Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * i;

			newSquare.Initialize(i, this);

			newSquare.gameObject.SetActive(true);

			var newSolutionSquare = Instantiate(_solutionSquareTemplate, _solutionSquareTemplate.transform.parent).GetComponent<Square>();
			newSolutionSquare.transform.localPosition = -(Vector3.right * (_solutionSquareTemplateRectangle.Width + _solutionSquaresDistance) * (squares - 1)) / 2f
				+ Vector3.right * (_solutionSquareTemplateRectangle.Width + _solutionSquaresDistance) * i;

			newSolutionSquare.gameObject.SetActive(true);

			SolutionSquares[i] = newSolutionSquare;

			Squares[i] = newSquare;

			indices[i] = i;
		}

		if (splitSquaresCode != null)
		{
			for (var i = 0; i < Squares.Length; i++)
			{
				var square = Squares[i];

				square.TargetScheme = (Square.TargetingScheme)int.Parse(splitSquaresCode[i][1].ToString());
				square.SetCascading(splitSquaresCode.Length > 2 && splitSquaresCode[i][2] == 'c', false);

				if (splitSquaresCode[i][0] == 't')
				{
					square.Toggle();
				}
			}
		}
		else
		{
			if (Squares.Length == 1)
			{
				var square = Squares[0];

				square.TargetScheme = Square.TargetingScheme.Self;
				square.SetCascading(false, false);
			}
			else if (Squares.Length == 2)
			{
				var firstSquare = Squares[UnityEngine.Random.Range(0, 2)];
				var otherSquare = firstSquare.PreviousSquare;

				/*if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
				{
					firstSquare.Toggle();
				}

				if(!firstSquare.Toggled)
				{
					otherSquare.Toggle();
				}*/

				firstSquare.TargetScheme = firstSquare.Id == 0 ? Square.TargetingScheme.Right : Square.TargetingScheme.Left;
				firstSquare.SetCascading(false, false);

				otherSquare.TargetScheme = Square.TargetingScheme.Self;
				otherSquare.SetCascading(false, false);

				otherSquare.Toggle();
			}
			else
			{
				var atLeastOneUntoggledSquare = _unclickableToggledSquares ? true : false;

				var shuffledSquaresArray = new List<Square>(Squares).OrderBy(a => System.Guid.NewGuid()).ToArray();

				for (var i = 0; i < shuffledSquaresArray.Length; i++)
				{
					if (!atLeastOneUntoggledSquare && i == shuffledSquaresArray.Length - 1)
					{
						break;
					}

					if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
					{
						shuffledSquaresArray[i].Toggle();
					}

					if (!atLeastOneUntoggledSquare && shuffledSquaresArray[i].ToggledState == Square.PossibleToggleState.Zero)
					{
						atLeastOneUntoggledSquare = true;
					}
				}

				var squaresForWrapAround = new List<Square>();

				if (_currentProgressionEntry.WrapAroundToggles != 0)
				{
					var numWrapAroundToggles = _currentProgressionEntry.WrapAroundToggles < 0
						? UnityEngine.Random.Range(0, 3)
						: UnityEngine.Random.Range(_currentProgressionEntry.WrapAroundToggles, 3);

					switch (numWrapAroundToggles)
					{
						case 1:
							squaresForWrapAround.Add(UnityEngine.Random.Range(0f, 1f) > 0.5f ? Squares[0] : Squares[Squares.Length - 1]);
							break;
						case 2:
							squaresForWrapAround.Add(Squares[0]);
							squaresForWrapAround.Add(Squares[Squares.Length - 1]);
							break;
					}
				}

				var shuffledSquares = new List<Square>(Squares).OrderBy(x => new System.Random().Next()).ToList();

				var targetingSchemes = new List<Square.TargetingScheme>();
				var orderedTargetingSchemes = new List<Square.TargetingScheme>();

				var targetingSchemeToProgressionEntryTargetsMap = new Dictionary<Square.TargetingScheme, int>
				{
					{ Square.TargetingScheme.SelfLeftRight, _currentProgressionEntry.SelfLeftRightTargets },
					{ Square.TargetingScheme.SelfLeft, _currentProgressionEntry.SelfLeftTargets },
					{ Square.TargetingScheme.SelfRight, _currentProgressionEntry.SelfRightTargets },
					{ Square.TargetingScheme.LeftRight, _currentProgressionEntry.LeftRightTargets }
				};

				foreach (var targetingSchemeToProgressionEntryTargetsItem in targetingSchemeToProgressionEntryTargetsMap)
				{
					if (targetingSchemeToProgressionEntryTargetsItem.Value > 0)
					{
						targetingSchemes.Add(targetingSchemeToProgressionEntryTargetsItem.Key);

						for (var i = 0; i < targetingSchemeToProgressionEntryTargetsItem.Value; i++)
						{
							orderedTargetingSchemes.Add(targetingSchemeToProgressionEntryTargetsItem.Key);
						}
					}
					else if (targetingSchemeToProgressionEntryTargetsItem.Value < 0)
					{
						targetingSchemes.Add(targetingSchemeToProgressionEntryTargetsItem.Key);
					}
				}

				targetingSchemes.Add(Square.TargetingScheme.Left);
				orderedTargetingSchemes.Add(Square.TargetingScheme.Left);

				targetingSchemes.Add(Square.TargetingScheme.Right);
				orderedTargetingSchemes.Add(Square.TargetingScheme.Right);

				targetingSchemes.Add(Square.TargetingScheme.Self);
				orderedTargetingSchemes.Add(Square.TargetingScheme.Self);

				foreach (var squareForWrapAround in squaresForWrapAround)
				{
					var possibleTargetingSchemes = (squareForWrapAround.Id == 0
						? orderedTargetingSchemes.Where(x => x != Square.TargetingScheme.SelfRight && x != Square.TargetingScheme.Right && x != Square.TargetingScheme.Self)
						: orderedTargetingSchemes.Where(x => x != Square.TargetingScheme.SelfLeft && x != Square.TargetingScheme.Left && x != Square.TargetingScheme.Self))
						.ToList();

					foreach (var targetingSchemeToProgressionEntryTargetsItem in targetingSchemeToProgressionEntryTargetsMap)
					{
						if (targetingSchemeToProgressionEntryTargetsItem.Value >= 0
							|| (squareForWrapAround.Id == 0 && targetingSchemeToProgressionEntryTargetsItem.Key == Square.TargetingScheme.SelfRight)
							|| (squareForWrapAround.Id == Squares.Length - 1 && targetingSchemeToProgressionEntryTargetsItem.Key == Square.TargetingScheme.SelfLeft))
						{
							continue;
						}

						possibleTargetingSchemes.Add(targetingSchemeToProgressionEntryTargetsItem.Key);
					}

					squareForWrapAround.TargetScheme = possibleTargetingSchemes[UnityEngine.Random.Range(0, possibleTargetingSchemes.Count)];

					if (orderedTargetingSchemes.Contains(squareForWrapAround.TargetScheme))
					{
						orderedTargetingSchemes.Remove(squareForWrapAround.TargetScheme);
					}

					shuffledSquares.Remove(squareForWrapAround);

					// The random assignment of possible targeting schemes for wrap around squares might make it so
					// "mandatory" targeting schemes aren't assigned to any square. This can happen because the wrap around
					// squares "take up space" that is reserved for random assignment, not priority assignment.
					// Maybe we can solve this by doing a "final pass" to change the non-mandatory targeting schemes of edge 
					// squares to "mandatory" targeting schemes that haven't been assigned to any square yet because there 
					// wasn't enough space for them because of the wrap around toggles
					// This only happens in very small arrays and might not even occur in the final progression 
					// where small arrays only have a limited number of features so we'll ignore it for now and revisit
					// this only if it actually proves to be an issue
				}

				for (var i = 0; i < shuffledSquares.Count; i++)
				{
					var shuffledSquare = shuffledSquares[i];
					var possibleOrderedTargetingSchemes = new List<Square.TargetingScheme>();
					var possibleTargetingSchemes = new List<Square.TargetingScheme>();

					if (shuffledSquare.Id == 0)
					{
						Func<Square.TargetingScheme, bool> condition = x =>
						x != Square.TargetingScheme.Left &&
						x != Square.TargetingScheme.LeftRight &&
						x != Square.TargetingScheme.SelfLeft &&
						x != Square.TargetingScheme.SelfLeftRight;

						possibleOrderedTargetingSchemes = orderedTargetingSchemes.Where(condition).ToList();
						possibleTargetingSchemes = targetingSchemes.Where(condition).ToList();
					}
					else if (shuffledSquare.Id == Squares.Length - 1)
					{
						Func<Square.TargetingScheme, bool> condition = x =>
						x != Square.TargetingScheme.Right &&
						x != Square.TargetingScheme.LeftRight &&
						x != Square.TargetingScheme.SelfRight &&
						x != Square.TargetingScheme.SelfLeftRight;

						possibleOrderedTargetingSchemes = orderedTargetingSchemes.Where(condition).ToList();
						possibleTargetingSchemes = targetingSchemes.Where(condition).ToList();
					}
					else
					{
						possibleOrderedTargetingSchemes = orderedTargetingSchemes;
						possibleTargetingSchemes = targetingSchemes;
					}

					var targetingSchemesToRemove = new[]
					{
						Square.TargetingScheme.Self,
						Square.TargetingScheme.Left,
						Square.TargetingScheme.Right
					};

					foreach (var targetingSchemeToRemove in targetingSchemesToRemove)
					{
						if (possibleOrderedTargetingSchemes.Contains(targetingSchemeToRemove))
						{
							possibleOrderedTargetingSchemes.Remove(targetingSchemeToRemove);
						}
					}

					// TODO: the correct targeting schemes are not copied into the temp (possible) list correctly in this loop...
					// Enforcing a targeting scheme with a 1 doesn't work all the time...
					// ...Can we reproduce this? It seems like it's fixed?

					shuffledSquare.TargetScheme = possibleOrderedTargetingSchemes.Count > 0
						? possibleOrderedTargetingSchemes[0]
						: possibleTargetingSchemes[UnityEngine.Random.Range(0, possibleTargetingSchemes.Count)];

					if (orderedTargetingSchemes.Contains(shuffledSquare.TargetScheme))
					{
						orderedTargetingSchemes.Remove(shuffledSquare.TargetScheme);
					}
				}

				if (Squares.Length == 3 && Squares.All(x => x.TargetScheme == Square.TargetingScheme.Self))
				{
					Debug.Log("Ended up generating a level with all three squares self targeting, correcting this");

					Squares[1].TargetScheme = UnityEngine.Random.Range(0f, 1f) > 0.5f ? Square.TargetingScheme.Left : Square.TargetingScheme.Right;
				}

				/*foreach(var targetingSchemeToProgressionEntryTargetsItem in targetingSchemeToProgressionEntryTargetsMap)
				{
					if(targetingSchemeToProgressionEntryTargetsItem.Value > 0
						&& Squares.Count(x => x.TargetScheme == targetingSchemeToProgressionEntryTargetsItem.Key) < targetingSchemeToProgressionEntryTargetsItem.Value)
					{
						var possibleSquares = new List<Square>();

						switch (targetingSchemeToProgressionEntryTargetsItem.Key)
						{
							case Square.TargetingScheme.SelfLeft:
								break;
							case Square.TargetingScheme.SelfRight:
								break;
							case Square.TargetingScheme.LeftRight:
								break;
							case Square.TargetingScheme.SelfLeftRight:
								break;
						}
					}
				}*/

				// Up here is an attempt at solving the "wrap around squares overtaking priority assignment with random
				// assignment" issue. We're ignoring it until it proves to be an actual problem instead of an hypothetical one

				if (_currentProgressionEntry.CascadingToggles != 0)
				{
					var squaresThatCanBeCascading = new List<Square>();

					foreach (var square in Squares)
					{
						if (!squaresThatCanBeCascading.Contains(square.PreviousSquare)
							&& (square.TargetScheme == Square.TargetingScheme.Left
							|| square.TargetScheme == Square.TargetingScheme.LeftRight
							|| square.TargetScheme == Square.TargetingScheme.SelfLeft
							|| square.TargetScheme == Square.TargetingScheme.SelfLeftRight))
						{
							squaresThatCanBeCascading.Add(square.PreviousSquare);
						}

						if (!squaresThatCanBeCascading.Contains(square.NextSquare)
							&& (square.TargetScheme == Square.TargetingScheme.Right
							|| square.TargetScheme == Square.TargetingScheme.LeftRight
							|| square.TargetScheme == Square.TargetingScheme.SelfRight
							|| square.TargetScheme == Square.TargetingScheme.SelfLeftRight))
						{
							squaresThatCanBeCascading.Add(square.NextSquare);
						}
					}

					squaresThatCanBeCascading = squaresThatCanBeCascading.OrderBy(x => new System.Random().Next()).ToList();

					var numCascadingToggles = _currentProgressionEntry.CascadingToggles < 0
						? UnityEngine.Random.Range(0, squaresThatCanBeCascading.Count + 1)
						: _currentProgressionEntry.CascadingToggles;

					var assignedCascadingToggles = 0;

					for (var i = 0; i < squaresThatCanBeCascading.Count; i++)
					{
						var square = squaresThatCanBeCascading[i];

						if (!_currentProgressionEntry.AdjacentCascadingToggles)
						{
							if (square.NextSquare.Cascading
								&& (square.NextSquare.TargetScheme == Square.TargetingScheme.Left
								|| square.NextSquare.TargetScheme == Square.TargetingScheme.LeftRight
								|| square.NextSquare.TargetScheme == Square.TargetingScheme.SelfLeft
								|| square.NextSquare.TargetScheme == Square.TargetingScheme.SelfLeftRight))
							{
								continue;
							}

							if (square.NextSquare.Cascading
								&& (square.TargetScheme == Square.TargetingScheme.Right
								|| square.TargetScheme == Square.TargetingScheme.LeftRight
								|| square.TargetScheme == Square.TargetingScheme.SelfRight
								|| square.TargetScheme == Square.TargetingScheme.SelfLeftRight))
							{
								continue;
							}

							if (square.PreviousSquare.Cascading
								&& (square.PreviousSquare.TargetScheme == Square.TargetingScheme.Right
								|| square.PreviousSquare.TargetScheme == Square.TargetingScheme.LeftRight
								|| square.PreviousSquare.TargetScheme == Square.TargetingScheme.SelfRight
								|| square.PreviousSquare.TargetScheme == Square.TargetingScheme.SelfLeftRight))
							{
								continue;
							}

							if (square.PreviousSquare.Cascading
								&& (square.TargetScheme == Square.TargetingScheme.Left
								|| square.TargetScheme == Square.TargetingScheme.LeftRight
								|| square.TargetScheme == Square.TargetingScheme.SelfLeft
								|| square.TargetScheme == Square.TargetingScheme.SelfLeftRight))
							{
								continue;
							}
						}

						squaresThatCanBeCascading[i].SetCascading(true, false);

						assignedCascadingToggles++;

						if (assignedCascadingToggles >= numCascadingToggles)
						{
							break;
						}
					}

					if (_currentProgressionEntry.CascadingToggles > 0
						&& assignedCascadingToggles < _currentProgressionEntry.CascadingToggles)
					{
						Debug.Log("Couldn't assign the specified number of cascading toggles, not enough squares are targeted by others");
					}
				}
			}
		}

		_squareHistory.Clear();

		var firstHistorySnapshot = new HistorySquare[Squares.Length];

		_testSquares = new TestSquare[squares];

		for (var i = 0; i < Squares.Length; i++)
		{
			var square = Squares[i];

			firstHistorySnapshot[i] = new HistorySquare()
			{
				ToggledState = square.ToggledState,
				Interactable = _unclickableToggledSquares ? Squares[i].ToggledState == Square.PossibleToggleState.Zero : true,
				Cascading = square.Cascading
			};

			SolutionSquares[i].Initialize(this, square);
			SolutionSquares[i].TargetScheme = square.TargetScheme;
			SolutionSquares[i].SetCascading(square.Cascading, false);
			SolutionSquares[i].ToggleTo(square.ToggledState);

			_testSquares[i] = new TestSquare(square);

			square.SetupTargets(Squares);
			SolutionSquares[i].SetupTargets(SolutionSquares);

			_testSquares[i].SetupTargets(square);

			//square.SolutionCheck = SolutionSquares[i].SolutionCheck;
			//square.SolutionCross = SolutionSquares[i].SolutionCross;
		}

		for (var i = 0; i < Squares.Length; i++)
		{
			Squares[i].SetupPredictions(true);
		}

		_squareHistory.Add(firstHistorySnapshot);

		_rectangle.Width = (_squareTemplateRectangle.Width + _squaresDistance) * squares + _squaresDistance/* * 2*/;
		_rectangle.Height = _squareTemplateRectangle.Height + _squaresDistance * 2;

		_solutionRectangle.Width = (_solutionSquareTemplateRectangle.Width + _solutionSquaresDistance) * squares + _solutionSquaresDistance/* * 2*/;
		_solutionRectangle.Height = _solutionSquareTemplateRectangle.Height + _solutionSquaresDistance * 2;

		if (splitGoalCode != null && splitSolutionsCode != null)
		{
			GenerateSolution(splitGoalCode, splitSolutionsCode);
		}
		else
		{
			GenerateSolution(indices, 0);
		}

		Clicks = 0;

		_levelPanel.SetupSolutionBoxes(Solutions);
		_levelPanel.UpdateClicksCounter(false);

		SquaresToggledLastClick = new List<Square>(Squares);

		CheckLevelCompletion(false);

		//UpdateChecksAndCrosses();

		_levelPanel.UpdateHistoryButtons(false);

		//_generatedLevels.Add(_levelCode);

		/*var confirmationSquares = new TestSquare[Squares.Length];

		for (var i = 0; i < confirmationSquares.Length; i++)
		{
			confirmationSquares[i] = new TestSquare(Squares[i]);
		}

		for (var i = 0; i < confirmationSquares.Length; i++)
		{
			confirmationSquares[i].SetupTargets(Squares[i]);
		}

		for (var i = 0; i < Solutions[0].Sequence.Length; i++)
		{
			confirmationSquares[Solutions[0].Sequence[i]].Click(confirmationSquares);
		}

		var good = true;

		for (var i = 0; i < confirmationSquares.Length; i++)
		{
			if(confirmationSquares[i].Toggled != SolutionSquares[i].Toggled)
			{
				Debug.Log("There was an issue");

				good = false;

				break;
			}
		}

		if (good)
		{
			Debug.Log("We're good");
		}*/
	}

	private void GenerateSolution(string[] splitGoalCode, string[] splitSolutionsCode)
	{
		Solutions = new List<Solution>();

		foreach (var solutionstring in splitSolutionsCode)
		{
			var newSolution = new Solution();
			var sequence = new List<int>();

			for (var i = 0; i < solutionstring.Length; i++)
			{
				var character = solutionstring[i];
				var parsed = int.TryParse(character.ToString(), out int solutionInt);

				if (parsed)
				{
					sequence.Add(solutionInt);
				}
			}

			newSolution.Sequence = new List<int>(sequence);

			Solutions.Add(newSolution);
		}

		for (var i = 0; i < SolutionSquares.Length; i++)
		{
			var solutionSquare = SolutionSquares[i];

			solutionSquare.ToggleTo(splitGoalCode[i] == "t" ? Square.PossibleToggleState.Two : Square.PossibleToggleState.Zero);
		}
	}

	private void GenerateSolution(int[] indices, int attempt)
	{
		if (attempt > 0)
		{
			foreach (var solutionSquare in SolutionSquares)
			{
				solutionSquare.Reinitialize();
			}
		}

		Solutions = new List<Solution>();

		var validSolutionSequence = false;

		var mainSolution = (Solution)null;

		for (var i = 0; i < _solutionGenerationAttempts; i++)
		{
			if (indices.Length <= 2)
			{
				var levelsToDequeue = _playedLevels.Count - 1;

				for (var j = 0; j < levelsToDequeue; j++)
				{
					_playedLevels.Dequeue();
				}

				mainSolution = new Solution()
				{
					Sequence = new List<int>(new int[] { indices.Length == 1
					? 0
					: Squares.First(x => x.TargetScheme == Square.TargetingScheme.Self).PreviousSquare.Id
				})
				};

				SolutionSquares[mainSolution.Sequence[0]].Click(false, false);

				for (var j = 0; j < Squares.Length; j++)
				{
					if (Squares[j].ToggledState != SolutionSquares[j].ToggledState)
					{
						validSolutionSequence = true;

						break;
					}
				}
			}
			else
			{
				validSolutionSequence = false;

				if (_unclickableToggledSquares)
				{
					mainSolution = new Solution
					{
						Sequence = new List<int>()
					};

					var lastToggledIndex = -1;
					var maxSolutionSequenceLength = indices.Length - _maxClicksBufferForSolution;

					for (var j = 0; j < maxSolutionSequenceLength; j++)
					{
						var untoggledSquares = SolutionSquares.Where(x => x.ToggledState == Square.PossibleToggleState.Zero && x.Id != lastToggledIndex);

						if (untoggledSquares.Count() == 0)
						{
							break;
						}

						lastToggledIndex = untoggledSquares.OrderBy(a => System.Guid.NewGuid()).First().Id;

						mainSolution.Sequence.Add(lastToggledIndex);

						SolutionSquares[mainSolution.Sequence[j]].Click(false, false);
					}
				}
				else
				{
					mainSolution = new Solution
					{
						Sequence = new List<int>(new int[indices.Length - _maxClicksBufferForSolution])
					};

					/*if(!_binaryStateSquares)
					{
						var indicesTwice = new List<int>();
						indicesTwice.AddRange(indices);
						indicesTwice.AddRange(indices);

						indices = indicesTwice.ToArray();
					}*/

					var shuffledIndices = indices.OrderBy(a => System.Guid.NewGuid()).ToArray();

					for (var j = 0; j < mainSolution.Sequence.Count; j++)
					{
						mainSolution.Sequence[j] = shuffledIndices[j];

						SolutionSquares[mainSolution.Sequence[j]].Click(false, false);
					}
				}

				//_solutionSequence = new int[Random.Range(_minClicksForSolution, squares - _maxClicksBufferForSolution)];

				for (var j = 0; j < Squares.Length; j++)
				{
					if (Squares[j].ToggledState != SolutionSquares[j].ToggledState)
					{
						validSolutionSequence = true;

						//Debug.Log($"Generated a valid solution sequence in {i + 1} attempt(s)");

						break;
					}
				}
			}

			var tentativeLevelCode = GetCurrentLevelCode();
			var splitTentativeLevelCode = tentativeLevelCode.Split(';');

			foreach (var _playedLevel in _playedLevels)
			{
				var splitPlayedLevelLine = _playedLevel.Split(';');

				if (splitTentativeLevelCode[0].Equals(splitPlayedLevelLine[0], StringComparison.OrdinalIgnoreCase)
					&& splitTentativeLevelCode[1].Equals(splitPlayedLevelLine[1], StringComparison.OrdinalIgnoreCase))
				{
					validSolutionSequence = false;

					break;
				}
			}

			if (validSolutionSequence)
			{
				Solutions.Add(mainSolution);

				break;
			}
		}

		if (!validSolutionSequence)
		{
			Debug.Log("Couldn't generate a valid solution sequence, loading a prevalidated level from file");

			if (_unclickableToggledSquares || _distanceToggles || _binaryStateSquares)
			{
				Debug.Log($"Loading prevalidated levels from file isn't supported for non-regular levels. " +
					$"Press D + Enter to try and generate a new random level");

				_levelPanel.UpdateInvalidLevelX(true);

				return;
			}

			var pregeneratedLevel = GetValidPregeneratedLevel();

			if (pregeneratedLevel != null)
			{
				_levelCode = pregeneratedLevel;

				OverwriteLevel(_levelCode);

				//_levelPanel.SetupSolutionBoxes(Solutions);

				return;
			}
			else
			{
				Debug.Log($"Couldn't find a valid unplayed pregenerated level... What do we do here???");

				_levelPanel.UpdateInvalidLevelX(true);
			}
		}

		// Other solutions

		var array = new int[Squares.Length];

		for (var i = 0; i < array.Length; i++)
		{
			array[i] = i;
		}

		for (var i = 1; i <= Solutions[0].Sequence.Count; i++)
		{
			// Should the order of clicks matter??? Right now it doesn't
			// I expect that this will depend on what toggle features are implemented in the future 
			// So I'll leave the code that handles permutations in, if the need to use it arises at some point

			//GetPermutations(array, i);
			GetCombinations(array, i);
		}

#if UNITY_EDITOR
		var saveLevelToFile = _saveLevelsToFile;
#endif

		if ((_solutionType == SolutionType.SingleSolution && _forceSingleSolution && Solutions.Count > 1)
			|| (_solutionType == SolutionType.MultipleSolutions && _forceMultipleSolution && Solutions.Count == 1))
		{
			if (attempt < _solutionGenerationAttempts)
			{
				//Debug.Log("Had to recurse");

				attempt++;

				GenerateSolution(indices, attempt);

				return;
			}

			//Debug.Log("Couldn't force a single solution, settling for multiple");
			Debug.Log($"Couldn't force {(_solutionType == SolutionType.SingleSolution ? "a single solution" : "multiple solutions")}" +
				$" for a randomly generated level, loading a prevalidated level from file");

			/*if(_unclickableToggledSquares)
			{
				Debug.Log($"Loading prevalidated levels from file isn't supported for levels with unclickable toggled squares." +
					$"Press D + Enter to try and generate a new random level");

				_levelPanel.UpdateInvalidLevelX(true);

				return;
			}*/

			if (_unclickableToggledSquares || _distanceToggles || _binaryStateSquares)
			{
				Debug.Log($"Loading prevalidated levels from file isn't supported for non-regular levels." +
					$"settling for a random level");

#if UNITY_EDITOR
				saveLevelToFile = false;
#endif
			}
			else
			{
				var pregeneratedLevel = GetValidPregeneratedLevel();

				if (pregeneratedLevel != null)
				{
					_levelCode = pregeneratedLevel;

					OverwriteLevel(_levelCode);

					//_levelPanel.SetupSolutionBoxes(Solutions);

					return;
				}

#if UNITY_EDITOR
				saveLevelToFile = false;
#endif

				Debug.Log($"Couldn't find a valid unplayed pregenerated level, settling for a random " +
					$"{(_solutionType == SolutionType.SingleSolution ? "multi-solution" : "single-solution")} level");
			}
		}

		_levelCode = GetCurrentLevelCode();

#if UNITY_EDITOR
		if (saveLevelToFile)
		{
			var levelLine = _levelCode + "\n";

			var lines = File.ReadAllLines(_levelsFilePath);
			var save = true;

			foreach (var line in lines)
			{
				var splitLine = line.Split(';');
				var splitLevelCode = _levelCode.Split(';');

				if (splitLine[0].Equals(splitLevelCode[0], StringComparison.OrdinalIgnoreCase)
					&& splitLine[1].Equals(splitLevelCode[1], StringComparison.OrdinalIgnoreCase)
					&& splitLine[2].Length == splitLevelCode[2].Length)
				{
					save = false;

					break;
				}
			}

			if (save)
			{
				File.AppendAllText(_levelsFilePath, levelLine);
			}
		}
#endif

		if(_unclickableToggledSquares)
		{
			Solutions = Solutions.OrderByDescending(x => x.Sequence.Count).ToList();
		}
		else
		{
			Solutions = Solutions.OrderBy(x => x.Sequence.Count).ToList();
		}

#if UNITY_EDITOR
		if (_printSolutions)
		{
			PrintSolutions();
		}
#endif
	}

	[Button]
	private void PrintSolutions()
	{
		if (Solutions == null)
		{
			Debug.Log("Printing solutions is only possible while playing");

			return;
		}

		Debug.Log($"{Solutions.Count} possible solutions:");

		var solutionStrings = new string[Solutions.Count];

		for (var i = 0; i < Solutions.Count; i++)
		{
			Debug.Log(string.Join(", ", Solutions[i].Sequence));

			solutionStrings[i] = string.Join(string.Empty, Solutions[i].Sequence);
		}

		_levelPanel.UpdateDebugSolutionText(string.Join(" | ", solutionStrings));
	}

	private string GetValidPregeneratedLevel()
	{
		var lines = Resources.Load<TextAsset>(_levelsFileName.Split('.')[0]).text.Split('\n');

		var possibleLines = new List<string>();

		foreach (var line in lines)
		{
			/*if (_playedLevels.Any(x => x.Equals(line)))
			{
				continue;
			}*/

			var splitLevelCode = line.Split(';');
			var splitSquaresCode = splitLevelCode[0].Split(',');

			if (splitSquaresCode.Length < Squares.Length)
			{
				continue;
			}

			var alreadyPlayed = false;

			foreach (var _playedLevel in _playedLevels)
			{
				var splitPlayedLevelLine = _playedLevel.Split(';');

				if (splitLevelCode[0].Equals(splitPlayedLevelLine[0], StringComparison.OrdinalIgnoreCase)
					&& splitLevelCode[1].Equals(splitPlayedLevelLine[1], StringComparison.OrdinalIgnoreCase)
					&& splitLevelCode[2].Length == splitPlayedLevelLine[2].Length)
				{
					alreadyPlayed = true;

					break;
				}
			}

			if (alreadyPlayed)
			{
				continue;
			}

			// This assumes the levels are ordered in ascending number of squares in the file
			if (splitSquaresCode.Length > Squares.Length)
			{
				break;
			}

			var tentativeLevelProgression = new ProgressionEntry();

			for (var i = 0; i < splitSquaresCode.Length; i++)
			{
				var squareCode = splitSquaresCode[i];

				var previousSquareCode = i > 0 ? splitSquaresCode[i - 1] : string.Empty;
				var nextSquareCode = i < splitSquaresCode.Length - 1 ? splitSquaresCode[i + 1] : string.Empty;

				var squareTargetingScheme = (Square.TargetingScheme)int.Parse(squareCode[1].ToString());

				if (squareCode.Length > 2 && squareCode[2] == 'c')
				{
					tentativeLevelProgression.CascadingToggles++;

					if (!tentativeLevelProgression.AdjacentCascadingToggles &&
						((!string.IsNullOrEmpty(previousSquareCode) && previousSquareCode.Length > 2 && previousSquareCode[2] == 'c')
						|| (!string.IsNullOrEmpty(nextSquareCode) && nextSquareCode.Length > 2 && nextSquareCode[2] == 'c')))
					{
						tentativeLevelProgression.AdjacentCascadingToggles = true;
					}
				}

				var first = i == 0;
				var last = i == splitSquaresCode.Length - 1;

				switch (squareTargetingScheme)
				{
					case Square.TargetingScheme.Self:

						break;

					case Square.TargetingScheme.Left:

						if (first)
						{
							tentativeLevelProgression.WrapAroundToggles++;
						}

						break;

					case Square.TargetingScheme.Right:

						if (last)
						{
							tentativeLevelProgression.WrapAroundToggles++;
						}

						break;

					case Square.TargetingScheme.SelfLeft:

						if (first)
						{
							tentativeLevelProgression.WrapAroundToggles++;
						}

						tentativeLevelProgression.SelfLeftTargets++;

						break;

					case Square.TargetingScheme.SelfRight:

						if (last)
						{
							tentativeLevelProgression.WrapAroundToggles++;
						}

						tentativeLevelProgression.SelfRightTargets++;

						break;

					case Square.TargetingScheme.LeftRight:

						if (first || last)
						{
							tentativeLevelProgression.WrapAroundToggles++;
						}

						tentativeLevelProgression.LeftRightTargets++;

						break;

					case Square.TargetingScheme.SelfLeftRight:

						if (first || last)
						{
							tentativeLevelProgression.WrapAroundToggles++;
						}

						tentativeLevelProgression.SelfLeftRightTargets++;

						break;
				}
			}

			if ((_currentProgressionEntry.WrapAroundToggles == 0 && tentativeLevelProgression.WrapAroundToggles > 0)
				|| (_currentProgressionEntry.WrapAroundToggles > 0 && _currentProgressionEntry.WrapAroundToggles > tentativeLevelProgression.WrapAroundToggles)
				|| (_currentProgressionEntry.LeftRightTargets == 0 && tentativeLevelProgression.LeftRightTargets > 0)
				|| (_currentProgressionEntry.LeftRightTargets >= 0 && _currentProgressionEntry.LeftRightTargets > tentativeLevelProgression.LeftRightTargets)
				|| (_currentProgressionEntry.SelfRightTargets == 0 && tentativeLevelProgression.SelfRightTargets > 0)
				|| (_currentProgressionEntry.SelfRightTargets >= 0 && _currentProgressionEntry.SelfRightTargets != tentativeLevelProgression.SelfRightTargets)
				|| (_currentProgressionEntry.SelfLeftTargets == 0 && tentativeLevelProgression.SelfLeftTargets > 0)
				|| (_currentProgressionEntry.SelfLeftTargets >= 0 && _currentProgressionEntry.SelfLeftTargets != tentativeLevelProgression.SelfLeftTargets)
				|| (_currentProgressionEntry.SelfLeftRightTargets == 0 && tentativeLevelProgression.SelfLeftRightTargets > 0)
				|| (_currentProgressionEntry.SelfLeftRightTargets >= 0 && _currentProgressionEntry.SelfLeftRightTargets != tentativeLevelProgression.SelfLeftRightTargets)
				|| (_currentProgressionEntry.CascadingToggles != tentativeLevelProgression.CascadingToggles)
				|| (_currentProgressionEntry.AdjacentCascadingToggles != tentativeLevelProgression.AdjacentCascadingToggles))
			{
				continue;
			}

			possibleLines.Add(line);
		}

		return possibleLines.Count > 0 ? possibleLines[UnityEngine.Random.Range(0, possibleLines.Count)] : null;
	}

	private string GetCurrentLevelCode()
	{
		var levelLine = string.Empty;

		for (var i = 0; i < Squares.Length; i++)
		{
			var square = Squares[i];

			levelLine += $"{(square.ToggledState != Square.PossibleToggleState.Zero ? "t" : "f")}{(int)square.TargetScheme}{(square.Cascading ? "c" : string.Empty)}{(i < Squares.Length - 1 ? "," : string.Empty)}";
		}

		levelLine += ";";

		for (var i = 0; i < SolutionSquares.Length; i++)
		{
			var solutionSquare = SolutionSquares[i];

			levelLine += $"{(solutionSquare.ToggledState != Square.PossibleToggleState.Zero ? "t" : "f")}{(i < SolutionSquares.Length - 1 ? "," : string.Empty)}";
		}

		levelLine += ";";

		for (var i = 0; i < Solutions.Count; i++)
		{
			var solution = Solutions[i];

			levelLine += $"{string.Join("", solution.Sequence)}{(i < Solutions.Count - 1 ? "," : string.Empty)}";
		}

		return levelLine;
	}

	public void GetCombinations(int[] array, int n)
	{
		var currentCombination = new List<int>();

		Combine(array, currentCombination, 0, n);
	}

	private void Combine(int[] array, List<int> currentCombination, int startIndex, int n)
	{
		if (_solutionType == SolutionType.SingleSolution && Solutions.Count > 1)
		{
			return;
		}

		if (currentCombination.Count == n)
		{
			//var sameAsMainSolutionSequence = currentCombination.SequenceEqual(PotentialSolutions[0].Sequence.OrderBy(x => x));
			var sameAsMainSolutionSequence = currentCombination.SequenceEqual(Solutions[0].Sequence.OrderBy(x => x));

			if (sameAsMainSolutionSequence)
			{
				return;
			}

			// Decided to not compile solutions of the same length,
			// to avoid duplicate clicks count on the interface
			// and to avoid the complications of checking for exact
			// solution sequences instead of simple numbers of clicks
			//var sameLengthAsAnotherSolution = PotentialSolutions.Any(x => x.Sequence.Length == currentCombination.Count);
			var sameLengthAsAnotherSolution = Solutions.Any(x => x.Sequence.Count == currentCombination.Count);

			if (sameLengthAsAnotherSolution)
			{
				return;
			}

			for (var i = 0; i < _testSquares.Length; i++)
			{
				_testSquares[i].Reset();
			}

			for (var i = 0; i < currentCombination.Count; i++)
			{
				_testSquares[currentCombination[i]].Click(_testSquares);
			}

			for (var i = 0; i < _testSquares.Length; i++)
			{
				if (_testSquares[i].ToggledState != SolutionSquares[i].ToggledState)
				{
					return;
				}
			}

			//PotentialSolutions.Add(new Solution
			Solutions.Add(new Solution
			{
				Sequence = currentCombination.ToList()
			});

			return;
		}

		for (int i = startIndex; i < array.Length; i++)
		{
			if (_solutionType == SolutionType.SingleSolution && Solutions.Count > 1)
			{
				return;
			}

			currentCombination.Add(array[i]);

			Combine(array, currentCombination, i + 1, n);

			// Backtrack
			currentCombination.RemoveAt(currentCombination.Count - 1);
		}
	}

	/*private void GetPermutations(int[] array, int n)
	{
		var currentPermutation = new List<int>();
		var used = new bool[array.Length];

		Permute(array, currentPermutation, used, n);
	}*/

	/*private void Permute(int[] array, List<int> currentPermutation, bool[] used, int n)
	{
		if (currentPermutation.Count == n)
		{
			var skip = _solutionSequences.Any(x => x.OrderBy(x => x).SequenceEqual(currentPermutation.OrderBy(x => x)));

			// Also checks if the same as the MainSolutionSequence

			if(skip)
			{
				// order of clicks doesn't matter????
				// if not, we shouldn't get every permutation, then.....

				return;
			}

			for (var i = 0; i < _testSquares.Length; i++)
			{
				_testSquares[i].Reset();
			}

			for (var i = 0; i < currentPermutation.Count; i++)
			{
				_testSquares[currentPermutation[i]].Click(_testSquares);
			}

			for (var i = 0; i < _testSquares.Length; i++)
			{
				if(_testSquares[i].Toggled != SolutionSquares[i].Toggled)
				{
					return;
				}
			}

			_solutionSequences.Add(currentPermutation.ToArray());

			return;
		}

		for (int i = 0; i < array.Length; i++)
		{
			if (!used[i])
			{
				used[i] = true;
				currentPermutation.Add(array[i]);

				Permute(array, currentPermutation, used, n);

				// Backtrack
				used[i] = false;
				currentPermutation.RemoveAt(currentPermutation.Count - 1);
			}
		}
	}*/

	public void NextLevel()
	{
		/*if(_progressionIndexBuffer < 0)
		{
			_progressionIndexBuffer++;

			GenerateLevel(_generatedLevels[_progressionIndex + _progressionIndexBuffer]);

			return;
		}*/

		if (_solutionType == SolutionType.SingleSolution
			&& _clicksCountRestriction == ClicksCountToNextLevelRestriction.SoftRestriction
			&& ClicksLeft < 0)
		{
			_levelPanel.ShakeNextLevelButton();
			_levelPanel.ShakeClicksCounter();
		}
		else
		{
			GenerateLevel();
		}
	}

	/*public void Previouslevel()
	{
		_progressionIndexBuffer--;

		GenerateLevel(_generatedLevels[_progressionIndex + _progressionIndexBuffer]);
	}*/

	/*public void UpdateChecksAndCrosses()
	{
		foreach (var square in Squares)
		{
			var correct = square.Toggled == SolutionSquares[square.Id].Toggled;

			SolutionSquares[square.Id].UpdateCheckAndCross(correct, false);
		}
	}*/

	public void ResetLevel()
	{
		LoadHistorySnapshot(0);

		CheckLevelCompletion(false);

		//UpdateChecksAndCrosses();

		_levelPanel.UpdateHistoryButtons(false);
	}

	public void Undo()
	{
		if (Clicks == 0)
		{
			return;
		}

		LoadHistorySnapshot(Clicks - 1);

		CheckLevelCompletion(false);

		//UpdateChecksAndCrosses();

		_levelPanel.UpdateHistoryButtons(false);
	}

	public void Redo()
	{
		if (Clicks >= _squareHistory.Count - 1)
		{
			return;
		}

		LoadHistorySnapshot(Clicks + 1);

		CheckLevelCompletion(false);

		//UpdateChecksAndCrosses();

		_levelPanel.UpdateHistoryButtons(false);
	}

	private void AddNewHistorySnapshot()
	{
		for (var i = _squareHistory.Count - 1; i >= Clicks; i--)
		{
			_squareHistory.RemoveAt(i);
		}

		var newHistorySnapshot = new HistorySquare[Squares.Length];

		for (var i = 0; i < Squares.Length; i++)
		{
			newHistorySnapshot[i] = new HistorySquare()
			{
				ToggledState = Squares[i].ToggledState,
				Interactable = Squares[i].Interactable,
				Cascading = Squares[i].Cascading
			};
		}

		_squareHistory.Add(newHistorySnapshot);
	}

	private void LoadHistorySnapshot(int index)
	{
		Clicks = index;

		var historySnapshot = _squareHistory[Clicks];

		for (var i = 0; i < Squares.Length; i++)
		{
			Squares[i].ToggleTo(historySnapshot[i].ToggledState);
			Squares[i].Interactable = _unclickableToggledSquares ? Squares[i].ToggledState == Square.PossibleToggleState.Zero : historySnapshot[i].Interactable;
			Squares[i].SetCascading(historySnapshot[i].Cascading, false);
		}

		foreach (var square in Squares)
		{
			square.SetupPredictions(false);
		}

		_levelPanel.UpdateClicksCounter(false);
	}

	public bool GetLevelCompletion(bool fromClickedSquare)
	{
		/*if(_solutionType == SolutionType.MultipleSolutions)
		{
			foreach (var solution in Solutions)
			{
				if(solution.Solved && solution.Sequence.Length == _clicks)
				{
					return false;
				}
			}
		}*/

		var levelComplete = true;

		for (var i = 0; i < Squares.Length; i++)
		{
			var correct = Squares[i].ToggledState == SolutionSquares[i].ToggledState;

			if (!correct && levelComplete)
			{
				levelComplete = false;
			}

			/*if(SquaresToggledLastClick.Contains(Squares[i]))
			{
				SolutionSquares[i].UpdateCheckAndCross(correct, fromClickedSquare);
			}*/
		}

		return levelComplete;
	}

	public void CheckLevelCompletion(bool fromClickedSquare)
	{
		var levelComplete = GetLevelCompletion(fromClickedSquare);
		var allSolutionsFound = true;
		var solutionAlreadyFound = false;

		if (_solutionType == SolutionType.MultipleSolutions)
		{
			foreach (var solution in Solutions)
			{
				if (solution.Solved)
				{
					if (Clicks == solution.Sequence.Count)
					{
						solutionAlreadyFound = true;
					}

					continue;
				}

				var solutionClicks = int.Parse(solution.SolutionClicksBox.ClicksText.text);

				solution.SolutionClicksBox.BustedOverlay.SetActive(Clicks > solutionClicks
					|| Clicks == solutionClicks && !levelComplete);

				solution.Solved = Clicks == solutionClicks && levelComplete;

				if (!solution.Solved)
				{
					allSolutionsFound = false;
				}

				solution.SolutionClicksBox.SolvedOverlay.SetActive(solution.Solved);
			}
		}
		else
		{
			if (Solutions[0].Solved)
			{
				if (Clicks == Solutions[0].Sequence.Count)
				{
					solutionAlreadyFound = true;
				}
			}
			else
			{
				Solutions[0].Solved = Clicks == Solutions[0].Sequence.Count && levelComplete;
			}
		}

		if (_solutionType == SolutionType.SingleSolution
			&& (_clicksCountRestriction != ClicksCountToNextLevelRestriction.SoftRestriction || ClicksLeft >= 0))
		{
			for (var i = 0; i < Squares.Length; i++)
			{
				var square = Squares[i];

				if (levelComplete)
				{
					square.Interactable = false;
				}
				//square.Interactable = !levelComplete;

				if (square.Highlighted)
				{
					square.OnMouseOverExit();
				}
			}
		}

		var coroutine = PlayLevelCompleteSequence(levelComplete, solutionAlreadyFound, allSolutionsFound);

		StartCoroutine(coroutine);
	}

	private IEnumerator PlayLevelCompleteSequence(bool levelComplete, bool solutionAlreadyFound, bool allSolutionsFound)
	{
		if (levelComplete)
		{
			if (_solutionType == SolutionType.SingleSolution
			&& _clicksCountRestriction == ClicksCountToNextLevelRestriction.SoftRestriction
			&& ClicksLeft < 0)
			{
				LevelPanel.ShakeClicksCounter();
			}

			yield return new WaitForSeconds(_levelCompleteSequenceDelays[0]);

			ShowCorrectSolutionAnimation(!solutionAlreadyFound);

			yield return new WaitForSeconds(_levelCompleteSequenceDelays[1]);

			ShowLevelCompleteAnimation(!solutionAlreadyFound);

			if (!_trulyCompleted)
			{
				var trueCompletion = _solutionType == SolutionType.SingleSolution && ClicksLeft == 0
				|| _solutionType == SolutionType.MultipleSolutions && allSolutionsFound;

				if (trueCompletion)
				{
					_trulyCompleted = true;
				}

				if ((_solutionType == SolutionType.MultipleSolutions && allSolutionsFound)
					|| _solutionType == SolutionType.SingleSolution)
				{
					_progressionIndex++;

					yield return new WaitForSeconds(_levelCompleteSequenceDelays[2]);

					_levelPanel.UpdateLevelsClearedText(_progressionIndex, true/*, true*/);

					/*if (_unclickableToggledSquares)
					{
						_playedLevels.Enqueue(_levelCode);

						if (_playedLevels.Count > _playedLevelQueueSize)
						{
							_playedLevels.Dequeue();
						}
					}*/
				}
			}
		}

		if (_solutionType == SolutionType.MultipleSolutions)
		{
			_levelPanel.UpdateNextLevelButton(_trulyCompleted || (levelComplete
				&& (_completedSolutionsToNextLevelRestriction == CompletedSolutionsToNextLevelRestriction.AtLeastOneSolution && levelComplete
					|| _completedSolutionsToNextLevelRestriction == CompletedSolutionsToNextLevelRestriction.AllSolutions && allSolutionsFound
					)));
		}
		else
		{
			LevelPanel.UpdateNextLevelButton(_trulyCompleted || (levelComplete
				&& (_clicksCountRestriction == ClicksCountToNextLevelRestriction.NoRestriction
					|| (_clicksCountRestriction == ClicksCountToNextLevelRestriction.SoftRestriction && ClicksLeft >= 0)
					|| (_clicksCountRestriction == ClicksCountToNextLevelRestriction.HardRestriction && ClicksLeft >= 0)
					)));
		}
	}

	private void LoadProgressionEntries()
	{
		var lines = Resources.Load<TextAsset>(_progressionFile.Split('.')[0]).text.Split('\n');

		foreach (var line in lines)
		{
			var splitLine = line.Split(';');

			var newProgressionEntry = new ProgressionEntry()
			{
				Squares = int.Parse(splitLine[0]),
				LeftRightTargets = int.Parse(splitLine[1]),
				SelfRightTargets = int.Parse(splitLine[2]),
				SelfLeftTargets = int.Parse(splitLine[3]),
				SelfLeftRightTargets = int.Parse(splitLine[4]),
				WrapAroundToggles = int.Parse(splitLine[5]),
				CascadingToggles = int.Parse(splitLine[6]),
				AdjacentCascadingToggles = splitLine[7].Contains("1")
			};

			_progressionEntries.Add(newProgressionEntry);
		}
	}

	private void ShowLevelCompleteAnimation(bool trueCompletion)
	{
		if (_levelRectangleScale != null)
		{
			_levelRectangleScale.Kill(true);
		}

		_levelRectangleScale = transform.DOPunchScale(
			trueCompletion ? _solutionRectanglePunchScale : _solutionRectanglePunchScale / 2f,
			_solutionRectanglePunchTime,
			_solutionRectanglePunchVibrato,
			_solutionRectanglePunchElasticity
		).OnComplete(() =>
		{
			transform.localScale = Vector3.one;

			_levelRectangleScale = null;
		});

		var levelCompletionFeedbackWidth = 1f;
		var levelCompletionFeedbackHeight = 1f;
		var levelCompletionFeedbackThicknessBaseValue = 0.15f;

		if (_levelCompleteWidthScale != null)
		{
			_levelCompleteWidthScale.Kill(true);
		}

		_levelCompleteWidthScale = DOTween.To(() => levelCompletionFeedbackWidth, x =>
		{
			levelCompletionFeedbackWidth = x;

			_levelCompletionFeedback.Width = levelCompletionFeedbackWidth;
		},
		_levelCompletionFeedbackFinalSize.x * (trueCompletion ? 1f : 0.8125f),
		_levelCompletionTime).SetEase(_levelCompletionCurve).OnComplete(() =>
		{
			_levelCompletionFeedback.Width = _levelCompletionFeedbackFinalSize.x / 100f;

			_levelCompleteWidthScale = null;

		});

		if (_levelCompleteHeightScale != null)
		{
			_levelCompleteHeightScale.Kill(true);
		}

		_levelCompleteHeightScale = DOTween.To(() => levelCompletionFeedbackHeight, x =>
		{
			levelCompletionFeedbackHeight = x;

			_levelCompletionFeedback.Height = levelCompletionFeedbackHeight;
		},
		_levelCompletionFeedbackFinalSize.y * (trueCompletion ? 1f : 0.8125f),
		_levelCompletionTime).SetEase(_levelCompletionCurve).OnComplete(() =>
		{
			_levelCompletionFeedback.Height = _levelCompletionFeedbackFinalSize.y / 100f;

			_levelCompleteHeightScale = null;

		});

		if (_levelCompleteThicknessScale != null)
		{
			_levelCompleteThicknessScale.Kill(true);
		}

		var time = 0f;

		_levelCompletionFeedback.Color = trueCompletion ? _trueLevelCompletionColor : _levelCompletionFeedbackBaseColor;

		_levelCompleteThicknessScale = DOTween.To(() => time, x =>
		{
			time = x;

			_levelCompletionFeedback.Thickness = levelCompletionFeedbackThicknessBaseValue
			* _levelCompletionThicknessCurve.Evaluate(time)
			* (trueCompletion ? 1f : 0.375f);

			var color = _levelCompletionFeedback.Color;
			color.a = _levelCompletionAlphaCurve.Evaluate(time);

			_levelCompletionFeedback.Color = color;
		},
		1f,
		_levelCompletionTime).SetEase(_levelCompletionCurve).OnComplete(() =>
		{
			_levelCompletionFeedback.Thickness = levelCompletionFeedbackThicknessBaseValue;
			_levelCompletionFeedback.Color = _levelCompletionFeedbackBaseColor;

			_levelCompleteThicknessScale = null;
		});
	}

	private void ShowCorrectSolutionAnimation(bool trueCompletion)
	{
		if (_solutionRectangleScale != null)
		{
			_solutionRectangleScale.Kill(true);
		}

		_solutionRectangleScale = _solutionRectangle.transform.DOPunchScale(
			trueCompletion ? _solutionRectanglePunchScale : _solutionRectanglePunchScale / 3f,
			_solutionRectanglePunchTime,
			_solutionRectanglePunchVibrato,
			_solutionRectanglePunchElasticity
		).OnComplete(() =>
		{
			_solutionRectangle.transform.localScale = Vector3.one;

			_solutionRectangleScale = null;
		});
	}

#if UNITY_EDITOR
	[Button("Sort levels in files")]
	public void SortLevels()
	{
		var pathPrefix = Application.persistentDataPath + "/";

		SortLevels(pathPrefix + _singleSolutionLevelsFile);
		SortLevels(pathPrefix + _multiSolutionsLevelsFile);
	}

	private void SortLevels(string levelsFilePath)
	{
		var lines = File.ReadAllLines(levelsFilePath);

		var newLines = new List<string>();

		foreach (var line in lines)
		{
			var squareCodes = line.Split(';')[0].Split(',');

			if (squareCodes.Length > 1 && squareCodes.All(x => x.Contains("0")))
			{
				continue;
			}

			var splitLine = line.Split(';');

			var duplicate = newLines.Any(x =>
			{
				var otherSplitLine = x.Split(';');

				return splitLine[0].Equals(otherSplitLine[0], StringComparison.OrdinalIgnoreCase)
				&& splitLine[1].Equals(otherSplitLine[1], StringComparison.OrdinalIgnoreCase)
				&& splitLine[2].Length == otherSplitLine[2].Length;
			});

			if (duplicate)
			{
				continue;
			}

			newLines.Add(line);
		}

		var sortedLines = newLines.OrderBy(x => x.Split(';')[0].Split(',').Length);

		File.WriteAllLines(levelsFilePath, sortedLines);
	}
#endif
}
