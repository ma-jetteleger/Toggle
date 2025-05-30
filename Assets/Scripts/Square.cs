using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using NaughtyAttributes;

public class Square : MonoBehaviour
{
    public class ToggleProperties
    {
		public bool Cascading;
		public TargetingScheme TargetingScheme;
		public int DistanceToggleFactor;
	}

    public enum PossibleToggleState
    {
        Zero,
        One,
        Two//,
        //Three
    }

    public enum TargetingScheme
    {
        Self,
        Left,
        Right,
        SelfLeft,
        SelfRight,
        LeftRight,
        SelfLeftRight
    }


    // Components
	[SerializeField] private GameObject _outline = null;
    [SerializeField] private Rectangle _noMoreClicksOverlay = null;
    [SerializeField] private Rectangle _uninteractableOverlay = null;
    [SerializeField] private GameObject _targetPredictionTemplate = null;
    [SerializeField] private GameObject _conditionalSection = null;
    [SerializeField] private GameObject[] _targetIndicators = null;
    [SerializeField] private GameObject[] _conditionalTargetIndicators = null;
    [SerializeField] private Disc _cascadingIndicator = null;
	[SerializeField] private Disc _conditionalCascadingIndicator = null;
	[SerializeField] private GameObject _solutionCheck = null;
    [SerializeField] private GameObject _solutionCross = null;
    [SerializeField] private GameObject[] _distanceToggleIndicators = null;
    [SerializeField] private GameObject[] _conditionalDistanceToggleIndicators = null;

	[HorizontalLine(1)]

	// Animation/visual parameters
	[SerializeField] private Color _clickedOutlineColor = Color.black;
    //[SerializeField] private Sprite[] _targetSchemeSprites = null;
    [SerializeField] private Color _toggled1Color = Color.black;
    [SerializeField] private Color _toggled2Color = Color.black;
    //[SerializeField] private Color _toggled3Color = Color.black;
    //[SerializeField] private Color _toggledTargetIndicatorColor = Color.black;

    [SerializeField] private Gradient _shakeGradient = null;
    [SerializeField] private float _shakeTime = 0f;
    [SerializeField] private float _shakeStrength = 0f;
    [SerializeField] private int _shakeVibrato = 0;
    [SerializeField] private float _shakeRandomness = 0f;
    [SerializeField] private bool _shakeSnapping = false;
    [SerializeField] private bool _shakeFadeOut = false;

    [SerializeField] private Vector3 _checkAndCrossPunchScale = Vector3.zero;
    [SerializeField] private float _checkAndCrossPunchTime = 0f;
    [SerializeField] private int _checkAndCrossPunchVibrato = 0;
    [SerializeField] private float _checkAndCrossPunchElasticity = 0f;

    [SerializeField] private float _targetIndicatorMoveDistance = 0f;
    [SerializeField] private AnimationCurve _targetIndicatorMoveCurve = null;

    public Color ClickedOutlineColor { get { return _clickedOutlineColor; } set { _clickedOutlineColor = value; } }
    public Color Toggled1Color { get { return _toggled1Color; } set { _toggled1Color = value; } }
    public Color Toggled2Color { get { return _toggled2Color; } set { _toggled2Color = value; } }
    //public Color Toggled3Color { get { return _toggled3Color; } set { _toggled3Color = value; } }
    //public Color ToggledTargetIndicatorColor { get { return _toggledTargetIndicatorColor; } set { _toggledTargetIndicatorColor = value; } }

    public bool Interactable
    {
        get
        {
            return _interactable;
        }

        set
        {
            _interactable = value;

            if (_uninteractableOverlay.gameObject.activeSelf == _interactable)
            {
                MatchUninteractableOverlayColorWithRectangle();

                _uninteractableOverlay.gameObject.SetActive(!_interactable);
            }
        }
    }

    public bool Cascading
    {
        get
        {
            return _cascading;
        }
    }

    public TargetingScheme TargetScheme
    {
        get
        {
            return _targetingScheme;
        }
        set
        {
            _targetingScheme = value;

            if (!SolutionSquare)
            {
                for (var i = 0; i < _targetIndicators.Length; i++)
                {
                    var activate = i == (int)_targetingScheme;

                    _targetIndicators[i].SetActive(activate);

                    if (activate)
                    {
                        _targetIndicatorSprites = _targetIndicators[i].GetComponentsInChildren<SpriteRenderer>();
                        _targetIndicatorSpritesOriginalPosition = new Vector3[_targetIndicatorSprites.Length];

                        for (int j = 0; j < _targetIndicatorSprites.Length; j++)
                        {
                            var targetIndicatorSprite = _targetIndicatorSprites[j];

                            _targetIndicatorSpritesOriginalPosition[j] = targetIndicatorSprite.transform.localPosition;
                        }
                    }
                }
            }
        }
    }

