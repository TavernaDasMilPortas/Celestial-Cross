using UnityEngine;
using UnityEngine.UI;
using CelestialCross.Artifacts;
using DG.Tweening;
using CelestialCross.Audio;

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
        private global::System.Action onEquipCallback;
        private global::System.Action onBack;

        private DG.Tweening.Sequence currentAnimSeq;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(() => { CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.MenuClose01); Hide(); });
            if (equipButton != null) equipButton.onClick.AddListener(() => { CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ItemEquip01); OnEquipClicked(); });
            if (backButton != null) backButton.onClick.AddListener(() => { CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.MenuClose01); OnBackClicked(); });
        }

        public void Show(string unitId, ArtifactType slotType, global::System.Action onCompleteCallback, global::System.Action onBackCallback = null)
        {
            gameObject.SetActive(true);
            currentUnitId = unitId;
            currentFilterSlot = slotType;
            onEquipCallback = onCompleteCallback;
            onBack = onBackCallback;
            selectedArtifactGuid = string.Empty;

            if (backButton != null)
            {
                backButton.gameObject.SetActive(onBack != null);
            }

            if (UnitSceneController.Instance != null) UnitSceneController.Instance.ShowModalOverlay();

            transform.SetAsLastSibling();
            
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
                    onComplete?.Invoke();
                });
            }
            else
            {
                gameObject.SetActive(false);
                if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
                onComplete?.Invoke();
            }
        }

        private void OnBackClicked()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null && gameObject.activeSelf)
            {
                rect.DOKill();
                rect.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                    gameObject.SetActive(false);
                    if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
                    onBack?.Invoke();
                });
            }
            else
            {
                gameObject.SetActive(false);
                if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
                onBack?.Invoke();
            }
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

            currentAnimSeq?.Kill();
            currentAnimSeq = DG.Tweening.DOTween.Sequence();
            currentAnimSeq.SetUpdate(true);
            currentAnimSeq.SetLink(gameObject);
            float delay = 0.2f;

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
                        CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ButtonClick01);
                        selectedArtifactGuid = art.idGUID;
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
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(SoundKey.ItemEquip01);
                    }
                }
            }
            Hide();
        }
    }
}
