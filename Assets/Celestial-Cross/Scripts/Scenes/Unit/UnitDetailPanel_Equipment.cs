using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data;
using DG.Tweening;

namespace CelestialCross.Scenes.Unit
{
    public class UnitDetailPanel_Equipment : MonoBehaviour
    {
        [Header("Slots")]
        public Button[] artifactSlotButtons = new Button[6];
        public Image[] artifactSlotImages = new Image[6];
        
        [Header("Set Skills")]
        public Transform skillsContainer;
        public GameObject skillIconPrefab;

        [Header("Modals")]
        public UnitArtifactSelectModal artifactSelectModal;
        public ArtifactMiniInfoModal miniInfoModal;
        public ArtifactSetBonusModal setBonusModal;
        public ArtifactActionModal actionModal;
        public CelestialCross.Giulia_UI.ArtifactUpgradeModal upgradeModal;

        [Header("Sets List")]
        public Transform setListContainer;
        public GameObject setListItemPrefab;

        private UnitData currentUnitData;
        private RuntimeUnitData currentRuntimeData;
        private ArtifactSetCatalog currentArtifactCatalog;
        private string currentUnitId;
        private DG.Tweening.Sequence currentAnimSeq;

        private void Start()
        {
            if (artifactSlotButtons != null)
            {
                for (int i = 0; i < artifactSlotButtons.Length; i++)
                {
                    int slotIndex = i; // capture index
                    if (artifactSlotButtons[i] != null)
                    {
                        artifactSlotButtons[i].onClick.AddListener(() => {
                            CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ButtonClick01);
                            OnSlotClicked(slotIndex);
                        });
                    }
                }
            }
        }

