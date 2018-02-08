using UnityEngine;
using System.Collections;

public class CylinderOperations : ContainerOperations {


    [Header("Size")]
    public float InnerDiameter = 1f;
    public float WallWidth = 0.1f;
    public float height = 1f;
    public int nbSides = 200;

    // Names
    string containerName = "Cylinder";
    string containerTag = "Container";
    string containerSideTag = "ContainerSide";
    string containerLidTag = "ContainerLid";
    string containerBottomTag = "ContainerBottom";


    // Used Properties
    string materialName = "Container";
    string physicsMaterial = "Container";


    // Use this for initialization
    void Start()
    {
        buildContainer();
        applyContainerScale();
    }


    void buildContainer()
    {

        gameObject.name = containerName;
        gameObject.tag = containerTag;
        Material renderMaterial = Resources.Load("Material/" + materialName) as Material;
        PhysicMaterial physMaterial = buildPhysicsMaterial(containerDynamicFriction, containerStaticFriction, containerBounciness, containerFrictionCombine, containerBounceCombine);


        gameObject.AddComponent<Rigidbody>();
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;



        // Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2
        float bottomRadius1 = (InnerDiameter + WallWidth) / 2;
        float bottomRadius2 = WallWidth;
        float topRadius1 = bottomRadius1;
        float topRadius2 = bottomRadius2;

        GameObject side = new GameObject();
        side.AddComponent<MeshFilter>();
        side.GetComponent<MeshFilter>().mesh = buildSideMesh(1, bottomRadius1, bottomRadius2, topRadius1, topRadius2, nbSides);
        side.AddComponent<MeshRenderer>();
        side.GetComponent<MeshRenderer>().material = renderMaterial;
        side.AddComponent<MeshCollider>();
        side.GetComponent<MeshCollider>().convex = false;
        side.GetComponent<Transform>().parent = gameObject.transform;
        side.GetComponent<Transform>().localScale = new Vector3(1, height, 1);

        side.name = "Side";
        side.tag =  containerSideTag;

        // Bottom side
        Vector3 BottomCoverLocation = new Vector3(0, -WallWidth/2, 0);
        Vector3 BottomCoverScale = new Vector3(InnerDiameter+ 2*WallWidth, WallWidth/2, InnerDiameter + 2 * WallWidth);
        GameObject BottomCover = buildCover(BottomCoverLocation, BottomCoverScale, physMaterial, renderMaterial, "Bottom" , containerBottomTag);

        Vector3 TopCoverLocation = new Vector3(0, height + WallWidth / 2, 0);
        Vector3 TopCoverScale = new Vector3(InnerDiameter + 2 * WallWidth, WallWidth / 2, InnerDiameter + 2 * WallWidth);
        GameObject TopCover = buildCover(TopCoverLocation, TopCoverScale, physMaterial, renderMaterial, "Top", containerLidTag);
        gameObject.GetComponent<ContainerOperations>().Lid = TopCover;
        TopCover.SetActive(false);



    }

    GameObject buildCover(Vector3 location, Vector3 scale, PhysicMaterial physMaterial, Material renderMaterial, string name, string tag )
    {
        GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cover.GetComponent<Transform>().localPosition = location;
        cover.GetComponent<Transform>().localScale = scale;
        cover.GetComponent<Transform>().parent = transform;
        Destroy(cover.GetComponent<CapsuleCollider>());
        cover.AddComponent<MeshCollider>();
        cover.GetComponent<MeshCollider>().material = physMaterial;
        cover.GetComponent<MeshRenderer>().material = renderMaterial;

        cover.name = name;
        cover.tag = tag;
        return cover;
    }


    Mesh buildSideMesh(float height, float bottomRadius1, float bottomRadius2, float topRadius1, float topRadius2,  int nbSides)
    {


        Mesh mesh = new Mesh();

        int nbVerticesCap = nbSides * 2 + 2;
        int nbVerticesSides = nbSides * 2 + 2;
        #region Vertices

        // bottom + top + sides
        Vector3[] vertices = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];
        int vert = 0;
        float _2pi = Mathf.PI * 2f;

        // Bottom cap
        int sideCounter = 0;
        while (vert < nbVerticesCap)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            vertices[vert] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0f, sin * (bottomRadius1 - bottomRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0f, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }

