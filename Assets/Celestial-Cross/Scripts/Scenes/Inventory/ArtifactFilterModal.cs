using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CelestialCross.Artifacts;

namespace CelestialCross.Scenes.Inventory
{
    public class ArtifactFilterData
    {
        public List<string> sets = new List<string>();
        public List<CelestialCross.Artifacts.ArtifactType> types = new List<CelestialCross.Artifacts.ArtifactType>();
        public string mainStat = "Qualquer";
        public List<string> subStats = new List<string>();
    }

    public class ArtifactFilterModal : MonoBehaviour
    {
        public global::System.Action<ArtifactFilterData> OnFilterApplied;

        [Header("Containers")]
        public Transform setsGridContainer;
        public Transform typesGridContainer;
        
        [Header("Dropdowns")]
        public TMP_Dropdown mainStatDropdown;
        public TMP_Dropdown[] subStatsDropdowns = new TMP_Dropdown[4];

        [Header("Prefabs")]
        public GameObject filterIconPrefab; // Prefab com botão e highlight
        
        [Header("Actions")]
        public Button applyFilterButton;
        public Button closeButton;
        public Button resetButton;

        private List<string> selectedSetIDs = new List<string>();
        private List<ArtifactType> selectedTypes = new List<ArtifactType>();

        private void Awake()
        {
            if (applyFilterButton != null) applyFilterButton.onClick.AddListener(ApplyFilter);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (resetButton != null) resetButton.onClick.AddListener(ResetFilter);

            SetupDropdownListeners();
        }

        private bool isUpdatingDropdowns = false;

        private void Start()
        {
            // Inicializa as opções
            UpdateDropdownOptions();
            InitializeGrids();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            UpdateDropdownOptions();
            UpdateGridHighlights();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void InitializeGrids()
        {
            // Limpa containers por garantia
            if (setsGridContainer != null)
            {
                foreach (Transform child in setsGridContainer) Destroy(child.gameObject);
            }
            if (typesGridContainer != null)
            {
                foreach (Transform child in typesGridContainer) Destroy(child.gameObject);
            }

            if (filterIconPrefab == null) return;

            // 1. Popular Slots (Types)
            var allTypes = (ArtifactType[])global::System.Enum.GetValues(typeof(ArtifactType));
            foreach (var type in allTypes)
            {
                var typeBtnObj = Instantiate(filterIconPrefab, typesGridContainer);
                typeBtnObj.SetActive(true);

                // Set label text
                var labelText = typeBtnObj.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (labelText != null)
                {
                    labelText.text = GetAbbreviatedType(type);
                }

                // Definir cor de fundo para a imagem
                var btnImg = typeBtnObj.GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = new Color(0.18f, 0.18f, 0.25f, 1f); // Tom escuro azulado
                }

                var highlight = typeBtnObj.transform.Find("Highlight")?.gameObject;
                if (highlight != null) highlight.SetActive(selectedTypes.Contains(type));

                // Click listener
                var btn = typeBtnObj.GetComponent<Button>();
                var t = type; // captura local
                if (btn != null)
                {
                    btn.onClick.AddListener(() => {
                        if (selectedTypes.Contains(t))
                        {
                            selectedTypes.Remove(t);
                            if (highlight != null) highlight.SetActive(false);
                        }
                        else
                        {
                            selectedTypes.Add(t);
                            if (highlight != null) highlight.SetActive(true);
                        }
                    });
                }
            }

            // 2. Popular Sets
            if (InventorySceneController.Instance != null && InventorySceneController.Instance.artifactSetCatalog != null)
            {
                var allSets = InventorySceneController.Instance.artifactSetCatalog.GetAllSets();
                foreach (var set in allSets)
                {
                    var setBtnObj = Instantiate(filterIconPrefab, setsGridContainer);
                    setBtnObj.SetActive(true);

                    // Set label text
                    var labelText = setBtnObj.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
                    if (labelText != null)
                    {
                        labelText.text = set.setName;
                        labelText.fontSize = 11f; // Ajusta tamanho do texto para nomes grandes
                    }

                    var btnImg = setBtnObj.GetComponent<Image>();
                    if (btnImg != null)
                    {
                        if (set.setFilterIcon != null)
                        {
                            btnImg.sprite = set.setFilterIcon;
                            btnImg.color = Color.white;
                        }
                        else
                        {
                            btnImg.color = new Color(0.22f, 0.18f, 0.25f, 1f); // Tom escuro roxo
                        }
                    }

                    var highlight = setBtnObj.transform.Find("Highlight")?.gameObject;
                    if (highlight != null) highlight.SetActive(selectedSetIDs.Contains(set.id));

                    var btn = setBtnObj.GetComponent<Button>();
                    var sId = set.id; // captura local
                    if (btn != null)
                    {
                        btn.onClick.AddListener(() => {
                            if (selectedSetIDs.Contains(sId))
                            {
                                selectedSetIDs.Remove(sId);
                                if (highlight != null) highlight.SetActive(false);
                            }
                            else
                            {
                                selectedSetIDs.Add(sId);
                                if (highlight != null) highlight.SetActive(true);
                            }
                        });
                    }
                }
            }
        }

