using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {


    //public GameObject WorldBuilder;
    public GameObject Container;

    Vector3  initialPosition;
    Quaternion intialRotation;

    Vector3 userPosition = new Vector3();
    Quaternion userRotation = Quaternion.identity;
    float userFieldOfView;

    public bool isStabilizeCamera = false;

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update()
    {
        float rotationSpeed = 5f / Time.timeScale;
        float translationSpeed = 5f / Time.timeScale;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            rotationSpeed *= 2f;
            translationSpeed *= 10f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.RotateAround(transform.position, transform.right, -transform.position.y * rotationSpeed / Time.timeScale);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.RotateAround(transform.position, transform.right, +transform.position.y * rotationSpeed / Time.timeScale);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(transform.position, transform.up, +transform.position.y * rotationSpeed / Time.timeScale);
        }


        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.RotateAround(transform.position, transform.up, -transform.position.y * rotationSpeed / Time.timeScale);
        }

        if (Input.GetKey(KeyCode.W))
        {
            Vector3 move =  translationSpeed / Time.timeScale * transform.forward;
            move = translationSpeed / Time.timeScale * transform.forward;
            transform.Translate(move, Space.World);
            userPosition = transform.position - Container.GetComponent<Transform>().position;

        }


        if (Input.GetKey(KeyCode.S))
        {
            Vector3 move = - translationSpeed / Time.timeScale * transform.forward;
            transform.Translate(move, Space.World);
            userPosition = transform.position - Container.GetComponent<Transform>().position;

        }

        if (Input.GetKey(KeyCode.PageDown))
        {
            gameObject.GetComponent<Camera>().fieldOfView += 10 / Time.timeScale;
            if (gameObject.GetComponent<Camera>().fieldOfView > 150) { gameObject.GetComponent<Camera>().fieldOfView = 150; }
        }


        if (Input.GetKey(KeyCode.PageUp))
        {
            gameObject.GetComponent<Camera>().fieldOfView -= 10 / Time.timeScale;
            if (gameObject.GetComponent<Camera>().fieldOfView < 1) { gameObject.GetComponent<Camera>().fieldOfView = 1; }
        }


        if (Input.GetKey(KeyCode.A))
        {
            Vector3 move = - translationSpeed / Time.timeScale * transform.right;
            transform.Translate(move, Space.World);
            userPosition = transform.position - Container.GetComponent<Transform>().position;

        }

        if (Input.GetKey(KeyCode.D))
        {
            Vector3 move =  translationSpeed / Time.timeScale * transform.right;
            transform.Translate(move, Space.World);
            userPosition = transform.position - Container.GetComponent<Transform>().position;
        }

        if (isStabilizeCamera == true) { gameObject.GetComponent<CameraController>().stabilizeCamera(); }

        if (Input.GetKeyUp(KeyCode.Tab)) { gameObject.GetComponent<CameraController>().resetView(); }

    }


    void FixedUpdate()
    {

      

    }

    public void stabilizeCamera()
    {
        Vector3 containerPosition = Container.GetComponent<Transform>().position;
        gameObject.GetComponent<Transform>().position = userPosition + containerPosition;
        // Quaternion boxRotation = boxGameObject.GetComponent<Transform>().rotation;
        //gameObject.GetComponent<Transform>().rotation = boxRotation;
        //updateUserPosition();
    }


    public void initializeCamera(Vector3 cameraPosition, Vector3 lookLocation)
    {
        gameObject.GetComponent<Transform>().localPosition = cameraPosition;
        gameObject.GetComponent<Transform>().LookAt(lookLocation);

        initialPosition = transform.position;
        intialRotation = transform.rotation;
        updateUserPosition();
    }

    public void resetView()
    {
        Vector3 containerPosition = Container.GetComponent<Transform>().position;
        transform.position = initialPosition + containerPosition;
        transform.rotation = intialRotation;
        gameObject.GetComponent<Camera>().fieldOfView = 60;

        updateUserPosition();

    }

    void updateUserPosition()
    {
        userPosition = initialPosition;
        userRotation = intialRotation;
        userFieldOfView = gameObject.GetComponent<Camera>().fieldOfView;
    }
}
