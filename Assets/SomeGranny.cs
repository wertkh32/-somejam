using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeGranny : MonoBehaviour
{
    public float speed = 0.005f;
    public int step = 20;
    private Camera cam;
    private Vector3[] dirVector = new Vector3[] { Vector3.left, Vector3.right, Vector3.up, Vector3.down, Vector3.zero };
    private Coroutine moveRoutine;
    private Coroutine splitRoutine;
    public List<GameObject> Grannies = new List<GameObject>();
	public bool start;
    // Use this for initialization
    void Awake()
    {
        cam = FindObjectOfType<Camera>();
		start = true;
    }
    IEnumerator Move(Vector3 dir, float speed, int step)
    {
        int currentStep = step;
        while (currentStep > 0)
        {
			if (!start) 
			{
				yield return new WaitForEndOfFrame ();
				continue;			
			}

            Vector3 vp = cam.WorldToViewportPoint(transform.position);
            currentStep -= 1;
            vp += speed * dir;
            vp.x = Mathf.Clamp01(vp.x);
            vp.y = Mathf.Clamp(vp.y, 1f - 0.618f, 0.8f);
            transform.position = cam.ViewportToWorldPoint(vp);

            yield return new WaitForEndOfFrame();
        }
        moveRoutine = null;
    }
    IEnumerator Split(Vector3 dir, float speed, int step)
    {
        int currentStep = step;
        int mid = Grannies.Count / 2;
        int seed = Random.Range(0, 100);
        while (currentStep > 0)
        {
			if (!start) 
			{
				yield return new WaitForEndOfFrame ();
				continue;
			}

            if (seed > 0)
            {
                if (currentStep > step / 2)
                {
                    for (int i = 0; i < mid; ++i)
                    {
                        Vector3 vp = cam.WorldToViewportPoint(Grannies[i].transform.position);
                        vp += speed * dir;
                        Grannies[i].transform.position = cam.ViewportToWorldPoint(vp);
                    }

                    int j;
                    if (Grannies.Count % 2 == 0)
                    {
                        j = mid;
                    }
                    else
                    {
                        j = mid + 1;
                    }
                    for (; j < Grannies.Count; ++j)
                    {
                        Vector3 vp = cam.WorldToViewportPoint(Grannies[j].transform.position);
                        vp -= speed * dir;
                        Grannies[j].transform.position = cam.ViewportToWorldPoint(vp);
                    }
                }
                else
                {
                    for (int i = 0; i < mid; ++i)
                    {
                        Vector3 vp = cam.WorldToViewportPoint(Grannies[i].transform.position);
                        vp -= speed * dir;
                        Grannies[i].transform.position = cam.ViewportToWorldPoint(vp);
                    }

                    int j;
                    if (Grannies.Count % 2 == 0)
                    {
                        j = mid;
                    }
                    else
                    {
                        j = mid + 1;
                    }
                    for (; j < Grannies.Count; ++j)
                    {
                        Vector3 vp = cam.WorldToViewportPoint(Grannies[j].transform.position);
                        vp += speed * dir;
                        Grannies[j].transform.position = cam.ViewportToWorldPoint(vp);
                    }
                }
            }
            --currentStep;
            yield return new WaitForEndOfFrame();
        }
        splitRoutine = null;
    }
    // Update is called once per frame
    void Update()
    {
        if (moveRoutine == null)
        {
            moveRoutine = StartCoroutine(Move(dirVector[Random.Range(0, dirVector.Length)], speed, step));
        }
        if (splitRoutine == null && Grannies.Count > 1)
        {
            splitRoutine = StartCoroutine(Split(dirVector[Random.Range(0, dirVector.Length)], speed/5f, step*30));
        }
    }
}
