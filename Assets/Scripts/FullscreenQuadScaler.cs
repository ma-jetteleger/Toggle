using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FullscreenQuadScaler : MonoBehaviour
{
	void Start()
	{
		Camera cam = Camera.main;
		float height = 2f * cam.orthographicSize;
		float width = height * cam.aspect;

		transform.localScale = new Vector3(width, height, 1f);
	}
}
