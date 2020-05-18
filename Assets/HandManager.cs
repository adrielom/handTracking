using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HandDetection
{
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
        public static List<Point> listOfPoints = new List<Point>();
        public static Dictionary<string, List<Point>> hand = new Dictionary<string, List<Point>>();
        public static Dictionary<string, List<Point>> palm = new Dictionary<string, List<Point>>();
        public GameObject spherePrefab, nodesContainer;
        public List<GameObject> fingers;
        public List<GameObject> ListOfJoints = new List<GameObject>();
        public Material materialOrange, materialBlue;
        public float ratio = 100;
        static string points = "points.json", pointsNormalized = "pointsNormalized.json", pointsNew = "pointsNew.json";
        string jsonPath = $@"C:\Users\adriel.oliveira\Desktop\TeachingRobson\Assets\{pointsNew}";

        private Camera cam;

        void Start()
        {
            cam = Camera.main;
            // string json = File.ReadAllText(jsonPath).ToString();
            // var pointsWrapper = JsonUtility.FromJson<PointsWrapper>(json);
            // listOfPoints = pointsWrapper.points.ToList();
            // Debug.Log(listOfPoints.Count);
            // InstantiatePoint();

        }


        void InstantiatePoint()
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject g = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
                g.transform.SetParent(nodesContainer.transform);
                ListOfJoints.Add(g);
            }
        }

        public void RepositionPoints()
        {
            List<Vector3> positions = new List<Vector3>();

            fingers.ForEach(f =>
            {
                LineRenderer lR = f.GetComponent<LineRenderer>();
                lR.SetWidth(0, 10);
                for (int i = 0; i < lR.positionCount - 1; i++)
                {
                    positions.Add(lR.GetPosition(i));
                }
            });

            for (int i = 0; i < positions.Count - 1; i++)
            {
                ListOfJoints[i].transform.position = positions[i];
            }
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if (hand.Count != 0)
                DrawHand(hand);
        }

        /// <summary>
        /// LateUpdate is called every frame, if the Behaviour is enabled.
        /// It is called after all Update functions have been called.
        /// </summary>
        void LateUpdate()
        {
            listOfPoints?.ForEach(p =>
            {
                // p.nextPoints.Clear();
            });
        }

        public void DrawHand(Dictionary<string, List<Point>> hand)
        {
            // Debug.Log($"{hand["thumb"][0].x} {hand["thumb"][0].y} {hand["thumb"][0].z}");
            if (ListOfJoints.Count <= 0) InstantiatePoint();

            int index = 0;
            hand.Keys.ToList().ForEach(key =>
            {
                if (key != "palmBase")
                {
                    Debug.Log(key);
                    DrawFinger(index, key.ToString());
                    index++;
                }
                else
                {
                    DrawPalm(hand["palmBase"][0]);
                }
            });
            RepositionPoints();
        }

        public void DrawPalm(Point palmPoint)
        {
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
                lR.material = materialBlue;
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

        public void DrawFinger(int fingerIndex, string fingerKey)
        {
            //thumb
            LineRenderer lR;
            if (fingers[fingerIndex].GetComponent<LineRenderer>() == null)
            {
                lR = fingers[fingerIndex].AddComponent<LineRenderer>();
                lR.material = materialBlue;
            }
            else
            {
                lR = fingers[fingerIndex].GetComponent<LineRenderer>();
            }
            lR.positionCount = 4;
            int index = 0;
            hand[fingerKey].ForEach(t =>
            {
                lR.SetPosition(index, t.GetPositionVector(ratio));
                index++;
            });
        }

        public void HandStructureLinkedNodes(Point p)
        {
            //Thumb
            if (p.id == 0)
            {
                LineRenderer lR;
                if (fingers[0].GetComponent<LineRenderer>() == null)
                {
                    lR = fingers[0].AddComponent<LineRenderer>();
                    lR.material = materialBlue;
                }
                else
                {
                    lR = fingers[0].GetComponent<LineRenderer>();
                }
                lR.positionCount = 5;
                for (int i = 0; i < 5; i++)
                {
                    p.nextPoints.AddLast(HandManager.listOfPoints[i + 1]);
                    lR.SetPosition(i, listOfPoints[i].GetPositionVector(ratio));
                }
            }
            // Index
            else if (p.id == 5)
            {
                LineRenderer lR;
                if (fingers[1].GetComponent<LineRenderer>() == null)
                {
                    lR = fingers[1].AddComponent<LineRenderer>();
                    lR.material = materialBlue;
                }
                else
                {
                    lR = fingers[1].GetComponent<LineRenderer>();
                }
                lR.positionCount = 4;
                int index = 0;
                for (int i = 5; i < 9; i++)
                {
                    p.nextPoints.AddLast(HandManager.listOfPoints[i + 1]);
                    lR.SetPosition(index, listOfPoints[i].GetPositionVector(ratio));
                    index++;
                }
            }
            // Middle
            else if (p.id == 9)
            {
                LineRenderer lR;
                if (fingers[2].GetComponent<LineRenderer>() == null)
                {
                    lR = fingers[2].AddComponent<LineRenderer>();
                    lR.material = materialBlue;
                }
                else
                {
                    lR = fingers[2].GetComponent<LineRenderer>();
                }
                lR.positionCount = 4;
                int index = 0;
                for (int i = 9; i < 13; i++)
                {
                    p.nextPoints.AddLast(HandManager.listOfPoints[i + 1]);
                    lR.SetPosition(index, listOfPoints[i].GetPositionVector(ratio));
                    index++;
                }
            }
            //Ring
            else if (p.id == 13)
            {
                LineRenderer lR;
                if (fingers[3].GetComponent<LineRenderer>() == null)
                {
                    lR = fingers[3].AddComponent<LineRenderer>();
                    lR.material = materialBlue;
                }
                else
                {
                    lR = fingers[3].GetComponent<LineRenderer>();
                }
                lR.positionCount = 4;
                int index = 0;
                for (int i = 13; i < 17; i++)
                {
                    p.nextPoints.AddLast(HandManager.listOfPoints[i + 1]);
                    lR.SetPosition(index, listOfPoints[i].GetPositionVector(ratio));
                    index++;
                }
            }
            //Pinky
            else if (p.id == 17)
            {
                LineRenderer lR;
                if (fingers[4].GetComponent<LineRenderer>() == null)
                {
                    lR = fingers[4].AddComponent<LineRenderer>();
                    lR.material = materialBlue;
                }
                else
                {
                    lR = fingers[4].GetComponent<LineRenderer>();
                }
                lR.positionCount = 4;
                int index = 0;
                for (int i = 17; i < 21; i++)
                {
                    if (i + 1 < 21)
                        p.nextPoints.AddLast(HandManager.listOfPoints[i + 1]);
                    else
                        p.nextPoints.AddLast(HandManager.listOfPoints[i]);
                    lR.SetPosition(index, listOfPoints[i].GetPositionVector(ratio));
                    index++;
                }
            }
            //Palm
            else if (p.id == 20)
            {
                LineRenderer lR;
                if (fingers[5].GetComponent<LineRenderer>() == null)
                {
                    lR = fingers[5].AddComponent<LineRenderer>();
                    lR.material = materialBlue;
                }
                else
                {
                    lR = fingers[5].GetComponent<LineRenderer>();
                }
                lR.positionCount = 6;
                lR.SetPosition(0, listOfPoints[0].GetPositionVector(ratio));
                lR.SetPosition(1, listOfPoints[5].GetPositionVector(ratio));
                lR.SetPosition(2, listOfPoints[9].GetPositionVector(ratio));
                lR.SetPosition(3, listOfPoints[13].GetPositionVector(ratio));
                lR.SetPosition(4, listOfPoints[17].GetPositionVector(ratio));
                lR.SetPosition(5, listOfPoints[0].GetPositionVector(ratio));
            }
        }

    }
}