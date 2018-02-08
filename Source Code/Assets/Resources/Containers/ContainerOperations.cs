using UnityEngine;
using System.Collections;

public class ContainerOperations : MonoBehaviour {

    public Vector3 scale = new Vector3(1, 1, 1);
    public GameObject Lid;
    public GameObject Bottom;
    public float lidSpeed = .001f;
    public float containerDynamicFriction = 0.6f;
    public float containerStaticFriction = 0.6f;
    public float containerBounciness = 0f;
    public PhysicMaterialCombine containerFrictionCombine = PhysicMaterialCombine.Average;
    public PhysicMaterialCombine containerBounceCombine = PhysicMaterialCombine.Average;
    public float bufferLength = 0f;
    public Rock rock;

    //===================================================================
    // Use this for initialization
    void Start () {

    }
    //===================================================================


    //===================================================================
    // Update is called once per frame
    void Update () {
        
        if (Input.GetKeyUp(KeyCode.L) == true)
        {
            Lid.SetActive(!Lid.activeSelf);
            rock.WakeUp();

        }

        if (Input.GetKey(KeyCode.End))
        {
            float movement = scale.y * .001f;
            Vector3 newPosition = Lid.GetComponent<Transform>().localPosition;
            newPosition.y = newPosition.y - movement;
            Lid.GetComponent<Transform>().localPosition = newPosition;
            rock.WakeUp();
        }

        if (Input.GetKey(KeyCode.Home))
        {
            float movement = scale.y * .001f;
            Vector3 newPosition = Lid.GetComponent<Transform>().localPosition;
            newPosition.y = newPosition.y + movement;
            Lid.GetComponent<Transform>().localPosition = newPosition;
            rock.WakeUp();
        }

    }
    //===================================================================


    //===================================================================
    public Vector3 getInitialCameraPosition()
    {
        float cameraHight = .8f * scale.y + scale.y;
        float cameraZposition = scale.z - 3.5f*scale.z;
        return new Vector3(0, cameraHight, cameraZposition);
    }
    //===================================================================


    //===================================================================
    public Vector3 getInitialLookLocation()
    {
       return new Vector3(0, scale.y / 2, 0);
    }
    //===================================================================


    //===================================================================
    public PhysicMaterial buildPhysicsMaterial(float dynamicFriction, float staticFriction, float bounciness, PhysicMaterialCombine frictionCombine, PhysicMaterialCombine bounceCombine)
    {
        PhysicMaterial physicsMaterial = new PhysicMaterial();
        physicsMaterial.dynamicFriction = dynamicFriction;
        physicsMaterial.staticFriction = staticFriction;
        physicsMaterial.bounciness = bounciness;
        physicsMaterial.frictionCombine = frictionCombine;
        physicsMaterial.bounceCombine = bounceCombine;
        return physicsMaterial;
    }
    //===================================================================


    //===================================================================
    public void setContainerScale(Vector3 newScale)
    {
        scale = newScale;
    }
    //===================================================================


    //===================================================================
    public Vector3 getContainerScale(Vector3 newScale)
    {
        return scale;
    }
    //===================================================================


    //===================================================================
    public void applyContainerScale()
    {
        gameObject.transform.localScale = scale;
    }
    //===================================================================

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].thisCollider.tag == "ContainerBottom")
        {
            if (collision.gameObject.GetComponent<GrainOperations>().disappearAtBottom == true)
            {
                collision.gameObject.SetActive(false);
            }
        }
   }


    // Overides
    public virtual Vector3 getNewLocation(GameObject grainPrefab) { return new Vector3(); }
    public virtual Vector3  GetScaleFromVolume(float grainVolume, float porosity){return new Vector3();}


}


