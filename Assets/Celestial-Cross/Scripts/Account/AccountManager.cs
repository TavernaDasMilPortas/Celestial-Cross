using UnityEngine;
using System;
using System.IO;
using global::System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CelestialCross.Storage;
using CelestialCross.Authentication;

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }
    public static event Action OnAccountReady;
    
    [Header("Sync Settings")]
    [SerializeField] private bool useCloudSave = false;
    [SerializeField] private string accountKey = "player_account";

    [Header("Bootstrap / Debug")]
    [SerializeField] private AccountBootstrapConfig bootstrapConfig;
    [SerializeField] private AccountProfile debugProfile;
    [SerializeField] private bool useDebugProfile = false;

    public AccountBootstrapConfig BootstrapConfig => bootstrapConfig;
    public Account PlayerAccount { get; private set; }

    private IStorageProvider _localProvider;
    private IStorageProvider _cloudProvider;

    private void Awake()
    {
        Debug.Log($"[AccountManager] Awake chamado. Instance? {Instance != null}");
        if (Instance != null && Instance != this)
        {
            Debug.Log("[AccountManager] Instância duplicada detectada e destruída.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
        Debug.Log($"[AccountManager] Awake - Iniciando auto-configuração no editor. debugProfile? {debugProfile != null}, bootstrapConfig? {bootstrapConfig != null}");
        // Auto-configuração no editor para cenas iniciadas diretamente
        if (debugProfile == null)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("AccountProfileTest t:AccountProfile");
            if (guids.Length == 0) guids = UnityEditor.AssetDatabase.FindAssets("t:AccountProfile");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                debugProfile = UnityEditor.AssetDatabase.LoadAssetAtPath<AccountProfile>(path);
                Debug.Log($"[AccountManager] Auto-carregado debugProfile: {debugProfile.name}");
            }
        }
        if (bootstrapConfig == null)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AccountBootstrapConfig");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                bootstrapConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<AccountBootstrapConfig>(path);
                Debug.Log($"[AccountManager] Auto-carregado bootstrapConfig: {bootstrapConfig.name}");
            }
        }
        
        // Se foi criado dinamicamente, evitamos forçar o useDebugProfile caso o jogador queira testar o save real
        if (gameObject.name.Contains("AutoCreated"))
        {
            Debug.Log("[AccountManager] AutoCreated detectado. O uso do save ou do bootstrap será mantido conforme a configuração padrão.");
        }