        public void Refresh(UnitData unitData, RuntimeUnitData runtimeData, ArtifactSetCatalog artifactCatalog)
        {
            var acc = AccountManager.Instance?.PlayerAccount;
            if (acc == null || runtimeData == null) return;

            currentUnitData = unitData;
            currentRuntimeData = runtimeData;
            currentArtifactCatalog = artifactCatalog;
            currentUnitId = runtimeData.UnitID;

            // Limpar
            for(int i = 0; i < 6; i++)
            {
                if (artifactSlotImages[i] != null) artifactSlotImages[i].gameObject.SetActive(false);
            }

            if (skillsContainer != null)
            {
                foreach(Transform child in skillsContainer)
                {
                    if (child.gameObject != skillIconPrefab) Destroy(child.gameObject);
                }
            }

            if (setListContainer != null)
            {
                foreach(Transform child in setListContainer)
                {
                    if (child.gameObject != setListItemPrefab) Destroy(child.gameObject);
                }
            }

            var loadout = acc.GetLoadoutForUnit(runtimeData.UnitID);
            if (loadout == null) return;

            CelestialCross.Artifacts.ArtifactType[] slotTypes = (CelestialCross.Artifacts.ArtifactType[])global::System.Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType));
            var equippedSets = new global::System.Collections.Generic.Dictionary<CelestialCross.Artifacts.ArtifactSet, int>();

            for (int i = 0; i < 6; i++)
            {
                var sType = slotTypes[i];
                string equippedGuid = null;

                switch (sType)
                {
                    case CelestialCross.Artifacts.ArtifactType.Helmet: equippedGuid = loadout.HelmetID; break;
                    case CelestialCross.Artifacts.ArtifactType.Chestplate: equippedGuid = loadout.ChestplateID; break;
                    case CelestialCross.Artifacts.ArtifactType.Gloves: equippedGuid = loadout.GlovesID; break;
                    case CelestialCross.Artifacts.ArtifactType.Boots:  equippedGuid = loadout.BootsID; break;
                    case CelestialCross.Artifacts.ArtifactType.Necklace: equippedGuid = loadout.NecklaceID; break;
                    case CelestialCross.Artifacts.ArtifactType.Ring:   equippedGuid = loadout.RingID; break;
                }

                if (!string.IsNullOrEmpty(equippedGuid))
                {
                    var artifact = acc.GetArtifactByGuid(equippedGuid);
                    if (artifact != null && artifactCatalog != null)
                    {
                        var set = artifactCatalog.GetSetById(artifact.artifactSetId);
                        if (set != null)
                        {
                            if (artifactSlotImages[i] != null)
                            {
                                artifactSlotImages[i].sprite = set.GetIconForSlot(artifact.slot);
                                artifactSlotImages[i].gameObject.SetActive(true);
                                artifactSlotImages[i].preserveAspect = true;
                            }
                            
                            if (equippedSets.ContainsKey(set))
                                equippedSets[set]++;
                            else
                                equippedSets[set] = 1;
                        }
                    }
                }
            }

            currentAnimSeq?.Kill();
            currentAnimSeq = DG.Tweening.DOTween.Sequence();
            currentAnimSeq.SetUpdate(true);
            currentAnimSeq.SetLink(gameObject);
            float delay = 0f;

            for (int i = 0; i < artifactSlotButtons.Length; i++)
            {
                if (artifactSlotButtons[i] != null)
                {
                    artifactSlotButtons[i].transform.DOKill();
                    artifactSlotButtons[i].transform.localScale = Vector3.zero;
                    currentAnimSeq.Insert(delay, artifactSlotButtons[i].transform.DOScale(1f, 0.3f).SetEase(DG.Tweening.Ease.OutBack).SetLink(artifactSlotButtons[i].gameObject));
                    delay += 0.05f;
                }
            }

            if (setListContainer != null && setListItemPrefab != null && setBonusModal != null)
            {
                float setDelay = delay;
                foreach(var kvp in equippedSets)
                {
                    var set = kvp.Key;
                    int count = kvp.Value;
                    
                    var go = Instantiate(setListItemPrefab, setListContainer);
                    go.SetActive(true);
                    
                    var txt = go.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        string sName = string.IsNullOrEmpty(set.setName) ? set.name : set.setName;
                        txt.text = $"{sName} x{count}";
                    }
                    
                    var btn = go.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.AddListener(() => {
                            CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ButtonClick01);
                            setBonusModal.Show(set, count);
                        });
                    }

                    go.transform.DOKill();
                    go.transform.localScale = Vector3.zero;
                    currentAnimSeq.Insert(setDelay, go.transform.DOScale(1f, 0.3f).SetEase(DG.Tweening.Ease.OutBack).SetLink(go));
                    setDelay += 0.05f;
                }
            }
        }

        private void OnSlotClicked(int index)
        {
            CelestialCross.Artifacts.ArtifactType[] slotTypes = (CelestialCross.Artifacts.ArtifactType[])global::System.Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType));
            if (index < 0 || index >= slotTypes.Length) return;

            var acc = AccountManager.Instance?.PlayerAccount;
            var loadout = acc?.GetLoadoutForUnit(currentUnitId);
            string equippedGuid = null;

            if (loadout != null)
            {
                switch (slotTypes[index])
                {
                    case CelestialCross.Artifacts.ArtifactType.Helmet: equippedGuid = loadout.HelmetID; break;
                    case CelestialCross.Artifacts.ArtifactType.Chestplate: equippedGuid = loadout.ChestplateID; break;
                    case CelestialCross.Artifacts.ArtifactType.Gloves: equippedGuid = loadout.GlovesID; break;
                    case CelestialCross.Artifacts.ArtifactType.Boots:  equippedGuid = loadout.BootsID; break;
                    case CelestialCross.Artifacts.ArtifactType.Necklace: equippedGuid = loadout.NecklaceID; break;
                    case CelestialCross.Artifacts.ArtifactType.Ring:   equippedGuid = loadout.RingID; break;
                }
            }

            if (!string.IsNullOrEmpty(currentUnitId))
            {
                if (UnitSelectionFlowHelper.Instance != null)
                {
                    UnitSelectionFlowHelper.Instance.GoToInventoryForArtifact(currentUnitId, slotTypes[index]);
                }
                else
                {
                    Debug.LogWarning("[UnitDetailPanel_Equipment] UnitSelectionFlowHelper ausente na cena.");
                }
            }
        }
    }
}
