using UnityEngine;
using System.Collections;
using System.Linq;
using LibNoise.Generator;
using MIConvexHull;

public class RealisticOperations : GrainOperations {

    #region Definitions
    public enum ResizeType { ByVolume, ByRadiusIn, ByRadiusOut};

    [Header("Perlin Noise")]
    public Vector2 amplitudeRange = new Vector2(.05f, .05f);
    public Vector2 frequencyRange = new Vector2(1f, 1f);
    public Vector2 lacunarityRange = new Vector2(2f, 2f);
    public Vector2 octaveCountRange = new Vector2(6f, 6f);
    public Vector2 persistenceRange = new Vector2(.5f, .5f);
    public int seed = 842842;
    public bool useRandomSeed = true;
    public ResizeType resizeType;

    //Amplitude = .05, Frequency = 1, Lacunarity = 2, OctaveCount = 6, Persistence = .5, ResizeType = ByVolume

    #endregion

    //===============================================================================================================================================
    void Start()
    {
        // Create Perlin Noise
        PerlinNoiseOptions perlinOptions = GetPerlinNoiseOptions(amplitudeRange, frequencyRange, lacunarityRange, octaveCountRange, persistenceRange, seed);


        //Debug.Log(perlinOptions.amplitude);
        //Debug.Log(perlinOptions.frequency);
        //Debug.Log(perlinOptions.lacunarity);
        //Debug.Log(perlinOptions.octaveCount);
        //Debug.Log(perlinOptions.persistence);
        //Debug.Log(perlinOptions.seed);

        if (useRandomSeed == true) {perlinOptions.seed = Mathf.RoundToInt(Random.value * 1000000); }
        Perlin noise = new Perlin(perlinOptions.frequency, perlinOptions.lacunarity, perlinOptions.persistence, perlinOptions.octaveCount, seed, perlinOptions.quality);

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] baseVertices = mesh.vertices;
        Vector3[] vertices = new Vector3[baseVertices.Length];

        // get initial Mass and Volume
        float initialVolume = MeshOperations.VolumeOfMesh(mesh, 1);
        float initialMass = gameObject.GetComponent<Rigidbody>().mass;

        var verticesCH = new Vertex[baseVertices.Length];

        // Apply Perlin Noise
        for (int i = 0; i < vertices.Length; i++)
        {
            var vertex = baseVertices[i]*2;
            float noiseValue = (float)noise.GetValue(vertex.x, vertex.y, vertex.z);
            vertex += vertex * noiseValue * perlinOptions.amplitude;
            vertices[i] = vertex;
            verticesCH[i] = new Vertex((double)vertex.x, (double)vertex.y, (double)vertex.z);
        }

        // Convex Hull
        mesh = CreateConvexMesh(vertices);
        Vector3[] vertices2 = mesh.vertices;


        // Scale Mesh to Equal Sphere Volume
        float scaleMultiplier = GetBestMultiplier(mesh, initialVolume, resizeType);
        for (int i = 0; i < vertices2.Length; i++)
        {
            var vertex = vertices2[i];
            vertex *= scaleMultiplier;
            vertices2[i] = vertex;
        }