#endif

        _localProvider = new LocalStorageProvider();
        _cloudProvider = new CloudStorageProvider();
    }

    private async void Start()
    {
        Debug.Log($"[AccountManager] Start chamado. useCloudSave: {useCloudSave}, useDebugProfile: {useDebugProfile}");
        if (useCloudSave)
        {
            if (AuthManager.Instance == null)
            {
                Debug.LogError("[AccountManager] AuthManager not found in scene!");
                return;
            }

            Debug.Log("[AccountManager] Iniciando login na nuvem...");
            await AuthManager.Instance.InitializeAndSignInAsync();
        }
        await LoadAndSyncAccountAsync();
    }

    public async Task LoadAndSyncAccountAsync()
    {
        Debug.Log($"[AccountManager] LoadAndSyncAccountAsync iniciado. useDebugProfile: {useDebugProfile}");
        if (useDebugProfile && debugProfile != null)
        {
            Debug.Log("[AccountManager] Redirecionando para LoadDebugAccount()...");
            LoadDebugAccount();
            return;
        }

        string localJson = await _localProvider.LoadAsync(accountKey);
        string cloudJson = null;
        Debug.Log($"[AccountManager] localJson vazio? {string.IsNullOrEmpty(localJson)}");

        if (useCloudSave && AuthManager.Instance != null && AuthManager.Instance.IsSignedIn)
        {
            cloudJson = await _cloudProvider.LoadAsync(accountKey);
            Debug.Log($"[AccountManager] cloudJson vazio? {string.IsNullOrEmpty(cloudJson)}");
        }

        if (string.IsNullOrEmpty(localJson) && string.IsNullOrEmpty(cloudJson))
        {
            Debug.Log("[AccountManager] Sem saves locais ou na nuvem. Criando nova conta...");
            CreateNewAccount();
            return;
        }

        // Resolução de Conflitos Simplificada
        if (!string.IsNullOrEmpty(localJson) && !string.IsNullOrEmpty(cloudJson))
        {
            Account localAcc = JsonUtility.FromJson<Account>(localJson);
            Account cloudAcc = JsonUtility.FromJson<Account>(cloudJson);

            // Comparar LastSaveTime
            DateTime localTime = DateTime.Parse(localAcc.LastSaveTime ?? DateTime.MinValue.ToString());
            DateTime cloudTime = DateTime.Parse(cloudAcc.LastSaveTime ?? DateTime.MinValue.ToString());

            if (cloudTime > localTime)
            {
                Debug.Log("[AccountManager] Cloud save é mais recente. Usando Cloud.");
                PlayerAccount = cloudAcc;
            }
            else
            {
                Debug.Log("[AccountManager] Local save é mais recente ou igual. Usando Local.");
                PlayerAccount = localAcc;
            }
        }
        else if (!string.IsNullOrEmpty(cloudJson))
        {
            Debug.Log("[AccountManager] Carregando cloudJson...");
            PlayerAccount = JsonUtility.FromJson<Account>(cloudJson);
        }
        else
        {
            Debug.Log("[AccountManager] Carregando localJson...");
            PlayerAccount = JsonUtility.FromJson<Account>(localJson);
        }

        PlayerAccount?.EnsureInitialized();
        Debug.Log($"[AccountManager] Conta carregada: {PlayerAccount.Money} Money, {PlayerAccount.Energy} Energy, {PlayerAccount.OwnedUnitIDs?.Count} Units, {PlayerAccount.OwnedRuntimePets?.Count} Pets, {PlayerAccount.OwnedArtifacts?.Count} Artifacts.");
        // Sincroniza local se viemos da nuvem ou vice-versa
        await SaveAccountAsync();
        Debug.Log("[AccountManager] Sincronização concluída. Disparando OnAccountReady...");
        OnAccountReady?.Invoke();
    }

    private void LoadDebugAccount()
    {
        Debug.Log($"[AccountManager] LoadDebugAccount iniciado para o perfil: {debugProfile.name}");
        PlayerAccount = new Account
        {
            Money = debugProfile.Money,
            EnergyInfo = new CelestialCross.Data.Energy.EnergyData { 
                CurrentEnergy = debugProfile.Energy,
                LastRegenTimestampUTC = System.DateTime.UtcNow.ToString("O"),
                LastServerTimestampUTC = System.DateTime.UtcNow.ToString("O")
            },
            Stardust = debugProfile.Stardust,
            StarMaps = debugProfile.StarMaps,
            OwnedUnitIDs = debugProfile.OwnedUnits.Select(u => u.UnitID).ToList(),
            OwnedPetIDs = debugProfile.OwnedPets.Select(p => p.id).ToList()
        };
        PlayerAccount.EnsureInitialized();
        Debug.Log($"[AccountManager] LoadDebugAccount - OwnedUnitIDs: {PlayerAccount.OwnedUnitIDs?.Count}, OwnedPetIDs: {PlayerAccount.OwnedPetIDs?.Count}");

        if (debugProfile.OwnedPets != null)
        {
            Debug.Log($"[AccountManager] LoadDebugAccount - Populando {debugProfile.OwnedPets.Count} Pets a partir do perfil.");
            foreach (var petSpecies in debugProfile.OwnedPets)
            {
                if (petSpecies == null) continue;
                if (string.IsNullOrWhiteSpace(petSpecies.id)) continue;
                
                // Evita adicionar duplicados
                if (PlayerAccount.OwnedRuntimePets.Exists(p => p.SpeciesID == petSpecies.id)) continue;

                var newPet = new CelestialCross.Data.Pets.RuntimePetData(
                    petSpecies.id, 
                    petSpecies.SpeciesName, 
                    3, // Default 3 stars
                    (int)petSpecies.MaxBaseHealth, 
                    (int)petSpecies.MaxBaseAttack, 
                    (int)petSpecies.MaxBaseDefense,
                    (int)petSpecies.MaxBaseSpeed,
                    (int)petSpecies.MaxBaseCriticalChance,
                    (int)petSpecies.MaxBaseCriticalDamage,
                    (int)petSpecies.MaxBaseEffectAccuracy,
                    (int)petSpecies.MaxBaseEffectResistance
                );
                PlayerAccount.OwnedRuntimePets.Add(newPet);
            }
        }
        Debug.Log($"[AccountManager] LoadDebugAccount - OwnedRuntimePets: {PlayerAccount.OwnedRuntimePets?.Count}");

#if UNITY_EDITOR
        Debug.Log($"[AccountManager] LoadDebugAccount - Verificando artefatos. Atuais: {PlayerAccount.OwnedArtifacts?.Count}");
        // Gerar alguns artefatos de teste caso a conta de debug não tenha nenhum
        if (PlayerAccount.OwnedArtifacts == null || PlayerAccount.OwnedArtifacts.Count == 0)
        {
            PlayerAccount.OwnedArtifacts = new List<CelestialCross.Artifacts.ArtifactInstanceData>();
            string[] catalogGuids = UnityEditor.AssetDatabase.FindAssets("t:ArtifactSetCatalog");
            Debug.Log($"[AccountManager] LoadDebugAccount - Encontrados {catalogGuids.Length} catálogos de artefatos.");
            if (catalogGuids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(catalogGuids[0]);
                var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ArtifactSetCatalog>(path);
                if (catalog != null)
                {
                    var sets = catalog.GetAllSets();
                    Debug.Log($"[AccountManager] LoadDebugAccount - Catalog {catalog.name} tem {sets.Count} sets.");
                    if (sets.Count > 0)
                    {
                        var targetSet = sets[0];
                        // Criar um de cada tipo/slot
                        foreach (CelestialCross.Artifacts.ArtifactType slotType in Enum.GetValues(typeof(CelestialCross.Artifacts.ArtifactType)))
                        {
                            var art = new CelestialCross.Artifacts.ArtifactInstanceData
                            {
                                artifactSetId = targetSet.id,
                                slot = slotType,
                                rarity = CelestialCross.Artifacts.ArtifactRarity.Legendary,
                                stars = CelestialCross.Artifacts.ArtifactStars.Five,
                                currentLevel = 0,
                                mainStat = new CelestialCross.Artifacts.StatModifierData(CelestialCross.Artifacts.StatType.HealthFlat, 500f)
                            };
                            PlayerAccount.OwnedArtifacts.Add(art);
                            Debug.Log($"[AccountManager] LoadDebugAccount - Criado artefato de teste para slot: {slotType}");
                        }
                    }
                }
            }
        }
#endif

        Debug.Log($"Conta de DEBUG carregada com sucesso: {debugProfile.name}. Disparando OnAccountReady...");
        OnAccountReady?.Invoke();
    }

    private void CreateNewAccount()
    {
        Debug.Log("[AccountManager] CreateNewAccount iniciado.");
        PlayerAccount = new Account();
        PlayerAccount.LastSaveTime = DateTime.Now.ToString();
        Debug.Log("Nenhum save encontrado. Criando nova conta.");

        if (bootstrapConfig != null)
        {
            Debug.Log($"[AccountManager] Aplicando bootstrap: {bootstrapConfig.name}");
            ApplyBootstrap(bootstrapConfig);
            _ = SaveAccountAsync();
        }
        Debug.Log("[AccountManager] Nova conta criada. Disparando OnAccountReady...");
        OnAccountReady?.Invoke();
    }

    public async Task SaveAccountAsync()
    {
        if (PlayerAccount == null) return;
        
        PlayerAccount.LastSaveTime = DateTime.Now.ToString();
        string json = JsonUtility.ToJson(PlayerAccount, true);

        // Salva local sempre (cache)
        await _localProvider.SaveAsync(accountKey, json);

        // Salva na nuvem se habilitado
        if (useCloudSave && AuthManager.Instance != null && AuthManager.Instance.IsSignedIn)
        {
            await _cloudProvider.SaveAsync(accountKey, json);
        }

        Debug.Log("Conta do jogador salva (Sync concluído).");
    }

    public void SaveAccount() => _ = SaveAccountAsync();

    public void ApplyRewards(CelestialCross.Data.Dungeon.RuntimeReward reward)
    {
        CelestialCross.System.RewardService.ApplyRuntimeRewardToAccount(reward);
    }

    void ApplyBootstrap(AccountBootstrapConfig config)
    {
        if (config == null || PlayerAccount == null)
            return;

        PlayerAccount.Money = config.StartingMoney;
        if (PlayerAccount.EnergyInfo == null)
        {
            PlayerAccount.EnergyInfo = new CelestialCross.Data.Energy.EnergyData
            {
                CurrentEnergy = config.StartingEnergyConfig != null ? config.StartingEnergyConfig.MaxEnergy : 100,
                LastRegenTimestampUTC = System.DateTime.UtcNow.ToString("O"),
                LastServerTimestampUTC = System.DateTime.UtcNow.ToString("O")
            };
        }
        PlayerAccount.Stardust = config.StartingStardust;
        PlayerAccount.StarMaps = config.StartingStarMaps;

        if (config.StartingUnits != null)
        {
            foreach (var unitData in config.StartingUnits)
            {
                if (unitData == null) continue;
                if (string.IsNullOrWhiteSpace(unitData.UnitID))
                {
                    Debug.LogWarning($"[AccountBootstrap] UnitData '{unitData.name}' sem UnitID. Reimporte/edite o asset para regenerar o ID automático.");
                    continue;
                }
                AddUnitToAccount(unitData.UnitID);
            }
        }

        if (config.GrantStartingPets && config.StartingPets != null)
        {
            foreach (var petSpecies in config.StartingPets)
            {
                if (petSpecies == null) continue;
                if (string.IsNullOrWhiteSpace(petSpecies.id))
                {
                    Debug.LogWarning($"[AccountBootstrap] PetSpeciesSO '{petSpecies.name}' sem id. Salve o asset para regenerar.");
                    continue;
                }
                
                var newPet = new CelestialCross.Data.Pets.RuntimePetData(
                    petSpecies.id, 
                    petSpecies.SpeciesName, 
                    3, // Default 3 stars
                    (int)petSpecies.MaxBaseHealth, 
                    (int)petSpecies.MaxBaseAttack, 
                    (int)petSpecies.MaxBaseDefense,
                    (int)petSpecies.MaxBaseSpeed,
                    (int)petSpecies.MaxBaseCriticalChance,
                    (int)petSpecies.MaxBaseCriticalDamage,
                    (int)petSpecies.MaxBaseEffectAccuracy,
                    (int)petSpecies.MaxBaseEffectResistance
                );
                PlayerAccount.OwnedRuntimePets.Add(newPet);
            }
        }
    }

    // Adiciona uma unidade à conta ou concede insígnia se for duplicata
    public void AddUnitToAccount(string unitID)
    {
        if (PlayerAccount == null) return;

        bool alreadyOwned = PlayerAccount.OwnedUnits.Exists(u => u.UnitID == unitID) || 
                            PlayerAccount.OwnedUnitIDs.Contains(unitID);

        if (!alreadyOwned)
        {
            PlayerAccount.OwnedUnitIDs.Add(unitID);
            PlayerAccount.OwnedUnits.Add(new CelestialCross.Data.RuntimeUnitData(unitID, 4));
            Debug.Log($"[AccountManager] Nova unidade adicionada: {unitID}");
        }
        else
        {
            // Duplicata! Converte em insígnia para o sistema de constelação
            string insigniaID = CelestialCross.System.ConstellationService.GetInsigniaItemID(unitID);
            PlayerAccount.AddItem(insigniaID, 1);
            Debug.Log($"[AccountManager] Duplicata de {unitID} convertida em 1 Insígnia Estelar.");
        }
        
        SaveAccount();
    }

    // Exemplo de como adicionar um pet à conta
    public void AddPetToAccount(string petID)
    {
        if (!PlayerAccount.OwnedPetIDs.Contains(petID))
        {
            PlayerAccount.OwnedPetIDs.Add(petID);
            SaveAccount();
        }
    }

    private void OnApplicationQuit()
    {
        SaveAccount();
    }
}


