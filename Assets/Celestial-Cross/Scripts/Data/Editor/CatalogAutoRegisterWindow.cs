using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Data.Pets;

namespace CelestialCross.Editor
{
    public class CatalogAutoRegisterWindow : EditorWindow
    {
        public UnitCatalog unitCatalog;
        public PetCatalog petCatalog;

        [MenuItem("Celestial Cross/Data Config/Auto-Register Catalogs")]
        public static void ShowWindow()
        {
            GetWindow<CatalogAutoRegisterWindow>("Catalog Auto-Register");
        }

        private void OnEnable()
        {
            FindDefaultCatalogs();
        }

        private void OnGUI()
        {
            GUILayout.Label("Auto-Register Units & Pets", EditorStyles.boldLabel);

            unitCatalog = (UnitCatalog)EditorGUILayout.ObjectField("Unit Catalog", unitCatalog, typeof(UnitCatalog), false);
            petCatalog = (PetCatalog)EditorGUILayout.ObjectField("Pet Catalog", petCatalog, typeof(PetCatalog), false);

            EditorGUILayout.Space();

            if (GUILayout.Button("Find and Register All"))
            {
                RegisterAll();
            }
        }

        private void FindDefaultCatalogs()
        {
            // Tenta encontrar caso o usuário não tenha selecionado
            if (unitCatalog == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:UnitCatalog");
                if (guids.Length > 0)
                {
                    unitCatalog = AssetDatabase.LoadAssetAtPath<UnitCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (petCatalog == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:PetCatalog");
                if (guids.Length > 0)
                {
                    petCatalog = AssetDatabase.LoadAssetAtPath<PetCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
        }

        private void RegisterAll()
        {
            if (unitCatalog != null)
            {
                RegisterUnits();
            }
            else
            {
                Debug.LogWarning("UnitCatalog não atribuído.");
            }

            if (petCatalog != null)
            {
                RegisterPets();
            }
            else
            {
                Debug.LogWarning("PetCatalog não atribuído.");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Registro de catálogos concluído!");
        }

        private void RegisterUnits()
        {
            string[] guids = AssetDatabase.FindAssets("t:UnitData");
            List<UnitData> foundUnits = new List<UnitData>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(path);
                if (data != null)
                {
                    foundUnits.Add(data);
                }
            }

            SerializedObject so = new SerializedObject(unitCatalog);
            SerializedProperty entriesProp = so.FindProperty("entries");
            
            entriesProp.ClearArray();
            entriesProp.arraySize = foundUnits.Count;

            for (int i = 0; i < foundUnits.Count; i++)
            {
                SerializedProperty elementProp = entriesProp.GetArrayElementAtIndex(i);
                SerializedProperty dataProp = elementProp.FindPropertyRelative("unitData");
                dataProp.objectReferenceValue = foundUnits[i];
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(unitCatalog);
            Debug.Log($"UnitCatalog: Registradas {foundUnits.Count} Unidades.");
        }

        private void RegisterPets()
        {
            string[] guids = AssetDatabase.FindAssets("t:PetSpeciesSO");
            List<PetSpeciesSO> foundPets = new List<PetSpeciesSO>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PetSpeciesSO data = AssetDatabase.LoadAssetAtPath<PetSpeciesSO>(path);
                if (data != null)
                {
                    foundPets.Add(data);
                }
            }

            SerializedObject so = new SerializedObject(petCatalog);
            SerializedProperty entriesProp = so.FindProperty("entries");
            
            entriesProp.ClearArray();
            entriesProp.arraySize = foundPets.Count;

            for (int i = 0; i < foundPets.Count; i++)
            {
                SerializedProperty elementProp = entriesProp.GetArrayElementAtIndex(i);
                SerializedProperty dataProp = elementProp.FindPropertyRelative("petSpecies");
                dataProp.objectReferenceValue = foundPets[i];
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(petCatalog);
            Debug.Log($"PetCatalog: Registrados {foundPets.Count} Pets.");
        }
    }
}


