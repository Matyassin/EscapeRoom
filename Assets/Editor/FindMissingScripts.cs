using UnityEditor;
using UnityEngine;

public static class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void FindMissingScriptsEverywhere()
    {
        FindInOpenScenes();
        FindInPrefabs();
    }

    private static void FindInOpenScenes()
    {
        GameObject[] objects = Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (GameObject go in objects)
            CheckGameObject(go, "Scene");
    }

    private static void FindInPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                continue;

            foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                CheckGameObject(child.gameObject, path);
        }
    }

    private static void CheckGameObject(GameObject go, string source)
    {
        Component[] components = go.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component == null)
                Debug.LogWarning($"Missing script found on {source}: {GetPath(go)}", go);
        }
    }

    private static string GetPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}