        private void UpdateGridHighlights()
        {
            // Atualizar Destaques de Slots (Types)
            if (typesGridContainer != null)
            {
                var allTypes = (ArtifactType[])global::System.Enum.GetValues(typeof(ArtifactType));
                for (int i = 0; i < allTypes.Length && i < typesGridContainer.childCount; i++)
                {
                    var child = typesGridContainer.GetChild(i);
                    var highlight = child.Find("Highlight")?.gameObject;
                    if (highlight != null)
                    {
                        highlight.SetActive(selectedTypes.Contains(allTypes[i]));
                    }
                }
            }

            // Atualizar Destaques de Sets
            if (setsGridContainer != null && InventorySceneController.Instance != null && InventorySceneController.Instance.artifactSetCatalog != null)
            {
                var allSets = InventorySceneController.Instance.artifactSetCatalog.GetAllSets();
                for (int i = 0; i < allSets.Count && i < setsGridContainer.childCount; i++)
                {
                    var child = setsGridContainer.GetChild(i);
                    var highlight = child.Find("Highlight")?.gameObject;
                    if (highlight != null)
                    {
                        highlight.SetActive(selectedSetIDs.Contains(allSets[i].id));
                    }
                }
            }
        }

        private string GetAbbreviatedType(ArtifactType type)
        {
            switch (type)
            {
                case ArtifactType.Helmet: return "Elmo";
                case ArtifactType.Chestplate: return "Peito";
                case ArtifactType.Gloves: return "Luvas";
                case ArtifactType.Boots: return "Botas";
                case ArtifactType.Necklace: return "Colar";
                case ArtifactType.Ring: return "Anel";
                default: return type.ToString();
            }
        }

        private void SetupDropdownListeners()
        {
            if (mainStatDropdown != null)
                mainStatDropdown.onValueChanged.AddListener(OnMainStatChanged);

            for (int i = 0; i < subStatsDropdowns.Length; i++)
            {
                int index = i; // capturar loop variável
                if (subStatsDropdowns[i] != null)
                    subStatsDropdowns[i].onValueChanged.AddListener((val) => OnSubStatChanged(index, val));
            }
        }

        private void OnMainStatChanged(int value)
        {
            UpdateDropdownOptions();
        }

        private void OnSubStatChanged(int dropdownIndex, int value)
        {
            UpdateDropdownOptions();
        }

        public static string GetStatFriendlyName(StatType type)
        {
            switch (type)
            {
                case StatType.HealthFlat: return "Vida (Fixo)";
                case StatType.HealthPercent: return "Vida (%)";
                case StatType.AttackFlat: return "Ataque (Fixo)";
                case StatType.AttackPercent: return "Ataque (%)";
                case StatType.DefenseFlat: return "Defesa (Fixo)";
                case StatType.DefensePercent: return "Defesa (%)";
                case StatType.Speed: return "Velocidade";
                case StatType.CriticalRate: return "Taxa Crítica";
                case StatType.CriticalDamage: return "Dano Crítico";
                case StatType.EffectResistance: return "Resistência a Efeitos";
                case StatType.EffectHitRate: return "Acerto de Efeitos";
                default: return type.ToString();
            }
        }

