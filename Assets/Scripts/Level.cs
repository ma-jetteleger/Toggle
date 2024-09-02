using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private GameObject _squareTemplate = null;
    [SerializeField] private Vector2Int _squaresRange = Vector2Int.zero;
    [SerializeField] private float _squaresDistance = 0f;

    public Square[] Squares { get; set; }

    private Square _previousHoveredSquare;
    private Rectangle _squareTemplateRectangle;
    private Square _lastSquareClickedDown;

	private void Awake()
	{
        _squareTemplateRectangle = _squareTemplate.GetComponent<Rectangle>();

        _squareTemplate.SetActive(false);
    }

	private void Start()
	{
		var squares = Random.Range(_squaresRange.x, _squaresRange.y + 1);

        Squares = new Square[squares];

        for (var i = 0; i < squares; i++)
		{
			var newSquare = Instantiate(_squareTemplate, _squareTemplate.transform.parent).GetComponent<Square>();
            newSquare.transform.position = Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * i;

            newSquare.Initialize(i, this);

            newSquare.gameObject.SetActive(true);

            Squares[i] = newSquare;
        }

        transform.position = -(Vector3.right * (_squareTemplateRectangle.Width + _squaresDistance) * (squares - 1)) / 2f;
    }

    private void Update()
    {
        if(_lastSquareClickedDown == null)
		{
            var squareHovered = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero).collider?.GetComponent<Square>();

            if (squareHovered != null && squareHovered != _previousHoveredSquare)
            {
                if (!squareHovered.Highlighted)
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
            var squareHovered = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero).collider?.GetComponent<Square>();

            var sameSquare = squareHovered != null && squareHovered;

            _lastSquareClickedDown.OnMouseClickUp();

            if(sameSquare)
			{
                _lastSquareClickedDown.ToggleTargets();
            }

            _lastSquareClickedDown = null;
        }
    }
}
