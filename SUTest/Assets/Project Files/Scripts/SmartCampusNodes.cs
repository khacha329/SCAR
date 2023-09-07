using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SmartCampusHandler
{
    public class SmartCampusNodes : MonoBehaviour
    {

        public static List<GameObject> instantiatedPrefabs { get; set; } = new List<GameObject>();
        public static GameObject CurrentPrefab;
        public static bool CurrentPrefabSet { get; set; } = false;
        public static bool CurrentSensor { get; set; } = false;
        public static bool CurrentGW { get; set; } = false;
        public static List<GameObject> InstantiatedGWs { get; set; } = new List<GameObject>();
        public static List<GameObject> InstantiatedSensors { get; set; } = new List<GameObject>();
        public static GameObject[] FoundSensors { get; set; } = null;
        public static GameObject[] FoundGWs { get; set; } = null;

    }
}
