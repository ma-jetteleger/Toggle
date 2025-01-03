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
    [SerializeField] private SpriteRenderer _targetIndicator = null;
    [SerializeField] private GameObject _cascadingIndicator = null;
    [SerializeField] private GameObject _solutionCheck = null;
    [SerializeField] private GameObject _solutionCross = null;

    [SerializeField] private Color _clickedOutlineColor = Color.black;
    [SerializeField] private Sprite[] _targetSchemeSprites = null;
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
        set
        {
            _cascading = value;

            if (_cascadingIndicator != null)
            {
                _cascadingIndicator.SetActive(_cascading);

                if (_cascading)
                {
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
                _targetIndicator.sprite = _targetSchemeSprites[(int)_targetingScheme];
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
    private Vector3 _normalPosition;
    private Color _normalOverlayColor;
    private Dictionary<Square, List<ShapeRenderer>> _targetPredictions;
    private float _uninteractableOverlayAlpha;
    private Square _referenceSquare;
    private bool _cascading;
    private TargetingScheme _targetingScheme;
    private bool _coloredTargetPrediction;

    public void Initialize(
        int id,
        Level level)
    {
        _rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;

        Id = id;
        _level = level;

        gameObject.name = $"Square({Id})";

        _coloredTargetPrediction = _targetPredictionTemplate.GetComponent<Rectangle>() != null;

        _targetPredictionTemplate.SetActive(false);

        _outlineRectangle = _outline.GetComponent<Rectangle>();
        _normalOutlineColor = _outlineRectangle.Color;
        _normalPosition = transform.position;
        _normalOverlayColor = _noMoreClicksOverlay.Color;
        _uninteractableOverlayAlpha = _uninteractableOverlay.Color.a;

        _outline.SetActive(false);

		Cascading = false;

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
        Toggle(toggle);

        TargetScheme = targetingScheme;
        Cascading = cascading;

        _targetIndicator.sprite = _targetSchemeSprites[(int)TargetScheme];
    }

    public void Reinitialize()
    {
        Toggle(_referenceSquare.Toggled);
        Cascading = _referenceSquare.Cascading;
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

    public void Click()
    {
        foreach (var target in Targets)
        {
            target.Toggle();

            if (target.Cascading && target != this)
            {
                target.Cascading = false;
                target.Click();
            }
        }

        foreach (var square in _level.Squares)
        {
            square.SetupPredictions(false);
        }
    }

    public void Toggle()
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
	}

    public void Toggle(bool toggle)
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
				_referenceSquare.ChangeSortingOrderOfComponents(-10);

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
			ChangeSortingOrderOfComponents(-10);

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
            ChangeSortingOrderOfComponents(-10);

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
		var trueFactor = (factor * Id) + (factor * (_level.LastSquareClicked == this ? 10 : 1));

		foreach (var component in components)
        {
            component.SortingOrder += trueFactor;
        }

		if(_targetIndicator != null)
		{
			_targetIndicator.sortingOrder += trueFactor;
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