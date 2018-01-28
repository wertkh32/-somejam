using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class aimer
{
	float radius;
	float angle;
}


public class SomeHead : MonoBehaviour 
{
	public Camera camera;
	const float EXPAND_SCALE = 100.0f;
	const bool DEVELOPMENT_BUILD = false;
	const Vector2 center = new Vector2( 0, -1 );
	// Use this for initialization
	bool isTouching;

	float sneezeStart;
	bool sneezeStarted;

	GameObject head;
	GameObject aimer;

	void Start () {
		isTouching = false;
		sneezeStart = Time.time;
		bool sneezeStarted = false;
		head = GameObject.Find ("HeadSprite");
		aimer = GameObject.Find ("Aimer");
	}
	

	void PullHead( Vector2 fingerPos )
	{
		Vector3 screenSpace = new Vector3 (fingerPos.x, fingerPos.y, 1.0f);
		Vector3 worldPos = camera.ScreenToWorldPoint( screenSpace );
		//transform.position = worldPos;
		Debug.Log ("Head pull down");
	}

	void ExpandNose( Vector2[] fingersPos )
	{
		Vector2 diff;
		float diffMag;
		float expandSize;
		float expandSizeInv;
		float varyScale;
		float invVaryScale;

		diff = fingersPos [0] - fingersPos[1];
		diffMag = diff.magnitude;

		varyScale = Mathf.Clamp01( diffMag / EXPAND_SCALE );
		invVaryScale = 1.0f - varyScale;

		expandSize = 0.5f + varyScale;
		expandSizeInv = 0.5f + invVaryScale;

		head.transform.localScale = new Vector3 ( expandSize, expandSize, 1.0f );
		aimer.transform.localScale = new Vector3 (expandSize, expandSizeInv * 1.5f, 1.0f);
	}

	void RotateHead( Vector2[] fingersPos )
	{
		Vector2 diff;
		float angle;
		Quaternion quat;

		diff = fingersPos [1] - fingersPos[0];
		
		angle = (Mathf.Atan2 (diff.y, diff.x) / Mathf.PI) * 180.0f;

		angle = Mathf.Clamp (angle, -60.0f, 60.0f);

		aimer.transform.localRotation = Quaternion.Euler (new Vector3 (0, 0, angle));
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

			{
				if (twoFingers [0].x > twoFingers [1].x) 
				{
					Vector2 tmp = twoFingers [0];
					twoFingers [0] = twoFingers [1];
					twoFingers [1] = tmp;
				}
			}


			isTouching = true;
			RotateHead (twoFingers);
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
 