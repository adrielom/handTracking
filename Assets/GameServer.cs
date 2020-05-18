using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HandDetection;
using SimpleJSON;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class GameServer : MonoBehaviour
{

    public static GameServer instance;
    WebSocketServer m_server;
    void Start()
    {
        if (instance == null)
            instance = this;

        m_server = new WebSocketServer(4649);
        m_server.AddWebSocketService<Echo>("/Echo");
        m_server.Start();
        // Console.ReadKey (true);
    }

    void OnApplicationQuit()
    {
        m_server.Stop();
    }
}

public class Echo : WebSocketBehavior
{
    public Dictionary<string, List<Point>> handStructure = new Dictionary<string, List<Point>>();
    //public Dictionary<string, List<Point>> palmBase = new Dictionary<string, List<Point>>();
    protected override void OnMessage(MessageEventArgs e)
    {
        // Debug.Log(e.Data);
        // Send ("");
        var json = JSON.Parse(e.Data);
        // if (handStructure.Count > 0) handStructure.Clear();
        Debug.Log("Inicio");
        Debug.Log($"{json.Count} function");
        PopulateHashmap(ref handStructure, "thumb", json);
        PopulateHashmap(ref handStructure, "indexFinger", json);
        PopulateHashmap(ref handStructure, "middleFinger", json);
        PopulateHashmap(ref handStructure, "ringFinger", json);
        PopulateHashmap(ref handStructure, "pinky", json);
        PopulateHashmap(ref handStructure, "palmBase", json);
        //PopulaPalm(ref handStructure, "palmBase", json);
        HandManager.hand = handStructure;
       // HandManager.palm = handStructure;
        
        //Debug.Log("Testando : " + json["landmarks"].Count);
    }
    

    public void PopulateHashmap(ref Dictionary<string, List<Point>> structure, string key, JSONNode json)
    {
        List<Point> points = new List<Point>();
        for (int i = 0; i < 4; i++)
        {
            try
            {
                //Debug.Log($"Json tem {json["annotations"].HasKey(key)}");
                if (!json["annotations"].HasKey(key)) return;
                var array = json["annotations"][key][i];
                Point p = new Point((float)array[0], (float)array[1], (float)array[2]);
                points.Add(p);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //Debug.Log($"{key} function, mão tem {points.Count} points");
        if(!structure.ContainsKey(key)){
            structure.Add(key, points);
        }
        else{
            structure[key] = points;
        }
    }
}