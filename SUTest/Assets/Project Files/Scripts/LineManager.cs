using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    [SerializeField]
    public GameObject LaserBeam;
    [SerializeField]
    List<Transform> StartPos = new List<Transform>();
    [SerializeField]
    private List<Transform> EndPos = new List<Transform>();

    private GameObject[] FoundSensors;
    private GameObject[] FoundGWs ;

    /// <summary>
    /// Used via in-game buttons, This will collect the start and end points from all nodes (prefabs) in scene 
    /// and put them in array then link them based on distance
    /// </summary>
    public void MakeConnection()
    {
        OnGW();
        OnSensors();       
        int SelectedStartPoint = -1;
        int SelectedEndPoint = -1;
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < StartPos.Count; i++)
        {
            var CurrentLazer = Instantiate(LaserBeam);
            LineRenderer Laser = CurrentLazer.GetComponentInChildren<LineRenderer>();
            float smallestDistance = float.MaxValue;
            for (int j = 0; j < EndPos.Count; j++)
            {              
                float   Distance = GeometryHandler.DistanceBetween2Vectors(StartPos[i].position, EndPos[j].position);
                if (smallestDistance > Distance)
                {
                    smallestDistance = Distance;
                    SelectedStartPoint = i;
                     SelectedEndPoint = j;
                }

            }
            if (SelectedStartPoint == -1)
            {
                Debug.Log("No starting point selected");
            }
            if (SelectedEndPoint == -1)
            {
                Debug.Log("No Ending point selected");
            }
            if (SelectedStartPoint != -1 && SelectedEndPoint != -1)
            {
                Laser.SetPositions(points.ToArray());
                points.Clear();
            }
        }      
    }

    /// <summary>
    /// This will find all active sensors in scene and store them in StartPos
    /// </summary>
    public void OnSensors()
    {
        FoundSensors = GameObject.FindGameObjectsWithTag("Sensor");
        Debug.Log(FoundSensors.Length);
        for (int i = 0; i < FoundSensors.Length; i++)
        {
            GameObject Child = FoundSensors[i].transform.GetChild(0).gameObject;
            StartPos.Add(Child.transform);
        }
        Debug.Log("Found Sensors");
    }

    /// <summary>
    /// This will find all active gateways in scene and store them in EndPos
    /// </summary>
    public void OnGW()
    {
        FoundGWs = GameObject.FindGameObjectsWithTag("GW");
        Debug.Log(FoundGWs.Length);
        for (int i = 0; i < FoundGWs.Length; i++)
        {
            GameObject Child = FoundGWs[i].transform.GetChild(0).gameObject;
            EndPos.Add(Child.transform);
        }
        Debug.Log("Found GWS");
    }
}