    public bool SolutionSquare => _referenceSquare != null;
    public Square PreviousSquare => Level.Squares[Id > 0 ? Id - 1 : Level.Squares.Length - 1];
    public Square NextSquare => Level.Squares[Id < Level.Squares.Length - 1 ? Id + 1 : 0];
    public bool Animating => _punch != null || _referenceSquarePunch != null || _leftArrowMove != null || _rightArrowMove != null || _diamondMove != null || _cascadingIndicatorMove != null;
    public bool Conditional => ConditionalToggleProperties != null;

    public Level Level { get; set; }
    public PossibleToggleState ToggledState { get; set; }
    public bool Highlighted { get; set; }
	public Dictionary<PossibleToggleState, ToggleProperties> ConditionalToggleProperties { get; set; }
	public int DistanceToggleFactor { get; set; }

	public List<Square> Targets { get; set; }
    public int Id { get; set; }

    private bool _interactable;
    private Color _normalColor;
    private Color _normalOutlineColor;
    private Rectangle _rectangle;
    private Rectangle _outlineRectangle;
    private Tweener _shake;
    private Tweener _punch;
    private Tweener _checkOrCrossPunch;
    private Tweener _referenceSquarePunch;
    private Tweener _colorChange;
    private Tweener _leftArrowMove;
    private Tweener _rightArrowMove;
    private Tweener _diamondMove;
    private Tweener _cascadingIndicatorMove;
    private Vector3 _normalPosition;
    private Color _normalOverlayColor;
    private Dictionary<Square, List<ShapeRenderer>> _targetPredictions;
    private float _uninteractableOverlayAlpha;
    private Square _referenceSquare;
    private bool _cascading;
    private TargetingScheme _targetingScheme;
	private bool _coloredTargetPrediction;
	private int _lastSortingOrderChangeFactor;
	private SpriteRenderer[] _targetIndicatorSprites;
	private Vector3[] _targetIndicatorSpritesOriginalPosition;
	private Color _normalTargetIndicatorColor;

    public void Initialize(
        int id,
        Level level)
    {
        _rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;

        Id = id;
        Level = level;

        gameObject.name = $"Square({Id})";

        if (_targetPredictionTemplate != null)
        {
            _coloredTargetPrediction = _targetPredictionTemplate.GetComponent<Rectangle>() != null;

            _targetPredictionTemplate.SetActive(false);
        }

        _outlineRectangle = _outline.GetComponent<Rectangle>();
        _normalOutlineColor = _outlineRectangle.Color;
        _normalPosition = transform.position;
        _normalOverlayColor = _noMoreClicksOverlay.Color;
        _uninteractableOverlayAlpha = _uninteractableOverlay.Color.a;

        _outline.SetActive(false);

        SetCascading(false, false);

        Interactable = Level.UnclickableToggledSquares ? ToggledState == PossibleToggleState.Zero : true;

        _normalTargetIndicatorColor = _targetIndicators[0].GetComponentInChildren<SpriteRenderer>().color;

        if (Level.ConditionalToggles && _conditionalSection != null)
        {
            if (Random.Range(0f, 1f) > 0.5f)
            {
                _conditionalSection.SetActive(false);
            }

            if (_conditionalSection.activeSelf)
            {
                ConditionalToggleProperties = new Dictionary<PossibleToggleState, ToggleProperties>();
            }
        }

        SetupDistanceToggles();
	}

    public void Initialize(Level level, Square referenceSquare)
    {
        _referenceSquare = referenceSquare;

        Id = _referenceSquare.Id;
        Level = level;

        _rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;

        gameObject.name = $"SolutionSquare({Id})";

        if (_solutionCheck != null)
        {
            _solutionCheck.transform.SetParent(transform.parent);
            _solutionCheck.name = $"Check({Id})";
        }

        if (_solutionCross != null)
        {
            _solutionCross.transform.SetParent(transform.parent);
            _solutionCross.name = $"Cross({Id})";
        }

        DistanceToggleFactor = referenceSquare.DistanceToggleFactor;
	}

