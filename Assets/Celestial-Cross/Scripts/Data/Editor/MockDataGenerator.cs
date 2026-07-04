using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;

namespace CelestialCross.Editor
{
    public static class MockDataGenerator
    {
        private const string BasePath = "Assets/Celestial-Cross/MockData";
        private const string UnitsPath = BasePath + "/Units";
        private const string PetsPath = BasePath + "/Pets";
        private const string AbilitiesPath = BasePath + "/Abilities";

        [MenuItem("Celestial Cross/2. Data & Assets/Config/Generate Mock Units and Pets")]
        public static void GenerateData()
        {
            EnsureDirectories();

            GeneratePets();
            GenerateUnits();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Foram gerados com sucesso 10 Units, 10 Pets e 40 Abilities de teste na pasta: " + BasePath);
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder(BasePath)) AssetDatabase.CreateFolder("Assets/Celestial-Cross", "MockData");
            if (!AssetDatabase.IsValidFolder(UnitsPath)) AssetDatabase.CreateFolder(BasePath, "Units");
            if (!AssetDatabase.IsValidFolder(PetsPath)) AssetDatabase.CreateFolder(BasePath, "Pets");
            if (!AssetDatabase.IsValidFolder(AbilitiesPath)) AssetDatabase.CreateFolder(BasePath, "Abilities");
        }

        private static void GeneratePets()
        {
            string[] petNames = { "DragÃƒÂ£o Flamejante", "Lobo de Gelo", "Golem de Terra", "FalcÃƒÂ£o dos Ventos", "Fada de Luz", "Pantera das Sombras", "EspÃƒÂ­rito da ÃƒÂgua", "PÃƒÂ¡ssaro TrovÃƒÂ£o", "Serpente Venenosa", "Coruja Arcana" };
            Color[] petColors = { Color.red, Color.cyan, new Color(0.6f, 0.3f, 0.1f), Color.white, Color.yellow, Color.black, Color.blue, Color.magenta, Color.green, new Color(0.5f, 0, 0.5f) };
            
            // Atributos BÃƒÂ´nus
            CombatStats[] petStats = {
                new CombatStats(20, 50, 10, 5, 5, 0), // Fogo (Atk)
                new CombatStats(30, 20, 20, 15, 2, 0), // Gelo (Balanceado/Spd)
                new CombatStats(100, 10, 40, -5, 0, 0), // Terra (Def/HP)
                new CombatStats(15, 30, 10, 20, 10, 0), // Vento (Spd/Crit)
                new CombatStats(80, 15, 25, 5, 0, 10), // Luz (HP/Acc)
                new CombatStats(10, 60, 5, 10, 15, 0), // Sombra (Atk/Crit)
                new CombatStats(60, 15, 30, 0, 0, 5),   // Agua (HP/Def)
                new CombatStats(10, 45, 5, 25, 5, 0),   // TrovÃƒÂ£o (Atk/Spd)
                new CombatStats(40, 25, 15, 5, 0, 20),  // Veneno (Acc/HP)
                new CombatStats(20, 40, 10, 10, 5, 15)  // Arcano (Atk/Acc)
            };

            for (int i = 0; i < 10; i++)
            {
                // 1. Criar Passiva
                Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO passive = ScriptableObject.CreateInstance<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO>();
                passive.abilityName = "Aura do " + petNames[i].Split(' ')[0];
                passive.abilityDescription = "Concede bÃƒÂ´nus instintivos e poder mÃƒÂ­stico passivo da criatura ao mestre.";
                var pStartData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData { type = AbilityType.Passive, subtype = AbilitySubtype.Buff, isBuff = true };
                passive.NodeData.Add(new Celestial_Cross.Scripts.Abilities.Graph.AbilityNodeData { NodeType = "StartNode", JsonData = JsonUtility.ToJson(pStartData) });
                AssetDatabase.CreateAsset(passive, $"{AbilitiesPath}/Ability_Pet_{i}.asset");

                // 2. Criar Pet
                CelestialCross.Data.Pets.PetSpeciesSO pet = ScriptableObject.CreateInstance<CelestialCross.Data.Pets.PetSpeciesSO>();
                pet.SpeciesName = petNames[i];
                pet.MinBaseHealth = petStats[i].health; pet.MaxBaseHealth = petStats[i].health * 1.5f; 
                pet.MinBaseAttack = petStats[i].attack; pet.MaxBaseAttack = petStats[i].attack * 1.5f; 
                pet.MinBaseDefense = petStats[i].defense; pet.MaxBaseDefense = petStats[i].defense * 1.5f; 
                pet.MinBaseSpeed = petStats[i].speed; pet.MaxBaseSpeed = petStats[i].speed * 1.5f; 
                pet.MinBaseCriticalChance = petStats[i].criticalChance; pet.MaxBaseCriticalChance = petStats[i].criticalChance * 1.5f; 
                pet.MinBaseEffectAccuracy = petStats[i].effectAccuracy; pet.MaxBaseEffectAccuracy = petStats[i].effectAccuracy * 1.5f;
                pet.AbilityGraphs.Add(passive);
                pet.Icon = GenerateColoredSprite(petColors[i], $"Icon_Pet_{i}");
                
                AssetDatabase.CreateAsset(pet, $"{PetsPath}/Pet_{i}.asset");
            }
        }

