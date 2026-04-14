using UnityEngine;
using CelestialCross.Data.Pets;
using System.Collections.Generic;

namespace CelestialCross.System
{
    public class PetReleaseManager : MonoBehaviour
    {
        public static PetReleaseManager Instance { get; private set; }

        [Header("ConfiguraÃ§Ã£o de Descarte")]
        [Tooltip("ReferÃªncia ao SO que dita quantas souls/stardust os pets dropam por estrela.")]
        public PetReleaseConfigSO ReleaseConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        public void ReleasePet(string petUUID)
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null)
            {
                Debug.LogError("[PetRelease] AccountManager nÃ£o encontrado.");
                return;
            }

            var account = AccountManager.Instance.PlayerAccount;
            RuntimePetData petToRelease = account.OwnedRuntimePets.Find(p => p.UUID == petUUID);

            if (petToRelease == null)
            {
                Debug.LogWarning($"[PetRelease] Pet com UUID {petUUID} nÃ£o encontrado no inventÃ¡rio.");
                return;
            }

            int stars = petToRelease.RarityStars;
            if (stars < 0 || stars > 5) stars = 1; // Fallback caso ocorra um roll bizarro

            int stardustGained = 0;
            int petSoulsGained = 0;

            if (ReleaseConfig != null)
            {
                stardustGained = ReleaseConfig.StardustPerStar[Mathf.Min(stars, ReleaseConfig.StardustPerStar.Length - 1)];
                petSoulsGained = ReleaseConfig.PetSoulsPerStar[Mathf.Min(stars, ReleaseConfig.PetSoulsPerStar.Length - 1)];
            }
            else
            {
                // Numeros base de seguranÃ§a (Hardcoded) caso nÃ£o liguem o SO no editor
                stardustGained = stars * 10;
                petSoulsGained = stars * 1;
            }

            // 1. Gera e Adiciona Stardust
            account.Stardust += stardustGained;

            // 2. Gera e Adiciona Pet-Souls baseada na ESPÃ‰CIE desse pet
            // A ID do item no inventÃ¡rio serÃ¡: "soul_{speciesID}" para podermos achar fÃ¡cil.
            string soulItemId = $"soul_{petToRelease.DisplayName}";
            account.AddItem(soulItemId, petSoulsGained);

            // 3. Remove o indivÃ­duo do banco de dados (tambÃ©m desequipando se estivesse ativo).
            account.UnequipPetFromAll(petUUID);
            account.OwnedRuntimePets.Remove(petToRelease);

            AccountManager.Instance.SaveAccount();

            Debug.Log($"[PetRelease] Libertado 1 {petToRelease.DisplayName} ({stars} Estrelas). Recompensas: +{stardustGained} Stardust | +{petSoulsGained} {soulItemId}");
        }
    }
}

