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
    
    [Header("Sync Settings")]
    [SerializeField] private bool useCloudSave = false;
    [SerializeField] private string accountKey = "player_account";

    [Header("Bootstrap / Debug")]
    [SerializeField] private AccountBootstrapConfig bootstrapConfig;
    [SerializeField] private AccountProfile debugProfile;
    [SerializeField] private bool useDebugProfile = false;

    public Account PlayerAccount { get; private set; }

    private IStorageProvider _localProvider;
    private IStorageProvider _cloudProvider;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _localProvider = new LocalStorageProvider();
        _cloudProvider = new CloudStorageProvider();
    }

    private async void Start()
    {
        if (useCloudSave)
        {
            if (AuthManager.Instance == null)
            {
                Debug.LogError("[AccountManager] AuthManager not found in scene!");
                return;
            }

            await AuthManager.Instance.InitializeAndSignInAsync();
        }
        await LoadAndSyncAccountAsync();
    }

    public async Task LoadAndSyncAccountAsync()
    {
        if (useDebugProfile && debugProfile != null)
        {
            LoadDebugAccount();
            return;
        }

        string localJson = await _localProvider.LoadAsync(accountKey);
        string cloudJson = null;

        if (useCloudSave && AuthManager.Instance != null && AuthManager.Instance.IsSignedIn)
        {
            cloudJson = await _cloudProvider.LoadAsync(accountKey);
        }
// ...

        if (string.IsNullOrEmpty(localJson) && string.IsNullOrEmpty(cloudJson))
        {
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
                Debug.Log("Cloud save é mais recente. Usando Cloud.");
                PlayerAccount = cloudAcc;
            }
            else
            {
                Debug.Log("Local save é mais recente ou igual. Usando Local.");
                PlayerAccount = localAcc;
            }
        }
        else if (!string.IsNullOrEmpty(cloudJson))
        {
            PlayerAccount = JsonUtility.FromJson<Account>(cloudJson);
        }
        else
        {
            PlayerAccount = JsonUtility.FromJson<Account>(localJson);
        }

        PlayerAccount?.EnsureInitialized();
        // Sincroniza local se viemos da nuvem ou vice-versa
        await SaveAccountAsync();
    }

    private void LoadDebugAccount()
    {
        PlayerAccount = new Account
        {
            Money = debugProfile.Money,
            Energy = debugProfile.Energy,
            Stardust = debugProfile.Stardust,
            StarMaps = debugProfile.StarMaps,
            OwnedUnitIDs = debugProfile.OwnedUnits.Select(u => u.UnitID).ToList(),
            OwnedPetIDs = debugProfile.OwnedPets.Select(p => p.id).ToList()
        };
        PlayerAccount.EnsureInitialized();
        Debug.Log($"Conta de DEBUG carregada: {debugProfile.name}");
    }

    private void CreateNewAccount()
    {
        PlayerAccount = new Account();
        PlayerAccount.LastSaveTime = DateTime.Now.ToString();
        Debug.Log("Nenhum save encontrado. Criando nova conta.");

        if (bootstrapConfig != null)
        {
            ApplyBootstrap(bootstrapConfig);
            _ = SaveAccountAsync();
        }
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
        if (reward == null || PlayerAccount == null)
            return;

        PlayerAccount.Money += reward.Money;
        PlayerAccount.Energy += reward.Energy;
        PlayerAccount.Stardust += reward.Stardust;

        if (reward.GeneratedArtifacts != null && reward.GeneratedArtifacts.Count > 0)
        {
            if (PlayerAccount.OwnedArtifacts == null)
            {
                PlayerAccount.OwnedArtifacts = new global::System.Collections.Generic.List<CelestialCross.Artifacts.ArtifactInstanceData>();
            }
            PlayerAccount.OwnedArtifacts.AddRange(reward.GeneratedArtifacts);
        }
        
        if (reward.GeneratedPets != null && reward.GeneratedPets.Count > 0)
        {
            if (PlayerAccount.OwnedRuntimePets == null)
            {
                PlayerAccount.OwnedRuntimePets = new global::System.Collections.Generic.List<CelestialCross.Data.Pets.RuntimePetData>();
            }
            PlayerAccount.OwnedRuntimePets.AddRange(reward.GeneratedPets);
        }

        SaveAccount();
    }

    void ApplyBootstrap(AccountBootstrapConfig config)
    {
        if (config == null || PlayerAccount == null)
            return;

        PlayerAccount.Money = config.StartingMoney;
        PlayerAccount.Energy = config.StartingEnergy;
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
                    (int)petSpecies.MaxBaseEffectAccuracy
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


