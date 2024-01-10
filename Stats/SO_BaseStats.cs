using AYellowpaper.SerializedCollections;
using UnityEngine;
using static StdNounou.IStatContainer;

namespace StdNounou
{
    [CreateAssetMenu(fileName = "New Stats", menuName = "Scriptable/Stats/Base Data")]
    public class SO_BaseStats : ScriptableObject, IStatContainer
    {
        [field: SerializeField] public string EntityName { get; private set; }
        [field: SerializeField] public E_Team Team { get; private set; }
        [field: SerializeField] public SerializedDictionary<E_StatType, StatData> Stats { get; private set; }

        public enum E_Team
        {
            Players,
            Enemies,
            Neutral,
        }

        public bool TryGetStatData(E_StatType type, out StatData statData)
        {
            statData = null;
            if (Stats.TryGetValue(type, out statData)) return true;
            return false;
        }

        public bool TryGetStatValue(E_StatType type, out float value)
        {
            value = -1;
            if (Stats.TryGetValue(type, out StatData statData))
            {
                value = statData.Value;
                return true;
            }
            return false;
        }

        public bool TryGetHigherAllowedValue(E_StatType type, out float value)
        {
            value = -1;
            if (Stats.TryGetValue(type, out StatData statData))
            {
                value = statData.HigherAllowedValue;
                return true;
            }
            return false;
        }

        public SerializedDictionary<E_StatType, StatData> GetAllStats()
            => Stats;

        public override string ToString()
        => EntityName;
    } 
}