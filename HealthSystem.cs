using System;
using System.Collections.Generic;
using UnityEngine;
using static StdNounou.IDamageable;

namespace StdNounou
{
    [RequireComponent(typeof(MonoStatsHandler))]
    public class HealthSystem : MonoBehaviourEventsHandler, IDamageable
    {
        [field: SerializeField] public MonoStatsHandler Stats { get; private set; }

        [SerializeField] private GameObject ownerObj;
        private IComponentHolder owner;

        [field: SerializeField, ReadOnly] public float CurrentHealth { get; protected set; }
        [field: SerializeField, ReadOnly] public float CurrentMaxHealth { get; protected set; }

        public Dictionary<string, TickDamages> UniqueTickDamages { get; private set; } = new Dictionary<string, TickDamages>();
        public Dictionary<string, List<TickDamages>> StackableTickDamages { get; private set; } = new Dictionary<string, List<TickDamages>>();

        [field: SerializeField] public Vector2 HealthPopupOffset { get; private set; }

        [field: SerializeField, ReadOnly] public float InvincibilityTimer { get; protected set; }

        public event Action<IDamageable.DamagesData> OnTookDamages;
        public event Action<float> OnHealed;
        public event Action OnDeath;

        private void Reset()
        {
            Stats = this.GetComponent<MonoStatsHandler>();
        }

        protected override void EventsSubscriber()
        {
            Stats.StatsHandler.OnStatChange += OnStatChange;
        }

        protected override void EventsUnSubscriber()
        {
            Stats.StatsHandler.OnStatChange -= OnStatChange;
        }

        protected override void Awake()
        {
            if (Stats == null) Stats = this.GetComponent<MonoStatsHandler>();
            base.Awake();
            Setup();

            owner = ownerObj.GetComponent<IComponentHolder>();
        }

        protected virtual void Update()
        {
            if (InvincibilityTimer > 0) InvincibilityTimer -= Time.deltaTime;
        }

        private void OnStatChange(StatsHandler.StatChangeEventArgs args)
        {
            if (args.Type == IStatContainer.E_StatType.MaxHP) UpdateMaxHealth(args.FinalValue, true);
        }

        private void Setup()
        {
            float maxHealth = -1;
            if (!Stats.StatsHandler.TryGetFinalStat(IStatContainer.E_StatType.MaxHP, out maxHealth))
                this.LogError("Could not find max HP in Stats");

            UpdateMaxHealth(maxHealth, false);
            CurrentHealth = maxHealth;
        }

        public void UpdateMaxHealth(float newHealth, bool healDifference)
        {
            float pastHealth = CurrentMaxHealth;

            // try get max allowed HP
            if (!Stats.StatsHandler.BaseStats.TryGetHigherAllowedValue(IStatContainer.E_StatType.MaxHP, out float maxAllowedHealth))
            {
                this.LogError("Could not find max HP in Stats");
                maxAllowedHealth = newHealth;
            }

            // clamp the max health between {0 ~ max allowed}
            CurrentMaxHealth = Mathf.Clamp(CurrentHealth + newHealth, 0, maxAllowedHealth);

            if (healDifference)
            {
                float diffenrece = newHealth - pastHealth;
                if (diffenrece > 0)
                    this.Heal(diffenrece, false);
            }
        }

        public virtual bool TryInflictDamages(DamagesData damagesData)
        {
            if (!IsAlive()) return false;
            if (InvincibilityTimer > 0) return false;
            // prevent entities from the same team to damage each other
            if (this.Stats.StatsHandler.GetTeam() != SO_BaseStats.E_Team.Neutral &&
                this.Stats.StatsHandler.GetTeam() == damagesData.DamagerTeam) return false;
            InflictDamages(damagesData);
            return true;
        }
        public virtual void InflictDamages(DamagesData damagesData)
        {
            CurrentHealth -= damagesData.Damages;
            this.OnTookDamages?.Invoke(damagesData);
            if (CurrentHealth <= 0) Kill();
        }

        public void Heal(float amount, bool isCrit)
        {
            if (!IsAlive()) return;
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, CurrentMaxHealth);
            this.OnHealed?.Invoke(amount);
        }

        public bool IsAlive()
            => CurrentHealth > 0;

        public void Kill()
        {
            this.OnDeath?.Invoke();
        }

        public bool TryAddTickDammages(SO_TickDamagesData data, float damages, float critChances, float critMultiplier)
        {
            if (data.Stackable)
            {
                if (!StackableTickDamages.ContainsKey(data.ID))
                    StackableTickDamages.Add(data.ID, new List<TickDamages>());

                StackableTickDamages[data.ID].Add(new TickDamages(data, this, damages, critChances, critMultiplier));
                return true;
            }

            if (UniqueTickDamages.ContainsKey(data.ID)) return false;
            UniqueTickDamages.Add(data.ID, new TickDamages(data, this, damages, critChances, critMultiplier));

            return true;
        }

        public void RemoveTickDamage(TickDamages tick)
        {
            if (tick.Data.Stackable)
            {
                StackableTickDamages[tick.Data.ID].Remove(tick);
                return;
            }

            UniqueTickDamages.Remove(tick.Data.ID);
        }

        public void SetInvincibilityTimer(float time)
            => InvincibilityTimer = time;

        public Vector2 GetHealthPopupPosition()
            => this.transform.position + (Vector3)HealthPopupOffset;

        #region EDITOR

#if UNITY_EDITOR
        [SerializeField] protected bool ED_debugMode;
#endif

        protected virtual void OnDrawGizmos()
        {
#if UNITY_EDITOR
            // displays a gizmo to see the health popup start position 

            if (!ED_debugMode) return;

            Vector2 healthBordersSize = new Vector2(0.75f, 0.5f);
            Gizmos.DrawWireCube((Vector2)this.transform.position + HealthPopupOffset, healthBordersSize);

            Color c = UnityEditor.Handles.color;
            UnityEditor.Handles.color = Color.red;

            Vector2 centeredPosition = (Vector2)this.transform.position + HealthPopupOffset;

            if (UnityEditor.SceneView.currentDrawingSceneView == null) return;

            var view = UnityEditor.SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(centeredPosition);


            Vector2 textOffset = new Vector2(-36, 7.5f);
            Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
            if (cam)
                centeredPosition = cam.ScreenToWorldPoint((Vector2)cam.WorldToScreenPoint(centeredPosition) + textOffset);


            UnityEditor.Handles.Label(centeredPosition, "Health Popup");

            UnityEditor.Handles.color = c;
#endif
        }

        #endregion
    } 
}
