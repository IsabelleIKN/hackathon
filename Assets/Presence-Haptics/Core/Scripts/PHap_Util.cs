using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Presence
{

    /// <summary> Contains Presense Haptic Utility Functions </summary>
    public static class PHap_Util
    {

        public static string PrintContents<T>(T[] array)
        {
            if (array.Length == 0)
                return "[]";
            string res = "[" + array[0].ToString();
            for (int i = 1; i < array.Length; i++)
            {
                res += ", " + array[i].ToString();
            }
            return res + "]";
        }

        /// <summary> Returns the index of obj inside list of it exists, otherwise returns -1 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int ListIndex<T>(List<T> list, T obj) where T : Object //Object since both Component (and by extension, Monobehaviour) and ScriptableObject inherit this.
        {
            for (int i=0; i<list.Count; i++)
            {
                if (list[i] == obj)
                    return 1;
            }
            return -1;
        }


        public static bool ContainsAllTypes(PHap_HapticModality[] uniqueModalities)
        {
            return uniqueModalities.Length >= ((int)PHap_HapticModality.All) - 1; //minus one because Unknown is 0. But we won't count it as it (should) never be on this list(!)
        }

        public static bool ContainsAllTypes(List<PHap_HapticModality> uniqueModalities)
        {
            return uniqueModalities.Count >= ((int)PHap_HapticModality.All) - 1; //minus one because Unknown is 0. But we won't count it as it (should) never be on this list(!)
        }

        public static bool ContainsAllLocations(PHap_BodyPart[] uniqueBodyParts)
        {
            return uniqueBodyParts.Length >= ((int)PHap_BodyPart.All) - 1; //minus one because Unknown is 0. But we won't count it as it (should) never be on this list(!)
        }

        public static bool ContainsAllLocations(List<PHap_BodyPart> uniqueBodyParts)
        {
            return uniqueBodyParts.Count >= ((int)PHap_BodyPart.All) - 1; //minus one because Unknown is 0. But we won't count it as it (should) never be on this list(!)
        }


        public static bool IsScriptableObject<T>(string filePath) where T : ScriptableObject
        {
#if UNITY_EDITOR
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(filePath);
            return asset != null;
#else
        return false;
#endif
        }


        /// <summary> Attempt to acquire a Component from a collider during initial collision </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool TryGetComponentFromCollision<T>(Collider col, bool checkConnectedBody, out T component) where T : MonoBehaviour
        {
            component = col.GetComponent<T>();
            if (component == null && checkConnectedBody && col.attachedRigidbody != null)
                component = col.attachedRigidbody.GetComponent<T>();
            return component != null;
        }


        public static bool TryLoadScriptableObject<T>(string editorPath, out T loadedResource) where T : ScriptableObject
        {
            loadedResource = null;

#if UNITY_EDITOR
            // Load the asset at the given file path in the Editor
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(editorPath);
            if (asset == null) 
            {
                Debug.LogWarning("No asset found at " + editorPath);
                return false;
            }
    
            loadedResource = asset;
#else
            // Build Time implementation
            if (ToResourcesPath(editorPath, out string runtimePath))
            {
                loadedResource = Resources.Load<T>(runtimePath);
                
                if (loadedResource == null)
                {
                    Debug.LogError($"Failed to load {typeof(T).Name} from Resources at path: '{runtimePath}'");
                }
            }
            else
            {
                Debug.LogError($"Cannot load {editorPath} at runtime. It is not located in a 'Resources' folder.");
            }
#endif

            return loadedResource != null;
        }

        public static bool ToResourcesPath(string assetPath, out string relativePath)
        {
            relativePath = "";
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }
            
            var normalizedPath = assetPath.Replace("\\", "/");
            var index = normalizedPath.IndexOf("/Resources/", System.StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                return false;
            }
            
            relativePath = normalizedPath.Substring(index + "/Resources/".Length);
            var extIndex = relativePath.LastIndexOf('.');
            if (extIndex != -1)
            {
                relativePath = relativePath.Substring(0, extIndex);
            }

            return true;
        }


        public static bool LoadFromStreamingAssets(string editorPath, out string contents)
        {
            contents = "";
            string fileName = System.IO.Path.GetFileName(editorPath);
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
            // For standalone builds and the editor. Test Android?
            if (System.IO.File.Exists(filePath))
            {
                contents = System.IO.File.ReadAllText(filePath);
                Debug.Log("Loaded all text from StreamingAssets file " + fileName);
                return true;
            }
            return false;
        }


        /// <summary> Editor only function: Retrieve the path of an Object linked via the inspector. Replaces all / with / </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Editor_GetFilePath(Object obj)
        {
#if UNITY_EDITOR
            return obj != null ? UnityEditor.AssetDatabase.GetAssetPath(obj).Replace('\\', '/') : "";
#else
        return "";
#endif
        }


        /// <summary> Returns true if the file at Path is of a particular ScriptableObject Type T </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool Editor_IsScribtableObjectType<T>(string filePath) where T : ScriptableObject
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogWarning("File path is empty or null.");
                return false;
            }
            // Load the asset at the given file path
            Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(filePath);
            if (asset == null)
            {
                Debug.LogWarning("No asset found at " + filePath);
                return false;
            }
            return asset is T;