        public static StatType? GetStatTypeFromFriendlyName(string friendlyName)
        {
            foreach (StatType type in global::System.Enum.GetValues(typeof(StatType)))
            {
                if (GetStatFriendlyName(type) == friendlyName)
                    return type;
            }
            return null;
        }

        private void UpdateDropdownOptions()
        {
            if (isUpdatingDropdowns) return;
            isUpdatingDropdowns = true;

            string currentMain = mainStatDropdown != null && mainStatDropdown.options.Count > 0 ? mainStatDropdown.options[mainStatDropdown.value].text : "Qualquer";
            string[] currentSubs = new string[4];
            for (int i=0; i<4; i++) {
                currentSubs[i] = subStatsDropdowns[i] != null && subStatsDropdowns[i].options.Count > 0 ? subStatsDropdowns[i].options[subStatsDropdowns[i].value].text : "Qualquer";
            }

            List<string> allStats = new List<string>();
            foreach (StatType stat in global::System.Enum.GetValues(typeof(StatType)))
            {
                allStats.Add(GetStatFriendlyName(stat));
            }

            if (mainStatDropdown != null) {
                List<string> available = new List<string>(allStats);
                for(int i=0; i<4; i++) if(currentSubs[i] != "Qualquer") available.Remove(currentSubs[i]);
                RebuildDropdown(mainStatDropdown, available, currentMain);
            }

            for(int i=0; i<4; i++) {
                if (subStatsDropdowns[i] != null) {
                    List<string> available = new List<string>(allStats);
                    if (currentMain != "Qualquer") available.Remove(currentMain);
                    for(int j=0; j<4; j++) {
                        if (i != j && currentSubs[j] != "Qualquer") available.Remove(currentSubs[j]);
                    }
                    RebuildDropdown(subStatsDropdowns[i], available, currentSubs[i]);
                }
            }

            isUpdatingDropdowns = false;
        }

        private void RebuildDropdown(TMP_Dropdown dd, List<string> availableStats, string currentSelection)
        {
            dd.ClearOptions();
            List<string> finalOptions = new List<string>();
            finalOptions.Add("Qualquer");
            finalOptions.AddRange(availableStats);
            
            if (currentSelection != "Qualquer" && !availableStats.Contains(currentSelection)) {
                currentSelection = "Qualquer";
            }

            dd.AddOptions(finalOptions);
            dd.value = finalOptions.IndexOf(currentSelection);
            dd.RefreshShownValue();
        }

        private void ApplyFilter()
        {
            var data = new ArtifactFilterData();
            
            string selectedMainFriendly = mainStatDropdown != null && mainStatDropdown.options.Count > 0 ? mainStatDropdown.options[mainStatDropdown.value].text : "Qualquer";
            var mainType = GetStatTypeFromFriendlyName(selectedMainFriendly);
            data.mainStat = mainType.HasValue ? mainType.Value.ToString() : "Qualquer";

            for(int i=0; i<4; i++) {
                if (subStatsDropdowns[i] != null && subStatsDropdowns[i].options.Count > 0) {
                    string sFriendly = subStatsDropdowns[i].options[subStatsDropdowns[i].value].text;
                    var subType = GetStatTypeFromFriendlyName(sFriendly);
                    if (subType.HasValue) data.subStats.Add(subType.Value.ToString());
                }
            }
            data.sets = new List<string>(selectedSetIDs);
            data.types = new List<ArtifactType>(selectedTypes);

            OnFilterApplied?.Invoke(data);
            Hide();
        }

        public void ResetFilter()
        {
            selectedSetIDs.Clear();
            selectedTypes.Clear();

            isUpdatingDropdowns = true;
            if (mainStatDropdown != null && mainStatDropdown.options.Count > 0) mainStatDropdown.value = 0;
            for (int i = 0; i < subStatsDropdowns.Length; i++)
            {
                if (subStatsDropdowns[i] != null && subStatsDropdowns[i].options.Count > 0) subStatsDropdowns[i].value = 0;
            }
            isUpdatingDropdowns = false;

            UpdateDropdownOptions();
            UpdateGridHighlights();
        }
    }
}
