using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CelestialCross.Data;

namespace CelestialCross.Gacha
{
    public interface IGachaProvider
    {
        Task<List<RuntimeGachaResult>> PullAsync(Account account, GachaBannerSO banner, int times);
    }
}
