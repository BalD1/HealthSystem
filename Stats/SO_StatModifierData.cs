using UnityEngine;

namespace StdNounou
{
    [CreateAssetMenu(fileName = "New StatModifier Data", menuName = "Scriptable/Stats/StatModifier Data")]
    public class SO_StatModifierData : ScriptableObject
    {
        [field: SerializeField] public string ID { get; private set; }

        [field: SerializeField] public bool Stackable { get; private set; }

        [field: SerializeField] public IStatContainer.E_StatType StatType { get; private set; }
        [field: SerializeField] public float Amount { get; private set; }
        [field: SerializeField] public E_ModifierType ModifierType { get; private set; }

        [field: SerializeField] public bool Temporary { get; private set; }
        [field: SerializeField] public int TicksLifetime { get; private set; }

        public enum E_ModifierType
        {
            Additive,
            Multiplier
        }
    }  
}