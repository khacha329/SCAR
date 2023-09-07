using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding;
using Microsoft.MixedReality.Toolkit.Experimental.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;

namespace SmartCampusHandler
{
    public class PrefabHandler : MonoBehaviour
    {
        [Header("Nodes")]
        [SerializeField]
        public GameObject InstantiatedSensor = null;
        [SerializeField]
        public GameObject InstantiatedGateWay = null;

        /// <summary>
        /// Used via in-game buttons, locates the nearest wall and adds 
        /// a sensor on its world position
        /// </summary>
        public void OnDemandSensor()
        {
            if (!SmartCampusNodes.CurrentPrefabSet)
            {
                SceneUnderstanding.observedSceneObjects.TryGetValue(SpatialAwarenessSurfaceTypes.Wall, out Dictionary<int, SpatialAwarenessSceneObject> WallsInScene);
                int nearestId = GeometryHandler.FindClosestQuad(SpatialAwarenessSurfaceTypes.Wall);
                if (nearestId == -1)
                {
                    Debug.Log("No walls found");
                }
                else
                {
                    SmartCampusNodes.CurrentPrefab = Instantiate(InstantiatedSensor);
                    SmartCampusNodes.CurrentPrefab.transform.SetPositionAndRotation(WallsInScene[nearestId].Position, WallsInScene[nearestId].Rotation);
                    SmartCampusNodes.CurrentPrefabSet = true;
                    SmartCampusNodes.CurrentSensor = true;
                }
            }
        }

        /// <summary>
        /// Used via in-game buttons, locates the nearest ceiling and adds 
        /// a Gateway on its world position
        /// </summary>
        public void OnDemandGW()
        {
            if (!SmartCampusNodes.CurrentPrefabSet)
            {
                SceneUnderstanding.observedSceneObjects.TryGetValue(SpatialAwarenessSurfaceTypes.Ceiling, out Dictionary<int, SpatialAwarenessSceneObject> CeillingInScene);
                int nearestId = GeometryHandler.FindClosestQuad(SpatialAwarenessSurfaceTypes.Ceiling);
                if (nearestId == -1)
                {
                    Debug.Log("No ceillings found");
                }
                else
                {
                    SmartCampusNodes.CurrentPrefab = Instantiate(InstantiatedGateWay);
                    SmartCampusNodes.CurrentPrefab.transform.SetPositionAndRotation(CeillingInScene[nearestId].Position, CeillingInScene[nearestId].Rotation);
                    SmartCampusNodes.CurrentPrefabSet = true;
                    SmartCampusNodes.CurrentGW = true;
                }
            }
        }
    }
}