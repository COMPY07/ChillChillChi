#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class MapBuildSetup
{
    // 필요한 레이어들을 정의합니다.
    private static readonly string[] requiredLayers = new string[]
    {
        "Floor",
        "Structure",
        "Wall",
        "Monster",
        "Player",
    };

    static MapBuildSetup()
    {
        SetupLayers();
    }

    static void SetupLayers()
    {
        // TagManager 에셋을 가져옵니다
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layerProps = tagManager.FindProperty("layers");

        if (layerProps == null)
        {
            Debug.LogError("Couldn't find layers property in TagManager.");
            return;
        }

        // 각 필요한 레이어에 대해
        foreach (string layerName in requiredLayers)
        {
            bool layerExists = false;
            
            // 이미 존재하는지 확인
            for (int i = 8; i < 32; i++)
            {
                var sp = layerProps.GetArrayElementAtIndex(i);
                if (sp.stringValue == layerName)
                {
                    layerExists = true;
                    Debug.Log($"Layer {layerName} already exists at {i}");
                    break;
                }
            }

            // 레이어가 없다면 빈 슬롯을 찾아 추가
            if (!layerExists)
            {
                for (int i = 8; i < 32; i++)
                {
                    var sp = layerProps.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(sp.stringValue))
                    {
                        sp.stringValue = layerName;
                        Debug.Log($"Added layer {layerName} at {i}");
                        break;
                    }
                }
            }
        }

        // 변경사항을 저장합니다
        tagManager.ApplyModifiedProperties();
        
        // 정렬 레이어도 설정합니다
        SetupSortingLayers();
    }

    static void SetupSortingLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var sortingLayerProps = tagManager.FindProperty("m_SortingLayers");

        if (sortingLayerProps == null)
        {
            Debug.LogError("Couldn't find sorting layers property in TagManager.");
            return;
        }

        // 현재 존재하는 정렬 레이어들을 확인
        HashSet<string> existingLayers = new HashSet<string>();
        for (int i = 0; i < sortingLayerProps.arraySize; i++)
        {
            var layerProp = sortingLayerProps.GetArrayElementAtIndex(i);
            var nameProp = layerProp.FindPropertyRelative("name");
            if (nameProp != null)
            {
                existingLayers.Add(nameProp.stringValue);
            }
        }

        // 필요한 레이어들을 추가
        foreach (string layerName in requiredLayers)
        {
            if (!existingLayers.Contains(layerName))
            {
                sortingLayerProps.arraySize++;
                var newLayer = sortingLayerProps.GetArrayElementAtIndex(sortingLayerProps.arraySize - 1);
                
                newLayer.FindPropertyRelative("name").stringValue = layerName;
                newLayer.FindPropertyRelative("uniqueID").intValue = layerName.GetHashCode();
                
                Debug.Log($"Added sorting layer: {layerName}");
            }
        }

        // 변경사항을 저장
        tagManager.ApplyModifiedProperties();
    }

    // 레이어 인덱스를 가져오는 메서드
    public static int GetLayerIndex(string layerName)
    {
        return LayerMask.NameToLayer(layerName);
    }
}
#endif