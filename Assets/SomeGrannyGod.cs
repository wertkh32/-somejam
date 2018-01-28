using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeGrannyGod : MonoBehaviour
{
    public List<GameObject> Grannies = new List<GameObject>();
    public GameObject GrannyPrefab;
    private void Awake()
    {
        if (GrannyPrefab == null)
            GrannyPrefab = Resources.Load<GameObject>("granny");
    }
    // Use this for initialization
    void Start()
    {
        SpawnGrannies(1);

    }
    GameObject SpawnGrannyGroup(int n)
    {
        GameObject grannyGroup = new GameObject(string.Format("grannyGroupOf{0}", n));
        grannyGroup.transform.parent = transform;

        SomeGranny g = grannyGroup.AddComponent<SomeGranny>();
        for (int i = 0; i < n; ++i)
        {
            GameObject newGranny = Instantiate<GameObject>(GrannyPrefab);
            newGranny.transform.parent = grannyGroup.transform;
            float leftMost = 0f - ((float)n - 1f) / 2f;
            newGranny.transform.localPosition = new Vector3(i * 1f + leftMost, 0, 0);
            g.Grannies.Add(newGranny);
        }
        return grannyGroup;
    }
    public void SpawnGrannies(int n)
    {
        for (int i = 0; i < n; ++i)
        {
            Grannies.Add(SpawnGrannyGroup(Random.Range(1, 5)));
        }
    }

    public void KillGrannies()
    {
        foreach (GameObject granny in Grannies)
        {
            DestroyObject(granny);
        }

		Grannies.Clear ();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
