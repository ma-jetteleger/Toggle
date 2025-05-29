using UnityEngine;
using System.Collections.Generic;
using Shapes;
using System.Linq;

public class SnapLineManager : MonoBehaviour
{
	[Header("Snap Line Settings")]
	public float spacing = 1f;
	public Vector2 direction = Vector2.right;
	public GameObject draggablePrefab;

	[Header("Rearrangement Thresholds")]
	[SerializeField] private float verticalThreshold = 0.5f;
	[SerializeField] private float horizontalThreshold = 1f;

	[HideInInspector] public List<Vector2> snapPoints = new();
	private List<DraggableSnapToLine> draggables = new();

	private DraggableSnapToLine currentlyDragging = null;
	private Level _level = null;

	public void Initialize(Level level)
	{
		_level = level;

		GenerateSnapPoints();
		InstantiateDraggables();
	}

	void GenerateSnapPoints()
	{
		snapPoints.Clear();
		Vector2 start = transform.position;

		for (int i = 0; i < _level.Squares.Length; i++)
		{
			var position = _level.Squares[i].transform.position;
			position.y = transform.position.y;

			snapPoints.Add(position);
		}
	}

	void InstantiateDraggables()
	{
		foreach(var draggable in draggables)
		{
			Destroy(draggable.gameObject);
		}

		draggables.Clear();

		var indices = new List<int>();

		for (var i = 0; i < _level.Squares.Length; i++)
		{
			indices.Add(i);
		}

		var shuffledIndices = indices.OrderBy(a => System.Guid.NewGuid()).ToArray();
		//var shuffledSnapPoints = snapPoints.OrderBy(a => System.Guid.NewGuid()).ToArray();

		for (int i = 0; i < shuffledIndices.Length; i++)
		{
			var shuffledIndex = shuffledIndices[i];

			GameObject obj = Instantiate(draggablePrefab, snapPoints[shuffledIndex], Quaternion.identity);
			DraggableSnapToLine draggable = obj.GetComponent<DraggableSnapToLine>();

			if (draggable == null)
			{
				Debug.LogError("Prefab missing DraggableSnapToLine component.");
				return;
			}

			draggable.Initialize(this, shuffledIndex);
			draggables.Add(draggable);

			if(i >= _level.ClicksLeft)
			{
				Destroy(draggable.GetComponent<Disc>());
				Destroy(draggable.GetComponent<CircleCollider2D>());

				foreach(Transform child in draggable.transform)
				{
					Destroy(child.gameObject);
				}
			}
			else
			{
				draggable.SetValue(i + 1);
			}
		}
	}

	public void OnBeginDrag(DraggableSnapToLine dragged)
	{
		currentlyDragging = dragged;
	}

	public void OnEndDrag(DraggableSnapToLine dragged)
	{
		int newIndex = GetClosestSnapIndex(dragged.transform.position);
		RearrangeDraggables(dragged, newIndex);
		currentlyDragging = null;
	}

	void RearrangeDraggables(DraggableSnapToLine dragged, int newIndex)
	{
		draggables.Remove(dragged);
		draggables.Insert(newIndex, dragged);

		for (int i = 0; i < draggables.Count; i++)
			draggables[i].SetSnapIndex(i);
	}

	public void UpdateDraggablePositions()
	{
		if (currentlyDragging == null) return;

		Vector3 dragPos = currentlyDragging.transform.position;
		float dragY = dragPos.y;
		float dragX = dragPos.x;
		float snapY = snapPoints[0].y;

		float yDist = Mathf.Abs(dragY - snapY);
		if (yDist > verticalThreshold) return;

		int insertIndex = GetClosestSnapIndex(dragPos);
		if (Mathf.Abs(dragX - snapPoints[insertIndex].x) > horizontalThreshold) return;

		// Rearranged positions, leaving room for the dragged object
		List<Vector2> newPositions = new();
		for (int i = 0, j = 0; i < snapPoints.Count; i++)
		{
			if (i == insertIndex)
			{
				newPositions.Add(Vector2.positiveInfinity); // Placeholder
			}
			else
			{
				while (draggables[j] == currentlyDragging) j++;
				newPositions.Add(snapPoints[i]);
				draggables[j++].SetTargetPosition(snapPoints[i]);
			}
		}
	}

	int GetClosestSnapIndex(Vector2 position)
	{
		float minDist = float.MaxValue;
		int closest = -1;
		for (int i = 0; i < snapPoints.Count; i++)
		{
			float dist = Vector2.Distance(position, snapPoints[i]);
			if (dist < minDist)
			{
				minDist = dist;
				closest = i;
			}
		}
		return closest;
	}

	public Vector2 GetSnapPoint(int index) => snapPoints[index];
}
