using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CelestialCross.Data;

namespace CelestialCross.Gacha
{
    public class LocalGachaProvider : IGachaProvider
    {
        public Task<List<RuntimeGachaResult>> PullAsync(Account account, GachaBannerSO banner, int times)
        {
            // O provider local apenas delega para o Service (que manipula os dados e salva na Account local).
            // Em um provider Cloud, ele chamaria uma API web aqui.
            var results = GachaService.Instance.ExecutePullsInternal(account, banner, times);
            return Task.FromResult(results);
        }
    }
}
