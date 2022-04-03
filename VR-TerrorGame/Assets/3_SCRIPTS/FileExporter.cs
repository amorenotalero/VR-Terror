using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileExporter : MonoBehaviour
{

    string filePath(string filename)
    {
        return Application.dataPath + "/" + filename;
    }

    void createFile(string path)
    {
        FileStream file = new FileStream(path, FileMode.Create);
    }

    //public void writeCSV(string filename, string[] content)
    public void writeCSV(string filename, List<string> content)
    {
        string path = filePath(filename);
        createFile(filename);
        StreamWriter writer = new StreamWriter(path);
        //for(int i=0;i<content.Length;i++)
        for (int i = 0; i < content.Count; i++)
        {
            writer.WriteLine(content[i]);
        }
        writer.Close();
    }


}
