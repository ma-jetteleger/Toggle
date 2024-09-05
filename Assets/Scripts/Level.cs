using Shapes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Level : MonoBehaviour
{
    [SerializeField] private GameObject _squareTemplate = null;
    [SerializeField] private GameObject _solutionSquareTemplate = null;
    [SerializeField] private Rectangle _rectangle = null;
    [SerializeField] private Rectangle _solutionRectangle = null;
    [SerializeField] private TextMeshProUGUI _solutionClicksText = null;
    [SerializeField] private Rectangle _levelCompletionFeedback = null;
    [SerializeField] private float _levelCompletionTime = 0f;
    [SerializeField] private AnimationCurve _levelCompletionCurve = null;
    [SerializeField] private AnimationCurve _levelCompletionThicknessCurve = null;
    [SerializeField] private AnimationCurve _levelCompletionAlphaCurve = null;
    [SerializeField] private Vector2Int _squaresRange = Vector2Int.zero;
    [SerializeField] private int _minClicksForSolution = 0;
    [SerializeField] private int _maxClicksBufferForSolution = 0;
    [SerializeField] private float _squaresDistance = 0f;
    [SerializeField] private float _solutionSquaresDistance = 0f;

    public Square[] Squares { get; set; }
    public Square[] SolutionSquares { get; set; }
    
    private Square _previousHoveredSquare;
    private Rectangle _squareTemplateRectangle;
    private Rectangle _solutionSquareTemplateRectangle;
    private Square _lastSquareClickedDown;
    private int[] _solutionSequence;
    private Vector2 _levelCompletionFeedbackFinalSize;
    private Color _levelCompletionFeedbackBaseColor;

    private void Awake()
	{
        _squareTemplateRectangle = _squareTemplate.GetComponent<Rectangle>();
        _solutionSquareTemplateRectangle = _solutionSquareTemplate.GetComponent<Rectangle>();

        _squareTemplate.SetActive(false);
        _solutionSquareTemplate.SetActive(false);
    }

	private void Start()
	{
        _levelCompletionFeedbackFinalSize = new Vector2(Screen.width, Screen.height) / 72.5f;
        _levelCompletionFeedback.Width = _levelCompletionFeedbackFinalSize.x / 100f;
        _levelCompletionFeedback.Height = _levelCompletionFeedbackFinalSize.y / 100f;
        _levelCompletionFeedbackBaseColor = _levelCompletionFeedback.Color;

        GenerateLevel();
    }

    private void Update()
    {
        if(_lastSquareClickedDown == null)
		{
            if(Input.GetKeyDown(KeyCode.Return))
			{
                GenerateLevel();

                //OnLevelCompletion();

                return;
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
                    squareHovered.OnMouseOverEnter();
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

            _lastSquareClickedDown.OnMouseClickUp();

            if(sameSquare)
			{
                _lastSquareClickedDown.ToggleTargets(Squares);

                CheckLevelCompletion();
            }

            _lastSquareClickedDown = null;
        }
    }

    public void GenerateLevel()
	{
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

        var squares = Random.Range(_squaresRange.x, _squaresRange.y + 1);
        var indices = new int[squares];

        Squares = new Square[squares];
        SolutionSquares = new Square[squares];

        for (var i = 0; i < squares; i++)
        {
            var newSquare = Instantiate(_squareTemplate, _squareTemplate.transform.parent).GetComponent<Square>();
            newSquare.transform.localPosition = -(Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * (squares - 1)) / 2f
                + Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * i;

            newSquare.Initialize(i);

            newSquare.gameObject.SetActive(true);

            Squares[i] = newSquare;

            indices[i] = i;

            var newSolutionSquare = Instantiate(_solutionSquareTemplate, _solutionSquareTemplate.transform.parent).GetComponent<Square>();
            newSolutionSquare.transform.localPosition = -(Vector3.right * (_solutionSquareTemplateRectangle.Width + _solutionSquaresDistance) * (squares - 1)) / 2f
                + Vector3.right * (_solutionSquareTemplateRectangle.Width + _solutionSquaresDistance) * i;

            newSolutionSquare.Initialize(i, newSquare);

            newSolutionSquare.gameObject.SetActive(true);

            SolutionSquares[i] = newSolutionSquare;
        }

        _rectangle.Width = (_squareTemplateRectangle.Width + _squaresDistance) * squares + _squaresDistance/* * 2*/;
        _rectangle.Height = _squareTemplateRectangle.Height + _squaresDistance * 2;

        _solutionRectangle.Width = (_solutionSquareTemplateRectangle.Width + _solutionSquaresDistance) * squares + _solutionSquaresDistance/* * 2*/;
        _solutionRectangle.Height = _solutionSquareTemplateRectangle.Height + _solutionSquaresDistance * 2;

        _solutionSequence = new int[Random.Range(_minClicksForSolution, squares - _maxClicksBufferForSolution)];

        var shuffledIndices = indices.OrderBy(a => System.Guid.NewGuid()).ToArray();

        for (var i = 0; i < _solutionSequence.Length; i++)
		{
            _solutionSequence[i] = shuffledIndices[i];

            SolutionSquares[_solutionSequence[i]].ToggleTargets(SolutionSquares);
        }

        _solutionClicksText.text = $"{_solutionSequence.Length}";

        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
	{
        var levelComplete = true;

        for(var i = 0; i < Squares.Length; i++)
		{
            if(Squares[i].Toggled != SolutionSquares[i].Toggled)
			{
                levelComplete = false;
            }
		}

        LevelPanel.Instance.UpdateNextLevelButton(levelComplete);

        if(levelComplete)
		{
            OnLevelCompletion();
        }
	}

    private void OnLevelCompletion()
	{
        for (var i = 0; i < Squares.Length; i++)
        {
            var square = Squares[i];

            square.Interactable = false;

            if(square.Highlighted)
			{
                square.OnMouseOverExit();
            }
        }

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
