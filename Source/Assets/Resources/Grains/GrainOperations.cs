using UnityEngine;
using System.Collections;

public class GrainOperations : MonoBehaviour {

    [Header("Main Parameters")]
    public int bedNumber = 0;
    public int grainNumber = 0;
    public int grainIdentifier = 0;
    public Color color;
    public bool exactVerticalCreation;
    public bool disappearAtBottom;
    public float scale;

    public float grainMass;
    public float grainVolume;
    public float grainDensity;
    public float grainSurfaceArea;


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (gameObject.GetComponent<Transform>().position.y < -20)
        {
            Destroy(gameObject);
        }

        //if (gameObject.GetComponent<Rigidbody>().velocity.magnitude < .00001)
        //{
            //Debug.Log(gameObject.GetComponent<Transform>().localPosition);
            //Destroy(gameObject);
        //}
    }

    public void UpdateGrainProperties()
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        grainVolume = MeshOperations.VolumeOfMesh(mesh, scale);
        //grainSurfaceArea = MeshOperations.GetMeshSurfaceArea(mesh, scale);
        grainMass = grainDensity * grainVolume;
        gameObject.GetComponent<Rigidbody>().mass = grainMass;

        //Debug.Log("Mass " + grainMass);
        //Debug.Log("Volume " + grainVolume);
        //Debug.Log("Density " + grainDensity);
        //Debug.Log("SA " + grainSurfaceArea);

    }

    public virtual void ProcessParametersString(string parametersString) { }

}
