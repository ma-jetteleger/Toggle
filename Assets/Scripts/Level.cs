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
	// Components
    [SerializeField] private GameObject _squareTemplate = null;
    [SerializeField] private GameObject _solutionSquareTemplate = null;
    [SerializeField] private Rectangle _rectangle = null;
    [SerializeField] private Rectangle _solutionRectangle = null;
    [SerializeField] private Rectangle _levelCompletionFeedback = null;

	// Generation parameters
	[SerializeField] private Vector2Int _squaresRange = Vector2Int.zero;
	//[SerializeField] private int _minClicksForSolution = 0;
	[SerializeField] private int _maxClicksBufferForSolution = 0;
	[SerializeField] private int _solutionGenerationAttempts = 0;

	// Animation/visual parameters
	[SerializeField] private float _squaresDistance = 0f;
	[SerializeField] private float _solutionSquaresDistance = 0f;
	[SerializeField] private Color _trueLevelCompletionColor = Color.black;
	[SerializeField] private float _levelCompletionTime = 0f;
    [SerializeField] private AnimationCurve _levelCompletionCurve = null;
    [SerializeField] private AnimationCurve _levelCompletionThicknessCurve = null;
    [SerializeField] private AnimationCurve _levelCompletionAlphaCurve = null;
    
	// Features
    [SerializeField] private bool _progression = false;
	[SerializeField] [HideIf(nameof(_progression))] private bool _wrapAroundToggles = false;
    [SerializeField] private SolutionType _solutionType = SolutionType.SingleSolution;
    [SerializeField] [ShowIf(nameof(_solutionType), SolutionType.SingleSolution)] private ClicksCountToNextLevelRestriction _clicksCountRestriction = ClicksCountToNextLevelRestriction.HardRestriction;
    [SerializeField] [ShowIf(nameof(_solutionType), SolutionType.SingleSolution)] private bool _forceSingleSolution = false;
	[SerializeField] [ShowIf(nameof(_solutionType), SolutionType.MultipleSolutions)] private CompletedSolutionsToNextLevelRestriction _completedSolutionsToNextLevelRestriction = CompletedSolutionsToNextLevelRestriction.AllSolutions;
	[SerializeField] [ShowIf(nameof(_solutionType), SolutionType.MultipleSolutions)] private bool _forceMultipleSolution = false;

	// File stuff
	[SerializeField] private bool _saveLevelsToFile = false;
	[SerializeField] private string _singleSolutionLevelsFile = string.Empty;
	[SerializeField] private string _multiSolutionsLevelsFile = string.Empty;
	[SerializeField] private int _playedLevelQueueSize = 0;

	public Square[] Squares { get; set; }
    public Square[] SolutionSquares { get; set; }
	public List<Solution> Solutions { get; set; }
	public List<Solution> PotentialSolutions { get; set; }

	public int ClicksLeft => Solutions[0].Sequence.Length - _clicks;
    public int Clicks => _clicks;
    public bool EmptyHistory => _squareHistory.Count == 1;
    public bool TopOfHistory => _clicks == _squareHistory.Count - 1;
    public bool BottomOfHistory => _clicks == 0;
    public SolutionType SolutionType => _solutionType;
    public bool WrapAroundToggles => _wrapAroundToggles;

    private string _levelsFileName => _solutionType == SolutionType.SingleSolution? _singleSolutionLevelsFile : _multiSolutionsLevelsFile;
    private string _levelsFilePath => Application.persistentDataPath + "/" + _levelsFileName;

    private Square _previousHoveredSquare;
    private Rectangle _squareTemplateRectangle;
    private Rectangle _solutionSquareTemplateRectangle;
    private Square _lastSquareClickedDown;
    private Vector2 _levelCompletionFeedbackFinalSize;
    private Color _levelCompletionFeedbackBaseColor;
    private int _clicks;
    private int _progressionIndex;
    private List<HistorySquare[]> _squareHistory;
	private TestSquare[] _testSquares;
	private Queue<string> _playedLevels;
	private string _levelCode;

    private void Awake()
	{
        _squareTemplateRectangle = _squareTemplate.GetComponent<Rectangle>();
        _solutionSquareTemplateRectangle = _solutionSquareTemplate.GetComponent<Rectangle>();

        _squareTemplate.SetActive(false);
        _solutionSquareTemplate.SetActive(false);

        _squareHistory = new List<HistorySquare[]>();

		_playedLevels = new Queue<string>();

	}

	private void Start()
	{
        _levelCompletionFeedbackFinalSize = new Vector2(Screen.width, Screen.height) / 50f;
        _levelCompletionFeedback.Width = _levelCompletionFeedbackFinalSize.x / 100f;
        _levelCompletionFeedback.Height = _levelCompletionFeedbackFinalSize.y / 100f;
        _levelCompletionFeedbackBaseColor = _levelCompletionFeedback.Color;

        GenerateLevel();

		LevelPanel.Instance.UpdateLevelsClearedText(_progressionIndex);
	}

    private void Update()
    {
        if(_lastSquareClickedDown == null)
		{
            if (Input.GetKeyDown(KeyCode.Return))
			{ 
				_playedLevels.Enqueue(_levelCode);
					
				if(_playedLevels.Count > _playedLevelQueueSize)
				{
					_playedLevels.Dequeue();
				}

				GenerateLevel();

				//OnLevelCompletion();

				return;
            }
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				_progressionIndex--;

				LevelPanel.Instance.UpdateLevelsClearedText(_progressionIndex);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				_progressionIndex++;

				LevelPanel.Instance.UpdateLevelsClearedText(_progressionIndex);
			}
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				_progressionIndex = 0;

				LevelPanel.Instance.UpdateLevelsClearedText(_progressionIndex);
			}

			var squareHovered = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero).collider?.GetComponent<Square>();

            if (squareHovered != null)
            {
                if(squareHovered.SolutionSquare || !squareHovered.Interactable)
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

            if(sameSquare)
			{
				if (_solutionType == SolutionType.SingleSolution
					&& _clicksCountRestriction == ClicksCountToNextLevelRestriction.HardRestriction 
					&& ClicksLeft <= 0)
				{
					_lastSquareClickedDown.Shake();

					LevelPanel.Instance.ShakeClicksCounter();
				}
				else
				{
					_lastSquareClickedDown.ToggleTargets();
					//_lastSquareClickedDown.ShowTargetPredictions();
					_lastSquareClickedDown.HideTargetPredictions();
					_lastSquareClickedDown.Interactable = false;

					_clicks++;

					AddNewHistorySnapshot();

					LevelPanel.Instance.UpdateClicksCounter();

					CheckLevelCompletion();
				}
			}

			_lastSquareClickedDown.OnMouseClickUp();
			_lastSquareClickedDown.OnMouseOverExit();

			_lastSquareClickedDown = null;
			_previousHoveredSquare = null;
        }
    }

	private void OverwriteLevel(string levelCode)
	{
		var splitLevelCode = levelCode.Split(';');
		var splitSquaresCode = splitLevelCode[0].Split(',');

		if(splitSquaresCode.Length != Squares.Length)
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

			Squares[i].Overwrite(squareCodeToggle, squareCodeTarget);
		}

		var firstHistorySnapshot = _squareHistory[0];

		for (var i = 0; i < Squares.Length; i++)
		{
			Squares[i].SetupTargetsAndPredictions(Squares);

			firstHistorySnapshot[i].Toggled = Squares[i].Toggled;
			firstHistorySnapshot[i].Interactable = true;
		}

		_squareHistory.Clear();
		_squareHistory.Add(firstHistorySnapshot);

		GenerateSolution(splitGoalCode, splitSolutionsCode);

		LevelPanel.Instance.SetupSolutionBoxes(Solutions);
	}

    public void GenerateLevel(string levelCode = null)
	{
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

		var splitLevelCode = levelCode != null ? levelCode.Split(';') : null;
		var splitSquaresCode = levelCode != null ? splitLevelCode[0].Split(',') : null;
		var splitGoalCode = levelCode != null ? splitLevelCode[1].Split(',') : null;
		var splitSolutionsCode = levelCode != null ? splitLevelCode[2].Split(',') : null;

		// Generation

		var squares = levelCode != null
			? splitSquaresCode.Length
			: _progression
				? (int)Mathf.Min(_squaresRange.x + Mathf.Floor(_progressionIndex / 3f), _squaresRange.y)
				: UnityEngine.Random.Range(_squaresRange.x, _squaresRange.y + 1);

		if(_progression)
		{
			_wrapAroundToggles = _progressionIndex >= 2;
		}
		
		var indices = new int[squares];

		Squares = new Square[squares];
		SolutionSquares = new Square[squares];

		_testSquares = new TestSquare[squares];

		_squareHistory.Clear();

		var firstHistorySnapshot = new HistorySquare[Squares.Length];

		for (var i = 0; i < squares; i++)
		{
			// Square

			var squareCodeToggle = splitSquaresCode != null
				? splitSquaresCode[i] != null
					? splitSquaresCode[i][0] == 't'
					: (bool?)null
				: null;
			var squareCodeTarget = splitSquaresCode != null
				? splitSquaresCode[i] != null
					? (Square.TargetingScheme)int.Parse(splitSquaresCode[i][1].ToString())
					: (Square.TargetingScheme?)null
				: null;

			var newSquare = Instantiate(_squareTemplate, _squareTemplate.transform.parent).GetComponent<Square>();
			newSquare.transform.localPosition = -(Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * (squares - 1)) / 2f
				+ Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * i;

			newSquare.Initialize(
				i, 
				this,
				squareCodeToggle,
				squareCodeTarget);

			newSquare.gameObject.SetActive(true);

			Squares[i] = newSquare;

			firstHistorySnapshot[i] = new HistorySquare()
			{
				Toggled = Squares[i].Toggled,
				Interactable = true
			};

			indices[i] = i;

			// Solution square

			var newSolutionSquare = Instantiate(_solutionSquareTemplate, _solutionSquareTemplate.transform.parent).GetComponent<Square>();
			newSolutionSquare.transform.localPosition = -(Vector3.right * (_solutionSquareTemplateRectangle.Width + _solutionSquaresDistance) * (squares - 1)) / 2f
				+ Vector3.right * (_solutionSquareTemplateRectangle.Width + _solutionSquaresDistance) * i;

			newSolutionSquare.Initialize(i, this, null, null, newSquare);

			newSolutionSquare.gameObject.SetActive(true);

			SolutionSquares[i] = newSolutionSquare;

			// Test square

			_testSquares[i] = new TestSquare(newSquare);
		}

		for (var i = 0; i < Squares.Length; i++)
		{
			Squares[i].SetupTargetsAndPredictions(Squares);

			_testSquares[i].SetupTargets(Squares[i]);
		}

		for (var i = 0; i < SolutionSquares.Length; i++)
		{
			SolutionSquares[i].SetupTargetsAndPredictions(SolutionSquares);
		}

		_squareHistory.Add(firstHistorySnapshot);

		_rectangle.Width = (_squareTemplateRectangle.Width + _squaresDistance) * squares + _squaresDistance/* * 2*/;
		_rectangle.Height = _squareTemplateRectangle.Height + _squaresDistance * 2;

		_solutionRectangle.Width = (_solutionSquareTemplateRectangle.Width + _solutionSquaresDistance) * squares + _solutionSquaresDistance/* * 2*/;
		_solutionRectangle.Height = _solutionSquareTemplateRectangle.Height + _solutionSquaresDistance * 2;

		if(levelCode != null)
		{
			GenerateSolution(splitGoalCode, splitSolutionsCode);
		}
		else
		{
			GenerateSolution(indices, 0);
		}

		_clicks = 0;

		LevelPanel.Instance.SetupSolutionBoxes(Solutions);
		LevelPanel.Instance.UpdateClicksCounter();

		CheckLevelCompletion();
	}

	private void GenerateSolution(string[] splitGoalCode, string[] splitSolutionsCode)
	{
		Solutions = new List<Solution>();

		foreach(var solutionstring in splitSolutionsCode)
		{
			var newSolution = new Solution
			{
				Sequence = new int[solutionstring.Length],
				Solved = false
			};

			for (var i = 0; i < solutionstring.Length; i++)
			{
				var character = solutionstring[i];
				newSolution.Sequence[i] = int.Parse(character.ToString());
			}

			Solutions.Add(newSolution);
		}

		for (var i = 0; i < SolutionSquares.Length; i++)
		{
			var solutionSquare = SolutionSquares[i];

			solutionSquare.Toggle(splitGoalCode[i] == "t");
		}
	}

	private void GenerateSolution(int[] indices, int attempt)
	{
		if(attempt > 0)
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
			validSolutionSequence = false;

			//_solutionSequence = new int[Random.Range(_minClicksForSolution, squares - _maxClicksBufferForSolution)];
			mainSolution = new Solution
			{
				Sequence = new int[indices.Length - _maxClicksBufferForSolution]
			};

			var shuffledIndices = indices.OrderBy(a => System.Guid.NewGuid()).ToArray();

			for (var j = 0; j < mainSolution.Sequence.Length; j++)
			{
				mainSolution.Sequence[j] = shuffledIndices[j];

				SolutionSquares[mainSolution.Sequence[j]].ToggleTargets();
			}

			for (var j = 0; j < Squares.Length; j++)
			{
				if (Squares[j].Toggled != SolutionSquares[j].Toggled)
				{
					validSolutionSequence = true;

					//Debug.Log($"Generated a valid solution sequence in {i + 1} attempt(s)");

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

			var pregeneratedLevel = GetValidPregeneratedLevel();

			if (pregeneratedLevel != null)
			{
				_levelCode = pregeneratedLevel;

				OverwriteLevel(_levelCode);

				return;
			}
			else
			{
				Debug.Log($"Couldn't find a valid unplayed pregenerated level... What do we do here???");
			}
		}

		// Other solutions

		var array = new int[Squares.Length];

		for (var i = 0; i < array.Length; i++)
		{
			array[i] = i;
		}

		for (var i = 1; i <= Solutions[0].Sequence.Length; i++)
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

			var pregeneratedLevel = GetValidPregeneratedLevel();

			if(pregeneratedLevel != null)
			{
				_levelCode = pregeneratedLevel;

				OverwriteLevel(_levelCode);

				return;
			}

#if UNITY_EDITOR
			saveLevelToFile = false;
#endif

			Debug.Log($"Couldn't find a valid unplayed pregenerated level, settling for a random " +
				$"{(_solutionType == SolutionType.SingleSolution ? "multi-solution" : "single-solution")} level");
		}

		_levelCode = GetCurrentLevelCode();

#if UNITY_EDITOR
		if (saveLevelToFile)
		{
			var levelLine = _levelCode + "\n";

			var lines = File.ReadAllLines(_levelsFilePath);

			if (!lines.Any(x => x.Equals(levelLine, StringComparison.OrdinalIgnoreCase)))
			{
				File.AppendAllText(_levelsFilePath, levelLine);
			}
		}
#endif

		// Order the solutions to be displayed in descending order?
		// Feels like going from highest to lowest, in terms of gameplay, 
		// makes for a bit more of a "climactic" progression/finish
		Solutions = Solutions.OrderByDescending(x => x.Sequence.Length).ToList();

		/*Debug.Log($"{Solutions.Count} possible solutions:");

		for (var i = 0; i < Solutions.Count; i++)
		{
			Debug.Log(string.Join(", ", Solutions[i].Sequence));
		}*/
	}

	private string GetValidPregeneratedLevel()
	{
		//var lines = File.ReadAllLines(_levelsFilePath);
		var lines = Resources.Load<TextAsset>(_levelsFileName.Split('.')[0]).text.Split('\n');

		var possibleLines = new List<string>();

		foreach(var line in lines)
		{
			if(_playedLevels.Any(x => x.Equals(line)))
			{
				continue;
			}

			var splitLevelCode = line.Split(';');
			var splitSquaresCode = splitLevelCode[0].Split(',');

			// This assumes the levels are ordered in ascending number of squares in the file
			if(splitSquaresCode.Length > Squares.Length)
			{
				break;
			}

			if(!_wrapAroundToggles)
			{
				var firstSquareTargetScheme = (Square.TargetingScheme)int.Parse(splitSquaresCode[0][1].ToString());
				var lastSquareTargetScheme = (Square.TargetingScheme)int.Parse(splitSquaresCode[splitSquaresCode.Length - 1][1].ToString());

				if (firstSquareTargetScheme == Square.TargetingScheme.Left ||
					firstSquareTargetScheme == Square.TargetingScheme.LeftRight ||
					firstSquareTargetScheme == Square.TargetingScheme.SelfLeft ||
					firstSquareTargetScheme == Square.TargetingScheme.SelfLeftRight ||
					lastSquareTargetScheme == Square.TargetingScheme.Right ||
					lastSquareTargetScheme == Square.TargetingScheme.LeftRight ||
					lastSquareTargetScheme == Square.TargetingScheme.SelfRight ||
					lastSquareTargetScheme == Square.TargetingScheme.SelfLeftRight)
				{
					continue;
				}
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

			levelLine += $"{(square.Toggled ? "t" : "f")}{(int)square.TargetScheme}{(i < Squares.Length - 1 ? "," : string.Empty)}";
		}

		levelLine += ";";

		for (var i = 0; i < SolutionSquares.Length; i++)
		{
			var solutionSquare = SolutionSquares[i];

			levelLine += $"{(solutionSquare.Toggled ? "t" : "f")}{(i < SolutionSquares.Length - 1 ? "," : string.Empty)}";
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
		if(_solutionType == SolutionType.SingleSolution && Solutions.Count > 1)
		{
			return;
		}

		if (currentCombination.Count == n)
		{
			//var sameAsMainSolutionSequence = currentCombination.SequenceEqual(PotentialSolutions[0].Sequence.OrderBy(x => x));
			var sameAsMainSolutionSequence = currentCombination.SequenceEqual(Solutions[0].Sequence.OrderBy(x => x));

			if(sameAsMainSolutionSequence)
			{
				return;
			}

			// Decided to not compile solutions of the same length,
			// to avoid duplicate clicks count on the interface
			// and to avoid the complications of checking for exact
			// solution sequences instead of simple numbers of clicks
			//var sameLengthAsAnotherSolution = PotentialSolutions.Any(x => x.Sequence.Length == currentCombination.Count);
			var sameLengthAsAnotherSolution = Solutions.Any(x => x.Sequence.Length == currentCombination.Count);

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
				if (_testSquares[i].Toggled != SolutionSquares[i].Toggled)
				{
					return;
				}
			}

			//PotentialSolutions.Add(new Solution
			Solutions.Add(new Solution
			{
				Sequence = currentCombination.ToArray()
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
        if (_solutionType == SolutionType.SingleSolution
			&& _clicksCountRestriction == ClicksCountToNextLevelRestriction.SoftRestriction 
			&& ClicksLeft < 0)
        {
            LevelPanel.Instance.ShakeNextLevelButton();
            LevelPanel.Instance.ShakeClicksCounter();
        }
        else
        {
            GenerateLevel();
        }
    }

    public void ResetLevel()
	{
        LoadHstorySnapshot(0);

		CheckLevelCompletion();
    }

    public void Undo()
	{
        if(_clicks == 0)
		{
            return;
        }

        LoadHstorySnapshot(_clicks - 1);

		CheckLevelCompletion();
	}

    public void Redo()
    {
        if (_clicks >= _squareHistory.Count - 1)
        {
            return;
        }

        LoadHstorySnapshot(_clicks + 1);

		CheckLevelCompletion();
	}

    private void AddNewHistorySnapshot()
	{
        for(var i = _squareHistory.Count - 1; i >= _clicks; i--)
		{
            _squareHistory.RemoveAt(i); 
        }

        var newHistorySnapshot = new HistorySquare[Squares.Length];

        for(var i = 0; i < Squares.Length; i++)
		{
			newHistorySnapshot[i] = new HistorySquare()
			{
				Toggled = Squares[i].Toggled,
				Interactable = Squares[i].Interactable
			};
        }

        _squareHistory.Add(newHistorySnapshot);
    }

    private void LoadHstorySnapshot(int index)
	{
        _clicks = index;

        var historySnapshot = _squareHistory[_clicks];

        for (var i = 0; i < Squares.Length; i++)
        {
            Squares[i].Toggle(historySnapshot[i].Toggled);
			Squares[i].Interactable = historySnapshot[i].Interactable;
        }

        LevelPanel.Instance.UpdateClicksCounter();
    }

	public bool GetLevelCompletion()
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
			if (Squares[i].Toggled != SolutionSquares[i].Toggled)
			{
				levelComplete = false;
			}
		}

		return levelComplete;
	}

	public void CheckLevelCompletion()
	{
        var levelComplete = GetLevelCompletion();

		var allSolutionsFound = true;

		foreach(var solution in Solutions)
		{
			if(!solution.Solved)
			{
				allSolutionsFound = false;
				
				break;
			}
		}

        LevelPanel.Instance.UpdateNextLevelButton(_completedSolutionsToNextLevelRestriction == CompletedSolutionsToNextLevelRestriction.AtLeastOneSolution && levelComplete
			|| _completedSolutionsToNextLevelRestriction == CompletedSolutionsToNextLevelRestriction.AllSolutions && allSolutionsFound);

		if (_solutionType == SolutionType.SingleSolution
			&& (_clicksCountRestriction != ClicksCountToNextLevelRestriction.SoftRestriction || ClicksLeft >= 0))
		{
			for (var i = 0; i < Squares.Length; i++)
			{
				var square = Squares[i];

				square.Interactable = !levelComplete;

				if (square.Highlighted)
				{
					square.OnMouseOverExit();
				}
			}
		}

		if (levelComplete)
		{
            if(_solutionType == SolutionType.SingleSolution
			&& _clicksCountRestriction == ClicksCountToNextLevelRestriction.SoftRestriction 
			&& ClicksLeft < 0)
			{
                LevelPanel.Instance.ShakeClicksCounter();
            }
			/*else
			{
                LevelPanel.Instance.UpdateHistoryButtons(false);
            }*/

			var trueCompletion = _solutionType == SolutionType.SingleSolution && ClicksLeft == 0
				|| _solutionType == SolutionType.MultipleSolutions && allSolutionsFound;

			OnLevelCompletion(trueCompletion);

			if((_solutionType == SolutionType.MultipleSolutions && allSolutionsFound)
				|| _solutionType == SolutionType.SingleSolution)
			{
				_progressionIndex++;

				LevelPanel.Instance.UpdateLevelsClearedText(_progressionIndex);

				_playedLevels.Enqueue(_levelCode);

				if (_playedLevels.Count > _playedLevelQueueSize)
				{
					_playedLevels.Dequeue();
				}
			}
        }
		/*else
		{
            
		}*/

		LevelPanel.Instance.UpdateHistoryButtons();
	}

    private void OnLevelCompletion(bool trueCompletion)
	{
        var levelCompletionFeedbackWidth = _levelCompletionFeedback.Width;
        var levelCompletionFeedbackHeight = _levelCompletionFeedback.Height;
        var levelCompletionFeedbackThicknessBaseValue = _levelCompletionFeedback.Thickness;

        DOTween.To(() => levelCompletionFeedbackWidth, x =>
        {
            levelCompletionFeedbackWidth = x;

            _levelCompletionFeedback.Width = levelCompletionFeedbackWidth;
        },
        _levelCompletionFeedbackFinalSize.x,
        _levelCompletionTime).SetEase(_levelCompletionCurve).OnComplete(() =>
		{
            _levelCompletionFeedback.Width = _levelCompletionFeedbackFinalSize.x / 100f;
        });

        DOTween.To(() => levelCompletionFeedbackHeight, x =>
        {
            levelCompletionFeedbackHeight = x;

            _levelCompletionFeedback.Height = levelCompletionFeedbackHeight;
        },
        _levelCompletionFeedbackFinalSize.y,
        _levelCompletionTime).SetEase(_levelCompletionCurve).OnComplete(() =>
        {
            _levelCompletionFeedback.Height = _levelCompletionFeedbackFinalSize.y / 100f;
        });

        var time = 0f;

		_levelCompletionFeedback.Color = trueCompletion ? _trueLevelCompletionColor : _levelCompletionFeedbackBaseColor;

		DOTween.To(() => time, x =>
        {
            time = x;

            _levelCompletionFeedback.Thickness = levelCompletionFeedbackThicknessBaseValue * _levelCompletionThicknessCurve.Evaluate(time);

            var color = _levelCompletionFeedback.Color;
            color.a = _levelCompletionAlphaCurve.Evaluate(time);

            _levelCompletionFeedback.Color = color;
        },
        1f,
        _levelCompletionTime).SetEase(_levelCompletionCurve).OnComplete(() =>
        {
            _levelCompletionFeedback.Thickness = levelCompletionFeedbackThicknessBaseValue;
            _levelCompletionFeedback.Color = _levelCompletionFeedbackBaseColor;
        });
    }
}
