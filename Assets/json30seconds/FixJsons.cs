using UnityEngine;
using System.IO;

class FixJsons : MonoBehaviour
{
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        string path = "/home/adrielom/Documentos/handTracking/Assets/json30seconds";
        foreach (var file in Directory.GetFiles(path))
        {
            string fileString = File.ReadAllText(file);

            string correct = AppendCorrections(fileString, "\"gesture\":", "}");
            File.WriteAllText(file, correct);
        }
    }

    public string AppendCorrections(string strSource, string strStart, string strEnd)
    {

        string str = strSource;
        if (strSource.Contains(strStart))
        {
            int Start, End;
            Start = strSource.IndexOf(strStart, 0) + strStart.Length;
            End = strSource.IndexOf(strEnd, Start);

            string re = str.Insert(Start, "\"");
            Debug.Log(re.Length);
            Debug.Log(Start + 1);
            string fin = re.Insert(re.Length - 1, "\"");
            string f = fin.Insert(fin.Length - 1, "}");
            Debug.Log(f);
            return f;
        }
        return "";

    }
}