using UnityEngine;
using System.Collections;

//[System.Serializable]
public class Rock
{

    public Bed[] beds;
    public int nBeds;
    public int grainCountGoal;
    public int nGrainsSimulated;
    public int currentBed = 0;
    public GameObject container;
    public GameObject rockObject;

    //===================================================================
    public Rock(Bed[] beds, int grainCountGoal, string projectFolderPath, string pdfFolderName, bool useVolumeProportion)
    {
        this.beds = beds;
        nBeds = beds.Length;
        BuildSample(grainCountGoal, projectFolderPath, pdfFolderName, useVolumeProportion);

        rockObject = new GameObject();
        rockObject.name = "Rock";

        for (int i = 0; i<nBeds; i++)
        {
            beds[i].bedObject.GetComponent<Transform>().parent = rockObject.transform;
        }

    }
    //===================================================================

    //===================================================================
    public void BuildSample(int grainCountGoal, string projectFolderPath, string pdfFolderName, bool useVolumeProportion)
    {
        int nBeds = beds.Length;
        for (int bedNumber = 0; bedNumber < nBeds; bedNumber++)
        {
            beds[bedNumber].bedNumber = bedNumber;
            beds[bedNumber].BuildBed(projectFolderPath, pdfFolderName, useVolumeProportion);
        }

        if (useVolumeProportion == true) { ConvertToVolumeBedPropotion(); }

        this.setGrainCountGoal(grainCountGoal);
    }
    //===================================================================

    //===================================================================
    public float GetVolume()
    {
        float totalVolume = 0;
        for (int bedNumber = 0; bedNumber < nBeds; bedNumber++)
        {
            //Debug.Log("Bed " + bedNumber + " Volume = " + beds[bedNumber].volume + " and Normalized "  + beds[bedNumber].volume * beds[bedNumber].proportion);
            totalVolume += beds[bedNumber].volume * beds[bedNumber].proportion;
        }
        //Debug.Log(totalVolume);
        return totalVolume;

    }
    //===================================================================

    //===================================================================
    public void ConvertToVolumeBedPropotion()
    {
        float totalPDF = 0;
        float[] volumetricPdfData = new float[nBeds];
        for (int i = 0; i < nBeds; i++)
        {
            float newPDFValueUnormalized = 1 / beds[i].volume * beds[i].proportion;
            volumetricPdfData[i] = newPDFValueUnormalized;
            totalPDF += newPDFValueUnormalized;
        }

        for (int i = 0; i < nBeds; i++)
        {
            beds[i].proportion = volumetricPdfData[i] / totalPDF;
        }
    }
    //===================================================================

    //===================================================================
    public void setGrainCountGoal(int grainCountGoal)
    {
        int actualNumberOfGrains = 0;
        for (int i = 0; i < nBeds; i++)
        {
            int currentGrainCountGoal = Mathf.RoundToInt(grainCountGoal * beds[i].proportion);
            beds[i].setGrainCountGoal(currentGrainCountGoal);
            actualNumberOfGrains += beds[i].grainCountGoal;
        }
        this.grainCountGoal = actualNumberOfGrains;

    }
    //===================================================================



    //===================================================================
    public void WakeUp()
    {
        for (int i = 0; i < nBeds; i++)
        {
            if (beds[i].bedObject.activeSelf == true) beds[i].WakeUp();
        }
    }
    //===================================================================

    //===================================================================
    public GameObject CreateNextGrain()
    {
        GameObject newGrain = null;

        while (beds[currentBed].nGrainsSimulated == beds[currentBed].grainCountGoal && nBeds != currentBed + 1) { currentBed += 1; }

        if (nGrainsSimulated != grainCountGoal)
        {
            newGrain = beds[currentBed].createGrain();
            nGrainsSimulated += 1;
            return newGrain;
        }
        return newGrain;
    }
    //===================================================================


    //===================================================================
    public bool IsDeposted()
    {
        if (nGrainsSimulated == grainCountGoal)
        {
            return true;
        }
        else { return false;}
    }
    //===================================================================

    //===================================================================
    public void CementAndUnCement(bool isCemented)
    {
        for (int i = 0; i < nBeds; i++)
        {
            beds[i].CementAndUncement(isCemented);
        }
    }
    //===================================================================

    //===================================================================
    public void MoveCementedGrains(Vector3 shake, Quaternion shakeR)
    {
        for (int i = 0; i < nBeds; i++)
        {
            if (beds[i].cemented == true)
            {
                beds[i].MoveCementedGrains(shake, shakeR);
            }
        }
    }
    //===================================================================

    //===================================================================
    public void instantiateGrain()
    {
        if (nGrainsSimulated != grainCountGoal)
        {
            GameObject grainPrefab = CreateNextGrain();
            Vector3 grainLocation = container.GetComponent<ContainerOperations>().getNewLocation(grainPrefab);
            Quaternion grainRotation = Random.rotation;

            GameObject grain = (GameObject)GameObject.Instantiate(grainPrefab, grainLocation, grainRotation);
            grain.GetComponent<Renderer>().material.color = grain.GetComponent<GrainOperations>().color;
            grain.GetComponent<Rigidbody>().mass = grain.GetComponent<Rigidbody>().mass;
            int bedNumber = grain.GetComponent<GrainOperations>().bedNumber;
            int GrainNumber = grain.GetComponent<GrainOperations>().grainNumber;
            string grainName = "B" + (bedNumber+1) + "-G" + (grain.GetComponent<GrainOperations>().grainNumber+1) + "-I" + grain.GetComponent<GrainOperations>().grainIdentifier;
            grain.name = grainName;
            grain.transform.parent = rockObject.GetComponent<Transform>().GetChild(bedNumber).GetComponent<Transform>().GetChild(GrainNumber).transform;
        }
    }
    //===================================================================


    //===================================================================
    public int GetGrainObjectsNumber()
    {

        int nGrains = 0;

        for (int bedNumber = 0; bedNumber < nBeds; bedNumber++)
        {
            nGrains += beds[bedNumber].GetGrainObjectsNumber();
        }

        return nGrains;
    }
    //===================================================================

    //===================================================================
    public GameObject[] GetGrainObjects()
    {
        int nGrains = GetGrainObjectsNumber();
        GameObject[] grainObjects = new GameObject[nGrains];
        int grainCount = 0;

        for (int bedNumber = 0; bedNumber < nBeds; bedNumber++)
        {
            GameObject[] currentGrainObjects = beds[bedNumber].GetGrainObjects();
            currentGrainObjects.CopyTo(grainObjects, grainCount);
            grainCount += beds[bedNumber].GetGrainObjectsNumber();
        }
        return grainObjects;
    }
    //===================================================================


}


