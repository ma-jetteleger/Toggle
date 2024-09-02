using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    [SerializeField] private GameObject _outline = null;
    [SerializeField] private Color _clickedColor = Color.black;

    public bool Highlighted { get; set; }

    private int _id;
    private Color _normalColor;
    private Rectangle _rectangle;

	private void Awake()
	{
        _rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;
    }

	public void Initialize(int id)
	{
        _id = id;

        _outline.SetActive(false);

        gameObject.name = $"Square({_id})";
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
        _rectangle.Color = _clickedColor;
    }

    public void OnMouseClickUp()
    {
        _rectangle.Color = _normalColor;
    }
}