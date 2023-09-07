using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MemorySaver : MonoBehaviour
{
    // Start is called before the first frame update
    // this will save the prefabs last location, probably add prefab id and then retrieve them based on id will see
    //PlayerPrefs has player preferences aka what does he own and stats could be made into okay my player owns 20 sensors around a map

    public void OnManipulationEnded()
    {
        // Store away our local position, rotation and scale in settings type storage.
        var srtToString = this.LocalSRTToString();

        Debug.Log($"MT: Written out SRT to string of {srtToString}");

        PlayerPrefs.SetString(this.gameObject.name, srtToString);
        PlayerPrefs.Save();
    }
    string LocalSRTToString()
    {
        var t = this.gameObject.transform.localPosition;
        var s = this.gameObject.transform.localScale;
        var r = this.gameObject.transform.localRotation;

        return ($"{Vector3ToString(s)} {QuaternionToString(r)} {Vector3ToString(t)}");
    }


    static string Vector3ToString(Vector3 v) => $"{v.x} {v.y} {v.z}";
    static string QuaternionToString(Quaternion q) => $"{q.x} {q.y} {q.z} {q.w}";
 
}
