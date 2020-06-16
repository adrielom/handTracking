using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HandDetection
{
    public class Hand
    {

        public string name;
        public Dictionary<string, List<Point>> hand = new Dictionary<string, List<Point>>();
        public Dictionary<string, List<Point>> palm = new Dictionary<string, List<Point>>();
        public List<GameObject> fingers = new List<GameObject>();
        public List<GameObject> ListOfJoints = new List<GameObject>();
        public Color color;
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

    public class HandManager : MonoBehaviour
    {
        public GameObject spherePrefab, nodesContainerLeft, nodesContainerRight;
        public List<GameObject> fingersLeft, fingersRight;
        List<GameObject> ListOfJoints = new List<GameObject>();
        List<GameObject> leftJoints = new List<GameObject>(), rightJoints = new List<GameObject>();
        public Material materialLeft, materialRight;
        public float ratio = 100;
        public static Hand leftHand;
        public static Hand rightHand;

        private Camera cam;

        void Awake()
        {
            cam = Camera.main;
            // string json = File.ReadAllText(jsonPath).ToString();
            // var pointsWrapper = JsonUtility.FromJson<PointsWrapper>(json);
            // listOfPoints = pointsWrapper.points.ToList();
            // Debug.Log(listOfPoints.Count);
            // InstantiatePoint();
            leftHand = new Hand("left", fingersLeft, leftJoints, Color.red);
            rightHand = new Hand("right", fingersRight, rightJoints, Color.yellow);
        }


        void InstantiatePoint(Hand hand)
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject g = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
                g.transform.SetParent(hand.name == "left" ? nodesContainerLeft.transform : nodesContainerRight.transform);
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
                hand.ListOfJoints[i].transform.position = positions[i];
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {

            if (leftHand.hand.Count != 0)
            {
                if (leftHand.name == null) leftHand.name = "left";
                if (leftHand.fingers.Count == 0) leftHand.fingers = fingersLeft;
                DrawHand(leftHand);
            }
            if (rightHand.hand.Count != 0)
            {
                if (rightHand.name == null) rightHand.name = "right";
                if (rightHand.fingers.Count == 0) rightHand.fingers = fingersRight;
                DrawHand(rightHand);
            }
        }

        public void DrawHand(Hand hand)
        {
            // Debug.Log($"{hand["thumb"][0].x} {hand["thumb"][0].y} {hand["thumb"][0].z}");
            if (hand.ListOfJoints.Count <= 0) InstantiatePoint(hand);

            int index = 0;
            hand.hand.Keys.ToList().ForEach(key =>
            {
                if (key != "palmBase")
                {
                    Debug.Log(key);
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
                lR.material = hand.name == "left" ? materialLeft : materialRight;
            }
            else
            {
                lR = fingers[5].GetComponent<LineRenderer>();
            }
            lR.positionCount = 7;
            int index = 0;
            points.ForEach(t =>
            {
                lR.SetPosition(index, t.GetPositionVector(ratio));
                index++;
            });
            Vector3 palmPointVector = points[0].GetPositionVector(ratio);
            lR.SetPosition(index, palmPointVector);
        }

        public void DrawFinger(int fingerIndex, string fingerKey, Hand hand)
        {
            var fingers = hand.fingers;
            //thumb
            LineRenderer lR;
            if (fingers[fingerIndex].GetComponent<LineRenderer>() == null)
            {
                lR = fingers[fingerIndex].AddComponent<LineRenderer>();
                lR.material = hand.name == "left" ? materialLeft : materialRight;
            }
            else
            {
                lR = fingers[fingerIndex].GetComponent<LineRenderer>();
            }
            lR.positionCount = 4;
            int index = 0;
            hand.hand[fingerKey].ForEach(t =>
            {
                lR.SetPosition(index, t.GetPositionVector(ratio));
                index++;
            });
        }

    }
}
