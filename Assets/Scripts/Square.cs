using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class Square : MonoBehaviour
{
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

    [SerializeField] private GameObject _outline = null;
    [SerializeField] private Rectangle _noMoreClicksOverlay = null;
    [SerializeField] private Rectangle _uninteractableOverlay = null;
    [SerializeField] private GameObject _targetPredictionTemplate = null;
    [SerializeField] private GameObject[] _targetIndicators = null;
    [SerializeField] private Disc _cascadingIndicator = null;
    [SerializeField] private GameObject _solutionCheck = null;
    [SerializeField] private GameObject _solutionCross = null;

    [SerializeField] private Color _clickedOutlineColor = Color.black;
    //[SerializeField] private Sprite[] _targetSchemeSprites = null;
    [SerializeField] private Color _toggledColor = Color.black;

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
                    }
                }
            }
        }
    }

    public bool SolutionSquare => _referenceSquare != null;
    public Square PreviousSquare => _level.Squares[Id > 0 ? Id - 1 : _level.Squares.Length - 1];
    public Square NextSquare => _level.Squares[Id < _level.Squares.Length - 1 ? Id + 1 : 0];

    public bool Toggled { get; set; }
    public bool Highlighted { get; set; }

    public List<Square> Targets { get; set; }
    public int Id { get; set; }

    private bool _interactable;
    private Level _level;
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

    public void Initialize(
        int id,
        Level level)
    {
        _rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;

        Id = id;
        _level = level;

        gameObject.name = $"Square({Id})";

        if(_targetPredictionTemplate != null)
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

        Interactable = true;
    }

    public void Initialize(Level level, Square referenceSquare)
    {
        _referenceSquare = referenceSquare;

        Id = _referenceSquare.Id;
        _level = level;

        _rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;

        gameObject.name = $"SolutionSquare({Id})";

		_solutionCheck.transform.SetParent(transform.parent);
		_solutionCheck.name = $"Check({Id})";

		_solutionCross.transform.SetParent(transform.parent);
		_solutionCross.name = $"Cross({Id})";
	}

    public void Overwrite(bool toggle, TargetingScheme targetingScheme, bool cascading)
    {
        ToggleTo(toggle);

        TargetScheme = targetingScheme;
        SetCascading(cascading, false);
    }

    public void Reinitialize()
    {
        ToggleTo(_referenceSquare.Toggled);
        SetCascading(_referenceSquare.Cascading, false);
    }

    public void OnMouseOverEnter(bool showOutline)
    {
        if (showOutline && !_outline.activeSelf)
        {
            _outline.SetActive(true);
        }

        Highlighted = true;

        ShowTargetPredictions();
    }

    public void OnMouseOverExit()
    {
        if (_outline.activeSelf)
        {
            _outline.SetActive(false);
        }

        Highlighted = false;

        HideTargetPredictions();
    }

    public void OnMouseClickDown()
    {
        _outlineRectangle.Color = _clickedOutlineColor;
    }

    public void OnMouseClickUp()
    {
        _outlineRectangle.Color = _normalOutlineColor;
    }

    public void SetupTargets(Square[] targetArray)
    {
        Targets = new List<Square>();

        switch (TargetScheme)
        {
            case TargetingScheme.Self:

                Targets.Add(this);

                break;

            case TargetingScheme.Left:

                if (Id > 0)
                {
                    Targets.Add(targetArray[Id - 1]);
                }
                else
                {
                    Targets.Add(targetArray[targetArray.Length - 1]);
                }

                break;

            case TargetingScheme.Right:

                if (Id < targetArray.Length - 1)
                {
                    Targets.Add(targetArray[Id + 1]);
                }
                else
                {
                    Targets.Add(targetArray[0]);
                }

                break;

            case TargetingScheme.SelfLeft:

                Targets.Add(this);

                if (Id > 0)
                {
                    Targets.Add(targetArray[Id - 1]);
                }
                else
                {
                    Targets.Add(targetArray[targetArray.Length - 1]);
                }

                break;

            case TargetingScheme.SelfRight:

                Targets.Add(this);

                if (Id < targetArray.Length - 1)
                {
                    Targets.Add(targetArray[Id + 1]);
                }
                else
                {
                    Targets.Add(targetArray[0]);
                }

                break;

            case TargetingScheme.LeftRight:

                if (Id > 0)
                {
                    Targets.Add(targetArray[Id - 1]);
                }
                else
                {
                    Targets.Add(targetArray[targetArray.Length - 1]);
                }

                if (Id < targetArray.Length - 1)
                {
                    Targets.Add(targetArray[Id + 1]);
                }
                else
                {
                    Targets.Add(targetArray[0]);
                }

                break;

            case TargetingScheme.SelfLeftRight:

                Targets.Add(this);

                if (Id > 0)
                {
                    Targets.Add(targetArray[Id - 1]);
                }
                else
                {
                    Targets.Add(targetArray[targetArray.Length - 1]);
                }

                if (Id < targetArray.Length - 1)
                {
                    Targets.Add(targetArray[Id + 1]);
                }
                else
                {
                    Targets.Add(targetArray[0]);
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

        if(!originalSetup && _coloredTargetPrediction)
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

		var cascadingFlags = new bool[_level.Squares.Length];

		for (var i = 0; i < _level.Squares.Length; i++)
		{
			cascadingFlags[i] = _level.Squares[i].Cascading;
		}

		InstantiatePredictionIndicator(cascadingFlags, Targets, true);
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

    public void Click(bool fromPlayer, bool extraDelay, bool turnOffCascading = false)
    {
        _level.SquaresToggledLastClick.Clear();

        if (!SolutionSquare)
		{
            var coroutine = DelayedClick(fromPlayer, extraDelay, turnOffCascading);

            StartCoroutine(coroutine);
        }
		else
		{
            InstantClick(fromPlayer);
        }
    }

    private IEnumerator DelayedClick(bool fromPlayer, bool extraDelay, bool turnOffCascading = false)
    {
        var animationTime = _level.SolutionSquares[0]._checkAndCrossPunchTime / 4f;

        /*if(extraDelay)
		{
            animationTime *= 2f;
        }*/

        if(fromPlayer && extraDelay && Cascading)
		{
            AnimateCascadingIndicator();

            yield return new WaitForSeconds(animationTime * 1.75f);
        }

        if (turnOffCascading)
        {
            SetCascading(false, true);
        }

        AnimateTargetIndicator(animationTime);

        yield return new WaitForSeconds(animationTime);

        InstantClick(fromPlayer);
    }

    private void InstantClick(bool fromPlayer)
	{
        var endClickSequence = true;

        foreach (var target in Targets)
        {
            target.Toggle(fromPlayer);

            _level.TogglesThisClickSequence++;

            if (target.Cascading && target != this)
            {
                if(endClickSequence)
				{
                    endClickSequence = false;
                }

                if (!fromPlayer)
                {
                    SetCascading(false, false);
                } 

                target.Click(fromPlayer, true, true);
            }
        }

        foreach (var square in _level.Squares)
        {
            square.SetupPredictions(false);
        }

        _level.OnSquareClicked();

        if (endClickSequence)
		{
            _level.EndClickSequence(fromPlayer);
        }
    }

    public void SetCascading(bool value, bool fromPlayer)
	{
        _cascading = value;

        if (_cascadingIndicator != null)
        {
            if(!fromPlayer)
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
        var time = _level.SolutionSquares[0]._checkAndCrossPunchTime / 4f;

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
        Toggled = !Toggled;

        _rectangle.Color = Toggled
            ? _toggledColor
            : _normalColor;

        if (!Interactable && !SolutionSquare)
        {
            MatchUninteractableOverlayColorWithRectangle();
        }

		if (!_level.SquaresToggledLastClick.Contains(this))
		{
			_level.SquaresToggledLastClick.Add(this);
		}

        var correct = Toggled == _level.SolutionSquares[Id].Toggled;

        _level.SolutionSquares[Id].UpdateCheckAndCross(correct, fromPlayer);
    }

    public void ToggleTo(bool toggle)
    {
        Toggled = toggle;

        _rectangle.Color = Toggled
            ? _toggledColor
            : _normalColor;

        if (!Interactable && !SolutionSquare)
        {
            MatchUninteractableOverlayColorWithRectangle();
        }
    }

    public void ShowTargetPredictions()
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
                    targetPrediction.Color = targetPredictionEntry.Key.Toggled
                        ? _normalColor
                        : _toggledColor;
                }

                if (!targetPrediction.gameObject.activeSelf)
                {
                    targetPrediction.gameObject.SetActive(true);
                }
            }
        }
    }

    public void HideTargetPredictions()
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
                    targetPrediction.Color = targetPredictionEntry.Key.Toggled
                        ? _normalColor
                        : _toggledColor;
                }

                if (targetPrediction.gameObject.activeSelf)
                {
                    targetPrediction.gameObject.SetActive(false);
                }
            }
        }
    }

    private void AnimateTargetIndicator(float time)
	{
        for (var i = 0; i < _targetIndicatorSprites.Length; i++)
        {
            var targetIndicatorSprite = _targetIndicatorSprites[i];
            var originalPosition = targetIndicatorSprite.transform.localPosition;

            if (targetIndicatorSprite.gameObject.name.Contains("Left"))
			{
                if(_leftArrowMove != null)
				{
                    _leftArrowMove.Kill(false);

                    targetIndicatorSprite.transform.localPosition = originalPosition;

                    _leftArrowMove = null;
                }

                _leftArrowMove = MoveTargetIndicatorSprite(targetIndicatorSprite, originalPosition, Vector3.left, time);
            }
            else if(targetIndicatorSprite.gameObject.name.Contains("Right"))
			{
                if (_rightArrowMove != null)
                {
                    _rightArrowMove.Kill(false);

                    targetIndicatorSprite.transform.localPosition = originalPosition;

                    _rightArrowMove = null;
                }

                _rightArrowMove = MoveTargetIndicatorSprite(targetIndicatorSprite, originalPosition, Vector3.right, time);
            }
            else if(targetIndicatorSprite.gameObject.name.Contains("Diamond"))
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
		_solutionCheck.SetActive(correct);
		_solutionCross.SetActive(!correct);

		if(!animate)
		{
			return;
		}

		if(_referenceSquare != null)
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

		if (_punch != null)
		{
			_punch.Kill(true);
		}

		ChangeSortingOrderOfComponents(10);

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
		});

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
		var trueFactor = (factor * Id) + (factor * (_level.SquaresToggledLastClick.Contains(this) ? 10 + (_level.TogglesThisClickSequence * 5) : 1));

		foreach (var component in components)
        {
            component.SortingOrder += trueFactor;
        }

		if(_targetIndicatorSprites != null)
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

		if(_solutionCheck != null)
		{
			Destroy(_solutionCheck);
		}

		if (_solutionCross != null)
		{
			Destroy(_solutionCross);
		}
	}
}