        // Finalize Mesh
        mesh.vertices = vertices2;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Update Meshes
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        // Update Mass
        gameObject.GetComponent<Rigidbody>().ResetCenterOfMass();
        UpdateGrainProperties();
    }
    //===============================================================================================================================================
    Mesh CreateConvexMesh(System.Collections.Generic.IEnumerable<Vector3> stars)
    {
        Mesh m = new Mesh();
        m.name = "ScriptedMesh";
        System.Collections.Generic.List<int> triangles = new System.Collections.Generic.List<int>();

        var vertices = stars.Select(x => new Vertex(x)).ToList();

        var result = ConvexHull.Create(vertices);
        m.vertices = result.Points.Select(x => x.ToVec()).ToArray();
        var xxx = result.Points.ToList();

        foreach (var face in result.Faces)
        {
            triangles.Add(xxx.IndexOf(face.Vertices[0]));
            triangles.Add(xxx.IndexOf(face.Vertices[1]));
            triangles.Add(xxx.IndexOf(face.Vertices[2]));
        }

        m.triangles = triangles.ToArray();
        m.RecalculateNormals();
        return m;
    }
    //===============================================================================================================================================
    public float GetBestMultiplier(Mesh mesh, float volumeGoal, ResizeType resizeType)
    {
        MinMaxFloat minMaxRadius;
        float initialDiameter = Mathf.Pow(3f / 4f / Mathf.PI * volumeGoal, 1f / 3f) * 2;
        //Debug.Log("initial Diameter" + initialDiameter);


        switch (resizeType)
        {

            // ================================
            case ResizeType.ByVolume:
                float minMultiplier = .001f;
                float maxMultiplier = 5f;
                float middleDiff = 1f;
                float middleMultiplier = 0;

                while (middleDiff > .001)
                {
                    middleMultiplier = (minMultiplier + maxMultiplier) / 2;
                    float middleVolume = MeshOperations.VolumeOfMesh(mesh, middleMultiplier);
                    middleDiff = Mathf.Abs(middleVolume - volumeGoal) / volumeGoal;

                    if (middleVolume >= volumeGoal)
                    {
                        maxMultiplier = middleMultiplier;
                    }
                    if (middleVolume < volumeGoal)
                    {
                        minMultiplier = middleMultiplier;
                    }
                }

                return middleMultiplier;

            // ================================
            case ResizeType.ByRadiusIn:
                minMaxRadius = GetMinMaxRadius(mesh);
                return initialDiameter/(minMaxRadius.max *2);

            // ================================
            case ResizeType.ByRadiusOut:
                minMaxRadius = GetMinMaxRadius(mesh);
                return initialDiameter/(minMaxRadius.min *2);
        }

        return 1f;

    }
    //===============================================================================================================================================
    public MinMaxFloat GetMinMaxRadius(Mesh mesh)
    {
        int nVerticies = mesh.vertices.Length;

        float maximumRadius = float.MinValue;
        float minimumRadius = float.MaxValue;
        float currentMagnitude = 0;

        for (int i = 0; i <nVerticies; i++)
        {
            currentMagnitude = mesh.vertices[i].sqrMagnitude;
            if (currentMagnitude > maximumRadius) maximumRadius = currentMagnitude;
            if (currentMagnitude < minimumRadius) minimumRadius = currentMagnitude;
        }

        MinMaxFloat minMaxRadius;
        //Debug.Log(minimumRadius);
        //Debug.Log(maximumRadius);

        minMaxRadius.min = Mathf.Pow(minimumRadius, .5f);
        minMaxRadius.max = Mathf.Pow(maximumRadius, .5f);

        return minMaxRadius;
    }

    //===============================================================================================================================================
    public override void ProcessParametersString(string parametersString) {

        string[] parameters = parametersString.Split(',');

        ParameterGroup grainParameters = new ParameterGroup(parameters);
        amplitudeRange = grainParameters.GetRange("Amplitude");
        frequencyRange = grainParameters.GetRange("Frequency");

        lacunarityRange = grainParameters.GetRange("Lacunarity");
        octaveCountRange = grainParameters.GetRange("OctaveCount");
        persistenceRange = grainParameters.GetRange("Persistence");
        resizeType = grainParameters.getEnum<ResizeType>("ResizeType");

        if (parameters.Length > 6)
        {
            seed = grainParameters.getInteger("Seed");
        }
        else
        {
            seed = Mathf.RoundToInt(Random.value * 1000000);
        }

    }
    //===============================================================================================================================================
    private PerlinNoiseOptions GetPerlinNoiseOptions(Vector2 amplitudeRange, Vector2 frequencyRange, Vector2 lacunarityRange, Vector2 octaveCountRange, Vector2 persistenceRange, int seed)
    {
        PerlinNoiseOptions perlinOptions = new PerlinNoiseOptions();

        perlinOptions.amplitude = Random.Range(amplitudeRange[0], amplitudeRange[1]);
        perlinOptions.frequency = Random.Range(frequencyRange[0], frequencyRange[1]);
        perlinOptions.lacunarity = Random.Range(lacunarityRange[0], lacunarityRange[1]);
        perlinOptions.persistence = Random.Range(persistenceRange[0], persistenceRange[1]);
        perlinOptions.octaveCount = Mathf.RoundToInt(Random.Range(octaveCountRange[0], octaveCountRange[1]));
        perlinOptions.seed = seed;
        perlinOptions.quality = LibNoise.QualityMode.High;
        return perlinOptions;
    }
    //===============================================================================================================================================
    private struct PerlinNoiseOptions
    {
        public float amplitude;
        public float frequency;
        public float lacunarity;
        public float persistence;
        public int octaveCount;
        public int seed;
        public LibNoise.QualityMode quality;
    }
    //===============================================================================================================================================
    public struct MinMaxFloat
    {
        public float min;
        public float max;
    }
    //===============================================================================================================================================


}
