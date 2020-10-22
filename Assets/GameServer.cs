using System.IO;
using HandDetection;
using SimpleJSON;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class GameServer : MonoBehaviour
{

    public static GameServer instance;
    WebSocketServer m_server;
    int frame = 0;
    JSONNode json;
    void Start()
    {
        if (instance == null)
            instance = this;

        m_server = new WebSocketServer(4649);
        m_server.AddWebSocketService<Echo>("/Echo");
        m_server.Start();
        string s = File.ReadAllText(@"C:\Users\adriel.oliveira\Desktop\home\handTracking\Assets\data.json");
        json = JSON.Parse(s);
        Debug.Log(json["bboxes"][25]["hand1"]["gesture"]);
    }

    void Update()
    {
        // GestureWatcher.SetGesture(HandManager.hand1, ExtensionMethods.ParseEnum<Gesture>(json["bboxes"][frame]["hand1"]["gesture"]), () => { });
        // GestureWatcher.SetGesture(HandManager.hand2, ExtensionMethods.ParseEnum<Gesture>(json["bboxes"][frame]["hand2"]["gesture"]), () => { });
        if (frame <= 803)
        {
            HandTracking.StartHandTracking(json);
            frame++;
            HandTracking.SetFrame(frame);
        }
        else
        {
            HandTracking.ResetFrame();
            frame = 0;
        }

    }

    void OnApplicationQuit()
    {
        m_server.Stop();
    }
}

public class Echo : WebSocketBehavior
{
    //public Dictionary<string, List<Point>> palmBase = new Dictionary<string, List<Point>>();
    protected override void OnMessage(MessageEventArgs e)
    {
        //!Retrieving data from the websocket
        // Debug.Log(e.Data);
        // Send ("");
        // var json = JSON.Parse(e.Data);
        Debug.Log("hey");
        string s = File.ReadAllText(@"C:\Users\adriel.oliveira\Desktop\home\handTracking\Assets\data.json");
        var json = JSON.Parse(s);

        // if (handStructure.Count > 0) handStructure.Clear();
        Debug.Log("Inicio");
        Debug.Log($"{json.Count} function");

        HandTracking.StartHandTracking(json);

    }

}