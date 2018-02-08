using UnityEngine;
using System.Collections;

[System.Serializable]
public class Bed {

    public string bedName;
    [Range(0, 1)]
    public float proportion = 0;
    public int waitAfterDepostion = 10;
    public bool cementAfterDeposition = false;
    public bool disappearAfterDeposition = false;
    public Grain[] grains;

    [System.NonSerialized]
    public int grainCountGoal;

    [System.NonSerialized]
    public int nGrainsSimulated = 0;

    [System.NonSerialized]
    public float volume;

    [System.NonSerialized]
    public int nGrainGroups;

    [System.NonSerialized]
    Distribution bedDistribution;

    [System.NonSerialized]
    public int bedNumber;
    [System.NonSerialized]
    public bool cemented = false;
    [System.NonSerialized]
    public bool appear = false;
    [System.NonSerialized]
    public GameObject bedObject;


    public Bed()
    {

    }

    //===================================================================
    public void BuildBed(string projectFolderPath, string pdfFolderName, bool useVolumeProportion)
    {
        nGrainGroups = grains.Length;
        buildGrains(projectFolderPath, pdfFolderName, useVolumeProportion);
        if (useVolumeProportion == true) { ConvertToVolumeGrainPropotion(); }
        volume = GetVolume();
        buildBedPDF();

        bedObject = new GameObject();
        bedObject.name = "Bed " + (bedNumber+1) + " - " + bedName;
        bedObject.AddComponent<Rigidbody>();
        bedObject.GetComponent<Rigidbody>().isKinematic = true;

        for (int i = 0; i < nGrainGroups; i++)
        {
            grains[i].grainObject.GetComponent<Transform>().parent = bedObject.transform;
        }
    }
    //===================================================================

    //===================================================================
    public void buildGrains(string projectFolderPath, string pdfFolderName, bool useVolumeProportion)
    {
        for (int grainGroupNumber = 0; grainGroupNumber < nGrainGroups; grainGroupNumber++)
        {
            grains[grainGroupNumber].bedNumber = bedNumber;
            grains[grainGroupNumber].grainGroupNumber = grainGroupNumber;
            grains[grainGroupNumber].BuildGrain(projectFolderPath, pdfFolderName, useVolumeProportion);
        }
    }
    //===================================================================

    //===================================================================
    public void setGrainCountGoal(int grainCountGoal)
    {
        int actualNumberOfGrains = 0;
        for (int i = 0; i < nGrainGroups; i++)
        {
            int currentGrainCountGoal = Mathf.RoundToInt(grainCountGoal * grains[i].proportion);
            grains[i].setGrainCountGoal(currentGrainCountGoal);
            actualNumberOfGrains += currentGrainCountGoal;
        }

        this.grainCountGoal = actualNumberOfGrains;

    }
    //===================================================================

    //===================================================================
    public void WakeUp()
    {
        for (int i = 0; i < nGrainGroups; i++)
        {
            if (grains[i].grainObject.activeSelf == true) grains[i].WakeUp();
        }


    }
    //===================================================================

    //===================================================================
    public float GetVolume()
    {
        float totalVolume = 0;
        for (int grainNumber = 0; grainNumber < nGrainGroups; grainNumber++)
        {
            totalVolume += grains[grainNumber].volume * grains[grainNumber].proportion;
        }

        return totalVolume;

    }
    //===================================================================

    //===================================================================
    public void ConvertToVolumeGrainPropotion()
    {
        float totalPDF = 0;
        float[] volumetricPdfData = new float[nGrainGroups];
        for (int i = 0; i < nGrainGroups; i++)
        {
            float newPDFValueUnormalized = 1 / grains[i].volume * grains[i].proportion;
            volumetricPdfData[i] = newPDFValueUnormalized;
            totalPDF += newPDFValueUnormalized;
        }

        for (int i = 0; i < nGrainGroups; i++)
        {
            grains[i].proportion = volumetricPdfData[i] / totalPDF;
        }

    }
    //===================================================================

    //===================================================================
    public void buildBedPDF()
    {
        float[,] pdfData = new float[nGrainGroups, 2];
        for (int i = 0; i < nGrainGroups; i++)
        {
            pdfData[i, 0] = i;
            pdfData[i, 1] = grains[i].proportion;
        }
        bedDistribution = new Distribution(pdfData);
    }
    //===================================================================

    //===================================================================
    public GameObject createGrain()
    {
        GameObject newGrain;
        int grainNumber = (int)bedDistribution.GetValuefromCDF(Random.value);
        //Debug.Log("Simulating Bed Number " + bedNumber + " and Grain Type " + grainNumber);
        while (grains[grainNumber].nGrainsSimulated == grains[grainNumber].grainCountGoal)
        {
            bedDistribution.removePdfValue(grainNumber);
            grainNumber = (int)bedDistribution.GetValuefromCDF(Random.value);
        }

        newGrain = grains[grainNumber].createGrain();
        nGrainsSimulated += 1;
        return newGrain;

    }
    //===================================================================

    //===================================================================
    public void CementAndUncement(bool isCemented)
    {
        int nGrains = bedObject.GetComponent<Transform>().childCount;

        for (int i = 0; i < nGrains; i++)
        {
            int nGrainsIndiv = bedObject.GetComponent<Transform>().GetChild(i).GetComponent<Transform>().childCount;
            for (int j = 0; j < nGrainsIndiv; j++)
            {
                bedObject.GetComponent<Transform>().GetChild(i).GetComponent<Transform>().GetChild(j).GetComponent<Rigidbody>().isKinematic = isCemented;
            }
        }
        cemented = isCemented;
    }
    //===================================================================

    //===================================================================
    public void AppearAndDisappear(bool isDisappear)
    {
        bedObject.SetActive(isDisappear);

        appear = isDisappear;
    }
    //===================================================================

    //===================================================================
    public bool IsDeposted()
    {
        if (nGrainsSimulated == grainCountGoal)
        {
            return true;
        }
        else { return false; }

    }
    //===================================================================

    //===================================================================
    public void MoveCementedGrains(Vector3 shake, Quaternion shakeR)
    {

       Vector3 currentPosition = bedObject.GetComponent<Transform>().position;
        bedObject.GetComponent<Rigidbody>().MovePosition(currentPosition + shake);
        bedObject.GetComponent<Rigidbody>().MoveRotation(shakeR);

    }
    //===================================================================

    //===================================================================
    public int GetGrainObjectsNumber()
    {

        int nGrains = 0;

        for (int grainGroupNumber = 0; grainGroupNumber < nGrainGroups; grainGroupNumber++) {
            nGrains += grains[grainGroupNumber].GetGrainObjectsNumber();
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

        for (int grainGroupNumber = 0  ; grainGroupNumber < nGrainGroups; grainGroupNumber++)
        {
            GameObject[] currentGrainObjects = grains[grainGroupNumber].GetGrainObjects();
            currentGrainObjects.CopyTo(grainObjects, grainCount);
            grainCount += grains[grainGroupNumber].GetGrainObjectsNumber();
        }
        return grainObjects;
    }
    //===================================================================

}