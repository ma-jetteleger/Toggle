using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSquare
{
	public Square.PossibleToggleState OriginalToggledState;
	public Square.PossibleToggleState ToggledState;
	public int[] TargetIds;
	public bool Cascading;
	public bool OriginalCascading;

	private Square _referenceSquare;

	public TestSquare(Square referenceSquare)
	{
		_referenceSquare = referenceSquare;

		OriginalToggledState = _referenceSquare.ToggledState;
		ToggledState = OriginalToggledState;

		Cascading = _referenceSquare.Cascading;
		OriginalCascading = Cascading;
	}

	public void Reset()
	{
		ToggledState = OriginalToggledState;
		Cascading = OriginalCascading;
	}

	public void SetupTargets(Square referenceSquare)
	{
		TargetIds = new int[referenceSquare.Targets.Count];

		for (var i = 0; i < referenceSquare.Targets.Count; i++)
		{
			TargetIds[i] = referenceSquare.Targets[i].Id;
		}
	}

	public void Click(TestSquare[] testSquares)
	{
		foreach (var targetId in TargetIds)
		{
			var target = testSquares[targetId];

			target.ToggledState = _referenceSquare.Level.BinaryStateSquares
				? (Square.PossibleToggleState)(((int)target.ToggledState) + 2)
				: (Square.PossibleToggleState)(((int)target.ToggledState) + 1);

			if ((int)target.ToggledState >= System.Enum.GetNames(typeof(Square.PossibleToggleState)).Length)
			{
				target.ToggledState = Square.PossibleToggleState.Zero;
			}

			if (target.Cascading && target != this)
			{
				target.Cascading = false;
				target.Click(testSquares);
			}
		}
	}
}
