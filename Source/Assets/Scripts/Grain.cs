using UnityEngine;
using System.Collections;

[System.Serializable]
public class Grain
{
    //public enum DepostionType { Random, FiningUpward, CoarseningUpward, LaminatedFining, LaminatedCoarsening };
    public enum DepostionType { Random, FiningUpward, CoarseningUpward};
    public enum GrainType {Sphere, Realistic}
    public enum GrainColorType {Base, Secondary, Size, Random}

    [Header("Distribution")]
    public string grainName;
    public GrainType Type = GrainType.Sphere;
    public string parameters ="";
    public string PDF;
    public float PDFMultiplier = 1;
    public float PDFOffset = 0;
    public float density = 2650;
    public bool exactVerticalCreation = false;
    public bool disappearAtBottom = false;
    [Range(0, 1)]
    public float proportion;
    public DepostionType depostionType;
    public Vector3 scale = new Vector3(1,1,1);


    [Header("Physics")]
    [Range(0, 1)]
    public float dynamicFriction = 0.6f;
    [Range(0, 1)]
    public float staticFriction = 0.6f;
    [Range(0, 1)]
    public float bounciness = 0f;
    public PhysicMaterialCombine frictionCombine = PhysicMaterialCombine.Average;
    public PhysicMaterialCombine bounceCombine = PhysicMaterialCombine.Average;

    [Header("Visulization")]
    public GrainColorType colorType = GrainColorType.Base;
    public Color baseColor;
    public Color secondaryColor;


    [Header("Tracking")]
    //[System.NonSerialized]
    public int grainCountGoal;
    //[System.NonSerialized]
    public int nGrainsSimulated = 0;

    [System.NonSerialized]
    public Distribution distribution;
    [System.NonSerialized]
    public GameObject grain;

    [System.NonSerialized]
    public float volume;

    [System.NonSerialized]
    public int bedNumber;
    [System.NonSerialized]
    public int grainGroupNumber;
    [System.NonSerialized]
    public GameObject grainObject;

    //===================================================================
    public void BuildGrain(string projectFolderPath, string pdfFolderName, bool useVolumeProportion)
    {
        string pdfFileName = projectFolderPath + "/" + pdfFolderName + "/" + PDF + ".txt";
        string grainFileName = "Grains/" + Type.ToString();
        distribution = new Distribution(pdfFileName, useVolumeProportion, PDFMultiplier, PDFOffset);
        grain = Resources.Load<GameObject>(grainFileName);
        grain.transform.localScale = new Vector3(1, 1, 1);
        if (Type == GrainType.Sphere) {
            if (scale == new Vector3(1, 1, 1))
            {
                grain.GetComponent<SphereCollider>().enabled = true;
                grain.GetComponent<MeshCollider>().enabled = false;
            }
            else {
                grain.GetComponent<SphereCollider>().enabled = false;
                grain.GetComponent<MeshCollider>().enabled = true;
            }
        }
        volume = GetVolume();
        grainObject = new GameObject();
        grainObject.name = "Grain " + (grainGroupNumber+1) + " - " + grainName;
    }
    //===================================================================

    //===================================================================
    public void setGrainCountGoal(int grainCountGoal)
    {
        this.grainCountGoal = grainCountGoal;
    }
    //===================================================================

    //===================================================================
    // Calculate the volume of the distribution
    public float GetVolume()
    {
        float volume = distribution.GetVolume();
        return volume;
    }
    //===================================================================

    //===================================================================
    // Get scale value based on depostion type
    private float GetScaleValueSample()
    {
        float scaleValue;
        float randomValue;
        switch (depostionType)
        {
            case DepostionType.Random:
                randomValue = Random.value;
                scaleValue = distribution.GetValuefromCDF(randomValue);
                break;
            case DepostionType.CoarseningUpward:
                randomValue = (nGrainsSimulated+.000001f) / grainCountGoal;
                scaleValue = distribution.GetValuefromCDF(randomValue);
                break;
            case DepostionType.FiningUpward:
                randomValue = 1.000001f - ((float)nGrainsSimulated / (float) grainCountGoal);
                scaleValue = distribution.GetValuefromCDF(randomValue);
                break;
            default:
                scaleValue = distribution.GetValuefromCDF(Random.value);
                break;
        }
        return scaleValue;
    }
    //===================================================================

