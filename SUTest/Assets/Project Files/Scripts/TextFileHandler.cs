using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace DataBasesLibrary
{
    public class TextFileHandler: MonoBehaviour
    {
        /// <summary>
        /// Read Anchor IDs from the file with the passed name
        /// </summary>
        public static List<String> ReadFromFile(string NameFile)
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
        /// <summary>
        /// write Anchor IDs to the file with the passed name
        /// </summary>
        public static void WriteToFile(string NameFile, string ID)
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
    }

}