        private static void GenerateUnits()
        {
            string[] unitNames = { "Paladino GuardiÃƒÂ£o", "Assassino Furtivo", "Piromante Supremo", "ClÃƒÂ©riga Divina", "BÃƒÂ¡rbaro Sangrento", "Patrulheiro Ãƒâ€°lfico", "Cavaleiro Real", "Necromante Obscuro", "Bardo Inspirador", "Evocador Astral" };
            Color[] unitColors = { Color.white, Color.black, Color.red, Color.yellow, new Color(0.8f, 0.1f, 0.1f), Color.green, Color.gray, new Color(0.3f, 0, 0.4f), Color.cyan, Color.magenta };

            CombatStats[] unitStats = {
                new CombatStats(800, 40, 60, 30, 5, 20), // Paladino
                new CombatStats(350, 95, 25, 75, 30, 20), // Assassino
                new CombatStats(400, 100, 20, 50, 15, 40), // Piromante
                new CombatStats(550, 30, 45, 45, 5, 50), // Cleriga
                new CombatStats(700, 110, 15, 60, 20, 10), // Barbaro
                new CombatStats(450, 80, 25, 80, 25, 30), // Patrulheiro
                new CombatStats(750, 55, 70, 25, 5, 10), // Cavaleiro
                new CombatStats(500, 85, 35, 40, 10, 60), // Necromante
                new CombatStats(480, 45, 40, 70, 10, 50), // Bardo
                new CombatStats(400, 90, 30, 40, 15, 70)  // Evocador
            };

            for (int i = 0; i < 10; i++)
            {
                // Criar Habilidades (1 Passiva, 2 Ativas)
                Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO passiva = ScriptableObject.CreateInstance<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO>();
                passiva.abilityName = "TÃƒÂ©cnica de " + unitNames[i].Split(' ')[0];
                passiva.abilityDescription = $"Habilidade passiva de um verdadeiro {unitNames[i].Split(' ')[0]}.";
                var upStartData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData { type = AbilityType.Passive, subtype = AbilitySubtype.Buff, isBuff = true };
                passiva.NodeData.Add(new Celestial_Cross.Scripts.Abilities.Graph.AbilityNodeData { NodeType = "StartNode", JsonData = JsonUtility.ToJson(upStartData) });
                AssetDatabase.CreateAsset(passiva, $"{AbilitiesPath}/Ability_Unit_{i}_Passive.asset");

                Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO ativa1 = ScriptableObject.CreateInstance<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO>();
                ativa1.abilityName = "Ataque BÃƒÂ¡sico";
                ativa1.abilityDescription = "Golpe rÃƒÂ¡pido e direto em um inimigo.";
                var ua1StartData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData { type = AbilityType.Active, subtype = AbilitySubtype.Attack, isBuff = false };
                ativa1.NodeData.Add(new Celestial_Cross.Scripts.Abilities.Graph.AbilityNodeData { NodeType = "StartNode", JsonData = JsonUtility.ToJson(ua1StartData) });
                AssetDatabase.CreateAsset(ativa1, $"{AbilitiesPath}/Ability_Unit_{i}_Active1.asset");

                Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO ativa2 = ScriptableObject.CreateInstance<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO>();
                ativa2.abilityName = "Poder Ultimate";
                ativa2.abilityDescription = "Usa todo o dom do herÃƒÂ³i para um impacto destrutivo ou curativo massivo.";
                var ua2StartData = new Celestial_Cross.Scripts.Abilities.Graph.Runtime.StartNodeData { type = AbilityType.Active, subtype = AbilitySubtype.Attack, isBuff = false };
                ativa2.NodeData.Add(new Celestial_Cross.Scripts.Abilities.Graph.AbilityNodeData { NodeType = "StartNode", JsonData = JsonUtility.ToJson(ua2StartData) });
                AssetDatabase.CreateAsset(ativa2, $"{AbilitiesPath}/Ability_Unit_{i}_Active2.asset");

                // Criar Unidade
                UnitData unit = ScriptableObject.CreateInstance<UnitData>();
                unit.displayName = unitNames[i];
                unit.baseStats = unitStats[i];
                unit.icon = GenerateColoredSprite(unitColors[i], $"Icon_Unit_{i}");
                
                unit.abilityGraphs = new List<Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO> { passiva, ativa1, ativa2 };

                AssetDatabase.CreateAsset(unit, $"{UnitsPath}/Unit_{i}.asset");
            }
        }

        private static Sprite GenerateColoredSprite(Color color, string spriteName)
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            // Fundo da cor e borda preta
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x < 4 || x > size - 4 || y < 4 || y > size - 4)
                        pixels[y * size + x] = Color.black; 
                    else
                        pixels[y * size + x] = color;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            string path = $"{BasePath}/{spriteName}.png";
            global::System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);

            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}








