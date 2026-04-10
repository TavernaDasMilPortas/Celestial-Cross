using UnityEngine;
using System.IO;
using global::System.Collections.Generic;
using System.Linq;

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }

    [Header("Bootstrap / Debug")]
    [SerializeField] private AccountBootstrapConfig bootstrapConfig;
    [SerializeField] private AccountProfile debugProfile;
    [SerializeField] private bool useDebugProfile = false;

    public Account PlayerAccount { get; private set; }

    private string savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "account.json");
        LoadAccount();
    }

    public void LoadAccount()
    {
        if (useDebugProfile && debugProfile != null)
        {
            PlayerAccount = new Account
            {
                Money = debugProfile.Money,
                Energy = debugProfile.Energy,
                OwnedUnitIDs = debugProfile.OwnedUnits.Select(u => u.UnitID).ToList(),
                OwnedPetIDs = debugProfile.OwnedPets.Select(p => p.PetID).ToList()
            };

            Debug.Log($"Conta de DEBUG carregada: {debugProfile.name}");
            return;
        }

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            PlayerAccount = JsonUtility.FromJson<Account>(json);
            PlayerAccount?.EnsureInitialized();
            Debug.Log("Conta do jogador carregada.");
        }
        else
        {
            PlayerAccount = new Account();
            Debug.Log("Nenhum save encontrado. Criando nova conta.");

            if (bootstrapConfig != null)
            {
                ApplyBootstrap(bootstrapConfig);
                SaveAccount();
            }
        }
    }

    public void SaveAccount()
    {
        string json = JsonUtility.ToJson(PlayerAccount, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Conta do jogador salva em: " + savePath);
    }

    public void ApplyRewards(CelestialCross.Data.Dungeon.RuntimeReward reward)
    {
        if (reward == null || PlayerAccount == null)
            return;

        PlayerAccount.Money += reward.Money;
        PlayerAccount.Energy += reward.Energy;

        if (reward.GeneratedArtifacts != null && reward.GeneratedArtifacts.Count > 0)
        {
            if (PlayerAccount.OwnedArtifacts == null)
            {
                PlayerAccount.OwnedArtifacts = new global::System.Collections.Generic.List<CelestialCross.Artifacts.ArtifactInstanceData>();
            }
            PlayerAccount.OwnedArtifacts.AddRange(reward.GeneratedArtifacts);
        }

        SaveAccount();
    }

    void ApplyBootstrap(AccountBootstrapConfig config)
    {
        if (config == null || PlayerAccount == null)
            return;

        PlayerAccount.Money = config.StartingMoney;
        PlayerAccount.Energy = config.StartingEnergy;

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

        if (config.StartingPets != null)
        {
            foreach (var petData in config.StartingPets)
            {
                if (petData == null) continue;
                if (string.IsNullOrWhiteSpace(petData.PetID))
                {
                    Debug.LogWarning($"[AccountBootstrap] PetData '{petData.name}' sem PetID. Reimporte/edite o asset para regenerar o ID automático.");
                    continue;
                }
                AddPetToAccount(petData.PetID);
            }
        }
    }

    // Exemplo de como adicionar uma unidade à conta
    public void AddUnitToAccount(string unitID)
    {
        if (!PlayerAccount.OwnedUnitIDs.Contains(unitID))
        {
            PlayerAccount.OwnedUnitIDs.Add(unitID);
            SaveAccount();
        }
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
