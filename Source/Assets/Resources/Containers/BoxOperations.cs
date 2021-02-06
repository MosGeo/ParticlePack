using UnityEngine;
using System.Collections;

public class BoxOperations : ContainerOperations {

    [Header("Size")]
    public Vector3 InnerSize =  new Vector3 (1,1,1);
    public Vector3 WallWidth =  new Vector3(0.1f, 0.1f, 0.1f);

    // Names
    string containerName = "Box";
    string containerTag = "Container";
    string containerSideTag = "ContainerSide";
    string containerLidTag = "ContainerLid";
    string containerBottomTag = "ContainerBottom";

    // Used Properties
    string materialName = "Container";
    // ==============================================================================================================================================
    void Start()
    {
        buildContainer();
        applyContainerScale();
    }
    // ==============================================================================================================================================
    // Build the box
    void buildContainer()
    {
        gameObject.name = containerName;
        gameObject.tag = containerTag;
        Material renderMaterial = Resources.Load("Material\\" + materialName) as Material;
        PhysicMaterial physMaterial = buildPhysicsMaterial(containerDynamicFriction, containerStaticFriction, containerBounciness, containerFrictionCombine, containerBounceCombine);

        gameObject.AddComponent<Rigidbody>();
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        //gameObject.GetComponent<Rigidbody>().collisionDetectionMode =CollisionDetectionMode.Continuous;


        Vector3 slapWidth = InnerSize + 2 * WallWidth;

        // Bottom side
        Vector3 BottomSideLocation = new Vector3(0, -WallWidth.y / 2, 0);
        Vector3 BottomSideScale = new Vector3(slapWidth.x, WallWidth.y, slapWidth.z);
        GameObject BottomSide = buildSide(BottomSideLocation, BottomSideScale, renderMaterial, physMaterial, "Bottom", containerBottomTag);
        //BottomSide.AddComponent<BottomOperations>();

        // Back side
        Vector3 backSideLocation = new Vector3(0, InnerSize.y / 2, InnerSize.z/2 + WallWidth.z/2);
        Vector3 backSideScale = new Vector3(slapWidth.x, InnerSize.y, WallWidth.z);
        GameObject BackSide = buildSide(backSideLocation, backSideScale, renderMaterial, physMaterial, "Back", containerSideTag);

        // Front side
        Vector3 FrontSideLocation = new Vector3(0, InnerSize.y / 2, - (InnerSize.z / 2 + WallWidth.z / 2));
        Vector3 FrontSideScale = new Vector3(slapWidth.x, InnerSize.y, WallWidth.z);
        GameObject FrontSide = buildSide(FrontSideLocation, FrontSideScale, renderMaterial, physMaterial, "Front", containerSideTag);

        // Right side
        Vector3 RightSideLocation = new Vector3(InnerSize.x / 2 + WallWidth.x / 2, InnerSize.y / 2, 0);
        Vector3 RightSideScale = new Vector3(WallWidth.x, InnerSize.y, slapWidth.z);
        GameObject RightSide = buildSide(RightSideLocation, RightSideScale, renderMaterial, physMaterial, "Right", containerSideTag);

        // Left side
        Vector3 LeftSideLocation = new Vector3(-(InnerSize.x / 2 + WallWidth.x / 2), InnerSize.y / 2, 0);
        Vector3 LeftSideScale = new Vector3(WallWidth.x, InnerSize.y, slapWidth.z);
        GameObject LeftSide = buildSide(LeftSideLocation, LeftSideScale, renderMaterial, physMaterial, "Left", containerSideTag);

        // Top side
        Vector3 TopSideLocation = new Vector3(0, InnerSize.y + WallWidth.y / 2, 0);
        Vector3 TopSideScale = new Vector3(slapWidth.x, WallWidth.y, slapWidth.z);
        GameObject TopSide = buildSide(TopSideLocation, TopSideScale, renderMaterial, physMaterial, "Top", containerLidTag);

        TopSide.SetActive(false);

        gameObject.GetComponent<ContainerOperations>().Lid = TopSide;
    }

