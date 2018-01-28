using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SomeHead : MonoBehaviour 
{
	public Camera camera;
	const float EXPAND_SCALE = 100.0f;
	const bool DEVELOPMENT_BUILD = false;
	// Use this for initialization
	bool isTouching;

	float sneezeStart;
	bool sneezeStarted;

	void Start () {
		isTouching = false;
		sneezeStart = Time.time;
		bool sneezeStarted = false;
	}
	

	void PullHead( Vector2 fingerPos )
	{
		Vector3 screenSpace = new Vector3 (fingerPos.x, fingerPos.y, 1.0f);
		Vector3 worldPos = camera.ScreenToWorldPoint( screenSpace );
		transform.position = worldPos;
		Debug.Log ("Head pull down");
	}

	void ExpandNose( Vector2[] fingersPos )
	{
		Vector2 diff;
		float diffMag;
		float expandSize;

		diff = fingersPos [0] - fingersPos[1];
		diffMag = diff.magnitude;

		expandSize = 1.0f + diffMag / EXPAND_SCALE;

		this.transform.localScale = new Vector3 ( expandSize, expandSize, expandSize );
	}

	void ReleaseHead()
	{
		Debug.Log ("Head Released");
	}

	void ProcessInputTouch()
	{
		int touchCount;

		touchCount = Input.touchCount;

		if (touchCount == 0) 
		{
			if (isTouching) 
			{
				ReleaseHead ();			
			}

			isTouching = false;
			return;
		}

		if (touchCount >= 1) 
		{
			isTouching = true;
			PullHead (Input.GetTouch (0).position);
		}

		if( touchCount > 1 )
		{
			Vector2[] twoFingers = { Input.GetTouch (0).position, Input.GetTouch (1).position };

			isTouching = true;
			ExpandNose ( twoFingers );

			Debug.Log ("Many touch");
		}
	}


	void ProcessInputMouse()
	{
		Vector2 fingerPos = new Vector2( Input.mousePosition.x, Input.mousePosition.y );
		float scale = 0.0f;
		bool isLeftMouseButtonHold = Input.GetMouseButton (0);
		bool isRightMouseButtonHold = Input.GetMouseButton (1);

		if (isRightMouseButtonHold) 
		{
			scale = ( Mathf.Sin (Time.timeSinceLevelLoad) + 1.0f ) * 50.0f;
		}
			
		//Debug.Log (fingerPos);

		if (isLeftMouseButtonHold) 
		{
			Debug.Log( "left mouse button down" );
			PullHead (fingerPos);
		}

		if (isRightMouseButtonHold) 
		{
			Vector2[] twofingers = { new Vector2 (0, 0), new Vector2 (scale, 0) };

			Debug.Log ("right mouse button down");
			ExpandNose ( twofingers );
		}

	}


	// Update is called once per frame
	void Update () 
	{
		if (DEVELOPMENT_BUILD) 
		{
			ProcessInputMouse ();
		} else 
		{
			ProcessInputTouch ();
		}
	}
}
 