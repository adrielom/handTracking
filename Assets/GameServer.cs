using System;
using System.IO;
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
    public Dictionary<string, List<Point>> handStructureLeft = new Dictionary<string, List<Point>>();
    public Dictionary<string, List<Point>> handStructureRight = new Dictionary<string, List<Point>>();
    //public Dictionary<string, List<Point>> palmBase = new Dictionary<string, List<Point>>();
    protected override void OnMessage(MessageEventArgs e)
    {
        // Debug.Log(e.Data);
        // Send ("");
        // var json = JSON.Parse(e.Data);
        Debug.Log("hey");
        string s = File.ReadAllText(@"C:\Users\adriel.oliveira\Desktop\home\handTracking\Assets\exmWebSocket.json");
        var json = JSON.Parse(s);

        // if (handStructure.Count > 0) handStructure.Clear();
        Debug.Log("Inicio");
        Debug.Log($"{json.Count} function");

        GetHandFromJSON(ref handStructureLeft, "left", json);
        HandManager.leftHand.hand = handStructureLeft;

        GetHandFromJSON(ref handStructureRight, "right", json);
        HandManager.rightHand.hand = handStructureRight;

    }

    public void GetHandFromJSON(ref Dictionary<string, List<Point>> structure, string hand, JSONNode json)
    {
        PopulateHashmap(ref structure, hand, "thumb", json);
        PopulateHashmap(ref structure, hand, "indexFinger", json);
        PopulateHashmap(ref structure, hand, "middleFinger", json);
        PopulateHashmap(ref structure, hand, "ringFinger", json);
        PopulateHashmap(ref structure, hand, "pinky", json);
        PopulateHashmap(ref structure, hand, "palmBase", json);
    }

    public void PopulateHashmap(ref Dictionary<string, List<Point>> structure, string hand, string key, JSONNode json)
    {
        List<Point> points = new List<Point>();
        for (int i = 0; i < 4; i++)
        {
            try
            {
                //! This may vary based on the actual format of the json that's returned from the ML
                if (!json[hand]["annotations"].HasKey(key)) return;
                var array = json[hand]["annotations"][key][i];
                Point p = new Point((float)array[0], (float)array[1], (float)array[2]);
                points.Add(p);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        //Debug.Log($"{key} function, mão tem {points.Count} points");
        if (!structure.ContainsKey(key))
        {
            structure.Add(key, points);
        }
        else
        {
            structure[key] = points;
        }
    }
}