        // Top cap
        sideCounter = 0;
        while (vert < nbVerticesCap * 2)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            vert += 2;
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);

            vertices[vert] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);

            vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
            vert += 2;
        }
        #endregion

        #region Normales

        // bottom + top + sides
        Vector3[] normales = new Vector3[vertices.Length];
        vert = 0;

        // Bottom cap
        while (vert < nbVerticesCap)
        {
            normales[vert++] = Vector3.down;
        }

        // Top cap
        while (vert < nbVerticesCap * 2)
        {
            normales[vert++] = Vector3.up;
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;

            normales[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
            normales[vert + 1] = normales[vert];
            vert += 2;
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;

            normales[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
            normales[vert + 1] = normales[vert];
            vert += 2;
        }
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];

        vert = 0;
        // Bottom cap
        sideCounter = 0;
        while (vert < nbVerticesCap)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(0f, t);
            uvs[vert++] = new Vector2(1f, t);
        }

        // Top cap
        sideCounter = 0;
        while (vert < nbVerticesCap * 2)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(0f, t);
            uvs[vert++] = new Vector2(1f, t);
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(t, 0f);
            uvs[vert++] = new Vector2(t, 1f);
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(t, 0f);
            uvs[vert++] = new Vector2(t, 1f);
        }
        #endregion

        #region Triangles
        int nbFace = nbSides * 4;
        int nbTriangles = nbFace * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        // Bottom cap
        int i = 0;
        sideCounter = 0;
        while (sideCounter < nbSides)
        {
            int current = sideCounter * 2;
            int next = sideCounter * 2 + 2;

            triangles[i++] = next + 1;
            triangles[i++] = next;
            triangles[i++] = current;

            triangles[i++] = current + 1;
            triangles[i++] = next + 1;
            triangles[i++] = current;

            sideCounter++;
        }

        // Top cap
        while (sideCounter < nbSides * 2)
        {
            int current = sideCounter * 2 + 2;
            int next = sideCounter * 2 + 4;

            triangles[i++] = current;
            triangles[i++] = next;
            triangles[i++] = next + 1;

            triangles[i++] = current;
            triangles[i++] = next + 1;
            triangles[i++] = current + 1;

            sideCounter++;
        }

        // Sides (out)
        while (sideCounter < nbSides * 3)
        {
            int current = sideCounter * 2 + 4;
            int next = sideCounter * 2 + 6;

            triangles[i++] = current;
            triangles[i++] = next;
            triangles[i++] = next + 1;

            triangles[i++] = current;
            triangles[i++] = next + 1;
            triangles[i++] = current + 1;

            sideCounter++;
        }


        // Sides (in)
        while (sideCounter < nbSides * 4)
        {
            int current = sideCounter * 2 + 6;
            int next = sideCounter * 2 + 8;

            triangles[i++] = next + 1;
            triangles[i++] = next;
            triangles[i++] = current;

            triangles[i++] = current + 1;
            triangles[i++] = next + 1;
            triangles[i++] = current;

            sideCounter++;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        ;

        return mesh;

    }


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

        float phiLocation = Random.Range(0, 2f * Mathf.PI);
        float yLocation = Random.Range(scale.y + minimumDistanceToContainer, scale.y + maximumDistanceToContainer);
        float maxX  = Mathf.Abs((scale.x - hitColliderSearch) / 2 * Mathf.Cos(phiLocation)) - bufferLength;
        float maxZ = Mathf.Abs((scale.z - hitColliderSearch) / 2 * Mathf.Sin(phiLocation)) - bufferLength;
        float xLocation = Random.Range(-maxX, maxX);
        float zLocation = Random.Range(-maxZ, maxZ);

        //Debug.Log((-scale.x + hitColliderSearch) / 2);
        grainLocation = this.GetComponent<Transform>().position + new Vector3(xLocation, yLocation, zLocation);

        Collider[] hitColliders = Physics.OverlapSphere(grainLocation, hitColliderSearch);
        int nTrys = 0;
        while (hitColliders.Length != 0 & nTrys <= 1000)
        {
             phiLocation = Random.Range(0, 2f * Mathf.PI);
             yLocation = Random.Range(scale.y + minimumDistanceToContainer, scale.y + maximumDistanceToContainer);
            maxX = Mathf.Abs((scale.x - hitColliderSearch) / 2 * Mathf.Cos(phiLocation)) - bufferLength;
            maxZ = Mathf.Abs((scale.z - hitColliderSearch) / 2 * Mathf.Sin(phiLocation)) - bufferLength;
            xLocation = Random.Range(-maxX, maxX);
             zLocation = Random.Range(-maxZ, maxZ);

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

    //===================================================================
    public float GetVolume()
    {
        float volume = Mathf.PI * scale.x / 2f * scale.z / 2f * scale.y;
        return volume;
    }
    //===================================================================

    //===================================================================
    public override Vector3 GetScaleFromVolume(float grainVolume, float porosity)
    {

        float expectedTotalVolume = grainVolume + porosity * grainVolume / (1 - porosity);
        //Debug.Log("Grain Volume + Porosity " + expectedTotalVolume);
        float scalingFactor = Mathf.Pow(expectedTotalVolume / GetVolume(), 1f / 3f);
        //Debug.Log("scale " + scale);
        //Debug.Log("ScalingFactor " + scalingFactor);
        scale = scale * scalingFactor;
        //Debug.Log("New Volume " + GetVolume());

        return scale;
    }
    //===================================================================


}
