using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CelestialCross.Gacha
{
    public class LocalGachaProvider : IGachaProvider
    {
        public Task<List<GachaRewardEntry>> PullAsync(Account account, GachaBannerSO banner, int times)
        {
            // Reutiliza a lógica existente que agora foi movida/encapsulada
            return Task.FromResult(GachaService.Instance.ExecutePullsInternal(account, banner, times));
        }
    }
}
