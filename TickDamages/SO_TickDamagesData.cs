using UnityEngine;

namespace StdNounou
{
    [CreateAssetMenu(fileName = "New TickDamages Data", menuName = "Scriptable/TickDamages/Data")]
    public class SO_TickDamagesData : ScriptableObject
    {
        [field: SerializeField] public string ID { get; private set; }

        [field: SerializeField] public IDamageable.E_DamagesType DamagesType { get; private set; }

        [field: SerializeField] public bool Stackable { get; private set; }
        [field: SerializeField, Range(0, 100)] public float ChancesToApply { get; private set; } = 50;

        [field: SerializeField] public int TicksLifetime { get; private set; }
        [field: SerializeField] public int RequiredTicksToTrigger { get; private set; }

        [field: SerializeField] public float DamagesModifier { get; private set; } = 1;
        [field: SerializeField] public int CritChancesModifier { get; private set; } = 1;
        [field: SerializeField] public float CritMultiplierModifier { get; private set; } = 1;

        [field: SerializeField] public ParticlesPlayer Particles { get; private set; }
    } 
}