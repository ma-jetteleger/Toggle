using TMPro;
using UnityEngine;

public class DraggableSnapToLine : MonoBehaviour
{
    private Camera cam;
    private Vector3 offset;
    private bool isDragging = false;

    private SnapLineManager manager;
    private int snapIndex;
    private Vector3 targetPosition;
    private TextMeshPro text;

    [SerializeField] private float moveSpeed = 10f;

    public int Value { get; set; }

    public void Initialize(SnapLineManager mgr, int index)
    {
        cam = Camera.main;
        manager = mgr;
        snapIndex = index;
        targetPosition = manager.GetSnapPoint(index);
        transform.position = targetPosition;

        text = GetComponentInChildren<TextMeshPro>();
	}

    public void SetSnapIndex(int index)
    {
        snapIndex = index;
        targetPosition = manager.GetSnapPoint(index);
    }

    public void SetTargetPosition(Vector2 position)
    {
        targetPosition = new Vector3(position.x, position.y, transform.position.z);
    }

    void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();
        manager.OnBeginDrag(this);
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        Vector3 newPos = GetMouseWorldPosition() + offset;
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        manager.UpdateDraggablePositions();
    }

    void OnMouseUp()
    {
        isDragging = false;
        manager.OnEndDrag(this);
    }

    void Update()
    {
        if (!isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = -cam.transform.position.z;
        return cam.ScreenToWorldPoint(screenPos);
    }

	public void SetValue(int value)
	{
        Value = value;

		text.text = Value.ToString();
	}
}
