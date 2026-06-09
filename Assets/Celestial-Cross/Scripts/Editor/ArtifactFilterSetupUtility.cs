using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Scenes.Inventory;

namespace CelestialCross.EditorUtils
{
    public class ArtifactFilterSetupUtility : EditorWindow
    {
        [MenuItem("Celestial Cross/3. UI Builders/3. Components/Setup Artifact Filter Component")]
        public static void SetupArtifactFilter()
        {
            ArtifactFilterModal modal = FindObjectOfType<ArtifactFilterModal>(true);
            if (modal == null)
            {
                Debug.LogError("ArtifactFilterModal not found in the scene.");
                return;
            }

            // Find all Dropdowns inside the Modal
            TMP_Dropdown[] dropdowns = modal.GetComponentsInChildren<TMP_Dropdown>(true);
            
            TMP_Dropdown mainStat = null;
            TMP_Dropdown[] subStats = new TMP_Dropdown[4];
            int subIndex = 0;

            foreach (var dd in dropdowns)
            {
                string name = dd.gameObject.name.ToLower();
                if (name.Contains("main") || name.Contains("principal"))
                {
                    mainStat = dd;
                }
                else if (name.Contains("sub"))
                {
                    if (subIndex < 4)
                    {
                        subStats[subIndex] = dd;
                        subIndex++;
                    }
                }
            }

            // Fallback strategy if names don't match (assuming 5 dropdowns total)
            if (mainStat == null && dropdowns.Length >= 5)
            {
                mainStat = dropdowns[0];
                for (int i = 0; i < 4; i++)
                {
                    subStats[i] = dropdowns[i + 1];
                }
            }

            if (mainStat != null)
            {
                modal.mainStatDropdown = mainStat;
            }
            if (subIndex > 0)
            {
                modal.subStatsDropdowns = subStats;
            }

            // Setup Reset Button
            Button[] buttons = modal.GetComponentsInChildren<Button>(true);
            Button resetBtn = null;
            foreach (var btn in buttons)
            {
                string name = btn.gameObject.name.ToLower();
                if (name.Contains("reset") || name.Contains("zerar") || name.Contains("limpar"))
                {
                    resetBtn = btn;
                    break;
                }
            }

            // If reset button not found, and apply button exists, duplicate it
            if (resetBtn == null && modal.applyFilterButton != null)
            {
                GameObject resetObj = Instantiate(modal.applyFilterButton.gameObject, modal.applyFilterButton.transform.parent);
                resetObj.name = "Btn_Zerar";
                resetBtn = resetObj.GetComponent<Button>();
                
                var tmp = resetObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = "Zerar";
                else
                {
                    var txt = resetObj.GetComponentInChildren<Text>();
                    if (txt != null) txt.text = "Zerar";
                }

                // Place it immediately before the apply filter button
                resetObj.transform.SetSiblingIndex(modal.applyFilterButton.transform.GetSiblingIndex());
            }

            if (resetBtn != null)
            {
                modal.resetButton = resetBtn;
            }

            // Fix all dropdown templates
            foreach (var dd in dropdowns)
            {
                FixDropdownTemplate(dd);
            }

            // Mark the component as dirty so changes are saved in the scene
            EditorUtility.SetDirty(modal);
            if (PrefabUtility.IsPartOfPrefabInstance(modal))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(modal);
            }

            Debug.Log("ArtifactFilterModal setup completed successfully! Dropdowns and Reset button assigned.", modal.gameObject);
        }

        private static void FixDropdownTemplate(TMP_Dropdown dropdown)
        {
            if (dropdown == null || dropdown.template == null) return;
            
            ScrollRect scrollRect = dropdown.template.GetComponent<ScrollRect>();
            if (scrollRect == null) return;

            RectTransform content = scrollRect.content;
            if (content == null) return;

            // Ensure it has VerticalLayoutGroup
            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.childAlignment = TextAnchor.UpperLeft;
                vlg.childControlHeight = false;
                vlg.childControlWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = true;
            }

            // Ensure it has ContentSizeFitter
            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = content.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            // Ensure the item has a valid height
            Toggle itemToggle = content.GetComponentInChildren<Toggle>(true);
            if (itemToggle != null)
            {
                RectTransform itemRect = itemToggle.GetComponent<RectTransform>();
                if (itemRect.rect.height <= 1f)
                {
                    itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, 30f);
                }
                
                // Set layout element to enforce min height
                LayoutElement le = itemToggle.GetComponent<LayoutElement>();
                if (le == null) le = itemToggle.gameObject.AddComponent<LayoutElement>();
                if (le.minHeight < 20f) le.minHeight = 30f;
            }
        }
    }
}