    //===================================================================
    // Get grain color
    private Color GetGrainColor(float grainScale)
    {
        Color colorUsed;
        switch (colorType)
        {
            case GrainColorType.Base:
                colorUsed = baseColor;
                break;
            case GrainColorType.Secondary:
                colorUsed = secondaryColor;
                break;
            case GrainColorType.Random:
                colorUsed = new Vector4(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(.5f, 1.0f));
                break;
            case GrainColorType.Size:
                colorUsed = GetColorFromGraident(grainScale, distribution.minimumValue, distribution.maximumValue, baseColor, secondaryColor);
                break;
            default:
                colorUsed = baseColor;
                break;
        }
        return colorUsed;
    }
    //===================================================================

    public Color GetColorFromGraident(float value, float minValue, float maxValue, Color baseColor, Color secondaryColor)
    {
        float normalizedValue = (value - minValue) / (maxValue - minValue);
        Gradient g;
        GradientColorKey[] gck;
        GradientAlphaKey[] gak;
        g = new Gradient();
        gck = new GradientColorKey[2];
        gck[0].color = secondaryColor;
        gck[0].time = 0.0F;
        gck[1].color = baseColor;
        gck[1].time = 1.0F;
        gak = new GradientAlphaKey[2];
        gak[0].alpha = 1.0F;
        gak[0].time = 0.0F;
        gak[1].alpha = 0.0F;
        gak[1].time = 1.0F;
        g.SetKeys(gck, gak);
        Color usedColor = g.Evaluate(normalizedValue);

        return usedColor;

    }

    //===================================================================
    public GameObject createGrain()
    {
        GameObject newGrain = grain;
        PhysicMaterial physicsMaterial = buildPhysicsMaterial(dynamicFriction,  staticFriction,  bounciness,  frictionCombine,  bounceCombine);
        nGrainsSimulated += 1;
        float scaleValue = GetScaleValueSample();
        newGrain.GetComponent<Transform>().localScale = scale;
        newGrain.GetComponent<Transform>().localScale = Vector3.Scale(new Vector3(scaleValue, scaleValue, scaleValue), scale);
        //newGrain.GetComponent<Transform>().localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        newGrain.GetComponent<Collider>().material = physicsMaterial;
        newGrain.GetComponent<GrainOperations>().grainIdentifier = nGrainsSimulated;
        newGrain.GetComponent<GrainOperations>().grainNumber = grainGroupNumber;
        newGrain.GetComponent<GrainOperations>().bedNumber = bedNumber;
        newGrain.GetComponent<GrainOperations>().color = GetGrainColor(scaleValue);
        newGrain.GetComponent<GrainOperations>().scale = scaleValue;
        newGrain.GetComponent<GrainOperations>().exactVerticalCreation = exactVerticalCreation;
        newGrain.GetComponent<GrainOperations>().ProcessParametersString(parameters);
        newGrain.GetComponent<GrainOperations>().grainDensity = density;
        newGrain.GetComponent<GrainOperations>().disappearAtBottom = disappearAtBottom;

        newGrain.GetComponent<Rigidbody>().SetDensity(density);
        newGrain.tag = "Grain";

        return newGrain;
    }
    //===================================================================


    //===================================================================
    public int GetGrainObjectsNumber()
    {
        int nGrains = grainObject.GetComponent<Transform>().childCount;
        return nGrains;
    }
    //===================================================================

    //===================================================================
    public GameObject[] GetGrainObjects()
    {
        int nGrains = GetGrainObjectsNumber();
        GameObject[] grainObjects = new GameObject[nGrains];

        for (int grainNumber = 0; grainNumber < nGrains; grainNumber++)
        {
            grainObjects[grainNumber] = grainObject.GetComponent<Transform>().GetChild(grainNumber).gameObject;
        }
        return grainObjects;
    }
    //===================================================================

    //===================================================================
    public void WakeUp()
    {
        int nGrains = GetGrainObjectsNumber();
        for (int grainNumber = 0; grainNumber < nGrains; grainNumber++)
        {
            grainObject.GetComponent<Transform>().GetChild(grainNumber).GetComponent<Rigidbody>().WakeUp();
        }
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





}
