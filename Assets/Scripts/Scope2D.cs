using UnityEngine;
using System.Collections;

// [RequireComponent(typeof(Camera))]
public class Scope2D : MonoBehaviour 
{
	private Camera scopeCamera;
	private GameObject magnifyBorders;
	private LineRenderer LeftBorder, RightBorder, TopBorder, BottomBorder; // Reference for lines of magnify glass borders
	private float ScopeOriginX,ScopeOriginY; // Scope Origin X and Y position
	private float ScopeWidth = Screen.width/5f,ScopeHeight = Screen.width/5f; // Scope width and height
	private Vector3 mousePos;

	void Start () 
	{
		createScope2D();
	}
	
	void Update () 
	{
		// Following lines set the camera's pixelRect and camera position at mouse position
		scopeCamera.pixelRect = new Rect (Input.mousePosition.x - ScopeWidth / 2.0f, Input.mousePosition.y - ScopeHeight / 2.0f, ScopeWidth, ScopeHeight);
		mousePos = getWorldPosition (Input.mousePosition);
		scopeCamera.transform.position = mousePos;
		mousePos.z = 0;
		magnifyBorders.transform.position = mousePos;
	}

	// Following method creates Scope2D
	private void createScope2D()
	{
		ScopeOriginX = Screen.width / 2f - ScopeWidth/2f; 
		ScopeOriginY = Screen.height / 2f - ScopeHeight/2f; 
		scopeCamera = GetComponent<Camera>();
		scopeCamera.pixelRect = new Rect(ScopeOriginX, ScopeOriginY, ScopeWidth, ScopeHeight);
		scopeCamera.transform.position = new Vector3(0,0,0);
		if(Camera.main.orthographic)
		{
			scopeCamera.orthographic = true;
			scopeCamera.orthographicSize = Camera.main.orthographicSize / 5.0f;//+ 1.0f;
			createBordersForScope ();
		}
		else
		{
			scopeCamera.orthographic = false;
			scopeCamera.fieldOfView = Camera.main.fieldOfView / 10.0f;//3.0f;
		}

	}

	// Following method sets border of Scope2D
	private void createBordersForScope()
	{
		magnifyBorders = new GameObject ();
		LeftBorder = getLine();
		LeftBorder.SetVertexCount(2);
		LeftBorder.SetPosition(0,new Vector3(getWorldPosition(new Vector3(ScopeOriginX,ScopeOriginY,0)).x,getWorldPosition(new Vector3(ScopeOriginX,ScopeOriginY,0)).y-0.1f,-1));
		LeftBorder.SetPosition(1,new Vector3(getWorldPosition(new Vector3(ScopeOriginX,ScopeOriginY+ScopeHeight,0)).x,getWorldPosition(new Vector3(ScopeOriginX,ScopeOriginY+ScopeHeight,0)).y+0.1f,-1));
		LeftBorder.transform.parent = magnifyBorders.transform;
		
		TopBorder = getLine();
		TopBorder.SetVertexCount(2);
		TopBorder.SetPosition(0,new Vector3(getWorldPosition(new Vector3(ScopeOriginX,ScopeOriginY+ScopeHeight,0)).x,getWorldPosition(new Vector3(ScopeOriginX,ScopeOriginY+ScopeHeight,0)).y,-1));
		TopBorder.SetPosition(1,new Vector3(getWorldPosition(new Vector3(ScopeOriginX+ScopeWidth,ScopeOriginY+ScopeHeight,0)).x,getWorldPosition(new Vector3(ScopeOriginX+ScopeWidth,ScopeOriginY+ScopeHeight,0)).y,-1));
		TopBorder.transform.parent = magnifyBorders.transform;
		
		RightBorder = getLine();
		RightBorder.SetVertexCount(2);
		RightBorder.SetPosition(0,new Vector3(getWorldPosition(new Vector3(ScopeOriginX+ScopeWidth,ScopeOriginY+ScopeWidth,0)).x,getWorldPosition(new Vector3(ScopeOriginX+ScopeWidth,ScopeOriginY+ScopeWidth,0)).y+0.1f,-1));
		RightBorder.SetPosition(1,new Vector3(getWorldPosition(new Vector3(ScopeOriginX+ScopeWidth,ScopeOriginY,0)).x,getWorldPosition(new Vector3(ScopeOriginX+ScopeWidth,ScopeOriginY,0)).y-0.1f,-1));
		RightBorder.transform.parent = magnifyBorders.transform;
		
		BottomBorder = getLine();
		BottomBorder.SetVertexCount(2);
		BottomBorder.SetPosition(0,new Vector3(getWorldPosition(new Vector3(ScopeOriginX+ScopeWidth,ScopeOriginY,0)).x,getWorldPosition(new Vector3(ScopeOriginX+ScopeWidth,ScopeOriginY,0)).y,-1));
		BottomBorder.SetPosition(1,new Vector3(getWorldPosition(new Vector3(ScopeOriginX,ScopeOriginY,0)).x,getWorldPosition(new Vector3(ScopeOriginX,ScopeOriginY,0)).y,-1));
		BottomBorder.transform.parent = magnifyBorders.transform;
	}

	// Following method creates new line for Scope2D's border
	private LineRenderer getLine()
	{
		LineRenderer line = new GameObject("Line").AddComponent<LineRenderer>();
		line.material =  new Material(Shader.Find("Diffuse"));
		line.SetVertexCount(2);
		line.SetWidth(0.2f,0.2f);
		// line.widthMultiplier(0.2f);
		line.SetColors(Color.black, Color.black);
		line.useWorldSpace = false; 
		return line;
	}
	private void setLine(LineRenderer line)
	{
		line.material =  new Material(Shader.Find("Diffuse"));
		line.SetVertexCount(2);
		line.SetWidth(0.2f,0.2f);
		line.SetColors(Color.black, Color.black);
		line.useWorldSpace = false; 
	}

	// Following method calculates world's point from screen point as per camera's projection type
	public Vector3 getWorldPosition(Vector3 screenPos)
	{
		Vector3 worldPos;
		if(Camera.main.orthographic)
		{
			worldPos = Camera.main.ScreenToWorldPoint (screenPos);
			worldPos.z = Camera.main.transform.position.z;
		}
		else
		{
			worldPos = Camera.main.ScreenToWorldPoint (new Vector3 (screenPos.x, screenPos.y, Camera.main.transform.position.z));
			worldPos.x *= -1;
			worldPos.y *= -1;
		}
		return worldPos;
	}
}
