﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SomeHead : MonoBehaviour 
{
	public Camera camera;
	public Sprite normalSprite;
	public Sprite trembleSprite;
	public Sprite sneezeSprite;
	public Sprite spitSprite;
	public Sprite winSprite;
	public Sprite loseSprite;

	public Sprite grannyNormal;
	public Sprite grannyHit;
	public Sprite grannyConfused;
	public Sprite grannySneeze;

	public SomeGrannyGod grannyGod;
	public RectTransform progressBarMask;
    public AudioClip preSneezeClip;
    public AudioClip sneezeClip;
    public AudioClip reactionFail;
    public AudioClip reactionOK;

    AudioSource manAudioSource;
    AudioSource grannyAudioSource;

    float progressBarInitRight;
	float progressBarInitWidth;
	public Text scoreText;
	public Text lifeText;

	const float TIME_BEFORE_TREMBLE = 5.0f;
	const float TIME_BEFORE_SNEEZE = 5.0f;
	const float TIME_LOSS_BY_LEVEL = 0.5f;
	const float TIME_LOSS_BY_LIFE = 1.0f;
	const int LIVES_AT_START = 3;

	const float EXPAND_SCALE = 200.0f;
	const bool DEVELOPMENT_BUILD = false;
	// Use this for initialization

	float timeBeforeTremble;
	float timeBeforeSneeze;
	float totalTimeBeforeSneeze;

	int lives;
	int level;
	int score;

	bool isTouching;
	int prevTouchCount;
	int curTouchCount;

	float sneezeStart;
	bool sneezeStarted;

	GameObject head;
	GameObject nose;
	GameObject aimer;
	Collider2D aimerCollider;
	List<GameObject> hitGrannies = new List<GameObject>();
	List<GameObject> allGrannies = new List<GameObject>();

	bool spitCoroutineEntered;

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


	void RecalculateTimers()
	{
		timeBeforeTremble = TIME_BEFORE_TREMBLE - TIME_LOSS_BY_LEVEL * (level - 1);
		timeBeforeTremble = timeBeforeTremble < 0.0f ? 0.0f : timeBeforeTremble;

		timeBeforeSneeze = TIME_BEFORE_SNEEZE - TIME_LOSS_BY_LIFE * ( LIVES_AT_START - lives );
		totalTimeBeforeSneeze = timeBeforeSneeze + timeBeforeTremble;

		Debug.Log ("Time before sneeze:" + timeBeforeSneeze + " " + "Time Before Tremble:" + timeBeforeTremble);
	}


	void Start () {
        manAudioSource = gameObject.AddComponent<AudioSource>();
        manAudioSource.playOnAwake = false;
        grannyAudioSource = gameObject.AddComponent<AudioSource>();
        grannyAudioSource.playOnAwake = false;
        isTouching = false;
		prevTouchCount = 0;
		curTouchCount = 0;
		score = 0;

		lives = LIVES_AT_START;
		level = 1;

		RecalculateTimers ();
		scoreText.text = score.ToString ();;
		lifeText.text = lives.ToString();

		progressBarInitWidth = Mathf.Abs( progressBarMask.offsetMax.x - progressBarMask.offsetMin.x ) * 1.2f;
		progressBarInitRight = progressBarMask.offsetMax.x - progressBarInitWidth;

		sneezeStart = Time.time;
		bool sneezeStarted = false;

		spitCoroutineEntered = false;

		head = GameObject.Find ("HeadSprite");
		nose = GameObject.Find ("NoseSprite");
		aimer = GameObject.Find ("Aimer");
		aimerCollider = GameObject.Find ("AimerSprite").GetComponent<Collider2D> ();

		DisableSneeze ();
		StartCoroutine (AutoSneeze ());
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

	void StopAllGrannies( bool stop )
	{
		foreach (GameObject grannyGroupObj in grannyGod.Grannies) 
		{
			SomeGranny grannyGroup = grannyGroupObj.GetComponent<SomeGranny> ();
			grannyGroup.start = !stop;

			foreach (GameObject granny in grannyGroup.Grannies) 
			{
				granny.GetComponent<Animator> ().enabled = !stop;
				granny.GetComponent<Animator> ().speed = 1.0f;
			}
		}
	}


	void GetAllGrannies()
	{
		allGrannies.Clear ();
		foreach (GameObject grannyGroupObj in grannyGod.Grannies) 
		{
			SomeGranny grannyGroup = grannyGroupObj.GetComponent<SomeGranny> ();

			foreach (GameObject granny in grannyGroup.Grannies) 
			{
				allGrannies.Add (granny);
			}
		}
	}


	void UpdateProgressBar( float percentage )
	{
		Debug.Log ("right " + progressBarInitRight + "width " + progressBarInitWidth);
		progressBarMask.offsetMax = new Vector2( progressBarInitRight + progressBarInitWidth * percentage, progressBarMask.offsetMax.y );

	}


	void ProcessLoseCondition()
	{
		if (lives == 0) 
		{
			//???
			return;
		}

		lives--;
		lifeText.text = lives.ToString ();


		if (lives == 0) 
		{
			// end game condition here
		}
	}


	void ProcessWinCondition()
	{
		score++;
		scoreText.text = score.ToString ();
	}


	IEnumerator SneezeSomeGrannies( bool autoLose )
	{
		bool anyGranniesHit = false;

		if (spitCoroutineEntered)
			yield break;

		spitCoroutineEntered = true;

		head.GetComponent<SpriteRenderer>().sprite = spitSprite;

		hitGrannies.Clear ();
		GetAllGrannies ();

		StopAllGrannies ( true );
		aimer.SetActive ( true );

		if (autoLose) 
		{
			anyGranniesHit = true;
			foreach (GameObject granny in allGrannies) 
				hitGrannies.Add (granny);
		}

		//check for granny collision
		foreach (GameObject granny in allGrannies) 
		{
			Collider2D grannyCollider = granny.GetComponent<Collider2D> ();

            Vector2 vp = camera.WorldToViewportPoint(granny.transform.position);
            
            bool grannyOutOfSight = vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f;
            Debug.Log(grannyOutOfSight);

            if (grannyCollider.IsTouching (aimerCollider) && !grannyOutOfSight) 
			{
				Debug.Log ("I hit a granny");
				hitGrannies.Add (granny);
				anyGranniesHit = true;
			}
		}


		if (anyGranniesHit) 
		{
			// lose this level
			foreach (GameObject granny in hitGrannies) 
			{
				granny.GetComponent<SpriteRenderer> ().sprite = grannyHit;
                
            }
            yield return new WaitForSeconds (0.6f);

            head.GetComponent<SpriteRenderer>().sprite = normalSprite;
			aimer.SetActive (false);

			foreach (GameObject granny in hitGrannies) 
			{
				granny.GetComponent<SpriteRenderer> ().sprite = grannySneeze;
			}

			yield return new WaitForSeconds (0.5f);

            grannyAudioSource.PlayOneShot(reactionFail);
            Debug.Log("?");
            // process lose condition
            head.GetComponent<SpriteRenderer>().sprite = loseSprite;

			yield return new WaitForSeconds (3.0f);
			ProcessLoseCondition ();
		} 
		else 
		{
			// win this level
			foreach (GameObject granny in allGrannies) 
			{
				granny.GetComponent<SpriteRenderer> ().sprite = grannyConfused;
				granny.GetComponent<Animator> ().enabled = true;
				granny.GetComponent<Animator> ().speed = 0.2f;
			}
            yield return new WaitForSeconds (0.6f);

            Debug.Log("??");
            grannyAudioSource.PlayOneShot(reactionOK);
            head.GetComponent<SpriteRenderer>().sprite = normalSprite;
			aimer.SetActive (false);

			yield return new WaitForSeconds (0.5f);
			//process win condition
			head.GetComponent<SpriteRenderer>().sprite = winSprite;
			yield return new WaitForSeconds (3.0f);

			ProcessWinCondition ();
		}

		{	//granny aftermath: on to next level

			head.GetComponent<SpriteRenderer>().sprite = normalSprite;

			foreach (GameObject granny in allGrannies) 
			{
				granny.GetComponent<SpriteRenderer> ().sprite = grannyNormal;
			}

			StopAllGrannies (false);
			grannyGod.KillGrannies ();
			grannyGod.SpawnGrannies (1);
		}

		level++;
		RecalculateTimers ();
		sneezeStarted = false;
		spitCoroutineEntered = false;
		isTouching = false;
	}


	void ReleaseHead( bool autoLose )
	{
		if (spitCoroutineEntered)
			return;
        manAudioSource.clip = sneezeClip;
        manAudioSource.Play();

        Debug.Log ("Head Released");
		nose.SetActive (false);

		StartCoroutine (SneezeSomeGrannies ( autoLose ));
	}

	void BeginPullHead()
	{
		Debug.Log ("Head Pull Begin");
		head.GetComponent<SpriteRenderer>().sprite = sneezeSprite;
		EnableSneeze ();
	}


	IEnumerator AutoSneeze()
	{
		float duration = 0.0f;
		float percentage = 0.0f;

		while (true) 
		{
			duration += Time.deltaTime;
			percentage = Mathf.Clamp01 (duration / totalTimeBeforeSneeze);

			if (duration > totalTimeBeforeSneeze) 
			{
				duration = 0.0f;

				bool autoLose = curTouchCount == 0;

				if(autoLose)
					aimer.transform.localScale = new Vector3 (3.0f, 3.0f, 1.0f);

				ReleaseHead ( autoLose );
			}

			if (spitCoroutineEntered) 
			{
				duration = 0.0f;
			}

			UpdateProgressBar ( percentage );

			if ( !sneezeStarted && duration > timeBeforeTremble ) 
			{
				sneezeStarted = true;
				head.GetComponent<SpriteRenderer> ().sprite = trembleSprite;
                manAudioSource.clip = preSneezeClip;
                manAudioSource.Play();

            }
            yield return new WaitForEndOfFrame();
		}
	}


	void ProcessInputTouch()
	{
		int touchCount;

		touchCount = Input.touchCount;
		curTouchCount = touchCount;

		if (spitCoroutineEntered)
			return;

		if (touchCount > 1 && touchCount > prevTouchCount) 
		{
			// can do touch start changes here
			if( sneezeStarted )
				BeginPullHead();
		}
			

		if (touchCount == 0) 
		{
			if (sneezeStarted && isTouching) 
			{
				ReleaseHead ( false );			
			}

			isTouching = false;
			return;
		}

		if ( sneezeStarted && touchCount >= 1 ) 
		{
			isTouching = true;
			PullHead (Input.GetTouch (0).position);
		}

		if( sneezeStarted && touchCount > 1 )
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
 