    public void Overwrite(PossibleToggleState toggledState, TargetingScheme targetingScheme, bool cascading)
    {
        ToggleTo(toggledState);

        TargetScheme = targetingScheme;
        SetCascading(cascading, false);
    }

    public void Reinitialize()
    {
        ToggleTo(_referenceSquare.ToggledState);
        SetCascading(_referenceSquare.Cascading, false);
    }

    public void OnMouseOverEnter(bool showOutline)
    {
        if (showOutline && !_outline.activeSelf)
        {
            _outline.SetActive(true);
        }

        Highlighted = true;

        //ShowTargetPredictions();
    }

    public void OnMouseOverExit()
    {
        if (_outline.activeSelf)
        {
            _outline.SetActive(false);
        }

        Highlighted = false;

        //HideTargetPredictions();
    }

    public void OnMouseClickDown()
    {
        _outlineRectangle.Color = _clickedOutlineColor;
    }

    public void OnMouseClickUp()
    {
        _outlineRectangle.Color = _normalOutlineColor;
    }

    public void SetupDistanceToggles()
    {
		if (Level.DistanceToggles && _distanceToggleIndicators != null)
		{
			if (Random.Range(0f, 1f) > 0.5f)
			{
				if (Level.Squares.Length > 7)
				{
					DistanceToggleFactor = Random.Range(1, 4);
				}
				else if (Level.Squares.Length > 5)
				{
					DistanceToggleFactor = Random.Range(1, 3);
				}
				else if (Level.Squares.Length > 3)
				{
					DistanceToggleFactor = 1;
				}
			}

			if (DistanceToggleFactor == 1 && !SolutionSquare)
			{
				_distanceToggleIndicators[0].SetActive(true);
				_distanceToggleIndicators[1].SetActive(false);
				_distanceToggleIndicators[2].SetActive(false);
			}
			else if (DistanceToggleFactor == 2 && !SolutionSquare)
			{
				_distanceToggleIndicators[1].SetActive(true);
				_distanceToggleIndicators[0].SetActive(false);
				_distanceToggleIndicators[2].SetActive(false);
			}
			else if (DistanceToggleFactor == 3 && !SolutionSquare)
			{
				_distanceToggleIndicators[2].SetActive(true);
				_distanceToggleIndicators[0].SetActive(false);
				_distanceToggleIndicators[1].SetActive(false);
			}
			else if (!SolutionSquare)
			{
				_distanceToggleIndicators[0].SetActive(false);
				_distanceToggleIndicators[1].SetActive(false);
				_distanceToggleIndicators[2].SetActive(false);
			}
		}
	}

    public void SetupTargets(Square[] targetArray)
    {
        Targets = new List<Square>();

        var targetId = 0;

        switch (TargetScheme)
        {
            case TargetingScheme.Self:

                Targets.Add(this);

                break;

            case TargetingScheme.Left:

                targetId = Id - (1 + DistanceToggleFactor);

                if(targetId < 0)
                {
                    targetId = targetArray.Length + targetId;
				}

                Targets.Add(targetArray[targetId]);

				break;

            case TargetingScheme.Right:

				targetId = Id + (1 + DistanceToggleFactor);

				if (targetId > targetArray.Length - 1)
				{
					targetId = targetId - targetArray.Length;
				}

				Targets.Add(targetArray[targetId]);

                break;

            case TargetingScheme.SelfLeft:

                Targets.Add(this);

				targetId = Id - (1 + DistanceToggleFactor);

				if (targetId < 0)
				{
					targetId = targetArray.Length + targetId;
				}

				if (!Targets.Contains(targetArray[targetId]))
				{
					Targets.Add(targetArray[targetId]);
				}

				break;

            case TargetingScheme.SelfRight:

                Targets.Add(this);

				targetId = Id + (1 + DistanceToggleFactor);

				if (targetId > targetArray.Length - 1)
				{
					targetId = targetId - targetArray.Length;
				}

				if (!Targets.Contains(targetArray[targetId]))
				{
					Targets.Add(targetArray[targetId]);
				}

				break;

            case TargetingScheme.LeftRight:

				targetId = Id - (1 + DistanceToggleFactor);

				if (targetId < 0)
				{
					targetId = targetArray.Length + targetId;
				}

				Targets.Add(targetArray[targetId]);

				targetId = Id + (1 + DistanceToggleFactor);

				if (targetId > targetArray.Length - 1)
				{
					targetId = targetId - targetArray.Length;
				}

                if (!Targets.Contains(targetArray[targetId]))
                {
					Targets.Add(targetArray[targetId]);
				}
				
				break;

            case TargetingScheme.SelfLeftRight:

                Targets.Add(this);

				targetId = Id - (1 + DistanceToggleFactor);

				if (targetId < 0)
				{
					targetId = targetArray.Length + targetId;
				}

				if (!Targets.Contains(targetArray[targetId]))
				{
					Targets.Add(targetArray[targetId]);
				}

				targetId = Id + (1 + DistanceToggleFactor);

				if (targetId > targetArray.Length - 1)
				{
					targetId = targetId - targetArray.Length;
				}

				if (!Targets.Contains(targetArray[targetId]))
				{
					Targets.Add(targetArray[targetId]);
				}

				break;
        }
    }

