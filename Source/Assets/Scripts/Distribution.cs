using System.Collections;
using UnityEngine;
using System.Globalization;

public class Distribution{

    //===================================================================
    // Variables
    public float[,] pdfData;
    public float[,] cdfData;
    public bool isInterpolateCDF = false;
    public float maximumValue;
    public float minimumValue;
    //===================================================================

    //===================================================================
    // Initialize the distrbution
    public Distribution(string fileName, bool useVolumeProportion, float PDFMultiplier, float PDFOffset)
    {
        pdfData = loadData(fileName);
        normalizePDF();
        if (useVolumeProportion == true) {pdfData = convertPdfToVolumetric();}
        if (PDFMultiplier != 1) { pdfData = MultiplyLength(pdfData, PDFMultiplier); }
        if (PDFOffset != 0) { pdfData = AddToLength(pdfData, PDFOffset); }
        cdfData = Pdf2Cdf();
        GetMaxMinValue(out minimumValue, out maximumValue);
        //printPDForCDF(cdfData);

    }
    public Distribution(float[,] pdfData)
    {
        this.pdfData = pdfData;
        normalizePDF();
        //printPDForCDF(pdfData);
        cdfData = Pdf2Cdf();
        GetMaxMinValue(out minimumValue, out maximumValue);
    }
    //===================================================================

    //===================================================================
    // Multiplly Length by a number
    public float[,] MultiplyLength(float[,] table, float PDFMultiplier)
    {
        float[,] resultsTable = pdfData;
        int nDataPoints = pdfData.GetLength(0);

        for (int i = 0; i < nDataPoints; i++)
        {
            resultsTable[i, 0] = pdfData[i, 0] * PDFMultiplier;
        }

        return resultsTable;
    }
    //===================================================================

    //===================================================================
    // Add to length
    public float[,] AddToLength(float[,] table, float PDFOffset)
    {
        float[,] resultsTable = pdfData;
        int nDataPoints = pdfData.GetLength(0);

        for (int i = 0; i < nDataPoints; i++)
        {
            resultsTable[i, 0] = pdfData[i, 0] + PDFOffset;
        }

        return resultsTable;
    }
    //===================================================================

    //===================================================================
    // Load PDF Data from a file
    public float[,] loadData(string fileName)
    {
        // Load Text File
        string rawTextData = System.IO.File.ReadAllText(fileName);
        string[] textData = rawTextData.Split('\n');

        float[,] readData = new float[textData.Length, 2];


        for (int i = 0; i <= textData.Length - 1; i++)
        {
            if (string.Equals(textData[i], "") == false)
            {
                if (textData[i][0] != '-')
                {
                    string[] splitText = textData[i].Split(',');
                    readData[i, 0] = float.Parse(splitText[0].Trim(), CultureInfo.InvariantCulture);
                    readData[i, 1] = float.Parse(splitText[1].Trim(), CultureInfo.InvariantCulture);
                }
            }
        }
        return readData;
    }
    //===================================================================

    //===================================================================
    // Retruns the maximum and minimum Value
    private void GetMaxMinValue(out float minValue, out float maxValue)
    {
        int nDataPoint = pdfData.GetLength(0);

        maxValue = float.MinValue;
        minValue = float.MaxValue;


        for (int i = 1; i <= nDataPoint - 1; i++)
        {
            if (pdfData[i, 0] > maxValue & pdfData[i, 1] > 0)
            {
                maxValue = pdfData[i, 0];
            }

            if (pdfData[i, 0] < minValue & pdfData[i, 1] > 0)
            {
                minValue = pdfData[i, 0];
            }
        }

    }
    //===================================================================

    //===================================================================
    // Convert PDF to volumetric PDF (Not based on Count)
    private float[,] convertPdfToVolumetric()
    {
        float[,] volumetricPdfData = pdfData;
        int nDataPoints = pdfData.GetLength(0);

        float totalPDF = 0;
        for(int i = 0; i< nDataPoints; i++)
        {
            float newPDFValueUnormalized = 1 / Mathf.Pow(pdfData[i, 0], 3) * pdfData[i, 1];
            volumetricPdfData[i, 1] = newPDFValueUnormalized;
            totalPDF += newPDFValueUnormalized;
        }

        for (int i = 0; i < nDataPoints; i++)
        {
            volumetricPdfData[i, 1] = volumetricPdfData[i, 1] / totalPDF;
        }

        return volumetricPdfData;
    }
    //===================================================================

    //===================================================================
    // Convert PDF volume

    public delegate float VolumeCalculator(float d, float prop);

