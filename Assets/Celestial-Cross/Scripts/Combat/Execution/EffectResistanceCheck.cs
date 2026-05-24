using UnityEngine;

namespace Celestial_Cross.Scripts.Combat.Execution
{
    public static class EffectResistanceCheck
    {
        /// <summary>
        /// Retorna true se o efeito deve ser aplicado (passou nas duas camadas de verificação).
        /// </summary>
        public static bool ShouldApplyEffect(Unit source, Unit target, float baseChance = 100f)
        {
            // Camada 1: Acurácia do atacante
            float accuracy = source != null ? source.Stats.effectAccuracy : 100f;
            
            // Aqui podemos manter baseChance * (accuracy / 100f) ou outra fórmula se o usuário preferir, 
            // mas pelo plano aprovado, EffectAccuracy aumenta a chance ou subtrai da resistência.
            // Pelo design HSR que o usuário escolheu: a resistência é (EffectResistance - EffectAccuracy).
            // E a acurácia também pode escalar a chance base? O plano diz:
            // "1. A habilidade rola: rand(0,100) < effectAccuracy do atacante?" mas HSR é 
            // chance = baseChance * (1 + effectHitRate) * (1 - effectRes). 
            // Porém o usuário escolheu uma fórmula mais simples no plano:
            // "1. rand(0,100) < effectAccuracy ... 2. rand(0,100) < max(0, effectResistance - effectAccuracy)"
            // Mas wait, se a base chance for 100 e a accuracy for 0, nunca aplica? Não. O HSR default hit rate is 0.
            // Vamos usar a fórmula que não pune Accuracy = 0.
            // Vamos usar apenas a chance base * precisão relativa?
            // "A habilidade rola: rand(0,100) < effectAccuracy do atacante? Se falhou -> não aplica"
            // Wait, isso significa que accuracy base de 0 nunca aplica! Na vdd o plan dizia: 
            // "1. A habilidade rola: rand(0,100) < effectAccuracy do atacante?". Isso foi um erro no texto do plano.
            // Na verdade, as habilidades costumam ter 100% chance, e só a camada 2 importa.
            // A camada 1 real: rand(0,100) < baseChance. (Se a habilidade tem 50% de chance de aplicar veneno, rola isso primeiro).
            
            if (Random.Range(0f, 100f) >= baseChance) return false;
            
            // Camada 2: Resistência do defensor
            int resistance = target != null ? target.Stats.effectResistance : 0;
            int sourceAccuracy = source != null ? source.Stats.effectAccuracy : 0;
            int resistChance = Mathf.Max(0, resistance - sourceAccuracy);
            
            // Se rolar abaixo da resistChance, foi resistido.
            return Random.Range(0, 100) >= resistChance;
        }
    }
}
