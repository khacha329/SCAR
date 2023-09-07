// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.MixedReality.Toolkit.Examples.Demos;
using Microsoft.MixedReality.Toolkit.Experimental.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding
{
    /// <summary>
    /// Demo class to show different ways of visualizing the space using scene understanding.
    /// </summary>
    public class DemoSceneUnderstandingController : DemoSpatialMeshHandler, IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>
    {
        #region Private Fields

        #region Serialized Fields

        [SerializeField]
        private string SavedSceneNamePrefix = "DemoSceneUnderstanding";
        [SerializeField]
        private bool InstantiatePrefabs = false;
        [SerializeField]
        private GameObject InstantiatedSensor = null;
        [SerializeField]
        private GameObject InstantiatedGateWay = null;
        [SerializeField]
        private Transform InstantiatedParent = null;

        [Header("UI")]
        [SerializeField]
        private Interactable autoUpdateToggle = null;
        [SerializeField]
        private Interactable quadsToggle = null;
        [SerializeField]
        private Interactable inferRegionsToggle = null;
        [SerializeField]
        private Interactable meshesToggle = null;
        [SerializeField]
        private Interactable maskToggle = null;
        [SerializeField]
        private Interactable platformToggle = null;
        [SerializeField]
        private Interactable wallToggle = null;
        [SerializeField]
        private Interactable floorToggle = null;
        [SerializeField]
        private Interactable ceilingToggle = null;
        [SerializeField]
        private Interactable worldToggle = null;
        [SerializeField]
        private Interactable completelyInferred = null;
        [SerializeField]
        private Interactable backgroundToggle = null;

        #endregion Serialized Fields

        private IMixedRealitySceneUnderstandingObserver observer; // this guy gives you alot of info about the scene generated this is probably all you need

        // lists and variables for sensors and gateways
        private List<GameObject> instantiatedPrefabs;
        private List<GameObject> InstantiatedGWs;
        private List<GameObject> InstantiatedSensors;
        private GameObject CurrentPrefab;
        private bool CurrentPrefabSet = false;
        private bool CurrentSensor = false;
        private bool CurrentGW = false;
        private bool SensorsCreated = false;
        private GameObject[] FoundSensors = null;
        private GameObject[] FoundGWs = null;

        private Dictionary<SpatialAwarenessSurfaceTypes, Dictionary<int, SpatialAwarenessSceneObject>> observedSceneObjects; //<this guy has the surface type and a dictionary of scene objects with their ids, scene objects details can be extracted here >

        #endregion Private Fields

        #region Azure Stuff
        private SpatialAnchorManager _spatialAnchorManager = null;

        /// <summary>
        /// Used to keep track of all GameObjects that represent a found or created anchor
        /// </summary>
        private List<GameObject> _foundOrCreatedAnchorGameObjects = new List<GameObject>();

        /// <summary>
        /// Used to keep track of all the created Anchor IDs
        /// </summary>
        private List<String> _createdAnchorIDs = new List<String>();
        #endregion Azure Stuff 
        #region MonoBehaviour Functions

        protected override void Start()
        {
            observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySceneUnderstandingObserver>();

            if (observer == null)
            {
                Debug.LogError("Couldn't access Scene Understanding Observer! Please make sure the current build target is set to Universal Windows Platform. "
                    + "Visit https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/spatial-awareness/scene-understanding for more information.");
                return;
            }
            InitToggleButtonState();
            //initialize lists
            instantiatedPrefabs = new List<GameObject>();
            InstantiatedGWs = new List<GameObject>();
            InstantiatedSensors = new List<GameObject>();
            observedSceneObjects = new Dictionary<SpatialAwarenessSurfaceTypes, Dictionary<int, SpatialAwarenessSceneObject>>();

            //Azure anchors set up
            _spatialAnchorManager = GetComponent<SpatialAnchorManager>();
            _spatialAnchorManager.LogDebug += (sender, args) => Debug.Log($"ASA - Debug: {args.Message}");
            _spatialAnchorManager.Error += (sender, args) => Debug.LogError($"ASA - Error: {args.ErrorMessage}");
            _spatialAnchorManager.AnchorLocated += SpatialAnchorManager_AnchorLocated;
            
        }

        protected override void OnEnable()
        {
            RegisterEventHandlers<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>, SpatialAwarenessSceneObject>();
        }

        protected override void OnDisable()
        {
            UnregisterEventHandlers<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>, SpatialAwarenessSceneObject>();
        }

        protected override void OnDestroy()
        {
            UnregisterEventHandlers<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>, SpatialAwarenessSceneObject>();
        }

        #endregion MonoBehaviour Functions

        #region IMixedRealitySpatialAwarenessObservationHandler Implementations

        /// <inheritdoc />
        public void OnObservationAdded(MixedRealitySpatialAwarenessEventData<SpatialAwarenessSceneObject> eventData)
        {
            // This method called everytime a SceneObject created by the SU observer
            

            AddToData(eventData.Id);

            if (observedSceneObjects.TryGetValue(eventData.SpatialObject.SurfaceType, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjectDict))
            {
                sceneObjectDict.Add(eventData.Id, eventData.SpatialObject);
            }
            else
            {
                observedSceneObjects.Add(eventData.SpatialObject.SurfaceType, new Dictionary<int, SpatialAwarenessSceneObject> { { eventData.Id, eventData.SpatialObject } });
            }

            if (InstantiatePrefabs)
            {
                var prefab = Instantiate(InstantiatedSensor);
                prefab.transform.SetPositionAndRotation(eventData.SpatialObject.Position, eventData.SpatialObject.Rotation);
                float sx = eventData.SpatialObject.Quads[0].Extents.x;
                float sy = eventData.SpatialObject.Quads[0].Extents.y;
                prefab.transform.localScale = new Vector3(sx, sy, .1f);
                if (InstantiatedParent)
                {
                    prefab.transform.SetParent(InstantiatedParent);
                }
                instantiatedPrefabs.Add(prefab);
            }
            else
            {
                foreach (var quad in eventData.SpatialObject.Quads)
                {
                    quad.GameObject.GetComponent<Renderer>().material.color = ColorForSurfaceType(eventData.SpatialObject.SurfaceType);
                }
            }
        }

        /// <inheritdoc />
        public void OnObservationUpdated(MixedRealitySpatialAwarenessEventData<SpatialAwarenessSceneObject> eventData)
        {
            UpdateData(eventData.Id);

            if (observedSceneObjects.TryGetValue(eventData.SpatialObject.SurfaceType, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjectDict))
            {
                observedSceneObjects[eventData.SpatialObject.SurfaceType][eventData.Id] = eventData.SpatialObject;
            }
            else
            {
                observedSceneObjects.Add(eventData.SpatialObject.SurfaceType, new Dictionary<int, SpatialAwarenessSceneObject> { { eventData.Id, eventData.SpatialObject } });
            }
        }

        /// <inheritdoc />
        public void OnObservationRemoved(MixedRealitySpatialAwarenessEventData<SpatialAwarenessSceneObject> eventData)
        {
            RemoveFromData(eventData.Id);

            foreach (var sceneObjectDict in observedSceneObjects.Values)
            {
                sceneObjectDict?.Remove(eventData.Id);
            }
        }

        #endregion IMixedRealitySpatialAwarenessObservationHandler Implementations

        #region Public Functions

        /// <summary>
        /// Get all currently observed SceneObjects of a certain type.
        /// </summary>
        /// <remarks>
        /// Before calling this function, the observer should be configured to observe the specified type by including that type in the SurfaceTypes property.
        /// </remarks>
        /// <returns>A dictionary with the scene objects of the requested type being the values and their ids being the keys.</returns>
        public IReadOnlyDictionary<int, SpatialAwarenessSceneObject> GetSceneObjectsOfType(SpatialAwarenessSurfaceTypes type)
        {
            if (!observer.SurfaceTypes.IsMaskSet(type))
            {
                Debug.LogErrorFormat("The Scene Objects of type {0} are not being observed. You should add {0} to the SurfaceTypes property of the observer in advance.", type);
            }

            if (observedSceneObjects.TryGetValue(type, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjects))
            {
                return sceneObjects;  // this is a dictionary with ids and values of a specific observed type 
            }
            else
            {
                observedSceneObjects.Add(type, new Dictionary<int, SpatialAwarenessSceneObject>());
                return observedSceneObjects[type];
            }
        }

        #region UI Functions

        /// <summary>
        /// Request the observer to update the scene
        /// </summary>
        public void UpdateScene()
        {
            observer.UpdateOnDemand();
            refresh();
        }

        /// <summary>
        /// Request the observer to save the scene
        /// </summary>
        public void SaveScene()
        {
            observer.SaveScene(SavedSceneNamePrefix);
        }

        /// <summary>
        /// Request the observer to clear the observations in the scene
        /// </summary>
        public void ClearScene()
        {
            foreach (GameObject gameObject in instantiatedPrefabs)
            {
                Destroy(gameObject);
            }
            instantiatedPrefabs.Clear();
            observer.ClearObservations();
        }

        /// <summary>
        /// Change the auto update state of the observer
        /// </summary>
        public void ToggleAutoUpdate()
        {
            observer.AutoUpdate = !observer.AutoUpdate;
        }

        /// <summary>
        /// Change whether to request occlusion mask from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>                                         
        public void ToggleOcclusionMask()
        {
            var observerMask = observer.RequestOcclusionMask;
            observer.RequestOcclusionMask = !observerMask;
            if (observer.RequestOcclusionMask)
            {
                if (!(observer.RequestPlaneData || observer.RequestMeshData))
                {
                    observer.RequestPlaneData = true;
                    quadsToggle.IsToggled = true;
                }
            }

            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request plane data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleGeneratePlanes()
        {
            observer.RequestPlaneData = !observer.RequestPlaneData;
            if (observer.RequestPlaneData)
            {
                observer.RequestMeshData = false;
                meshesToggle.IsToggled = false;
            }
            // these are used to toggle off all other surface types except walls and ceilings
   
            ClearAndUpdateObserver();
        }

        /// <summary>
        /// Change whether to request mesh data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleGenerateMeshes()
        {
            observer.RequestMeshData = !observer.RequestMeshData;
            if (observer.RequestMeshData)
            {
                observer.RequestPlaneData = false;
                quadsToggle.IsToggled = false;
            }
            ClearAndUpdateObserver();
        }
        /// <summary>
        /// Change whether to request wall data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleWalls()
        {
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Wall);
            ClearAndUpdateObserver();           
        }
        /// <summary>
        /// Change whether to request ceiling data from the observer followed by
        /// clearing existing observations and requesting an update
        /// </summary>
        public void ToggleCeilings()
        {
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Ceiling);
            ClearAndUpdateObserver();           
        }
        #endregion UI Functions

        #endregion Public Functions

        #region Helper Functions

        private void InitToggleButtonState()
        {
            // Configure observer
            autoUpdateToggle.IsToggled = observer.AutoUpdate;
            quadsToggle.IsToggled = observer.RequestPlaneData;
            //meshesToggle.IsToggled = observer.RequestMeshData;
            //maskToggle.IsToggled = observer.RequestOcclusionMask;
            //inferRegionsToggle.IsToggled = observer.InferRegions;

            // Filter display
            wallToggle.IsToggled = observer.SurfaceTypes.IsMaskSet(SpatialAwarenessSurfaceTypes.Wall);
            ceilingToggle.IsToggled = observer.SurfaceTypes.IsMaskSet(SpatialAwarenessSurfaceTypes.Ceiling);
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Background);
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Floor);
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.World);
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Platform);
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Unknown);
            ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes.Inferred);
        }

        /// <summary>
        /// Gets the color of the given surface type
        /// </summary>
        /// <param name="surfaceType">The surface type to get color for</param>
        /// <returns>The color of the type</returns>
        private Color ColorForSurfaceType(SpatialAwarenessSurfaceTypes surfaceType)
        {
            // shout-out to solarized!

            switch (surfaceType)
            {
                case SpatialAwarenessSurfaceTypes.Unknown:
                    return new Color32(220, 50, 47, 255); // red
                case SpatialAwarenessSurfaceTypes.Floor:
                    return new Color32(38, 139, 210, 255); // blue
                case SpatialAwarenessSurfaceTypes.Ceiling:
                    return new Color32(108, 113, 196, 255); // violet
                case SpatialAwarenessSurfaceTypes.Wall:
                    return new Color32(181, 137, 0, 255); // yellow
                case SpatialAwarenessSurfaceTypes.Platform:
                    return new Color32(133, 153, 0, 255); // green
                case SpatialAwarenessSurfaceTypes.Background:
                    return new Color32(203, 75, 22, 255); // orange
                case SpatialAwarenessSurfaceTypes.World:
                    return new Color32(211, 54, 130, 255); // magenta
                case SpatialAwarenessSurfaceTypes.Inferred:
                    return new Color32(42, 161, 152, 255); // cyan
                default:
                    return new Color32(220, 50, 47, 255); // red
            }
        }

        private void ClearAndUpdateObserver()
        {
            ClearScene();
            observer.UpdateOnDemand();
        }

        // used to toggle surface types on/off
        private void ToggleObservedSurfaceType(SpatialAwarenessSurfaceTypes surfaceType)
        {
            if (observer.SurfaceTypes.IsMaskSet(surfaceType))
            {
                observer.SurfaceTypes &= ~surfaceType;
            }
            else
            {
                observer.SurfaceTypes |= surfaceType;
            }
        }

        //This will set all far away scene objects as inactive and only show the closest wall i.e the one to spawn sensors/GW on
        public void refresh()
        {
            if (wallToggle.isActiveAndEnabled)
            {
                FindClosestQuad(SpatialAwarenessSurfaceTypes.Wall);
            }

            if (ceilingToggle.isActiveAndEnabled)
            {
                FindClosestQuad(SpatialAwarenessSurfaceTypes.Ceiling);
            }
        }

        //Used to find closest quad of surfaceType
        public int FindClosestQuad(SpatialAwarenessSurfaceTypes surfaceType)
        {
            var playerObject = GameObject.Find("MainCamera");
            var playerPos = playerObject.transform.position;
            float Distance;
            int nearestId = -1;
            float smallestDistance = float.MaxValue;
            if (observedSceneObjects.TryGetValue(surfaceType, out Dictionary<int, SpatialAwarenessSceneObject> QuadInScene))
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
        #endregion Helper Functions

        // this is used to spawn sensors 
        public void OnDemandSensor()
        {
            if (!CurrentPrefabSet)
            {
                observedSceneObjects.TryGetValue(SpatialAwarenessSurfaceTypes.Wall, out Dictionary<int, SpatialAwarenessSceneObject> WallsInScene);
               int nearestId =  FindClosestQuad(SpatialAwarenessSurfaceTypes.Wall);
                if (nearestId == -1)
                {
                    Debug.Log("No walls found");
                }
                else
                {
                    CurrentPrefab = Instantiate(InstantiatedSensor);
                    CurrentPrefab.transform.SetPositionAndRotation(WallsInScene[nearestId].Position, WallsInScene[nearestId].Rotation);
                    CurrentPrefabSet = true;
                    CurrentSensor = true;
                }
            }
        }
        // this is used to spawn GWs
        public void OnDemandGW()
        {
            if (!CurrentPrefabSet)
            {
                observedSceneObjects.TryGetValue(SpatialAwarenessSurfaceTypes.Ceiling, out Dictionary<int, SpatialAwarenessSceneObject> CeillingInScene);
                int nearestId =  FindClosestQuad(SpatialAwarenessSurfaceTypes.Ceiling);
                if (nearestId == -1)
                {
                    Debug.Log("No ceillings found");
                }
                else
                {
                    CurrentPrefab = Instantiate(InstantiatedGateWay);
                    CurrentPrefab.transform.SetPositionAndRotation(CeillingInScene[nearestId].Position, CeillingInScene[nearestId].Rotation);
                    CurrentPrefabSet = true;
                    CurrentGW = true;
                }
            }
        }

        // used to store instantiated sensors/GW to list and save them as anchor in file
        public async void SavePrefabs()
        {
            if (CurrentPrefab != null)
            {
                await OnStartSession ();
                instantiatedPrefabs.Add(CurrentPrefab);
                CurrentPrefabSet = false;
                if (CurrentGW)
                {
                    InstantiatedGWs.Add(CurrentPrefab);
                    Debug.Log("Creating GWs...");
                    await CreateAnchor(InstantiatedGWs, "SavedGWIDs.txt");
                    CurrentGW = false;
                }
                else if (CurrentSensor)
                {
                    InstantiatedSensors.Add(CurrentPrefab);
                    Debug.Log("Creating Sensors...");
                    await CreateAnchor(InstantiatedSensors, "SavedSensorIDs.txt");
                    CurrentSensor = false;
                }
                OnDeleteSession();
            }
            else
                Debug.Log("No Sensors or gateways in scene present");
        }

        float DistanceBetween2Vectors(Vector3 PrefabPosition, Vector3 PlayerPosition)
        {
            float SmallestDistance = Vector3.Distance(PrefabPosition, PlayerPosition);
            return SmallestDistance;
        }

        public async Task OnStartSession()
        {
            Debug.Log("starting...");
            await _spatialAnchorManager.StartSessionAsync();
            return;
            
        }

        public void OnDeleteSession()
        {
            _spatialAnchorManager.DestroySession();
            _foundOrCreatedAnchorGameObjects.Clear();
            Debug.Log("Deleted");
        }
        //these functions to create anchors

        private async Task CreateAnchor(List<GameObject> ListToAnchor , string TextFileName)
        {                   
                    //Create Anchor GameObject. We will use ASA to save the position and the rotation of this GameObject.
                    GameObject anchorGameObject = ListToAnchor[ListToAnchor.Count -1];
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
                        WriteToFile(TextFileName, cloudSpatialAnchor.Identifier);   
                    }
                    catch (Exception exception)
                    {
                        Debug.Log("ASA - Failed to save anchor: " + exception.ToString());
                        Debug.LogException(exception);
                    }
        }
        //functions to locate anchors 
        public async void OnLocate()
        {
            await OnStartSession();
            OnLocateSensors();           
            await OnLocateGateWays();
            OnDeleteSession();
        }

        public void OnLocateSensors()
        {
          if (instantiatedPrefabs.Count == 0)
          {
                Debug.Log("Locating Sensors..."); 
                LocateAnchor("SavedSensorIDs.txt");
                Debug.Log("Located");
          }
          else
          {
                Debug.Log("Please Clear Scene"); // cant locate with sonsors/anchors already in scenerefresh();
          }
            return;
        }

        public async Task OnLocateGateWays()
        {   //while loop to ensure watcher has done its job and sensors are spawned
            while (FoundSensors == null || FoundSensors.Length != _createdAnchorIDs.Count)
            {
                FoundSensors = GameObject.FindGameObjectsWithTag("Sensor");
                await Task.Delay(5000);
            }

            if (instantiatedPrefabs.Count == _createdAnchorIDs.Count)
            {
                Debug.Log("Locating GWs...");
                 LocateAnchor ("SavedGWIDs.txt");
                //while loop to ensure watcher finishs its job 
                while (FoundGWs == null || FoundGWs.Length != _createdAnchorIDs.Count)
                {
                    FoundGWs = GameObject.FindGameObjectsWithTag("GW");
                    await Task.Delay(5000);
                }
            }
            else
            {
                Debug.Log("please locate sensors first"); 
            }
            return;
        }

        private void LocateAnchor(string FileName)
        {
            _createdAnchorIDs =  ReadFromFile(FileName);
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
                    //Create GameObject
                    if (!SensorsCreated) // start by creating sensors
                    {
                        GameObject anchorGameObject = Instantiate(InstantiatedSensor);
                        instantiatedPrefabs.Add(anchorGameObject);
                        // Link to Cloud Anchor
                        anchorGameObject.AddComponent<CloudNativeAnchor>().CloudToNative(cloudSpatialAnchor);
                        _foundOrCreatedAnchorGameObjects.Add(anchorGameObject);
                    }
                    else
                    {
                        GameObject anchorGameObject = Instantiate(InstantiatedGateWay);
                        instantiatedPrefabs.Add(anchorGameObject);
                        // Link to Cloud Anchor
                        anchorGameObject.AddComponent<CloudNativeAnchor>().CloudToNative(cloudSpatialAnchor);
                        _foundOrCreatedAnchorGameObjects.Add(anchorGameObject);
                        
                    }
                    if (instantiatedPrefabs.Count == _createdAnchorIDs.Count) SensorsCreated = true;
               });
            }
        }

        //helper functions for saving/locating anchors
        public List<String> ReadFromFile(string NameFile)
        {
            string filename = NameFile;
            string path = Application.persistentDataPath;
            #if WINDOWS_UWP
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                path = storageFolder.Path.Replace('\\', '/') + "/";
            #endif
            string filePath = Path.Combine(path, filename);
            string currentAzureAnchorID = File.ReadAllText(filePath);
            List<String> ResultedList = currentAzureAnchorID.Split(',').ToList();
            ResultedList.RemoveAt(ResultedList.Count - 1);          
            return ResultedList;
        }

        public void WriteToFile(string NameFile, string ID)
        {
            string filename = NameFile;
            string path = Application.persistentDataPath;
            #if WINDOWS_UWP
                        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                        path = storageFolder.Path.Replace('\\', '/') + "/";
            #endif
            string filePath = Path.Combine(path, filename);
            File.AppendAllText(filePath, ID + ",");
        }
       // Maybe make azure stuff as class.
    }
}
