﻿using System.Collections;
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
	public Sprite normalSprite;
	public Sprite trembleSprite;
	public Sprite sneezeSprite;
	public Sprite spitSprite;

	const float EXPAND_SCALE = 200.0f;
	const bool DEVELOPMENT_BUILD = false;
	// Use this for initialization

	bool isTouching;
	int prevTouchCount;

	float sneezeStart;
	bool sneezeStarted;

	GameObject head;
	GameObject nose;
	GameObject aimer;

	void EnableSneeze()
	{
		aimer.SetActive (true);
		nose.SetActive (true);
	}

	void DisableSneeze()
	{
		aimer.SetActive (false);
		nose.SetActive (false);
	}

	void Start () {
		isTouching = false;
		prevTouchCount = 0;

		sneezeStart = Time.time;
		bool sneezeStarted = false;

		head = GameObject.Find ("HeadSprite");
		nose = GameObject.Find ("NoseSprite");
		aimer = GameObject.Find ("Aimer");

		DisableSneeze ();
	}
	

	void PullHead( Vector2 fingerPos )
	{
		Vector3 screenSpace = new Vector3 (fingerPos.x, fingerPos.y, 1.0f);
		Vector3 worldPos = camera.ScreenToWorldPoint( screenSpace );
		//transform.position = worldPos;
		Debug.Log ("Head pull down");
	}

	void ScaleNose( float varyScale )
	{
		float expandSize;
		float expandSizeInv;

		expandSize = 0.5f + varyScale * 2.0f;

		nose.transform.localScale = new Vector3 ( expandSize, expandSize, 1.0f );
	}


	void ScaleAimer( float varyScale )
	{
		float expandSize;
		float expandSizeInv;
		float invVaryScale;

		invVaryScale = 1.0f - varyScale;

		expandSize = 0.5f + varyScale;
		expandSizeInv = 0.5f + invVaryScale;

		if( invVaryScale > 0.5f )
		{
			expandSize = 0.3f + varyScale;
			expandSizeInv = 0.5f + invVaryScale * 2.0f;
		}

		aimer.transform.localScale = new Vector3 (expandSize, expandSizeInv, 1.0f);
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
		varyScale *= varyScale * varyScale;

		ScaleNose (varyScale);
		ScaleAimer (varyScale);
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
		head.GetComponent<SpriteRenderer>().sprite = normalSprite;
		DisableSneeze ();
	}

	void BeginPullHead()
	{
		Debug.Log ("Head Pull Begin");
		head.GetComponent<SpriteRenderer>().sprite = sneezeSprite;
		EnableSneeze ();
	}

	void ProcessInputTouch()
	{
		int touchCount;

		touchCount = Input.touchCount;

		if (touchCount > 1 && touchCount > prevTouchCount) 
		{
			// can do touch start changes here
			BeginPullHead();
		}
			

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
			Vector2[] twoFingers = { Input.GetTouch (0).position,
									 Input.GetTouch (1).position };

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

		prevTouchCount = touchCount;
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
 