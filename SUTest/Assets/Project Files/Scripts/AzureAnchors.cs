using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBasesLibrary;
using System;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using System.Threading.Tasks;
using SmartCampusHandler;

namespace AzureLibrary
{
    public class AzureAnchors : MonoBehaviour
    {
        [Header("Nodes")]
        [SerializeField]
        public GameObject InstantiatedSensor = null;
        [SerializeField]
        public GameObject InstantiatedGateWay = null;
        private bool SensorsCreated = false;
        #region Azure Stuff
        private static SpatialAnchorManager _spatialAnchorManager = null;

        /// <summary>
        /// Used to keep track of all GameObjects that represent a found or created anchor
        /// </summary>
        private static List<GameObject> _foundOrCreatedAnchorGameObjects = new List<GameObject>();

        /// <summary>
        /// Used to keep track of all the created Anchor IDs
        /// </summary>
        public static List<String> _createdAnchorIDs = new List<String>();
        #endregion Azure Stuff 
        void Start()
        {
            //Azure anchors set up
            _spatialAnchorManager = GetComponent<SpatialAnchorManager>();
            _spatialAnchorManager.LogDebug += (sender, args) => Debug.Log($"ASA - Debug: {args.Message}");
            _spatialAnchorManager.Error += (sender, args) => Debug.LogError($"ASA - Error: {args.ErrorMessage}");
            _spatialAnchorManager.AnchorLocated += SpatialAnchorManager_AnchorLocated;
        }

        public static async Task OnStartSession()
        {
            Debug.Log("starting...");
            await _spatialAnchorManager.StartSessionAsync();
            return;

        }

        public static void OnDeleteSession()
        {
            _spatialAnchorManager.DestroySession();
            _foundOrCreatedAnchorGameObjects.Clear();
            Debug.Log("Deleted");
        }

        /// <summary>
        /// Creates an anchor ID to represent a node (prefab) and saves its ID to a text file
        /// </summary>
        public static async Task CreateAnchor(List<GameObject> ListToAnchor, string TextFileName)
        {
            //Create Anchor GameObject. We will use ASA to save the position and the rotation of this GameObject.
            GameObject anchorGameObject = ListToAnchor[ListToAnchor.Count - 1];
            //Add and configure ASA components
            CloudNativeAnchor cloudNativeAnchor = anchorGameObject.AddComponent<CloudNativeAnchor>();
            await cloudNativeAnchor.NativeToCloud();
            CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;
            cloudSpatialAnchor.Expiration = DateTimeOffset.Now.AddDays(1);
            //Collect Environment Data
            while (!_spatialAnchorManager.IsReadyForCreate)
            {
                float createProgress = _spatialAnchorManager.SessionStatus.RecommendedForCreateProgress;
                Debug.Log($"ASA - Move your device to capture more environment data: {createProgress:0%}");
            }
            Debug.Log($"ASA - Saving cloud anchor... ");
            try
            {
                // Now that the cloud spatial anchor has been prepared, we can try the actual save here.
                await _spatialAnchorManager.CreateAnchorAsync(cloudSpatialAnchor);
                bool saveSucceeded = cloudSpatialAnchor != null;
                if (!saveSucceeded)
                {
                    Debug.LogError("ASA - Failed to save, but no exception was thrown.");
                    return;
                }
                Debug.Log($"ASA - Saved cloud anchor with ID: {cloudSpatialAnchor.Identifier}");
                _foundOrCreatedAnchorGameObjects.Add(anchorGameObject);
                _createdAnchorIDs.Add(cloudSpatialAnchor.Identifier);
                TextFileHandler.WriteToFile(TextFileName, cloudSpatialAnchor.Identifier);
            }
            catch (Exception exception)
            {
                Debug.Log("ASA - Failed to save anchor: " + exception.ToString());
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// Reads anchor IDs from a file and creates watcher to locate anchors in world space 
        /// </summary>
        public static void LocateAnchor(string FileName)
        {
            _createdAnchorIDs = TextFileHandler.ReadFromFile(FileName);
            Debug.Log(_createdAnchorIDs.Count);
            if (_createdAnchorIDs.Count > 0)
            {
                //Create watcher to look for all stored anchor IDs
                Debug.Log($"ASA - Creating watcher to look for {_createdAnchorIDs.Count} spatial anchors");
                AnchorLocateCriteria anchorLocateCriteria = new AnchorLocateCriteria();
                anchorLocateCriteria.Identifiers = _createdAnchorIDs.ToArray();
                _spatialAnchorManager.Session.CreateWatcher(anchorLocateCriteria);
                Debug.Log($"ASA - Watcher created!");
            }
        }

        /// <summary>
        /// Used to spawn the nodes (prefabs) back in world space from text files in order
        /// sensors -> gateways
        /// </summary>
        private void SpatialAnchorManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            Debug.Log($"ASA - Anchor recognized as a possible anchor {args.Identifier} {args.Status}");
            if (args.Status == LocateAnchorStatus.Located)
            {
                //Creating and adjusting GameObjects have to run on the main thread. We are using the UnityDispatcher to make sure this happens.
                UnityDispatcher.InvokeOnAppThread(() =>
                {
                    // Read out Cloud Anchor values
                    CloudSpatialAnchor cloudSpatialAnchor = args.Anchor;                  
                    if (!SensorsCreated) 
                    {
                        GameObject anchorGameObject = Instantiate(InstantiatedSensor);
                        SmartCampusNodes.instantiatedPrefabs.Add(anchorGameObject);
                        // Link to Cloud Anchor
                        anchorGameObject.AddComponent<CloudNativeAnchor>().CloudToNative(cloudSpatialAnchor);
                        _foundOrCreatedAnchorGameObjects.Add(anchorGameObject);
                    }
                    else
                    {
                        GameObject anchorGameObject = Instantiate(InstantiatedGateWay);
                        SmartCampusNodes.instantiatedPrefabs.Add(anchorGameObject);
                        // Link to Cloud Anchor
                        anchorGameObject.AddComponent<CloudNativeAnchor>().CloudToNative(cloudSpatialAnchor);
                        _foundOrCreatedAnchorGameObjects.Add(anchorGameObject);

                    }
                    if (SmartCampusNodes.instantiatedPrefabs.Count == _createdAnchorIDs.Count) SensorsCreated = true;
                });
            }
        }
    }
}
