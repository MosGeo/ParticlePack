using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomOperations : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("HitstartAll");

    }

    // Update is called once per frame
    void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("HitBottomAll");
        if (collision.gameObject.tag == "Grain")
            {
                Debug.Log("HitBottom");
            }

    }
}
