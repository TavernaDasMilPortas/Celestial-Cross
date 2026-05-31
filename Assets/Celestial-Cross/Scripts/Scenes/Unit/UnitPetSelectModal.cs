using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.Scenes.Unit
{
    public class UnitPetSelectModal : MonoBehaviour
    {
        [Header("UI")]
        public RectTransform gridContainer;
        public Button equipButton;
        public Button closeButton;
        
        public GameObject itemPrefab;
        private string currentUnitId;
        private string selectedPetId;
        private global::System.Action onComplete;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (equipButton != null) equipButton.onClick.AddListener(OnEquipClicked);
        }

        public void Show(string unitId, global::System.Action onCompleteCallback)
        {
            currentUnitId = unitId;
            onComplete = onCompleteCallback;
            selectedPetId = string.Empty;
            gameObject.SetActive(true);
            PopulateGrid();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        private void PopulateGrid()
        {
            var account = global::AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            foreach (Transform child in gridContainer) Destroy(child.gameObject);

            if (itemPrefab == null)
            {
                itemPrefab = new GameObject("PetItemPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
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

            var petCatalog = UnitSceneController.Instance?.petCatalog;

            foreach (var pet in account.OwnedRuntimePets)
            {
                if (pet == null) continue;

                var go = Instantiate(itemPrefab, gridContainer);
                go.SetActive(true);

                var species = petCatalog?.GetPetSpecies(pet.SpeciesID);
                var img = go.GetComponent<Image>();
                if (img != null) 
                {
                    if (species != null) {
                        img.sprite = species.sprite;
                        img.preserveAspect = true;
                    }
                    img.color = (selectedPetId == pet.UUID) ? Color.yellow : Color.white;
                }

                var text = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null) text.text = species != null ? "" : $"Pet {pet.UUID}"; 

                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => {
                        selectedPetId = pet.UUID;
                        PopulateGrid(); 
                    });
                }
            }
        }

        private void OnEquipClicked()
        {
            if (!string.IsNullOrEmpty(selectedPetId) && !string.IsNullOrEmpty(currentUnitId))
            {
                var account = global::AccountManager.Instance?.PlayerAccount;
                if (account != null)
                {
                    var loadout = account.GetLoadoutForUnit(currentUnitId);
                    if (loadout != null)
                    {
                        account.UnequipPetFromAll(selectedPetId);
                        loadout.PetID = selectedPetId;
                        global::AccountManager.Instance.SaveAccount();
                    }
                }
            }
            Hide();
        }
    }
}
