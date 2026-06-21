using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CelestialCross.Audio;

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
        private global::System.Action onEquipCallback;

        private DG.Tweening.Sequence currentAnimSeq;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(() => { CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.MenuClose01); Hide(); });
            if (equipButton != null) equipButton.onClick.AddListener(() => { CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ItemEquip01); OnEquipClicked(); });
        }

        public void Show(string unitId, global::System.Action onCompleteCallback)
        {
            currentUnitId = unitId;
            onEquipCallback = onCompleteCallback;
            selectedPetId = string.Empty;
            
            if (UnitSceneController.Instance != null) UnitSceneController.Instance.ShowModalOverlay();

            transform.SetAsLastSibling();
            gameObject.SetActive(true);

            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill();
                rect.localScale = Vector3.zero;
                rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            PopulateGrid();
        }

        public void Hide()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null && gameObject.activeSelf)
            {
                rect.DOKill();
                rect.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                    gameObject.SetActive(false);
                    if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
                    onEquipCallback?.Invoke();
                });
            }
            else
            {
                gameObject.SetActive(false);
                if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
                onEquipCallback?.Invoke();
            }
        }

        private void PopulateGrid()
        {
            var account = global::AccountManager.Instance?.PlayerAccount;
            if (account == null) return;

            foreach (Transform child in gridContainer) Destroy(child.gameObject);

            if (itemPrefab == null)
            {
                itemPrefab = new GameObject("PetItemPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
                itemPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 200);
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

            currentAnimSeq?.Kill();
            currentAnimSeq = DG.Tweening.DOTween.Sequence();
            currentAnimSeq.SetUpdate(true);
            currentAnimSeq.SetLink(gameObject);
            float delay = 0.2f;

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
                    var petData = pet;
                    btn.onClick.AddListener(() => {
                        CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ButtonClick01);
                        selectedPetId = petData.UUID;
                        PopulateGrid(); 
                    });
                }

                go.transform.DOKill();
                go.transform.localScale = Vector3.zero;
                currentAnimSeq.Insert(delay, go.transform.DOScale(1f, 0.2f).SetEase(DG.Tweening.Ease.OutQuad).SetLink(go));
                delay += 0.03f;
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
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(SoundKey.ItemEquip02);
                    }
                }
            }
            Hide();
        }
    }
}
