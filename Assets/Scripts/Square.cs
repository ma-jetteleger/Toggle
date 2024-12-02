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

    public bool Interactable 
    {
        get 
        { 
            return _interactable; 
        }

        set 
        { 
            _interactable = value;

            if(_uninteractableOverlay.gameObject.activeSelf == _interactable)
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

                if(_cascading)
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

			if(!SolutionSquare)
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
    private Tweener _colorChange;
    private Vector3 _normalPosition;
    private Color _normalOverlayColor;
	private Dictionary<Square, Rectangle> _targetPredictions;
    private float _uninteractableOverlayAlpha;
	private Square _referenceSquare;
	private bool _cascading;
	private TargetingScheme _targetingScheme;

    public void Initialize(
		int id, 
		Level level)
	{
		_rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;
        
        Id = id;
        _level = level;

		gameObject.name = $"Square({Id})";

		_targetPredictionTemplate.SetActive(false);

		_outlineRectangle = _outline.GetComponent<Rectangle>();
        _normalOutlineColor = _outlineRectangle.Color;
        _normalPosition = transform.position;
        _normalOverlayColor = _noMoreClicksOverlay.Color;
        _uninteractableOverlayAlpha = _uninteractableOverlay.Color.a;

        _outline.SetActive(false);

		Cascading = false;

		/*if(!targetingScheme.HasValue)
		{
            var first = Id == 0;
            var last = Id == _level.Squares.Length - 1;

            var possibleTargetSchemes = new List<TargetingScheme>();
            var targetSchemes = (TargetingScheme[])System.Enum.GetValues(typeof(TargetingScheme));

            for (var i = 0; i < targetSchemes.Length; i++)
			{
                var targetScheme = targetSchemes[i];

				switch (targetScheme)
				{
					case TargetingScheme.Self:
                        break;
					case TargetingScheme.Left:
                        if(!level.WrapAroundToggles && first)
						{
                            continue;
						}
						break;
					case TargetingScheme.Right:
                        if (!level.WrapAroundToggles && last)
                        {
                            continue;
                        }
                        break;
					case TargetingScheme.SelfLeft:
                        if ((!level.WrapAroundToggles && first)
                            || !level.SelfSideTarget)
                        {
                            continue;
                        }
                        break;
					case TargetingScheme.SelfRight:
                        if ((!level.WrapAroundToggles && last)
                            || !level.SelfSideTarget)
                        {
                            continue;
                        }
                        break;
					case TargetingScheme.LeftRight:
                        if ((!level.WrapAroundToggles && (first || last))
                            || !level.LeftRightTarget)
                        {
                            continue;
                        }
                        break;
					case TargetingScheme.SelfLeftRight:
                        if ((!level.WrapAroundToggles && (first || last))
                            || !level.SelfLeftRightTarget)
                        {
                            continue;
                        }
                        break;
				}

                possibleTargetSchemes.Add(targetScheme);
            }

            TargetScheme = possibleTargetSchemes[Random.Range(0, possibleTargetSchemes.Count)];
		}
		else
		{
            TargetScheme = targetingScheme.Value;
        }

        _targetIndicator.sprite = _targetSchemeSprites[(int)TargetScheme];

		if(!cascading.HasValue)
		{
			if (_level.CascadingToggles)
			{
				Cascading = Random.Range(0f, 1f) > 0.5f;
			}
			else
			{
				Cascading = false;
			}
		}
		else
		{
			Cascading = cascading.Value;
		}

		//_cascadingIndicator.SetActive(Cascading);

		if (!toggle.HasValue)
		{
            if (Random.Range(0f, 1f) > 0.5f)
            {
                Toggle();
            }
        }
		else
		{
            Toggle(toggle.Value);
        }*/

		Interactable = true;
	}

	public void Initialize(Level level, Square referenceSquare)
	{
		_referenceSquare = referenceSquare;

		Id = _referenceSquare.Id;
		_level = level;

		_rectangle = GetComponent<Rectangle>();
		_normalColor = _rectangle.Color;

		gameObject.name = $"{(SolutionSquare ? "Solution" : "")}Square({Id})";
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
        if(showOutline && !_outline.activeSelf)
		{
            _outline.SetActive(true);
        }
        
        Highlighted = true;

		ShowTargetPredictions();
	}

    public void OnMouseOverExit()
    {
        if(_outline.activeSelf)
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

    public void SetupTargetsAndPredictions(Square[] targetArray)
	{
		Targets = new List<Square>();

		switch (TargetScheme)
		{
			case TargetingScheme.Self:

				Targets.Add(this);

                break;

			case TargetingScheme.Left:

                if(Id > 0)
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

		if(!SolutionSquare)
		{
			_targetPredictions = new Dictionary<Square, Rectangle>();

			for (var i = 0; i < Targets.Count; i++)
			{
				var target = Targets[i];

				var newTargetPrediction = Instantiate(_targetPredictionTemplate, _targetPredictionTemplate.transform.parent).GetComponent<Rectangle>();
				newTargetPrediction.transform.position = new Vector3(target.transform.position.x, _targetPredictionTemplate.transform.position.y, 0f);

				newTargetPrediction.gameObject.SetActive(false);

				_targetPredictions.Add(target, newTargetPrediction);
			}
		}
	}

	/*public void TurnOffUnnecessaryCascading()
	{
		if(!Cascading)
		{
			return;
		}

		var turnOffCascading = true;

		foreach (var square in _level.Squares)
		{
			if(square == this)
			{
				continue;
			}

			if(square.Targets.Contains(this))
			{
				turnOffCascading = false;

				break;
			}
		}

		if(turnOffCascading)
		{
			//Debug.Log("It happened");

			Cascading = false;
		}
	}
    */
	public void Click()
	{
		foreach (var target in Targets)
		{
			target.Toggle();

			if(target.Cascading && target != this)
			{
				target.Cascading = false;
				target.Click();
			}
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
		foreach(var targetPredictions in _targetPredictions)
		{
			targetPredictions.Value.Color = targetPredictions.Key.Toggled
			? _normalColor
			: _toggledColor;

			if(!targetPredictions.Value.gameObject.activeSelf)
			{
				targetPredictions.Value.gameObject.SetActive(true);
			}
		}
	}

	public void HideTargetPredictions()
	{
		foreach (var targetPredictions in _targetPredictions)
		{
			targetPredictions.Value.Color = targetPredictions.Key.Toggled
			? _normalColor
			: _toggledColor;

			targetPredictions.Value.gameObject.SetActive(false);
		}
	}

	public void Shake()
    {
        ChangeSortingOrderOfComponents(10);

        if(_shake != null)
		{
            DOTween.Kill(_shake, true);

            transform.position = _normalPosition;
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

        if(_colorChange != null)
		{
            DOTween.Kill(_colorChange, true);

            _noMoreClicksOverlay.Color = _normalOverlayColor;
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
        var components = GetComponentsInChildren<Rectangle>(true);

        foreach(var component in components)
		{
            component.SortingOrder += factor;
        }

        _targetIndicator.sortingOrder += factor;
    }

	private void OnDestroy()
	{
		if(_targetPredictions != null)
		{
			foreach (var targetPrediction in _targetPredictions)
			{
				if (targetPrediction.Value != null)
				{
					Destroy(targetPrediction.Value.gameObject);
				}
			}
		}
	}
}