    // ==============================================================================================================================================
    // Build one side of the box
    GameObject buildSide(Vector3 location, Vector3 scale, Material renderMaterial, PhysicMaterial physMaterial, string name, string tag)
    {
        GameObject boxSide = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boxSide.GetComponent<Transform>().localPosition = location;
        boxSide.GetComponent<Transform>().localScale = scale;
        boxSide.GetComponent<Transform>().parent = transform;
        boxSide.GetComponent<BoxCollider>().material = physMaterial;
        boxSide.GetComponent<MeshRenderer>().material = renderMaterial;
        boxSide.name = name;
        boxSide.tag = tag;
        return boxSide;
    }
    // ==============================================================================================================================================
    public override Vector3 getNewLocation(GameObject grainPrefab)
    {
        Vector3 grainLocation;

        float minimumDistanceToContainer = 2f;
        float maximumDistanceToContainer = 5f;
        bool exactVerticalCreation = grainPrefab.GetComponent<GrainOperations>().exactVerticalCreation;
        if (exactVerticalCreation == true) { maximumDistanceToContainer = minimumDistanceToContainer; }

        Vector3 grainScale = grainPrefab.GetComponent<Transform>().localScale;
        float maxScale = Mathf.Max(Mathf.Max(grainScale.x, grainScale.y), grainScale.z);

        Bounds bounds = grainPrefab.GetComponent<MeshFilter>().sharedMesh.bounds;
        float hitColliderSearch = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z) * maxScale;

        float xLocation = Random.Range((-scale.x + hitColliderSearch) / 2 + bufferLength, (scale.x - hitColliderSearch) / 2 - bufferLength);
        float yLocation = Random.Range(scale.y + minimumDistanceToContainer, scale.y + maximumDistanceToContainer);
        float zLocation = Random.Range((-scale.z + hitColliderSearch) / 2 + bufferLength, (scale.z - hitColliderSearch) / 2)- bufferLength;
        grainLocation = this.GetComponent<Transform>().position + new Vector3(xLocation, yLocation, zLocation);

        Collider[] hitColliders = Physics.OverlapSphere(grainLocation, hitColliderSearch);
        int nTrys = 0;
        while (hitColliders.Length != 0 & nTrys <= 1000)
        {

             xLocation = Random.Range((-scale.x + hitColliderSearch) / 2 + bufferLength, (scale.x - hitColliderSearch) / 2 - bufferLength);
             yLocation = Random.Range(scale.y + minimumDistanceToContainer, scale.y + maximumDistanceToContainer);
             zLocation = Random.Range((-scale.z + hitColliderSearch) / 2 + bufferLength, (scale.z - hitColliderSearch) / 2) - bufferLength;
            grainLocation = this.GetComponent<Transform>().position + new Vector3(xLocation, yLocation, zLocation);
            //grainLocation = Container.GetComponent<Transform>().position + new Vector3(UnityEngine.Random.Range(-scale.x / bufferZone, scale.x / bufferZone), UnityEngine.Random.Range(scale.y + minimumDistanceToContainer, scale.y + maximumDistanceToContainer), UnityEngine.Random.Range(-scale.z / bufferZone, scale.z / bufferZone));
            hitColliders = Physics.OverlapSphere(grainLocation, hitColliderSearch);
            nTrys += 1;
            if (nTrys > 500)
            {
                maximumDistanceToContainer *= 2;
            }
        }

        return grainLocation;
    }
    // ==============================================================================================================================================
    public float GetVolume()
    {
        float volume =  scale.x * scale.z  * scale.y;
        return volume;
    }
    // ==============================================================================================================================================
    public override Vector3 GetScaleFromVolume(float grainVolume, float porosity)
    {
        //float expectedTotalVolume = grainVolume + porosity * grainVolume;
        float expectedTotalVolume = grainVolume / (1-porosity);
        //Debug.Log("Grain Volume + Porosity " + expectedTotalVolume);
        float scalingFactor = Mathf.Pow(expectedTotalVolume / GetVolume(), 1f/3f);
        //Debug.Log("scale " + scale);
        //Debug.Log("ScalingFactor " + scalingFactor);
        scale = scale * scalingFactor;
        return scale;
    }
    // ==============================================================================================================================================


}