    public float GetVolume(Grain.GrainType grainType=Grain.GrainType.Sphere)
    {

        int nDataPoints = pdfData.GetLength(0);

        VolumeCalculator vc;
        if (grainType == Grain.GrainType.Cube || grainType== Grain.GrainType.DeformedCube)
        {
            vc = delegate (float d, float prop)
            { return Mathf.Pow(d, 3) * prop;};
        }
        else
        {
            vc = delegate (float d, float prop)
            {return 4.0f / 3.0f * Mathf.PI * Mathf.Pow(d/2, 3) * prop; };
        }


        float totalVolume = 0;
        for (int i = 0; i < nDataPoints; i++)
        {
            float currentVolume = vc(pdfData[i, 0], pdfData[i, 1]);
            totalVolume += currentVolume;
        }
        return totalVolume;
    }
    //===================================================================

    //===================================================================
    // Calculates the CDF from a PDF
    private float[,] Pdf2Cdf()
    {
        int nDataPoint = pdfData.GetLength(0);
        float[,] cdfResults = new float[nDataPoint, 2];

        cdfResults[0, 0] = pdfData[0, 0];
        cdfResults[0, 1] = pdfData[0, 1];

        for (int i = 1; i <= nDataPoint - 1; i++)
        {
            cdfResults[i, 0] = pdfData[i, 0];
            cdfResults[i, 1] = cdfResults[i - 1, 1] + pdfData[i, 1];
        }

        return cdfResults;
    }
    //===================================================================

    //===================================================================
    // Sample value from CDF
    public float GetValuefromCDF(float prop)
    {
        return GetValuefromCDF(prop, false);
    }
    public float GetValuefromCDF(float prop, bool isInterpolateCDF)
    {

        float value = 0;
        int nDataPoint = cdfData.GetLength(0);


        // Discrete CDF Sampling
        if (isInterpolateCDF == false)
        {
            for (int i = 0; i <= nDataPoint - 1; i++)
            {
                if (cdfData[i, 1] - prop >= 0)
                {
                    value = cdfData[i, 0];
                    break;
                }

            }

        }

        // Continueous CDF Sampling
        if (isInterpolateCDF == true)
        {
            for (int i = 0; i <= nDataPoint - 1; i++)
            {
                if (cdfData[i, 1] - prop >= 0)
                {
                    value = cdfData[i - 1, 0] + (cdfData[i, 0] - cdfData[i - 1, 0]) * (prop - cdfData[i - 1, 1]) / (cdfData[i, 1] - cdfData[i - 1, 1]);
                    break;
                }

            }

        }

        return value;
    }
    //===================================================================

    //===================================================================
    // Remove Value and Renormalize
    public void removePdfValue(int indexToRemove)
    {

        int nDataPoints = pdfData.GetLength(0);
        float[,] newPdfData = new float[nDataPoints - 1, 2];

        int newIndex = 0;
        float totalPDF = 0;
        for (int i = 0; i < nDataPoints; i++)
        {
            if (i != indexToRemove) {
                newPdfData[newIndex, 0] = pdfData[i, 0];
                newPdfData[newIndex, 1] = pdfData[i, 1];
                totalPDF += pdfData[i, 1];
                newIndex += 1;
            }
        }

        for (int i = 0; i < nDataPoints-1; i++)
        {
            //newPdfData[i, 0] = newPdfData[i, 0];
            newPdfData[i, 1] = newPdfData[i, 1]/totalPDF;
        }


        pdfData = newPdfData;

        if (nDataPoints != 0){
            cdfData = Pdf2Cdf();
            GetMaxMinValue(out minimumValue, out maximumValue);
        }

    }
    //===================================================================

    // Normalize PDF
    public void normalizePDF()
    {

        int nDataPoints = pdfData.GetLength(0);
        float[,] newPdfData = new float[nDataPoints, 2];

        float totalPDF = 0;
        for (int i = 0; i < nDataPoints; i++)
        {
                newPdfData[i, 0] = pdfData[i, 0];
                newPdfData[i, 1] = pdfData[i, 1];
                totalPDF += pdfData[i, 1];
        }

        for (int i = 0; i < nDataPoints - 1; i++)
        {
            newPdfData[i, 1] = newPdfData[i, 1] / totalPDF;
        }

        pdfData = newPdfData;
    }

    //===================================================================
    // Print Table
    public void printPDForCDF(float[,] table)
    {
        int nDataPoints = table.GetLength(0);

        for (int i = 0; i < nDataPoints; i++)
        {
            Debug.Log("Line " + i + ": " + table[i, 0] + ", " + table[i, 1]);
        }

    }
    //===================================================================


}
