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

    public bool Toggled { get; set; }
    public bool Highlighted { get; set; }
    public bool SolutionSquare { get; set; }
    public TargetingScheme TargetScheme { get; set; }
	public List<Square> Targets { get; set; }
	public int Id { get; set; }

    private bool _interactable;
	private Level _level;
    private Color _normalColor;
    private Color _normalOutlineColor;
    private Rectangle _rectangle;
    private Rectangle _outlineRectangle;
    private SpriteRenderer _targetIndicator;
    private Tweener _shake;
    private Tweener _colorChange;
    private Vector3 _normalPosition;
    private Color _normalOverlayColor;
	private Dictionary<Square, Rectangle> _targetPredictions;
    private float _uninteractableOverlayAlpha;


    public void Initialize(int id, Level level, Square referenceSquare = null)
	{
		SolutionSquare = referenceSquare != null;

		_rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;
        
        Id = id;
        _level = level;

		gameObject.name = $"{(SolutionSquare ? "Solution" : "")}Square({Id})";

		if (!SolutionSquare)
		{
			_targetPredictionTemplate.SetActive(false);

			_outlineRectangle = _outline.GetComponent<Rectangle>();
            _normalOutlineColor = _outlineRectangle.Color;
            _normalPosition = transform.position;
            _normalOverlayColor = _noMoreClicksOverlay.Color;
            _uninteractableOverlayAlpha = _uninteractableOverlay.Color.a;

            _outline.SetActive(false);

			var first = Id == 0;
			var last = Id == _level.Squares.Length - 1;

            var possibleTargetSchemes = new List<TargetingScheme>();
            var targetSchemes = (TargetingScheme[])System.Enum.GetValues(typeof(TargetingScheme));

            if (level.WrapAroundToggles)
			{
                possibleTargetSchemes = targetSchemes.ToList();
            }
			else
			{
                for (var i = 0; i < targetSchemes.Length; i++)
                {
                    var targetScheme = targetSchemes[i];

                    if ((first && targetScheme == TargetingScheme.Left) ||
                        (first && targetScheme == TargetingScheme.LeftRight) ||
                        (first && targetScheme == TargetingScheme.SelfLeft) ||
                        (first && targetScheme == TargetingScheme.SelfLeftRight) ||
                        (last && targetScheme == TargetingScheme.Right) ||
                        (last && targetScheme == TargetingScheme.LeftRight) ||
                        (last && targetScheme == TargetingScheme.SelfRight) ||
                        (last && targetScheme == TargetingScheme.SelfLeftRight))
                    {
                        continue;
                    }

                    possibleTargetSchemes.Add(targetSchemes[i]);
                }
            }

			TargetScheme = possibleTargetSchemes[Random.Range(0, possibleTargetSchemes.Count)];

            _targetIndicator = GetComponentInChildren<SpriteRenderer>(true);
            _targetIndicator.sprite = _targetSchemeSprites[(int)TargetScheme];

            if (Random.Range(0f, 1f) > 0.5f)
            {
                Toggle();
            }

            Interactable = true;
        }
		else
		{
            TargetScheme = referenceSquare.TargetScheme;

            Toggle(referenceSquare.Toggled);
        }
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

				var newTargetPrediction = Instantiate(_targetPredictionTemplate, transform).GetComponent<Rectangle>();
				newTargetPrediction.transform.position = new Vector3(target.transform.position.x, _targetPredictionTemplate.transform.position.y, 0f);

				newTargetPrediction.gameObject.SetActive(false);

				_targetPredictions.Add(target, newTargetPrediction);
			}
		}
	}

	public void ToggleTargets()
	{
		foreach (var target in Targets)
		{
			target.Toggle();
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
}