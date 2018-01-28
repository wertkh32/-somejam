using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeGrannyGod : MonoBehaviour {
    public List<GameObject> Grannies;
    public GameObject GrannyPrefab;
    private void Awake()
    {
        if (GrannyPrefab == null)
            GrannyPrefab = Resources.Load<GameObject>("granny");
    }
    // Use this for initialization
    void Start () {
        SpawnGrannies(3);
	}
	
    void SpawnGrannies(int n)
    {
        for(int i = 0; i < n; ++i)
        {
            GameObject newGranny = Instantiate<GameObject>(GrannyPrefab);
            newGranny.transform.parent = transform;
            Grannies.Add(newGranny);
        }
    }

    void KillGrannies()
    {
        foreach(GameObject granny in Grannies)
        {
            DestroyObject(granny);
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
