using UnityEngine;
using System.Collections;

public static class DataSaver {


    public static void saveLocationData(Rock rock, string saveFolder)
    {
        int nBeds = rock.nBeds;

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Name PositionX PositionY PositionZ RotationEu1 RotationEu2 RotationEu3 ScaleX ScaleY ScaleZ Mass Density Volume SurfaceArea");
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;


        for (int bedNumber = 0; bedNumber < nBeds; bedNumber++)
        {
            GameObject currentBed = rock.rockObject.GetComponent<Transform>().GetChild(bedNumber).gameObject;
            int nGrainGroups = rock.beds[bedNumber].nGrainGroups;

            for (int grainGroupNumber = 0; grainGroupNumber < nGrainGroups; grainGroupNumber++)
            {
                GameObject grainGroup = currentBed.GetComponent<Transform>().GetChild(grainGroupNumber).gameObject;
                int nGrains = grainGroup.transform.childCount;
                if (nGrains > 0)
                {
                    for (int grainNumber = 0; grainNumber < nGrains; grainNumber++)
                    {
                        Vector3 grainPosition = grainGroup.transform.GetChild(grainNumber).gameObject.GetComponent<Transform>().localPosition;
                        Vector3 grainRatation = grainGroup.transform.GetChild(grainNumber).gameObject.GetComponent<Transform>().localEulerAngles;
                        Vector3 grainRadius = grainGroup.transform.GetChild(grainNumber).gameObject.GetComponent<Transform>().localScale;
                        float mass = grainGroup.transform.GetChild(grainNumber).gameObject.GetComponent<GrainOperations>().grainMass;
                        float density = grainGroup.transform.GetChild(grainNumber).gameObject.GetComponent<GrainOperations>().grainDensity;
                        float volume = grainGroup.transform.GetChild(grainNumber).gameObject.GetComponent<GrainOperations>().grainVolume;
                        float surfaceArea = grainGroup.transform.GetChild(grainNumber).gameObject.GetComponent<GrainOperations>().grainSurfaceArea;

                        string grainName = grainGroup.transform.GetChild(grainNumber).gameObject.GetComponent<GrainOperations>().name;
                        string LineToSave = grainName   + " " + grainPosition.x + " " + grainPosition.z + " " + grainPosition.y + " " + grainRatation.x + " " + grainRatation.z + " " + grainRatation.y + " " + grainRadius.x + " " + grainRadius.z + " " + grainRadius.y + " " + mass + " " + density + " " + volume + " " + surfaceArea;
                        csv.AppendLine(LineToSave);
                    }
                }
            }

        }

        string filePath = saveFolder + "/Results.dat";
        System.IO.File.WriteAllText(filePath, csv.ToString());
    }


    public static void saveMeshData(Rock rock, string saveFolder)
    {
        GameObject[] grains = rock.GetGrainObjects();
        if (grains.Length > 0)
        {
            string filePath = saveFolder + "/MeshData" + ".stl";
            StlExporter.MeshToFile(grains, filePath);
            Debug.Log("Saved");
        }
    }


    public static void saveSingleGrainsMesh(Rock rock, string saveFolder)
    {
        int grainCount = 0;
        int nBeds = rock.rockObject.transform.childCount;
        GameObject[] singleGrain = new GameObject[1];

        for (int bedNumber = 0; bedNumber < nBeds; bedNumber++)
        {
            GameObject currentBed = rock.rockObject.GetComponent<Transform>().GetChild(bedNumber).gameObject;
            int nGrainGroups = rock.beds[bedNumber].nGrainGroups;

            for (int grainGroupNumber = 0; grainGroupNumber < nGrainGroups; grainGroupNumber++)
            {
                GameObject grainGroup = currentBed.GetComponent<Transform>().GetChild(grainGroupNumber).gameObject;
                int nGrains = grainGroup.transform.childCount;
                if (nGrains > 0)
                {
                    for (int grainNumber = 0; grainNumber < nGrains; grainNumber++)
                    {
                        grainCount += 1;
                        singleGrain[0] = grainGroup.transform.GetChild(grainNumber).gameObject;
                        bool[] isLocationRotationScale = new bool[3];
                        isLocationRotationScale[0] = false;
                        isLocationRotationScale[1] = true;
                        isLocationRotationScale[2] = true;
                        string grainName = grainGroup.transform.GetChild(grainNumber).gameObject.GetComponent<GrainOperations>().name;
                        string filePath = saveFolder + "/Grain " + grainCount + " - " + grainName +  ".stl";
                        StlExporter.MeshToFile(singleGrain, new Vector3(), filePath, isLocationRotationScale);
                    }
                }
            }

        }

    }


}
