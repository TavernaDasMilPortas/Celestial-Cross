using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Account
{
    public int Money;
    public int Energy;

    // Usaremos os IDs para referenciar os ScriptableObjects
    public List<string> OwnedUnitIDs = new List<string>();
    public List<string> OwnedPetIDs = new List<string>();

    public Account()
    {
        Money = 100; // Valor inicial
        Energy = 50; // Valor inicial
        OwnedUnitIDs = new List<string>();
        OwnedPetIDs = new List<string>();
    }
}
