using Shapes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaletteManager : MonoBehaviour
{
	[System.Serializable]
	public class ColorPalette
	{
		public Color[] Colors;
	}
    
    [SerializeField] ColorPalette[] _colorPalettes;
    [SerializeField] int _activePaletteIndex;
    [SerializeField] Square _squareTemplate;
    [SerializeField] Square _solutionSquareTemplate;

	private void Start()
	{
		/*for (var i = 0; i < _colorPalettes[0].Colors.Length; i++)
		{
			if (_squareTemplate.ToggledColor == _colorPalettes[0].Colors[i])
			{
				_squareTemplate.ToggledColor = _colorPalettes[_activePaletteIndex].Colors[i];
			}

			if (_squareTemplate.ToggledTargetIndicatorColor == _colorPalettes[0].Colors[i])
			{
				_squareTemplate.ToggledTargetIndicatorColor = _colorPalettes[_activePaletteIndex].Colors[i];
			}

			if (_squareTemplate.ClickedOutlineColor == _colorPalettes[0].Colors[i])
			{
				_squareTemplate.ClickedOutlineColor = _colorPalettes[_activePaletteIndex].Colors[i];
			}

			if (_solutionSquareTemplate.ToggledColor == _colorPalettes[0].Colors[i])
			{
				_solutionSquareTemplate.ToggledColor = _colorPalettes[_activePaletteIndex].Colors[i];
			}

			if (_solutionSquareTemplate.ClickedOutlineColor == _colorPalettes[0].Colors[i])
			{
				_solutionSquareTemplate.ClickedOutlineColor = _colorPalettes[_activePaletteIndex].Colors[i];
			}

			if (AreColorsEqual(Camera.main.backgroundColor, _colorPalettes[0].Colors[i]))
			{
				Camera.main.backgroundColor = _colorPalettes[_activePaletteIndex].Colors[i];
			}
		}

		var images = Resources.FindObjectsOfTypeAll(typeof(Image)) as Image[];
		var texts = Resources.FindObjectsOfTypeAll(typeof(TextMeshProUGUI)) as TextMeshProUGUI[];
		var buttons = Resources.FindObjectsOfTypeAll(typeof(Button)) as Button[];
		var sprites = Resources.FindObjectsOfTypeAll(typeof(SpriteRenderer)) as SpriteRenderer[];
		var rectangles = Resources.FindObjectsOfTypeAll(typeof(Rectangle)) as Rectangle[];
		var discs = Resources.FindObjectsOfTypeAll(typeof(Disc)) as Disc[];
		

		foreach(var image in images)
		{
			for (var i = 0; i < _colorPalettes[0].Colors.Length; i++)
			{
				if (image.color == _colorPalettes[0].Colors[i])
				{
					image.color = _colorPalettes[_activePaletteIndex].Colors[i];

					break;
				}
			}
		}

		foreach (var text in texts)
		{
			for (var i = 0; i < _colorPalettes[0].Colors.Length; i++)
			{
				if (text.color == _colorPalettes[0].Colors[i])
				{
					text.color = _colorPalettes[_activePaletteIndex].Colors[i];

					break;
				}
			}
		}

		foreach (var button in buttons)
		{
			var newColorBlock = new ColorBlock()
			{
				normalColor = button.colors.normalColor,
				highlightedColor = button.colors.highlightedColor,
				pressedColor = button.colors.pressedColor,
				selectedColor = button.colors.selectedColor,
				disabledColor = button.colors.disabledColor,
				colorMultiplier = 1f,
				fadeDuration = 0f
			};

			var change = false;

			for (var i = 0; i < _colorPalettes[0].Colors.Length; i++)
			{
				if (newColorBlock.normalColor == _colorPalettes[0].Colors[i])
				{
					newColorBlock.normalColor = _colorPalettes[_activePaletteIndex].Colors[i];

					if(!change) change = true;
				}

				if (newColorBlock.highlightedColor == _colorPalettes[0].Colors[i])
				{
					newColorBlock.highlightedColor = _colorPalettes[_activePaletteIndex].Colors[i];

					if (!change) change = true;
				}

				if (newColorBlock.pressedColor == _colorPalettes[0].Colors[i])
				{
					newColorBlock.pressedColor = _colorPalettes[_activePaletteIndex].Colors[i];

					if (!change) change = true;
				}

				if (newColorBlock.selectedColor == _colorPalettes[0].Colors[i])
				{
					newColorBlock.selectedColor = _colorPalettes[_activePaletteIndex].Colors[i];

					if (!change) change = true;
				}

				if (newColorBlock.disabledColor == _colorPalettes[0].Colors[i])
				{
					newColorBlock.disabledColor = _colorPalettes[_activePaletteIndex].Colors[i];

					if (!change) change = true;
				}
			}

			if(change)
			{
				button.colors = newColorBlock;
			}
		}

		foreach (var sprite in sprites)
		{
			for (var i = 0; i < _colorPalettes[0].Colors.Length; i++)
			{
				if (sprite.color == _colorPalettes[0].Colors[i])
				{
					sprite.color = _colorPalettes[_activePaletteIndex].Colors[i];

					break;
				}
			}
		}

		foreach (var rectangle in rectangles)
		{
			for (var i = 0; i < _colorPalettes[0].Colors.Length; i++)
			{
				if (rectangle.Color == _colorPalettes[0].Colors[i])
				{
					rectangle.Color = _colorPalettes[_activePaletteIndex].Colors[i];

					break;
				}
			}
		}

		foreach (var disc in discs)
		{
			for (var i = 0; i < _colorPalettes[0].Colors.Length; i++)
			{
				if (disc.Color == _colorPalettes[0].Colors[i])
				{
					disc.Color = _colorPalettes[_activePaletteIndex].Colors[i];

					break;
				}
			}
		}*/
	}

	private bool AreColorsEqual(Color color1, Color color2)
	{
		return Mathf.Abs(color1.r - color2.r) < 0.01f
			&& Mathf.Abs(color1.g - color2.g) < 0.01f
			&& Mathf.Abs(color1.b - color2.b) < 0.01f;
	}
}
