using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSquare
{
	public bool OriginalState;
	public bool Toggled;
	public int[] TargetIds;
	public bool Cascading;
	public bool OriginalCascading;

	public TestSquare(Square referenceSquare)
	{
		OriginalState = referenceSquare.Toggled;
		Toggled = OriginalState;

		Cascading = referenceSquare.Cascading;
		OriginalCascading = Cascading;
	}

	public void Reset()
	{
		Toggled = OriginalState;
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
			target.Toggled = !target.Toggled;

			if(target.Cascading && target != this)
			{
				target.Cascading = false;
				target.Click(testSquares);
			}
		}
	}
}
