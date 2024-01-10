using System;
using System.Collections.Generic;
using UnityEngine;
using static StdNounou.IStatContainer;

namespace StdNounou
{
    public class MonoStatsHandler : MonoBehaviour
    {
        [field: SerializeField] public StatsHandler StatsHandler { get; private set; }

        private void Awake()
        {
            StatsHandler.InitializeDictionaries();
        }
    }

    [System.Serializable]
    public class StatsHandler
    {
        public StatsHandler(SO_BaseStats baseStats)
        {
            this.BaseStats = baseStats;
            InitializeDictionaries();
        }

        [field: SerializeField] public SO_BaseStats BaseStats { get; private set; }
        public Dictionary<E_StatType, float> PermanentBonusStats { get; protected set; } = new Dictionary<E_StatType, float>();
        public Dictionary<E_StatType, float> TemporaryBonusStats { get; protected set; } = new Dictionary<E_StatType, float>();
        public Dictionary<E_StatType, float> BrutFinalStats { get; protected set; } = new Dictionary<E_StatType, float>();

        public Dictionary<string, StatsModifier> UniqueStatsModifiers { get; protected set; } = new Dictionary<string, StatsModifier>();
        public Dictionary<string, List<StatsModifier>> StackableStatsModifiers { get; protected set; } = new Dictionary<string, List<StatsModifier>>();

        public event Action OnAskReset;

        public enum E_ModifierAddResult
        {
            Success,
            StatAlreadyMaxed,
            Unstackable,
        }

        public event Action<StatChangeEventArgs> OnStatChange;
        public class StatChangeEventArgs : EventArgs
        {
            public StatChangeEventArgs(E_StatType type, float modifier, float finalVal)
            {
                this.Type = type;
                this.ModifierValue = modifier;
                this.FinalValue = finalVal;
            }
            public E_StatType Type { get; private set; }
            public float ModifierValue { get; private set; }
            public float FinalValue { get; private set; }
        }

        public void InitializeDictionaries()
        {
            PermanentBonusStats = new Dictionary<E_StatType, float>();
            TemporaryBonusStats = new Dictionary<E_StatType, float>();
            BrutFinalStats = new Dictionary<E_StatType, float>();

            UniqueStatsModifiers = new Dictionary<string, StatsModifier>();
            StackableStatsModifiers = new Dictionary<string, List<StatsModifier>>();
            if (BaseStats == null)
            {
                this.LogError("Stats SO was not set.");
                return;
            }
            foreach (var item in BaseStats.GetAllStats())
            {
                BrutFinalStats.Add(item.Key, item.Value.Value);
                PermanentBonusStats.Add(item.Key, 0);
                TemporaryBonusStats.Add(item.Key, 0);
            }
        }

        /// <summary>
        /// Tries to out "<paramref name="value"/>" the base stat of type "<paramref name="type"/>".
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetBaseStat(E_StatType type, out float value)
        {
            return BaseStats.TryGetStatValue(type, out value);
        }

        /// <summary>
        /// Tries to out "<paramref name="value"/>" the final stat, after all modifiers calculations, of type <paramref name="type"/>".
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetFinalStat(E_StatType type, out float value)
        {
            bool res = BrutFinalStats.TryGetValue(type, out value);
            if (!res) return false;

            float higherAllowedValue = BaseStats.Stats[type].HigherAllowedValue;

            if (value > higherAllowedValue)
                value = higherAllowedValue;

            return true;
        }

        public bool TryAddModifier(SO_StatModifierData data, out E_ModifierAddResult result)
        {
            if (data.Stackable)
                return TryAddStackableModifier(data, out result);
            else
                return TryAddUniqueModifier(data, out result);
        }

