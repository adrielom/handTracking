using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace HandDetection
{

    public static class ExtensionMethods
    {

        public static T ParseEnum<T>(this string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    return (T)Enum.Parse(typeof(T), "NONE", true);
                else
                    return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (Exception e)
            {
                // Debug.Log($"VALUE = {value} - {e.Message}");
                return default(T);
            }
        }
    }

    public enum Gesture { NONE, FIST, OK, STOP, SWAP, TWEEZERS }

    public class Hand
    {

        public string name;
        public Dictionary<string, List<Point>> hand = new Dictionary<string, List<Point>>();
        public Dictionary<string, List<Point>> palm = new Dictionary<string, List<Point>>();
        public List<GameObject> fingers = new List<GameObject>();
        public List<GameObject> ListOfJoints = new List<GameObject>();
        public Color color;
        Gesture gesture = Gesture.NONE;
        Gesture memoizedGesture = Gesture.NONE;

        public void setHandGesture(Gesture gesture)
        {
            if (gesture != memoizedGesture) {
                this.gesture = gesture;
            }
        }

        public Gesture getHandGesture()
        {
            return gesture;
        }

        public Hand(string name, List<GameObject> fingers, List<GameObject> listOfJoints)
        {
            name = this.name;
            fingers = this.fingers;
            listOfJoints = this.ListOfJoints;
        }
        public Hand(string name, List<GameObject> fingers, List<GameObject> listOfJoints, Color color)
        {
            name = this.name;
            fingers = this.fingers;
            listOfJoints = this.ListOfJoints;
            color = this.color;
        }

        public void AddCollisionToHand()
        {
            for (int i = 0; i < fingers.Count; i++)
            {
                if (fingers[i].GetComponent<HandPhysics>() == null)
                    fingers[i].AddComponent<HandPhysics>();
            }
        }
    }

    /// <summary>
    /// This class is a wrapper class to the Point List
    /// </summary>
    [Serializable]
    public class PointsWrapper
    {
        public List<Point> points;
    }

    /// <summary>
    /// This class is a Point Model
    /// </summary>
    [Serializable]
    public class Point
    {

        public Point() { }
        public Point(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public int id;
        public double x, y, z;

        [SerializeField]
        /// <summary>
        /// Linked List with the next point to be drawn
        /// </summary>
        /// <typeparam name="Point">This class' instance </typeparam>
        /// <returns></returns>
        public LinkedList<Point> nextPoints = new LinkedList<Point>();

        /// <summary>
        /// Method that returns the position vector of said instance
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public Vector3 GetPositionVector(float ratio = 1)
        {
            return new Vector3((float)x * ratio, (float)y * ratio, (float)z);
        }
    }

    public static class HandTracking
    {
        public static Dictionary<string, List<Point>> handStructureLeft = new Dictionary<string, List<Point>>();
        public static Dictionary<string, List<Point>> handStructureRight = new Dictionary<string, List<Point>>();
        public static int frame = 0;

        public static void SetFrame(int _frame)
        {
            frame = _frame;
        }

        public static int GetFrame()
        {
            return frame;
        }

        public static void ResetFrame()
        {
            frame = 0;
        }

        public static void StartHandTracking(JSONNode json)
        {
            handStructureLeft.Clear();
            handStructureRight.Clear();

            GetHandFromJSON(ref handStructureLeft, "hand1", json);
            HandManager.hand1.hand = handStructureLeft;
            if (ExtensionMethods.ParseEnum<Gesture>(json["bboxes"][frame]["hand1"]["gesture"]) != Gesture.NONE)
                Debug.Log("different");
            HandManager.hand1.setHandGesture(ExtensionMethods.ParseEnum<Gesture>(json["bboxes"][frame]["hand1"]["gesture"]));

            GetHandFromJSON(ref handStructureRight, "hand2", json);
            HandManager.hand2.hand = handStructureRight;
            HandManager.hand2.setHandGesture(ExtensionMethods.ParseEnum<Gesture>(json["bboxes"][frame]["hand2"]["gesture"]));
        }

        public static void GetHandFromJSON(ref Dictionary<string, List<Point>> structure, string hand, JSONNode json)
        {
            PopulateHashmap(ref structure, hand, "thumb", json);
            PopulateHashmap(ref structure, hand, "indexFinger", json);
            PopulateHashmap(ref structure, hand, "middleFinger", json);
            PopulateHashmap(ref structure, hand, "ringFinger", json);
            PopulateHashmap(ref structure, hand, "pinky", json);
            PopulateHashmap(ref structure, hand, "palmBase", json);
        }

        public static void PopulateHashmap(ref Dictionary<string, List<Point>> structure, string hand, string key, JSONNode json)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    //! This may vary based on the actual format of the json that's returned from the ML
                    if (!json["bboxes"][frame][hand]["annotations"].HasKey(key)) return;
                    var array = json["bboxes"][frame][hand]["annotations"][key][i];
                    Point p = new Point((float)array[0], (float)array[1], (float)array[2]);
                    points.Add(p);
                }
                catch (Exception e)
                {
                    // Debug.Log(e.Message);
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

    public class HandManager : MonoBehaviour
    {
        public GameObject spherePrefab, nodesContainerLeft, nodesContainerRight;
        public List<GameObject> fingersLeft, fingersRight;

        List<GameObject> ListOfJoints = new List<GameObject>();
        List<GameObject> leftJoints = new List<GameObject>(), rightJoints = new List<GameObject>();
        public Material materialLeft, materialRight;
        [Range(1, 10)]
        public float lineWidth = 1;
        public List<HandPhysics> handPhysicsList = new List<HandPhysics>();
        public float ratio = 100;
        public static Hand hand1;
        public static Hand hand2;
        public static HandManager instance;

        private Camera cam;

        void Awake()
        {
            cam = Camera.main;
            if (instance == null)
                instance = this;
            // string json = File.ReadAllText(jsonPath).ToString();
            // var pointsWrapper = JsonUtility.FromJson<PointsWrapper>(json);
            // listOfPoints = pointsWrapper.points.ToList();
            // Debug.Log(listOfPoints.Count);
            // InstantiatePoint();
            hand1 = new Hand("left", fingersLeft, leftJoints, Color.red);
            hand2 = new Hand("right", fingersRight, rightJoints, Color.yellow);
        }

        void InstantiatePoint(Hand hand)
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject g = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
                g.transform.SetParent(hand.name == "left" ? nodesContainerLeft.transform : nodesContainerRight.transform);
                g.transform.localScale = g.transform.parent.localScale.x * Vector3.one * 5;
                hand.ListOfJoints.Add(g);
            }
        }

        public void RepositionPoints(Hand hand)
        {
            var fingers = hand.fingers;
            List<Vector3> positions = new List<Vector3>();

            fingers.ForEach(f =>
            {
                LineRenderer lR = f.GetComponent<LineRenderer>();
                lR.widthMultiplier = 5;
                for (int i = 0; i < lR.positionCount - 1; i++)
                {
                    positions.Add(lR.GetPosition(i));
                }
            });

            for (int i = 0; i < positions.Count - 1; i++)
            {
                hand.ListOfJoints[i].transform.localPosition = positions[i];
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {

            if (hand1.hand.Count != 0)
            {
                if (hand1.name == null) hand1.name = "left";
                if (hand1.fingers.Count == 0) hand1.fingers = fingersLeft;
                DrawHand(hand1);
            }
            if (hand2.hand.Count != 0)
            {
                if (hand2.name == null) hand2.name = "right";
                if (hand2.fingers.Count == 0) hand2.fingers = fingersRight;
                DrawHand(hand2);
            }
            GestureWatcher.SetGesture(hand1, hand1.getHandGesture(), () => Debug.Log("hand1"));
            GestureWatcher.SetGesture(hand2, hand2.getHandGesture(), () => Debug.Log("hand2"));

        }

        public void DrawHand(Hand hand)
        {
            // Debug.Log($"{hand["thumb"][0].x} {hand["thumb"][0].y} {hand["thumb"][0].z}");
            if (hand.ListOfJoints.Count <= 0) InstantiatePoint(hand);

            if (hand.fingers.Count == 6)
            {
                hand.AddCollisionToHand();
            }

            int index = 0;
            hand.hand.Keys.ToList().ForEach(key =>
            {
                if (key != "palmBase")
                {
                    DrawFinger(index, key.ToString(), hand);
                    index++;
                }
                else
                {
                    DrawPalm(hand.hand["palmBase"][0], hand);
                }
            });
            RepositionPoints(hand);
        }

        public void DrawPalm(Point palmPoint, Hand hand)
        {
            var fingers = hand.fingers;
            List<Point> points = new List<Point>();
            for (int i = 0; i < 5; i++)
            {
                Vector3 vect = fingers[i].GetComponent<LineRenderer>().GetPosition(0);
                points.Add(new Point(vect.x, vect.y, vect.z));
            }
            points.Add(palmPoint);

            LineRenderer lR;
            if (fingers[5].GetComponent<LineRenderer>() == null)
            {
                lR = fingers[5].AddComponent<LineRenderer>();
                lR.useWorldSpace = false;
                lR.material = hand.name == "left" ? materialLeft : materialRight;
            }
            else
            {
                lR = fingers[5].GetComponent<LineRenderer>();
            }
            lR.positionCount = 7;
            int index = 0;
            Vector3 offset = hand.fingers[0].transform.parent.position;

            points.ForEach(t =>
            {
                lR.SetPosition(index, t.GetPositionVector(ratio));
                index++;
            });

            Vector3 palmPointVector = points[0].GetPositionVector(ratio);
            lR.SetPosition(index - 1, points[index - 1].GetPositionVector(ratio) + offset);
            lR.SetPosition(index, palmPointVector);
            lR.startWidth = lineWidth;
            HandPhysics.SetMeshCollider(lR.gameObject);
            HandPhysics.SetMeshCollider(lR.gameObject);
        }

        public void DrawFinger(int fingerIndex, string fingerKey, Hand hand)
        {
            var fingers = hand.fingers;
            //thumb
            LineRenderer lR;
            if (fingers[fingerIndex].GetComponent<LineRenderer>() == null)
            {
                lR = fingers[fingerIndex].AddComponent<LineRenderer>();
                lR.useWorldSpace = false;
                lR.material = hand.name == "left" ? materialLeft : materialRight;
            }
            else
            {
                lR = fingers[fingerIndex].GetComponent<LineRenderer>();
            }
            lR.positionCount = 4;
            int index = 0;
            Vector3 offset = hand.fingers[0].transform.parent.position;
            hand.hand[fingerKey].ForEach(t =>
            {
                lR.SetPosition(index, t.GetPositionVector(ratio) + offset);
                index++;
            });
            lR.startWidth = lineWidth;
            HandPhysics.SetMeshCollider(lR.gameObject);
            HandPhysics.SetMeshCollider(lR.gameObject);
        }

    }

}