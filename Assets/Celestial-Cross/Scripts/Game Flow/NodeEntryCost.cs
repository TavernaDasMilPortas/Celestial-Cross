using System;
using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.Progression
{
    [Serializable]
    public class ItemCostEntry
    {
        public string ItemID;
        public int Amount = 1;
        [Tooltip("Nome amigável para exibir na UI")]
        public string DisplayName;
    }

    [Serializable]
    public class NodeEntryCost
    {
        [Header("Custo de Energia")]
        public int EnergyCost = 0;
        
        [Header("Custos de Itens (cobrados a cada tentativa)")]
        [Tooltip("Lista de itens necessários para iniciar este nó")]
        public List<ItemCostEntry> ItemCosts = new List<ItemCostEntry>();
    }
}