    public void SetupPredictions(bool originalSetup)
    {
        if (_targetPredictionTemplate == null)
        {
            return;
        }

        if (SolutionSquare)
        {
            return;
        }

        if (!originalSetup && _coloredTargetPrediction)
        {
            return;
        }

        if (_targetPredictions != null)
        {
            foreach (var targetPredictionEntry in _targetPredictions)
            {
                if (targetPredictionEntry.Value != null)
                {
                    foreach (var targetPrediction in targetPredictionEntry.Value)
                    {
                        if (targetPrediction != null)
                        {
                            Destroy(targetPrediction.gameObject);
                        }
                    }
                }
            }
        }

        _targetPredictions = new Dictionary<Square, List<ShapeRenderer>>();

        var cascadingFlags = new bool[Level.Squares.Length];

        for (var i = 0; i < Level.Squares.Length; i++)
        {
            cascadingFlags[i] = Level.Squares[i].Cascading;
        }

        InstantiatePredictionIndicator(cascadingFlags, Targets, true);
    }

    public void SetupConditionalPreview()
    {
        foreach(var conditionalToggleProperty in ConditionalToggleProperties)
        {
            if(conditionalToggleProperty.Key == ToggledState)
            {

            }
        }

		if (DistanceToggleFactor == 1 && !SolutionSquare)
		{
			_distanceToggleIndicators[0].SetActive(true);
			_distanceToggleIndicators[1].SetActive(false);
			_distanceToggleIndicators[2].SetActive(false);
		}
		else if (DistanceToggleFactor == 2 && !SolutionSquare)
		{
			_distanceToggleIndicators[1].SetActive(true);
			_distanceToggleIndicators[0].SetActive(false);
			_distanceToggleIndicators[2].SetActive(false);
		}
		else if (DistanceToggleFactor == 3 && !SolutionSquare)
		{
			_distanceToggleIndicators[2].SetActive(true);
			_distanceToggleIndicators[0].SetActive(false);
			_distanceToggleIndicators[1].SetActive(false);
		}
		else if (!SolutionSquare)
		{
			_distanceToggleIndicators[0].SetActive(false);
			_distanceToggleIndicators[1].SetActive(false);
			_distanceToggleIndicators[2].SetActive(false);
		}
	}

