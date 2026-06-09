using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CelestialCross.Gacha
{
    public interface IGachaProvider
    {
        Task<List<GachaRewardEntry>> PullAsync(Account account, GachaBannerSO banner, int times);
    }
}
