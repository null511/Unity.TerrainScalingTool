using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Unity editor utility for offsetting and scaling terrains
/// while preserving the world positions of the existing content.
/// </summary>
/// <remarks>Joshua Miller © 2019 [null511@outlook.com]</remarks>
public class TerrainScalingTool : EditorWindow
{
    private readonly Dictionary<int, TerrainScalingHandle> handleMap;

    private float scale_y = 1f;
    private float offset_y = 0f;
    private bool enablePreview = true;


    public TerrainScalingTool()
    {
        handleMap = new Dictionary<int, TerrainScalingHandle>();
    }

    [MenuItem("Tools/Terrain/Scale")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(TerrainScalingTool));
        window.autoRepaintOnSceneChange = true;
    }

    public void OnGUI()
    {
        var hasNew = UpdateSelection();

        var _offset = EditorGUILayout.FloatField("Offset Y:", offset_y);
        var _scale = EditorGUILayout.Slider("Scale Y:", scale_y, 0.1f, 10f);

        GUILayout.BeginHorizontal();
        var _enablePreview = GUILayout.Toggle(enablePreview, "Preview");
        var applyButton = GUILayout.Button("Apply");
        GUILayout.EndHorizontal();

        var offsetChanged = Math.Abs(_offset - offset_y) > float.Epsilon;
        offset_y = _offset;

        var scaleChanged = Mathf.Abs(_scale - scale_y) > float.Epsilon;
        scale_y = _scale;
        
        var previewChanged = _enablePreview != enablePreview;
        enablePreview = _enablePreview;

        if (applyButton) {
            const string confirmMessage = "Are you sure you want to scale the TerrainData? You cannot undo this action.";
            var confirm = EditorUtility.DisplayDialog("Confirmation", confirmMessage, "Scale", "Cancel");
            if (!confirm) return;

            ApplyScaling();
            return;
        }

        if (!previewChanged && !scaleChanged && !offsetChanged && !hasNew) return;

        foreach (var handle in handleMap.Values) {
            if (enablePreview)
                handle.UpdatePreview(offset_y, scale_y);
            else
                handle.Revert();
        }
    }

    public void OnSelectionChange()
    {
        Repaint();
    }

    public void OnDestroy()
    {
        foreach (var key in handleMap.Keys)
            handleMap[key].Remove();

        handleMap.Clear();
    }

    private bool UpdateSelection()
    {
        var allSelectedTerrains = Selection.GetFiltered<Terrain>(SelectionMode.Deep);
        var allInstanceIds = allSelectedTerrains.Select(t => t.GetInstanceID()).ToArray();

        var removeKeys = handleMap.Keys.Where(k => !allInstanceIds.Contains(k)).ToArray();

        var newTerrains = allSelectedTerrains
            .Where(t => !handleMap.ContainsKey(t.GetInstanceID()))
            .Select(t => new TerrainScalingHandle(t))
            .ToArray();

        var total = removeKeys.Length + newTerrains.Length;
        if (total <= 0) return false;

        foreach (var key in removeKeys) {
            var handle = handleMap[key];
            handle.Remove();
            handleMap.Remove(key);
        }

        foreach (var handle in newTerrains) {
            handle.Initialize();

            var id = handle.Terrain.GetInstanceID();
            handleMap[id] = handle;
        }

        return newTerrains.Length > 0;
    }

    private void ApplyScaling()
    {
        EditorUtility.DisplayProgressBar("Scaling Terrain...", "Initializing...", 0f);

        var count = 0;
        var total = handleMap.Count;

        foreach (var key in handleMap.Keys) {
            var handle = handleMap[key];

            count++;
            var progress = count / (float)total;
            EditorUtility.DisplayProgressBar("Scaling TerrainData...", $"Scaling Terrain '{handle.Terrain.name}' ({count} of {total})...", progress);

            handle.Apply(offset_y, scale_y);
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Successful", "TerrainData has been scaled successfully.", "OK");
    }
}

internal class TerrainScalingHandle
{
    public readonly Terrain Terrain;
    private float[,] mapSource;
    private float[,] mapPreview;
    private int mapWidth, mapHeight;
    private Vector3 posSource;
    private Vector3 posPreview;
    private Vector3 sizeSource;
    private Vector3 sizePreview;

    private TerrainBoundingBox boundingBox;


    public TerrainScalingHandle(Terrain terrain)
    {
        Terrain = terrain;
    }

    public void Initialize()
    {
        var data = Terrain.terrainData;
        mapWidth = data.heightmapWidth;
        mapHeight = data.heightmapHeight;
        mapSource = data.GetHeights(0, 0, mapWidth, mapHeight);
        mapPreview = new float[mapWidth, mapHeight];

        posSource = Terrain.transform.localPosition;
        sizeSource = data.size;

        posPreview.x = posSource.x;
        posPreview.z = posSource.z;
        sizePreview.x = sizeSource.x;
        sizePreview.z = sizeSource.z;

        boundingBox = Terrain.gameObject.GetComponent<TerrainBoundingBox>()
            ?? Terrain.gameObject.AddComponent<TerrainBoundingBox>();
    }

    public void Remove()
    {
        Revert();

        if (boundingBox != null)
            Object.DestroyImmediate(boundingBox);
    }

    public void UpdatePreview(float offset, float scale)
    {
        posPreview.y = posSource.y + offset;
        Terrain.transform.localPosition = posPreview;

        sizePreview.y = sizeSource.y * scale;
        Terrain.terrainData.size = sizePreview;

        var scaleInverse = 1f / scale;
        var scaledOffset = offset / sizePreview.y;

        for (var y = 0; y < mapHeight; y++) {
            for (var x = 0; x < mapWidth; x++) {
                var scaledHeight = (mapSource[x, y]) * scaleInverse;
                mapPreview[x, y] = Mathf.Clamp01(scaledHeight - scaledOffset);
            }
        }

        var data = Terrain.terrainData;
        data.SetHeights(0, 0, mapPreview);
    }

    public void Apply(float offset, float scale)
    {
        posSource.y += offset;
        Terrain.transform.localPosition = posSource;

        sizeSource.y *= scale;
        Terrain.terrainData.size = sizeSource;

        var scaleInverse = 1f / scale;
        var scaledOffset = offset / sizeSource.y;

        for (var y = 0; y < mapHeight; y++) {
            for (var x = 0; x < mapWidth; x++) {
                var scaledHeight = (mapSource[x, y]) * scaleInverse;
                mapSource[x, y] = Mathf.Clamp01(scaledHeight - scaledOffset);
            }
        }

        Terrain.terrainData.SetHeights(0, 0, mapSource);
    }

    public void Revert()
    {
        var data = Terrain.terrainData;
        data.SetHeights(0, 0, mapSource);
        Terrain.transform.localPosition = posSource;
        data.size = sizeSource;
    }
}

[ExecuteInEditMode] 
public class TerrainBoundingBox : MonoBehaviour
{
    private Terrain terrain;


    public void Start()
    {
        terrain = GetComponent<Terrain>();
    }

    public void OnDrawGizmosSelected()
    {
        var pos = transform.position;
        pos += terrain.terrainData.size * 0.5f;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(pos, transform.rotation, terrain.terrainData.size);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