#else
        return false;
#endif
        }


        /// <summary> Returns true if the file at Path is of a particular ScriptableObject Type T </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool Editor_LoadScriptableObjectType<T>(string filePath, out T scriptable) where T : ScriptableObject
        {
            scriptable = null;
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogWarning("File path is empty or null.");
                return false;
            }
            // Load the asset at the given file path
            Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(filePath);
            if (asset == null)
            {
                Debug.LogWarning("No asset found at " + filePath);
                return false;
            }
            if (asset is T)
            {
                scriptable = (T)asset;
                return true;
            }
#endif
            return false;
        }



        public static Vector3[] CalculateCubeCorners(Vector3 center, Vector3 size)
        {
            Vector3 halfSize = size / 2;
            // Calculate the 8 corners of the cube. TransformPoint puts them in World Space relative to the Origin.
            return new Vector3[8]
            {
                ( center + new Vector3(-halfSize.x, -halfSize.y,  halfSize.z) ), // Bottom-front-left
                ( center + new Vector3( halfSize.x, -halfSize.y,  halfSize.z) ), // Bottom-front-right
                ( center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z) ), // Bottom-back-left
                ( center + new Vector3( halfSize.x, -halfSize.y, -halfSize.z) ), // Bottom-back-right

                ( center + new Vector3(-halfSize.x,  halfSize.y,  halfSize.z) ), // Top-front-left
                ( center + new Vector3( halfSize.x,  halfSize.y,  halfSize.z) ), // Top-front-right
                ( center + new Vector3(-halfSize.x,  halfSize.y, -halfSize.z) ), // Top-back-left
                ( center + new Vector3( halfSize.x,  halfSize.y, -halfSize.z) )  // Top-back-right
            };
        }


        public static void DrawDebugCube(Vector3[] c, Color colr)
        {
            if (c.Length < 8)
                return;

            DrawDebugCube(c[0], c[1], c[2], c[3], c[4], c[5], c[6], c[7], colr);
        }

        public static void DrawDebugCube(Vector3 bfl, Vector3 bfr, Vector3 bbl, Vector3 bbr, Vector3 tfl, Vector3 tfr, Vector3 tbl, Vector3 tbr, Color colr)
        {
            //bottom square
            Debug.DrawLine(bfl, bfr, colr);
            Debug.DrawLine(bfr, bbr, colr);
            Debug.DrawLine(bbr, bbl, colr);
            Debug.DrawLine(bbl, bfl, colr);

            //top square
            Debug.DrawLine(tfl, tfr, colr);
            Debug.DrawLine(tfr, tbr, colr);
            Debug.DrawLine(tbr, tbl, colr);
            Debug.DrawLine(tbl, tfl, colr);

            //vertical lines
            Debug.DrawLine(bfl, tfl, colr);
            Debug.DrawLine(bfr, tfr, colr);
            Debug.DrawLine(bbl, tbl, colr);
            Debug.DrawLine(bbr, tbr, colr);
        }
    }
}