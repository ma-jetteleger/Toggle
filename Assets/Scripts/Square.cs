using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public enum TargetScheme
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
    [SerializeField] private Color _clickedOutlineColor = Color.black;
    [SerializeField] private Sprite[] _targetSchemeSprites = null;
    [SerializeField] private Color _toggledColor = Color.black;

    public bool Highlighted { get; set; }
    public bool Toggled { get; set; }

    private int _id;
    private Level _level;
    private TargetScheme _targetScheme;
    private Color _normalColor;
    private Color _normalOutlineColor;
    private Rectangle _rectangle;
    private Rectangle _outlineRectangle;
    private SpriteRenderer _targetIndicator;

	public void Initialize(int id, Level level)
	{
        _rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;

        _outlineRectangle = _outline.GetComponent<Rectangle>();
        _normalOutlineColor = _outlineRectangle.Color;

        _targetIndicator = GetComponentInChildren<SpriteRenderer>(true);

        _id = id;
        _level = level;

        _outline.SetActive(false);

        gameObject.name = $"Square({_id})";

        _targetScheme = (TargetScheme)Random.Range(0, System.Enum.GetValues(typeof(TargetScheme)).Length);
        _targetIndicator.sprite = _targetSchemeSprites[(int)_targetScheme];
    }

    public void OnMouseOverEnter()
    {
        _outline.SetActive(true);

        Highlighted = true;
    }

    public void OnMouseOverExit()
    {
        _outline.SetActive(false);

        Highlighted = false;
    }

    public void OnMouseClickDown()
	{
        _outlineRectangle.Color = _clickedOutlineColor;
    }

    public void OnMouseClickUp()
    {
        _outlineRectangle.Color = _normalOutlineColor;
    }

    public void ToggleTargets()
	{
        var targets = new List<Square>();

		switch (_targetScheme)
		{
			case TargetScheme.Self:
                targets.Add(this);
                break;
			case TargetScheme.Left:
                if(_id > 0) targets.Add(_level.Squares[_id - 1]);
                break;
			case TargetScheme.Right:
                if (_id < _level.Squares.Length - 1) targets.Add(_level.Squares[_id + 1]);
                break;
			case TargetScheme.SelfLeft:
                targets.Add(this);
                if (_id > 0) targets.Add(_level.Squares[_id - 1]);
                break;
			case TargetScheme.SelfRight:
                targets.Add(this);
                if (_id < _level.Squares.Length - 1) targets.Add(_level.Squares[_id + 1]);
                break;
			case TargetScheme.LeftRight:
                if (_id > 0) targets.Add(_level.Squares[_id - 1]);
                if (_id < _level.Squares.Length - 1) targets.Add(_level.Squares[_id + 1]);
                break;
			case TargetScheme.SelfLeftRight:
                targets.Add(this);
                if (_id > 0) targets.Add(_level.Squares[_id - 1]);
                if (_id < _level.Squares.Length - 1) targets.Add(_level.Squares[_id + 1]);
                break;
		}

        foreach(var target in targets)
		{
            target.Toggle();
        }
	}

	public void Toggle()
	{
        _rectangle.Color = Toggled 
            ? _normalColor 
            : _toggledColor;

		Toggled = !Toggled;
    }
}