    private void InstantiatePredictionIndicator(bool[] cascadingFlags, List<Square> targets, bool originalClick)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];

            var newTargetPrediction = Instantiate(_targetPredictionTemplate, _targetPredictionTemplate.transform.parent).GetComponent<ShapeRenderer>();

            newTargetPrediction.gameObject.SetActive(false);

            if (_targetPredictions.ContainsKey(target))
            {
                _targetPredictions[target].Add(newTargetPrediction);
            }
            else
            {
                var newTargetPredictionList = new List<ShapeRenderer>();
                newTargetPredictionList.Add(newTargetPrediction);

                _targetPredictions.Add(target, newTargetPredictionList);
            }

            var yOffset = (_targetPredictionTemplate.transform.position.y < 0f ? -1f : 1f)
                * 0.3f
                * (_targetPredictions[target].Count - 1);

            newTargetPrediction.transform.position = new Vector3(target.transform.position.x, _targetPredictionTemplate.transform.position.y + yOffset, 0f);

            if (!_coloredTargetPrediction)
            {
                var targetTargets = new List<Square>(target.Targets);

                if (originalClick && target == this && targetTargets.Contains(this))
                {
                    continue;

                    //targetTargets.Remove(this);
                }

                if (cascadingFlags[target.Id])
                {
                    cascadingFlags[target.Id] = false;

                    InstantiatePredictionIndicator(cascadingFlags, targetTargets, false);
                }
            }
        }
    }

    public void Click(bool fromPlayer, bool extraDelay/*, bool turnOffCascading*/)
    {
        Level.SquaresToggledLastClick.Clear();

        if (!SolutionSquare)
        {
            var coroutine = DelayedClick(fromPlayer, extraDelay/*, turnOffCascading*/);

            StartCoroutine(coroutine);
        }
        else
        {
            InstantClick(fromPlayer);
        }
    }

    private IEnumerator DelayedClick(bool fromPlayer, bool extraDelay/*, bool turnOffCascading*/)
    {
        var animationTime = Level.SolutionSquares[0]._checkAndCrossPunchTime / 4f;

        /*if(extraDelay)
		{
            animationTime *= 2f;
        }*/

        if (fromPlayer && extraDelay && Cascading)
        {
            AnimateCascadingIndicator();

            yield return new WaitForSeconds(animationTime * 1.875f);

            SetCascading(false, fromPlayer);
        }

        AnimateTargetIndicator(animationTime);

        yield return new WaitForSeconds(animationTime);

        var endClickSequence = InstantClick(fromPlayer);

        Level.LevelPanel.UpdateClicksCounter(true);

        if (fromPlayer && endClickSequence)
        {
            yield return new WaitForSeconds(animationTime * 1.25f);

            if (!Level.Squares.Any(x => x.Animating))
            {
                Level.CanClick = true;
            }
        }
    }

    private bool InstantClick(bool fromPlayer)
    {
        var endClickSequence = true;

        foreach (var target in Targets)
        {
            target.Toggle(fromPlayer);

            Level.TogglesThisClickSequence++;

            if (target.Cascading && target != this)
            {
                if (endClickSequence)
                {
                    endClickSequence = false;
                }

                if (!fromPlayer)
                {
                    target.SetCascading(false, false);
                }

                target.Click(fromPlayer, true/*, true*/);
            }
        }

        foreach (var square in Level.Squares)
        {
            square.SetupPredictions(false);
        }

        if (fromPlayer)
        {
            Level.OnSquareClicked();
        }

        if (endClickSequence)
        {
            Level.EndClickSequence(fromPlayer);
        }

        return endClickSequence;
    }

    public void SetCascading(bool value, bool fromPlayer)
    {
        _cascading = value;

        if (_cascadingIndicator != null)
        {
            if (!fromPlayer)
            {
                _cascadingIndicator.gameObject.SetActive(_cascading);

                if (_cascading)
                {
                    _cascadingIndicator.Color = Color.black;

                    var position = _cascadingIndicator.transform.localPosition;

                    switch (_targetingScheme)
                    {
                        case TargetingScheme.Self:
                            position.y = 0.3125f;
                            position.x = 0f;
                            break;
                        case TargetingScheme.Left:
                            position.y = 0.1875f;
                            position.x = 0.1875f;
                            break;
                        case TargetingScheme.Right:
                            position.y = 0.1875f;
                            position.x = -0.1875f;
                            break;
                        case TargetingScheme.SelfLeft:
                            position.y = 0.375f;
                            position.x = 0.1875f;
                            break;
                        case TargetingScheme.SelfRight:
                            position.y = 0.375f;
                            position.x = -0.1875f;
                            break;
                        case TargetingScheme.LeftRight:
                            position.y = 0.1875f;
                            position.x = 0f;
                            break;
                        case TargetingScheme.SelfLeftRight:
                            position.y = 0.375f;
                            position.x = 0f;
                            break;
                    }

                    _cascadingIndicator.transform.localPosition = position;
                }
            }
            else
            {
                //_cascadingIndicator.gameObject.SetActive(_cascading);
            }
        }
    }

    public void SetupDistanceToggleFactor()
    {

    }

    public void SetTargetScheme()
    {

    }

    public void AnimateCascadingIndicator()
    {
        var originalPosition = _cascadingIndicator.transform.localPosition;

        if (_cascadingIndicatorMove != null)
        {
            _cascadingIndicatorMove.Kill(false);

            _cascadingIndicatorMove = null;

            _cascadingIndicator.transform.localPosition = originalPosition;

            _cascadingIndicator.transform.localScale = Vector3.one;
        }

        var movement = Vector3.up;
        var time = Level.SolutionSquares[0]._checkAndCrossPunchTime / 4f;

        _cascadingIndicatorMove = _cascadingIndicator.transform.DOLocalMove(originalPosition + (movement * _targetIndicatorMoveDistance / 2f), time).SetEase(_targetIndicatorMoveCurve)
        .OnComplete(() =>
        {
            _cascadingIndicator.transform.DOLocalMove(originalPosition, time).SetEase(_targetIndicatorMoveCurve).OnComplete(() =>
            {
                _cascadingIndicator.transform.localPosition = originalPosition;

            }).OnComplete(() =>
            {
                _cascadingIndicator.transform.DOScale(Vector3.zero, time / 3f).SetEase(_targetIndicatorMoveCurve).OnComplete(() =>
                {
                    _cascadingIndicatorMove = null;

                    _cascadingIndicator.gameObject.SetActive(false);

                    _cascadingIndicator.transform.localScale = Vector3.one;
                });
            });
        });
    }

    public void Toggle(bool fromPlayer = false)
    {
        ToggledState = Level.BinaryStateSquares
            ? (PossibleToggleState)(((int)ToggledState) + 2)
            : (PossibleToggleState)(((int)ToggledState) + 1);

        if ((int)ToggledState >= System.Enum.GetNames(typeof(Square.PossibleToggleState)).Length)
        {
            ToggledState = PossibleToggleState.Zero;
        }

        switch (ToggledState)
        {
            case PossibleToggleState.Zero:
                _rectangle.Color = _normalColor;
                break;
            case PossibleToggleState.One:
                _rectangle.Color = Toggled1Color;
                break;
            case PossibleToggleState.Two:
                _rectangle.Color = Toggled2Color;
                break;
            /*case PossibleToggleState.Three:
                _rectangle.Color = Toggled3Color;
                break;*/
        }

        /*if(_targetIndicatorSprites != null)
		{
            foreach (var targetIndicatorSprite in _targetIndicatorSprites)
            {
                targetIndicatorSprite.color = Toggled ? _toggledTargetIndicatorColor : _normalTargetIndicatorColor;
            }
        }*/

        if (Level.UnclickableToggledSquares && !SolutionSquare)
        {
            Interactable = ToggledState == PossibleToggleState.Zero;
        }

        if (!Interactable && !SolutionSquare)
        {
            MatchUninteractableOverlayColorWithRectangle();
        }

        if (fromPlayer)
        {
            if (!Level.SquaresToggledLastClick.Contains(this))
            {
                Level.SquaresToggledLastClick.Add(this);
            }

            var correct = ToggledState == Level.SolutionSquares[Id].ToggledState;

            Level.SolutionSquares[Id].UpdateCheckAndCross(correct, fromPlayer);
        }
    }

    public void ToggleTo(PossibleToggleState toggledState)
    {
        ToggledState = toggledState;

        switch (toggledState)
        {
            case PossibleToggleState.Zero:
                _rectangle.Color = _normalColor;
                break;
            case PossibleToggleState.One:
                _rectangle.Color = Toggled1Color;
                break;
            case PossibleToggleState.Two:
                _rectangle.Color = Toggled2Color;
                break;
            /*case PossibleToggleState.Three:
                _rectangle.Color = Toggled3Color;
                break;*/
        }

        if (Level.UnclickableToggledSquares && !SolutionSquare)
        {
            Interactable = ToggledState == PossibleToggleState.Zero;
        }

        if (!Interactable && !SolutionSquare)
        {
            MatchUninteractableOverlayColorWithRectangle();
        }
    }

    /*public void ShowTargetPredictions()
    {
        if (_targetPredictionTemplate == null)
        {
            return;
        }

        foreach (var targetPredictionEntry in _targetPredictions)
        {
            foreach (var targetPrediction in targetPredictionEntry.Value)
            {
                if (_coloredTargetPrediction)
                {
                    targetPrediction.Color = targetPredictionEntry.Key.ToggledState
                        ? _normalColor
                        : _toggledColor;
                }

                if (!targetPrediction.gameObject.activeSelf)
                {
                    targetPrediction.gameObject.SetActive(true);
                }
            }
        }
    }*/

    /*public void HideTargetPredictions()
    {
        if (_targetPredictionTemplate == null)
		{
            return;
		}

        foreach (var targetPredictionEntry in _targetPredictions)
        {
            foreach (var targetPrediction in targetPredictionEntry.Value)
            {
                if (_coloredTargetPrediction)
                {
                    targetPrediction.Color = targetPredictionEntry.Key.ToggledState
                        ? _normalColor
                        : _toggledColor;
                }

                if (targetPrediction.gameObject.activeSelf)
                {
                    targetPrediction.gameObject.SetActive(false);
                }
            }
        }
    }*/

    private void AnimateTargetIndicator(float time)
    {
        for (var i = 0; i < _targetIndicatorSprites.Length; i++)
        {
            var targetIndicatorSprite = _targetIndicatorSprites[i];
            var originalPosition = _targetIndicatorSpritesOriginalPosition[i];

            if (targetIndicatorSprite.gameObject.name.Contains("Left"))
            {
                if (_leftArrowMove != null)
                {
                    _leftArrowMove.Kill(false);

                    targetIndicatorSprite.transform.localPosition = originalPosition;

                    _leftArrowMove = null;
                }

                _leftArrowMove = MoveTargetIndicatorSprite(targetIndicatorSprite, originalPosition, Vector3.left, time);
            }
            else if (targetIndicatorSprite.gameObject.name.Contains("Right"))
            {
                if (_rightArrowMove != null)
                {
                    _rightArrowMove.Kill(false);

                    targetIndicatorSprite.transform.localPosition = originalPosition;

                    _rightArrowMove = null;
                }

                _rightArrowMove = MoveTargetIndicatorSprite(targetIndicatorSprite, originalPosition, Vector3.right, time);
            }
            else if (targetIndicatorSprite.gameObject.name.Contains("Diamond"))
            {
                if (_diamondMove != null)
                {
                    _diamondMove.Kill(false);

                    targetIndicatorSprite.transform.localPosition = originalPosition;

                    _diamondMove = null;
                }

                _diamondMove = MoveTargetIndicatorSprite(targetIndicatorSprite, originalPosition, Vector3.down, time);
            }
        }
    }

    private Tweener MoveTargetIndicatorSprite(SpriteRenderer targetIndicatorSprite, Vector3 originalPosition, Vector3 movement, float time)
    {
        var tweener = targetIndicatorSprite.transform.DOLocalMove(originalPosition + (movement * _targetIndicatorMoveDistance), time).SetEase(_targetIndicatorMoveCurve)
        .OnComplete(() =>
        {
            targetIndicatorSprite.transform.DOLocalMove(originalPosition, time).SetEase(_targetIndicatorMoveCurve).OnComplete(() =>
            {
                if (targetIndicatorSprite.gameObject.name.Contains("Left"))
                {
                    _leftArrowMove = null;
                }
                else if (targetIndicatorSprite.gameObject.name.Contains("Right"))
                {
                    _rightArrowMove = null;
                }
                else if (targetIndicatorSprite.gameObject.name.Contains("Diamond"))
                {
                    _diamondMove = null;
                }

                targetIndicatorSprite.transform.localPosition = originalPosition;
            });
        });

        return tweener;
    }

    public void UpdateCheckAndCross(bool correct, bool animate)
    {
        if (_solutionCheck != null && _solutionCross != null)
        {
            _solutionCheck.SetActive(correct);
            _solutionCross.SetActive(!correct);
        }

        if (!animate)
        {
            return;
        }

        if (_punch != null)
        {
            _punch.Kill(true);
        }

        /*ChangeSortingOrderOfComponents(10);

        _punch = transform.DOPunchScale(
            _checkAndCrossPunchScale / 2f,
            _checkAndCrossPunchTime,
            _checkAndCrossPunchVibrato,
            _checkAndCrossPunchElasticity
        ).OnComplete(() =>
        {
            RestoreSortingOrderOfComponents();

            transform.localScale = Vector3.one;

            _punch = null;
        });*/

        if (_referenceSquare != null)
        {
            if (_referenceSquarePunch != null)
            {
                _referenceSquarePunch.Kill(true);
            }

            _referenceSquare.ChangeSortingOrderOfComponents(10);

            _referenceSquarePunch = _referenceSquare.transform.DOPunchScale(
                _checkAndCrossPunchScale / 2f,
                _checkAndCrossPunchTime,
                _checkAndCrossPunchVibrato,
                _checkAndCrossPunchElasticity
            ).OnComplete(() =>
            {
                _referenceSquare.RestoreSortingOrderOfComponents();

                _referenceSquare.transform.localScale = Vector3.one;

                _referenceSquarePunch = null;
            });
        }

        if (_solutionCheck != null && _solutionCross != null)
        {
            if (_checkOrCrossPunch != null)
            {
                _checkOrCrossPunch.Kill(true);
            }

            if (correct)
            {
                _checkOrCrossPunch = _solutionCheck.transform.DOPunchScale(
                    _checkAndCrossPunchScale,
                    _checkAndCrossPunchTime,
                    _checkAndCrossPunchVibrato,
                    _checkAndCrossPunchElasticity
                ).OnComplete(() =>
                {
                    _solutionCheck.transform.localScale = Vector3.one;

                    _checkOrCrossPunch = null;
                });
            }
            else
            {
                _checkOrCrossPunch = _solutionCross.transform.DOPunchScale(
                    _checkAndCrossPunchScale,
                    _checkAndCrossPunchTime,
                    _checkAndCrossPunchVibrato,
                    _checkAndCrossPunchElasticity
                ).OnComplete(() =>
                {
                    _solutionCross.transform.localScale = Vector3.one;

                    _checkOrCrossPunch = null;
                });
            }
        }
    }

    public void Shake()
    {
        ChangeSortingOrderOfComponents(10);

        if (_shake != null)
        {
            _shake.Kill(true);
        }

        if (_punch != null)
        {
            _punch.Kill(true);
        }

        _shake = transform.DOShakePosition(
            _shakeTime,
            _shakeStrength,
            _shakeVibrato,
            _shakeRandomness,
            _shakeSnapping,
            _shakeFadeOut
        ).OnComplete(() =>
        {
            RestoreSortingOrderOfComponents();

            transform.position = _normalPosition;

            _shake = null;
        });

        var value = 0f;

        if (_colorChange != null)
        {
            _colorChange.Kill(true);
        }

        _colorChange = DOTween.To(() => value, x =>
        {
            value = x;

            _noMoreClicksOverlay.Color = _shakeGradient.Evaluate(value);
        },
        1f,
        _shakeTime).OnComplete(() =>
        {
            _colorChange = null;

            _noMoreClicksOverlay.Color = _normalOverlayColor;
        });
    }

    private void MatchUninteractableOverlayColorWithRectangle()
    {
        var color = _rectangle.Color;
        color.a = _uninteractableOverlayAlpha;

        _uninteractableOverlay.Color = color;
    }

    private void ChangeSortingOrderOfComponents(int factor)
    {
        var components = GetComponentsInChildren<ShapeRenderer>(true);
        var wasToggledLastClick = Level.SquaresToggledLastClick.Contains(this) || (_referenceSquare != null && Level.SquaresToggledLastClick.Contains(_referenceSquare));

        var trueFactor = (factor * Id) + (factor * (wasToggledLastClick ? 10 + (Level.TogglesThisClickSequence * 5) : 1));

        foreach (var component in components)
        {
            component.SortingOrder += trueFactor;
        }

        if (_targetIndicatorSprites != null)
        {
            for (var i = 0; i < _targetIndicatorSprites.Length; i++)
            {
                _targetIndicatorSprites[i].sortingOrder += trueFactor;
            }
        }

        _lastSortingOrderChangeFactor = -trueFactor;

    }

    private void RestoreSortingOrderOfComponents()
    {
        var components = GetComponentsInChildren<ShapeRenderer>(true);

        foreach (var component in components)
        {
            component.SortingOrder += _lastSortingOrderChangeFactor;
        }

        if (_targetIndicatorSprites != null)
        {
            for (var i = 0; i < _targetIndicatorSprites.Length; i++)
            {
                _targetIndicatorSprites[i].sortingOrder += _lastSortingOrderChangeFactor;
            }
        }
    }

    private void OnDestroy()
    {
        if (_targetPredictions != null)
        {
            foreach (var targetPredictionEntry in _targetPredictions)
            {
                if (targetPredictionEntry.Value != null)
                {
                    foreach (var targetPrediction in targetPredictionEntry.Value)
                    {
                        if (targetPrediction != null)
                        {
                            Destroy(targetPrediction.gameObject);
                        }
                    }
                }
            }
        }

        if (_solutionCheck != null)
        {
            Destroy(_solutionCheck);
        }

        if (_solutionCross != null)
        {
            Destroy(_solutionCross);
        }
    }
}