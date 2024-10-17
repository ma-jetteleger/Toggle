using Shapes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;

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
    [SerializeField] private bool _wrapAroundToggles = false;
    [SerializeField] private SolutionType _solutionType = SolutionType.SingleSolution;
    [SerializeField] [ShowIf(nameof(_solutionType), SolutionType.SingleSolution)] private ClicksCountToNextLevelRestriction _clicksCountRestriction = ClicksCountToNextLevelRestriction.HardRestriction;
    [SerializeField] [ShowIf(nameof(_solutionType), SolutionType.SingleSolution)] private bool _forceSingleSolution = false;
	[SerializeField] [ShowIf(nameof(_solutionType), SolutionType.MultipleSolutions)] private CompletedSolutionsToNextLevelRestriction _completedSolutionsToNextLevelRestriction = CompletedSolutionsToNextLevelRestriction.AllSolutions;
	[SerializeField] [ShowIf(nameof(_solutionType), SolutionType.MultipleSolutions)] private bool _forceMultipleSolution = false;

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

    private void Awake()
	{
        _squareTemplateRectangle = _squareTemplate.GetComponent<Rectangle>();
        _solutionSquareTemplateRectangle = _solutionSquareTemplate.GetComponent<Rectangle>();

        _squareTemplate.SetActive(false);
        _solutionSquareTemplate.SetActive(false);

        _squareHistory = new List<HistorySquare[]>();
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

    public void GenerateLevel()
	{
		// Clean up

        if(Squares != null && Squares.Length != 0)
		{
            foreach(var square in Squares)
			{
                Destroy(square.gameObject);
			}

            foreach (var solutionSquare in SolutionSquares)
            {
                Destroy(solutionSquare.gameObject);
            }
		}

		// Generation

        var squares = _progression 
			? (int)Mathf.Min(_squaresRange.x + Mathf.Floor(_progressionIndex / 2f), _squaresRange.y)
			: Random.Range(_squaresRange.x, _squaresRange.y + 1);
        var indices = new int[squares];

        Squares = new Square[squares];
        SolutionSquares = new Square[squares];

		_testSquares = new TestSquare[squares];

        _squareHistory.Clear();

        var firstHistorySnapshot = new HistorySquare[Squares.Length];

        for (var i = 0; i < squares; i++)
        {
			// Square

            var newSquare = Instantiate(_squareTemplate, _squareTemplate.transform.parent).GetComponent<Square>();
            newSquare.transform.localPosition = -(Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * (squares - 1)) / 2f
                + Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * i;

            newSquare.Initialize(i, this);

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

            newSolutionSquare.Initialize(i, this, newSquare);

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

		// Main solution

		Solutions = new List<Solution>();
		PotentialSolutions = new List<Solution>();

		var validSolutionSequence = false;

		for(var k = 0; k < _solutionGenerationAttempts; k++)
		{
			validSolutionSequence = false;

			for (var i = 0; i < _solutionGenerationAttempts; i++)
			{
				validSolutionSequence = false;

				//_solutionSequence = new int[Random.Range(_minClicksForSolution, squares - _maxClicksBufferForSolution)];
				var mainSolution = new Solution
				{
					Sequence = new int[squares - _maxClicksBufferForSolution]
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
					PotentialSolutions.Add(mainSolution);

					break;
				}
			}

			if (!validSolutionSequence)
			{
				Debug.Log("Couldn't generate a valid solution sequence, settling for a suboptimal one");
			}

			// Other solutions

			var array = new int[Squares.Length];

			for (var i = 0; i < array.Length; i++)
			{
				array[i] = i;
			}

			for (var i = 1; i <= PotentialSolutions[0].Sequence.Length; i++)
			{
				// Should the order of clicks matter??? Right now it doesn't
				// I expect that this will depend on what toggle features are implemented in the future 
				// So I'll leave the code that handles permutations in, if the need to use it arises at some point

				//GetPermutations(array, i);
				GetCombinations(array, i);
			}

			if (_solutionType == SolutionType.SingleSolution && _forceSingleSolution && PotentialSolutions.Count > 1)
			{
				if(k == _solutionGenerationAttempts - 1)
				{
					Debug.Log("Couldn't force a single solution, settling for multiple");
				}
				
				continue;
			}
			else if (_solutionType == SolutionType.MultipleSolutions && _forceMultipleSolution && PotentialSolutions.Count == 1)
			{
				if (k == _solutionGenerationAttempts - 1)
				{
					Debug.Log("Couldn't force multiple solutions, settling for a single one");
				}

				continue;
			}
			else
			{
				// Order the solutions to be displayed in descending order?
				// Feels like going from highest to lowest, in terms of gameplay, 
				// makes for a bit more of a "climactic" progression/finish
				Solutions = PotentialSolutions.OrderByDescending(x => x.Sequence.Length).ToList();

				break;
			}
		}

		Debug.Log($"{Solutions.Count} possible solutions:");

		for (var i = 0; i < Solutions.Count; i++)
		{
			Debug.Log(string.Join(", ", Solutions[i].Sequence));
		}

		// Other things

		_clicks = 0;

        LevelPanel.Instance.SetupSolutionBoxes(Solutions);
        LevelPanel.Instance.UpdateClicksCounter();
        
        CheckLevelCompletion();
    }

	public void GetCombinations(int[] array, int n)
	{
		var currentCombination = new List<int>();

		Combine(array, currentCombination, 0, n);
	}

	private void Combine(int[] array, List<int> currentCombination, int startIndex, int n)
	{
		if (currentCombination.Count == n)
		{
			var sameAsMainSolutionSequence = currentCombination.SequenceEqual(PotentialSolutions[0].Sequence.OrderBy(x => x));

			if(sameAsMainSolutionSequence)
			{
				return;
			}

			// Decided to not compile solutions of the same length,
			// to avoid duplicate clicks count on the interface
			// and to avoid the complications of checking for exact
			// solution sequences instead of simple numbers of clicks
			var sameLengthAsAnotherSolution = PotentialSolutions.Any(x => x.Sequence.Length == currentCombination.Count);

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

			PotentialSolutions.Add(new Solution
			{
				Sequence = currentCombination.ToArray()
			});

			return;
		}

		for (int i = startIndex; i < array.Length; i++)
		{
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
