using System;
using System.Collections;
using UnityEngine;
using CelestialCross.Data.Energy;

namespace CelestialCross.System
{
    public class EnergyService : MonoBehaviour
    {
        public static EnergyService Instance { get; private set; }

        [SerializeField] private EnergyConfig config;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                var go = new GameObject("EnergyService");
                Instance = go.AddComponent<EnergyService>();
                DontDestroyOnLoad(go);
            }
        }

        public event Action<int, int> OnEnergyChanged;
        public event Action<int> OnEnergyInsufficient;

        private bool _isRegenerating = false;
        private bool _isFrozen = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            AccountManager.OnAccountReady += InitializeEnergy;
            if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
            {
                InitializeEnergy();
            }
        }

        private void OnDestroy()
        {
            AccountManager.OnAccountReady -= InitializeEnergy;
        }

        private void InitializeEnergy()
        {
            var account = AccountManager.Instance.PlayerAccount;
            if (account == null) return;

            if (account.EnergyInfo == null)
            {
                account.EnergyInfo = new EnergyData
                {
                    CurrentEnergy = config != null ? config.MaxEnergy : 100,
                    LastRegenTimestampUTC = DateTime.UtcNow.ToString("O"),
                    LastServerTimestampUTC = DateTime.UtcNow.ToString("O")
                };
            }

            ValidateTimestampsAndCalculateOfflineRegen(account.EnergyInfo);

            if (!_isRegenerating)
            {
                _isRegenerating = true;
                StartCoroutine(RegenerationRoutine());
            }

            NotifyEnergyChanged();
        }

        private void ValidateTimestampsAndCalculateOfflineRegen(EnergyData energyData)
        {
            if (config == null) return;

            DateTime now = DateTime.UtcNow;
            
            if (DateTime.TryParse(energyData.LastServerTimestampUTC, out DateTime lastServerTime))
            {
                if (now < lastServerTime)
                {
                    Debug.LogWarning("[EnergyService] Manipulação de relógio detectada! Regeneração congelada.");
                    _isFrozen = true;
                    return;
                }
                else
                {
                    _isFrozen = false;
                }
            }

            energyData.LastServerTimestampUTC = now.ToString("O");

            if (energyData.CurrentEnergy < GetMaxEnergy())
            {
                if (DateTime.TryParse(energyData.LastRegenTimestampUTC, out DateTime lastRegenTime))
                {
                    TimeSpan passed = now - lastRegenTime;
                    float regenInterval = config != null ? config.RegenIntervalSeconds : 300f; // 5 minutes default
                    int energyToAdd = (int)(passed.TotalSeconds / regenInterval);

                    if (energyToAdd > 0)
                    {
                        energyData.CurrentEnergy = Mathf.Min(GetMaxEnergy(), energyData.CurrentEnergy + energyToAdd);
                        DateTime newRegenTime = lastRegenTime.AddSeconds(energyToAdd * regenInterval);
                        energyData.LastRegenTimestampUTC = newRegenTime.ToString("O");
                        AccountManager.Instance.SaveAccount();
                    }
                }
                else
                {
                    energyData.LastRegenTimestampUTC = now.ToString("O");
                }
            }
        }

        private IEnumerator RegenerationRoutine()
        {
            var wait = new WaitForSeconds(1f);
            while (true)
            {
                yield return wait;

                if (_isFrozen)
                {
                    var acc = AccountManager.Instance?.PlayerAccount;
                    if (acc?.EnergyInfo != null && DateTime.TryParse(acc.EnergyInfo.LastServerTimestampUTC, out DateTime lastServerTime))
                    {
                        if (DateTime.UtcNow >= lastServerTime)
                        {
                            _isFrozen = false;
                            Debug.Log("[EnergyService] Relógio voltou ao normal. Descongelando energia.");
                            acc.EnergyInfo.LastRegenTimestampUTC = DateTime.UtcNow.ToString("O");
                        }
                    }
                    continue;
                }

                // Continue regenerating even if config is null (using defaults)
                if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) continue;

                var energyData = AccountManager.Instance.PlayerAccount.EnergyInfo;
                if (energyData == null) continue;

                if (energyData.CurrentEnergy < GetMaxEnergy())
                {
                    DateTime now = DateTime.UtcNow;
                    if (DateTime.TryParse(energyData.LastRegenTimestampUTC, out DateTime lastRegenTime))
                    {
                        float regenInterval = config != null ? config.RegenIntervalSeconds : 300f;
                        if ((now - lastRegenTime).TotalSeconds >= regenInterval)
                        {
                            energyData.CurrentEnergy++;
                            energyData.LastRegenTimestampUTC = lastRegenTime.AddSeconds(regenInterval).ToString("O");
                            energyData.LastServerTimestampUTC = now.ToString("O");
                            
                            AccountManager.Instance.SaveAccount();
                            NotifyEnergyChanged();
                        }
                    }
                }
                else
                {
                    energyData.LastRegenTimestampUTC = DateTime.UtcNow.ToString("O");
                }
            }
        }

        public bool TryConsumeEnergy(int amount)
        {
            if (amount <= 0) return true;

            var account = AccountManager.Instance?.PlayerAccount;
            if (account?.EnergyInfo == null) return false;

            if (account.EnergyInfo.CurrentEnergy >= amount)
            {
                account.EnergyInfo.CurrentEnergy -= amount;
                
                int maxE = GetMaxEnergy();
                if (account.EnergyInfo.CurrentEnergy + amount >= maxE && account.EnergyInfo.CurrentEnergy < maxE)
                {
                    account.EnergyInfo.LastRegenTimestampUTC = DateTime.UtcNow.ToString("O");
                }

                AccountManager.Instance.SaveAccount();
                NotifyEnergyChanged();
                return true;
            }

            OnEnergyInsufficient?.Invoke(amount);
            return false;
        }

        public void AddEnergy(int amount)
        {
            if (amount <= 0) return;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account?.EnergyInfo == null) return;

            account.EnergyInfo.CurrentEnergy += amount;
            AccountManager.Instance.SaveAccount();
            NotifyEnergyChanged();
        }

        public int GetCurrentEnergy()
        {
            return AccountManager.Instance?.PlayerAccount?.EnergyInfo?.CurrentEnergy ?? 0;
        }

        public int GetMaxEnergy()
        {
            return config != null ? config.MaxEnergy : 100;
        }

        public float GetTimeUntilNextRegen()
        {
            if (config == null || _isFrozen) return -1f;
            var account = AccountManager.Instance?.PlayerAccount;
            if (account?.EnergyInfo == null) return -1f;

            if (account.EnergyInfo.CurrentEnergy >= config.MaxEnergy) return 0f;

            if (DateTime.TryParse(account.EnergyInfo.LastRegenTimestampUTC, out DateTime lastRegenTime))
            {
                float passed = (float)(DateTime.UtcNow - lastRegenTime).TotalSeconds;
                return Mathf.Max(0f, config.RegenIntervalSeconds - passed);
            }

            return config.RegenIntervalSeconds;
        }

        private void NotifyEnergyChanged()
        {
            OnEnergyChanged?.Invoke(GetCurrentEnergy(), GetMaxEnergy());
        }
    }
}
