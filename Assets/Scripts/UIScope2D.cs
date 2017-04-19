using UnityEngine;
using System.Collections;

// [RequireComponent(typeof(Camera))]
public class UIScope2D : MonoBehaviour 
{
	public RectTransform scopePanel;
	public GameObject scopeOutline;
	public Camera scopeCamera;
	public Texture2D cursorTexture;
	public Color cursorColor;
	public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

	void Update ()
	{
		Vector3 mousePos = Input.mousePosition;
		scopePanel.transform.position = mousePos;
		scopeOutline.transform.position = mousePos;
		mousePos.z = -1.0f;
		scopeCamera.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
	}

	void Start ()
	{
		Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
	}
}