using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class MidiFolderReader : MonoBehaviour
{

    public string midiFolderPath;

    void Start()
    {
        Debug.Log("Reading MIDI directory: " + midiFolderPath);
        processFolder(midiFolderPath);
    }

    private static void processFolder(string path)
    {
        Directory.GetFiles(path).Where(x => x.EndsWith("mid")).ToList()
        .ForEach(x => processFile(x));
    }

    ///<summary>
    /// Can read database etc. for each tracks scores, difficulty etc.
    ///</summary>
    private static void processFile(string path)
    {
        Debug.Log(path);
    }
}
