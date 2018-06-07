using UnityEngine;
using System.Collections;
using System.IO;

public static class FileOperations  {

    //===================================================================
    public static bool IsDirectoryEmpty(DirectoryInfo directoryInfo)
    {
        FileInfo[] files = directoryInfo.GetFiles();
        DirectoryInfo[] directories = directoryInfo.GetDirectories();

        if (files.Length == 0 && directories.Length == 0)
        {
            return true;
        }

        return false;
    }
    //===================================================================

    //===================================================================
    public static void EmptyDirectory(DirectoryInfo directoryInfo)
    {
        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
        {
            dir.Delete(true);
        }
    }
    //===================================================================

    //===================================================================
    public static string GetApplicationDirectory()
    {
        string path = Application.dataPath + "/";
        //if (Application.platform == RuntimePlatform.OSXPlayer)
        //{
        //   path += "/../../";
        //}
        //if (Application.platform == RuntimePlatform.WindowsPlayer)
        //{
        //path += "/../";
        //}

        //path = path.Replace("/", "\\");
        return path;
    }
    //===================================================================


}
