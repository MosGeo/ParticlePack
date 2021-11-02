using UnityEngine;
using System.Collections;
using System.Globalization;

public class ParameterGroup
{

    public string[] parameterName;
    public string[] parameterValue;

    //===================================================================
    public ParameterGroup(string[] textData)
    {
        AnalyzeParameters(textData);
    }
    //===================================================================

    //===================================================================
    public ParameterGroup(string fileLocation)
    {
        // Load Text File
        string rawData = System.IO.File.ReadAllText(fileLocation);
        string[] textData = rawData.Split('\n');

        AnalyzeParameters(textData);

    }
    //===================================================================

    //===================================================================
    private void AnalyzeParameters(string[] textData)
    {
        int nProperty = 0;
        string[] parameterNameAll = new string[textData.Length];
        string[] parameterValueAll = new string[textData.Length];

        for (int i = 0; i < textData.Length; i++)
        {
            if (textData[i].Trim().Length > 0)
            {
                if (textData[i][0] != '-')
                {
                    //string[] propertyAndValue = textData[i].Split('=');
                    //parameterNameAll[nProperty] = propertyAndValue[0].Trim();
                    //parameterValueAll[nProperty] = propertyAndValue[1].Trim();
                    int stringLength = textData[i].Length;
                    int indexEqualSign = textData[i].IndexOf('=');
                    parameterNameAll[nProperty] = textData[i].Substring(0, indexEqualSign).Trim();
                    if (stringLength > indexEqualSign)
                    {
                        parameterValueAll[nProperty] = textData[i].Substring(indexEqualSign + 1, textData[i].Length - indexEqualSign-1).Trim();
                    }
                    else
                    {
                        parameterValueAll[nProperty] = "";
                    }

                    nProperty += 1;
                }
            }
        }

        parameterName = new string[nProperty];
        parameterValue = new string[nProperty];

        for (int i = 0; i < nProperty; i++)
        {
            parameterName[i] = parameterNameAll[i];
            parameterValue[i] = parameterValueAll[i];
        }

    }
    //===================================================================

    //===================================================================
    public string getString(string parameterNameString) {
        int results = System.Array.FindIndex(parameterName, s => s.Equals(parameterNameString));
        return (parameterValue[results]);
    }
    //===================================================================

    //===================================================================
    public float getFloat(string parameterNameString) {
        int results = System.Array.FindIndex(parameterName, s => s.Equals(parameterNameString));
        return (float.Parse(parameterValue[results], CultureInfo.InvariantCulture));
    }
    //===================================================================

    //===================================================================
    public int getInteger(string parameterNameString) {
        int results = System.Array.FindIndex(parameterName, s => s.Equals(parameterNameString));
        return (int.Parse(parameterValue[results], CultureInfo.InvariantCulture));
    }
    //===================================================================

    //===================================================================
    public bool getBoolean(string parameterNameString) {
        //Debug.Log(parameterNameString);
        int results = System.Array.FindIndex(parameterName, s => s.Equals(parameterNameString));
        return (bool.Parse(parameterValue[results]));
    }
    //===================================================================

    //===================================================================
    public Vector3 getVector3(string parameterNameString)
    {

        int results = System.Array.FindIndex(parameterName, s => s.Equals(parameterNameString));
        string[] resultsString = parameterValue[results].Split(' ');
        resultsString[0] = resultsString[0].Remove(0, 1);
        resultsString[2] = resultsString[2].Remove(resultsString[2].Length - 1);
        Vector3 resultValue = new Vector3(float.Parse(resultsString[0]), float.Parse(resultsString[1]), float.Parse(resultsString[2]));
        return resultValue;
    }
    //===================================================================

    //===================================================================
    public Vector2 getVector2(string parameterNameString)
    {

        int results = System.Array.FindIndex(parameterName, s => s.Equals(parameterNameString));
        string[] resultsString = parameterValue[results].Split(' ');
        resultsString[0] = resultsString[0].Remove(0, 1);
        resultsString[1] = resultsString[1].Remove(resultsString[1].Length - 1);
        Vector2 resultValue = new Vector2(float.Parse(resultsString[0], CultureInfo.InvariantCulture), float.Parse(resultsString[1], CultureInfo.InvariantCulture));
        return resultValue;
    }

    //===================================================================
    public Type getEnum<Type>(string parameterNameString){
        string resultsString = getString(parameterNameString);
        return (Type)System.Enum.Parse(typeof(Type), resultsString, true);
    }
    //===================================================================

    //===================================================================
    public Color getColor(string parameterNameString)
    {
        int results = System.Array.FindIndex(parameterName, s => s.Equals(parameterNameString));
        string[] resultsString = parameterValue[results].Split(' ');
        resultsString[0] = resultsString[0].Remove(0, 1);
        resultsString[3] = resultsString[3].Remove(resultsString[3].Length - 1);

        float r = float.Parse(resultsString[0], CultureInfo.InvariantCulture) / 255f;

        float g = float.Parse(resultsString[1], CultureInfo.InvariantCulture) / 255f;

        float b = float.Parse(resultsString[2], CultureInfo.InvariantCulture) / 255f;

        float a = float.Parse(resultsString[3], CultureInfo.InvariantCulture) / 255f;

        Color color = new Color(r,g,b,a);
        return color;
    }
    //===================================================================

    //===================================================================
    public Vector2 GetRange(string parameterNameString)
    {
        int results = System.Array.FindIndex(parameterName, s => s.Equals(parameterNameString));
        string[] resultsString = parameterValue[results].Split(' ');

        Vector2 resultValue;

        if (resultsString.Length>1)
        {
            resultsString[0] = resultsString[0].Remove(0, 1);
            resultsString[1] = resultsString[1].Remove(resultsString[1].Length - 1);
            resultValue = new Vector3(float.Parse(resultsString[0], CultureInfo.InvariantCulture), float.Parse(resultsString[1], CultureInfo.InvariantCulture));
        }
        else
        {
            resultValue = new Vector3(float.Parse(resultsString[0], CultureInfo.InvariantCulture), float.Parse(resultsString[0], CultureInfo.InvariantCulture));
        }

        return resultValue;
    }
    //===================================================================




}