        private bool TryAddStackableModifier(SO_StatModifierData data, out E_ModifierAddResult result)
        {
            if (!TrySetModifier(data))
            {
                result = E_ModifierAddResult.StatAlreadyMaxed;
                return false;
            }

            //add the modifier to the list
            if (!StackableStatsModifiers.ContainsKey(data.ID))
                StackableStatsModifiers.Add(data.ID, new List<StatsModifier>());
            StackableStatsModifiers[data.ID].Add(new StatsModifier(data, this));

            result = E_ModifierAddResult.Success;
            ModifyBrutFinalStat(data.StatType, data.Amount);
            return true;
        }

        private bool TryAddUniqueModifier(SO_StatModifierData data, out E_ModifierAddResult result)
        {
            // if the unique modifier already exists, return
            if (UniqueStatsModifiers.ContainsKey(data.ID))
            {
                result = E_ModifierAddResult.Unstackable;
                return false;
            }
            if (!TrySetModifier(data))
            {
                result = E_ModifierAddResult.StatAlreadyMaxed;
                return false;
            }

            UniqueStatsModifiers.Add(data.ID, new StatsModifier(data, this));
            ModifyBrutFinalStat(data.StatType, data.Amount);
            result = E_ModifierAddResult.Success;
            return true;
        }

        private bool TrySetModifier(SO_StatModifierData data)
        {
            float modifierValue = GetModifierValue(data);
            if (data.Temporary)
            {
                if (TemporaryBonusStats.ContainsKey(data.StatType))
                    TemporaryBonusStats[data.StatType] += modifierValue;
                else
                    TemporaryBonusStats.Add(data.StatType, modifierValue);
            }
            else
            {
                // else, check if the sum of all permanent bonus is >= of max, return.
                // else, add it to permanent bonuses
                if (!PermanentBonusStats.ContainsKey(data.StatType))
                {
                    this.LogError($"Stat \"{data.StatType}\" type was not present in dictionnary.");
                    return false;
                }
                else
                {
                    if (PermanentBonusStats[data.StatType] >= (BaseStats.Stats[data.StatType].HigherAllowedValue - BaseStats.Stats[data.StatType].Value))
                        return false;
                    if (PermanentBonusStats.ContainsKey(data.StatType))
                        PermanentBonusStats[data.StatType] += modifierValue;
                    else
                        PermanentBonusStats.Add(data.StatType, modifierValue);
                }
            }
            return true;
        }

        private float GetModifierValue(SO_StatModifierData data)
        {
            switch (data.ModifierType)
            {
                case SO_StatModifierData.E_ModifierType.Additive:
                    return data.Amount;
                case SO_StatModifierData.E_ModifierType.Multiplier:
                    BaseStats.TryGetStatValue(data.StatType, out float baseStatValue);
                    return baseStatValue * data.Amount;
            }
            return 0;
        }

        private void ModifyBrutFinalStat(E_StatType type, float value)
        {
            if (BrutFinalStats.ContainsKey(type))
            {
                BrutFinalStats[type] += value;
                OnStatChange?.Invoke(new StatChangeEventArgs(type, value, BrutFinalStats[type]));
            }
        }

        public void RemoveStatModifier(StatsModifier modifier)
        {
            float modifierValue = GetModifierValue(modifier.Data);
            if (modifier.Data.Temporary)
                TemporaryBonusStats[modifier.Data.StatType] -= modifierValue;
            else
                PermanentBonusStats[modifier.Data.StatType] -= modifierValue;

            if (modifier.Data.Stackable)
            {
                ModifyBrutFinalStat(modifier.Data.StatType, -modifierValue);
                StackableStatsModifiers[modifier.Data.ID].Remove(modifier);

                return;
            }

            ModifyBrutFinalStat(modifier.Data.StatType, -modifierValue);
            UniqueStatsModifiers.Remove(modifier.Data.ID);
        }

        public void ChangeBaseStats(SO_BaseStats stats, bool resetModifiers)
        {
            BaseStats = stats;
            if (resetModifiers) RemoveAllModifiers();
        }

        public void RemoveAllModifiers()
        {
            OnAskReset?.Invoke();
        }

        public SO_BaseStats.E_Team GetTeam()
            => BaseStats.Team;
    }
}
