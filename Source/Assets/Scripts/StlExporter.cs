using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class StlExporter
{

    public static string MeshToString(GameObject[] gameObjectsToSave, Vector3 referencePoint, bool[] isLocationRotationScale)
    {

        StringBuilder sb = new StringBuilder();
        sb.Append("solid Grains" + "\n");
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;



        for (int objectNumber = 0; objectNumber < gameObjectsToSave.Length; objectNumber++)
        {

            Mesh mesh = gameObjectsToSave[objectNumber].GetComponent<MeshFilter>().mesh;

            Vector3 position;
            Vector3 scale;
            Quaternion rotation;

            if (isLocationRotationScale[0] == true)
            { position = gameObjectsToSave[objectNumber].GetComponent<Transform>().localPosition - referencePoint; }
            else {position = new Vector3(0, 0, 0);};

            if (isLocationRotationScale[1] == true) {
                rotation = gameObjectsToSave[objectNumber].GetComponent<Transform>().localRotation;
            }
            else {rotation = Quaternion.identity;}

            if (isLocationRotationScale[2] == true)
            {scale = gameObjectsToSave[objectNumber].GetComponent<Transform>().localScale;}
            else { 
                scale = new Vector3(1, 1, 1);
            }



            float tempFloat;
            tempFloat = position.z; position.z = position.y; position.y = tempFloat;


            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            int nFaces = triangles.Length / 3;

            Vector3[,] facets = new Vector3[nFaces, 3];

            Vector3[] V1 = new Vector3[nFaces];
            Vector3[] V2 = new Vector3[nFaces];
            Vector3[] normals = new Vector3[nFaces];

            for (int facesNum = 0; facesNum < nFaces; facesNum++)
            {
                int i = facesNum * 3;

                Vector3 v0 = new Vector3(vertices[triangles[i + 0]].x * scale.x, vertices[triangles[i + 0]].y * scale.y, vertices[triangles[i + 0]].z * scale.z);
                Vector3 v1 = new Vector3(vertices[triangles[i + 1]].x * scale.x, vertices[triangles[i + 1]].y * scale.y, vertices[triangles[i + 1]].z * scale.z);
                Vector3 v2 = new Vector3(vertices[triangles[i + 2]].x * scale.x, vertices[triangles[i + 2]].y * scale.y, vertices[triangles[i + 2]].z * scale.z);

                facets[facesNum, 0] = rotation * v1;
                facets[facesNum, 1] = rotation * v0;
                facets[facesNum, 2] = rotation * v2;


                float tempFloat2;
                tempFloat2 = facets[facesNum, 0].z;
                facets[facesNum, 0].z = facets[facesNum, 0].y;
                facets[facesNum, 0].y = tempFloat2;
                tempFloat2 = facets[facesNum, 1].z;
                facets[facesNum, 1].z = facets[facesNum, 1].y;
                facets[facesNum, 1].y = tempFloat2;
                tempFloat2 = facets[facesNum, 2].z;
                facets[facesNum, 2].z = facets[facesNum, 2].y;
                facets[facesNum, 2].y = tempFloat2;


                // Calculate Normals
                V1[facesNum].x = facets[facesNum, 0].y - facets[facesNum, 0].x;
                V1[facesNum].y = facets[facesNum, 1].y - facets[facesNum, 1].x;
                V1[facesNum].z = facets[facesNum, 2].y - facets[facesNum, 2].x;

                V2[facesNum].x = facets[facesNum, 0].z - facets[facesNum, 0].x;
                V2[facesNum].y = facets[facesNum, 1].z - facets[facesNum, 1].x;
                V2[facesNum].z = facets[facesNum, 2].z - facets[facesNum, 2].x;

                normals[facesNum].x = V1[facesNum].y * V2[facesNum].z - V2[facesNum].y * V1[facesNum].z;
                normals[facesNum].y = V1[facesNum].z * V2[facesNum].x - V2[facesNum].z * V1[facesNum].x;
                normals[facesNum].z = V1[facesNum].x * V2[facesNum].y - V2[facesNum].x * V1[facesNum].y;

               
                normals[facesNum] = rotation * normals[facesNum];
                normals[facesNum].Normalize();

                sb.Append("\tfacet normal " + normals[facesNum].x + " " + normals[facesNum].y + " " + normals[facesNum].z + "\n");
                sb.Append("\t\touter loop\n");
                sb.Append("\t\t\tvertex " + (facets[facesNum, 0].x + position.x) + " " + (facets[facesNum, 0].y + position.y) + " " + (facets[facesNum, 0].z + position.z) + "\n");
                sb.Append("\t\t\tvertex " + (facets[facesNum, 1].x + position.x) + " " + (facets[facesNum, 1].y + position.y) + " " + (facets[facesNum, 1].z + position.z) + "\n");
                sb.Append("\t\t\tvertex " + (facets[facesNum, 2].x + position.x) + " " + (facets[facesNum, 2].y + position.y) + " " + (facets[facesNum, 2].z + position.z) + "\n");
                sb.Append("\t\tendloop\n");
                sb.Append("\tendfacet\n");
            }


        }

        sb.Append("endsolid \n");

        return sb.ToString();


    }

    public static void MeshToFile(GameObject[] gameObjectsToSave, Vector3 referencePoint, string filename)
    {
        bool[] isLocationRotationScale = new bool[3];
        isLocationRotationScale[0] = true;
        isLocationRotationScale[1] = true;
        isLocationRotationScale[2] = true;

        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(gameObjectsToSave, referencePoint, isLocationRotationScale));
        }
    }

    public static void MeshToFile(GameObject[] gameObjectsToSave, Vector3 referencePoint, string filename, bool[] isLocationRotationScale)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(gameObjectsToSave, referencePoint, isLocationRotationScale));
        }
    }

    public static void MeshToFile(GameObject[] gameObjectsToSave, string filename)
    {
        Vector3 referencePoint = new Vector3(0, 0, 0);
        bool[] isLocationRotationScale = new bool[3];
        isLocationRotationScale[0] = true;
        isLocationRotationScale[1] = true;
        isLocationRotationScale[2] = true;

        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(gameObjectsToSave, referencePoint, isLocationRotationScale));
        }
    }


}