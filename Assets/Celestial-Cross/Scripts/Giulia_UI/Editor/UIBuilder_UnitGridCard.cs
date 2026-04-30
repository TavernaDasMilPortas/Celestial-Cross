using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.EditorArea
{
    public static class UIBuilder_UnitGridCard
    {
        [MenuItem("Celestial Cross/UI Builders/Generate Unit Grid Card Prefab")]
        public static void GenerateUnitGridCard()
        {
            InventoryUI inventory = Object.FindObjectOfType<InventoryUI>();
            if (inventory == null)
            {
                Debug.LogError("InventoryUI não encontrado!");
                return;
            }

            // Create the card root
            GameObject card = new GameObject("Prefab_UnitGridCard", typeof(RectTransform), typeof(Image), typeof(Button));
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            card.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);

            // Icon
            GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(card.transform, false);
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero; iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = iconRT.offsetMax = new Vector2(4, 4);
            iconGO.GetComponent<Image>().preserveAspect = true;

            // Level Badge (Canto superior esquerdo)
            GameObject badgeGO = new GameObject("Badge_Level", typeof(RectTransform), typeof(Image));
            badgeGO.transform.SetParent(card.transform, false);
            var badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0, 1);
            badgeRT.anchorMax = new Vector2(0, 1);
            badgeRT.pivot = new Vector2(0, 1);
            badgeRT.anchoredPosition = new Vector2(-5, 5);
            badgeRT.sizeDelta = new Vector2(40, 20);
            badgeGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            GameObject lvTxtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            lvTxtGO.transform.SetParent(badgeGO.transform, false);
            var lvTxt = lvTxtGO.GetComponent<TextMeshProUGUI>();
            lvTxt.fontSize = 12;
            lvTxt.text = "Lv.1";
            lvTxt.alignment = TextAlignmentOptions.Center;
            lvTxt.rectTransform.anchorMin = Vector2.zero; lvTxt.rectTransform.anchorMax = Vector2.one;
            lvTxt.rectTransform.offsetMin = lvTxt.rectTransform.offsetMax = Vector2.zero;

            // Constellation Mini Stars (Base do card)
            GameObject starsGO = new GameObject("MiniStars", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            starsGO.transform.SetParent(card.transform, false);
            var starsRT = starsGO.GetComponent<RectTransform>();
            starsRT.anchorMin = new Vector2(0, 0);
            starsRT.anchorMax = new Vector2(1, 0);
            starsRT.pivot = new Vector2(0.5f, 0);
            starsRT.anchoredPosition = new Vector2(0, 2);
            starsRT.sizeDelta = new Vector2(0, 10);

            var hlg = starsGO.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 2;
            hlg.childControlWidth = true; hlg.childForceExpandWidth = true;

            for (int i = 0; i < 6; i++)
            {
                GameObject s = new GameObject("star", typeof(RectTransform), typeof(Image));
                s.transform.SetParent(starsGO.transform, false);
                s.GetComponent<Image>().color = Color.yellow;
                s.SetActive(false); // Inativo por padrão, o script ativa baseados na constelação
            }

            Debug.Log("Card Prefab gerado! Lembre-se de transformar este GameObject em Prefab e arrastar para o campo SlotPrefab do InventoryUI.");
            
            // Linkar no inventory (opcional se for apenas para gerar o objeto na cena para o user prefabetizar)
            // inventory.slotPrefab = card;
            
            Selection.activeGameObject = card;
        }
    }
}
