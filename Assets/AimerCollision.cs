﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimerCollision : MonoBehaviour {
	void OnTriggerStay2D( Collider2D collision )
	{
		Debug.Log (collision.gameObject.name);

	}

}
