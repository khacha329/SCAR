using Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding;
using Microsoft.MixedReality.Toolkit.Experimental.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryHandler : MonoBehaviour
{    
    
    /// <summary>
    /// Calculate the distance between every quad and the player in the scene then
    /// return the ID of the closest quad, used to add nodes(prefabs)
    /// </summary>
    public static int FindClosestQuad(SpatialAwarenessSurfaceTypes surfaceType)
    {
        var playerObject = GameObject.Find("MainCamera");
        var playerPos = playerObject.transform.position;
        float Distance;
        int nearestId = -1;
        float smallestDistance = float.MaxValue;
        if (SceneUnderstanding.observedSceneObjects.TryGetValue(surfaceType, out Dictionary<int, SpatialAwarenessSceneObject> QuadInScene))
        {
            foreach (KeyValuePair<int, SpatialAwarenessSceneObject> entry in QuadInScene)
            {
                Distance = DistanceBetween2Vectors(entry.Value.Position, playerPos);
                if (smallestDistance > Distance)
                {
                    smallestDistance = Distance;
                    nearestId = entry.Key;
                }
                entry.Value.GameObject.SetActive(false);
            }
            if (nearestId == -1)
            {
                Debug.Log("No quads found");
                return -1;
            }
            QuadInScene[nearestId].GameObject.SetActive(true);
        }
        return nearestId;
    }

    public static float DistanceBetween2Vectors(Vector3 PrefabPosition, Vector3 PlayerPosition)
    {
        float SmallestDistance = Vector3.Distance(PrefabPosition, PlayerPosition);
        return SmallestDistance;
    }
}
