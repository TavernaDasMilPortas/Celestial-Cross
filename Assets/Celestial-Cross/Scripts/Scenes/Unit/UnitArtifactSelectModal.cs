using UnityEngine;
using UnityEngine.UI;
using CelestialCross.Artifacts;

namespace CelestialCross.Scenes.Unit
{
    public class UnitArtifactSelectModal : MonoBehaviour
    {
        [Header("UI")]
        public RectTransform gridContainer;
        public Button equipButton;
        public Button closeButton;
        public Button backButton;
        
        public GameObject itemPrefab;
        private ArtifactType currentFilterSlot;
        private string currentUnitId;
        private string selectedArtifactGuid;
        private global::System.Action onComplete;
        private global::System.Action onBack;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (equipButton != null) equipButton.onClick.AddListener(OnEquipClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        }

        public void Show(string unitId, ArtifactType slotType, global::System.Action onCompleteCallback, global::System.Action onBackCallback = null)
        {
            currentUnitId = unitId;
            currentFilterSlot = slotType;
            onComplete = onCompleteCallback;
            onBack = onBackCallback;
            selectedArtifactGuid = string.Empty;

            if (backButton != null)
            {
                backButton.gameObject.SetActive(onBack != null);
            }

            gameObject.SetActive(true);
            PopulateGrid();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        private void OnBackClicked()
        {
            gameObject.SetActive(false);
            onBack?.Invoke();
        }

        private void PopulateGrid()
        {
            var account = global::AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            foreach (Transform child in gridContainer) Destroy(child.gameObject);

            if (itemPrefab == null)
            {
                itemPrefab = new GameObject("ArtItemPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
                itemPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 250);
                var textObj = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
                textObj.transform.SetParent(itemPrefab.transform, false);
                var textComp = textObj.GetComponent<TMPro.TextMeshProUGUI>();
                textComp.alignment = TMPro.TextAlignmentOptions.Center;
                textComp.color = Color.black;
                textObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                textObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
                textObj.GetComponent<RectTransform>().offsetMin = textObj.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                itemPrefab.SetActive(false);
            }

            var artifactCatalog = UnitSceneController.Instance?.artifactSetCatalog;

            foreach (var art in account.OwnedArtifacts)
            {
                if (art == null) continue;
                if (art.slot != currentFilterSlot) continue; // Filter

                var go = Instantiate(itemPrefab, gridContainer);
                go.SetActive(true);

                var set = artifactCatalog?.GetSetById(art.artifactSetId);
                var img = go.GetComponent<Image>();
                if (img != null) 
                {
                    if (set != null) {
                        img.sprite = set.GetIconForSlot(art.slot);
                        img.preserveAspect = true;
                    }
                    img.color = (selectedArtifactGuid == art.idGUID) ? Color.yellow : Color.white;
                }

                var text = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null) text.text = set != null ? "" : $"Art {art.idGUID}";

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => {
                        selectedArtifactGuid = art.idGUID;
                        PopulateGrid();
                    });
                }
            }
        }

        private void OnEquipClicked()
        {
            if (!string.IsNullOrEmpty(selectedArtifactGuid) && !string.IsNullOrEmpty(currentUnitId))
            {
                var account = global::AccountManager.Instance?.PlayerAccount;
                if (account != null)
                {
                    var loadout = account.GetLoadoutForUnit(currentUnitId);
                    if (loadout != null)
                    {
                        account.UnequipArtifactFromAll(selectedArtifactGuid);
                        
                        switch (currentFilterSlot)
                        {
                            case ArtifactType.Helmet: loadout.HelmetID = selectedArtifactGuid; break;
                            case ArtifactType.Chestplate: loadout.ChestplateID = selectedArtifactGuid; break;
                            case ArtifactType.Gloves: loadout.GlovesID = selectedArtifactGuid; break;
                            case ArtifactType.Boots: loadout.BootsID = selectedArtifactGuid; break;
                            case ArtifactType.Necklace: loadout.NecklaceID = selectedArtifactGuid; break;
                            case ArtifactType.Ring: loadout.RingID = selectedArtifactGuid; break;
                        }

                        global::AccountManager.Instance.SaveAccount();
                    }
                }
            }
            Hide();
        }
    }
}
