using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeDude : MonoBehaviour
{
    private Camera cam;
    public float speed = 0.01f;
    public int minStep = 10;
    public int maxStep = 50;
    public bool Go = true;

    private bool rolled = false;
    private int dir = 0;
    private int steps = 0;
    private Vector3[] dirVector = new Vector3[] { Vector3.left, Vector3.right, Vector3.up, Vector3.down, Vector3.zero };
    // Use this for initialization
    void Awake()
    {
        cam = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Go) return;
        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        if (!rolled)
        {
            this.dir = Random.Range(0, dirVector.Length);
            this.steps = Random.Range(minStep, maxStep);
            rolled = true;
        }

        vp += speed * dirVector[dir];
        vp.x = Mathf.Clamp01(vp.x);
        vp.y = Mathf.Clamp(vp.y, 1f - 0.618f, 1f);
        transform.position = cam.ViewportToWorldPoint(vp);

        this.steps -= 1;
        if ((steps) == 0)
        {
            rolled = false;
        }
    }
}
