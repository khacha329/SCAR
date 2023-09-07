using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AzureLibrary;
using System.Threading.Tasks;

namespace SmartCampusHandler
{
    public class AzureAnchorHandler : MonoBehaviour
    {
        /// <summary>
        /// Used via in-game buttons, saves the node added to the scene in a textfile 
        /// depending on the type of node. This saves only 1 node at a time booleans 
        /// are used to prevent saving multiple nodes at once or add multiple nodes at once
        /// </summary>
        public async void SavePrefabs()
        {
            if (SmartCampusNodes.CurrentPrefab != null)
            {
                await AzureAnchors.OnStartSession();
                SmartCampusNodes.instantiatedPrefabs.Add(SmartCampusNodes.CurrentPrefab);
                SmartCampusNodes.CurrentPrefabSet = false;
                if (SmartCampusNodes.CurrentGW)
                {
                    SmartCampusNodes.InstantiatedGWs.Add(SmartCampusNodes.CurrentPrefab);
                    Debug.Log("Creating GWs...");
                    await AzureAnchors.CreateAnchor(SmartCampusNodes.InstantiatedGWs, "SavedGWIDs.txt");
                    SmartCampusNodes.CurrentGW = false;
                }
                else if (SmartCampusNodes.CurrentSensor)
                {
                    SmartCampusNodes.InstantiatedSensors.Add(SmartCampusNodes.CurrentPrefab);
                    Debug.Log("Creating Sensors...");
                    await AzureAnchors.CreateAnchor(SmartCampusNodes.InstantiatedSensors, "SavedSensorIDs.txt");
                    SmartCampusNodes.CurrentSensor = false;
                }
                AzureAnchors.OnDeleteSession();
            }
            else
                Debug.Log("No Sensors or gateways in scene present");
        }

        /// <summary>
        /// Used via in-game buttons, reads the anchors IDs from text files in order 
        /// Sensors -> Gateways
        /// </summary>
        public async void OnLocate()
        {
            await AzureAnchors.OnStartSession();
            LocateSensors();
            await LocateGateWays();
            AzureAnchors.OnDeleteSession();
        }

        /// <summary>
        /// Locates sensors from SavedSensorsIDs text file
        /// </summary>
        public void LocateSensors()
        {
            if (SmartCampusNodes.instantiatedPrefabs.Count == 0)
            {
                Debug.Log("Locating Sensors...");
                AzureAnchors.LocateAnchor("SavedSensorIDs.txt");
                Debug.Log("Located");
            }
            else
            {
                Debug.Log("Please Clear Scene"); // cant locate with sonsors/anchors already in scenerefresh();
            }
            return;
        }

        /// <summary>
        /// Locates Gateways from SavedGWIDs text file with a delay 
        /// to ensure sensors have been located
        /// </summary>
        public async Task LocateGateWays()
        {   //while loop to ensure watcher has done its job and sensors are spawned
            while (SmartCampusNodes.FoundSensors == null || SmartCampusNodes.FoundSensors.Length != AzureAnchors._createdAnchorIDs.Count)
            {
                SmartCampusNodes.FoundSensors = GameObject.FindGameObjectsWithTag("Sensor");
                await Task.Delay(5000);
            }

            if (SmartCampusNodes.instantiatedPrefabs.Count == AzureAnchors._createdAnchorIDs.Count)
            {
                Debug.Log("Locating GWs...");
                AzureAnchors.LocateAnchor("SavedGWIDs.txt");
                //while loop to ensure watcher finishs its job 
                while (SmartCampusNodes.FoundGWs == null || SmartCampusNodes.FoundGWs.Length != AzureAnchors._createdAnchorIDs.Count)
                {
                    SmartCampusNodes.FoundGWs = GameObject.FindGameObjectsWithTag("GW");
                    await Task.Delay(5000);
                }
            }
            else
            {
                Debug.Log("please locate sensors first");
            }
            return;
        }
    }
}
