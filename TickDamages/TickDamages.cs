using System;
using UnityEngine;

namespace StdNounou
{
    [System.Serializable]
    public class TickDamages : ITickable, IDisposable
    {
        [field: SerializeField, ReadOnly] public SO_TickDamagesData Data { get; private set; }
        [SerializeField, ReadOnly] private HealthSystem handler;

        private float damages;
        private float critChances;
        private float critMultiplier;

        private IDamageable.DamagesData damagesData;

        private int currentTicks;

        private ParticlesPlayer particles;

        public TickDamages(SO_TickDamagesData _data, HealthSystem _handler, float damages, float critChances, float critMultiplier)
        {
            this.Data = _data;

            this.handler = _handler;
            handler.OnDeath += OnEnd;

            TickManagerEvents.OnTick += OnTick;

            damagesData = new IDamageable.DamagesData();
            SetDamagesData();
            this.damages = damages;
            this.critChances = critChances;
            this.critMultiplier = critMultiplier;

            particles = Data.Particles?.Create(_handler.transform);
        }

        public void Dispose()
        {
            TickManagerEvents.OnTick -= OnTick;
        }

        public virtual void OnTick(int tick)
        {
            currentTicks++;
            if (currentTicks % Data.RequiredTicksToTrigger == 0)
            {
                ApplyDamages();
            }

            if (currentTicks >= Data.TicksLifetime) OnEnd();
        }

        protected virtual void ApplyDamages()
        {
            SetDamagesData();
            handler.InflictDamages(damagesData);
        }

        public void KillTick()
            => OnEnd();

        private void OnEnd()
        {
            TickManagerEvents.OnTick -= OnTick;
            if (handler != null)
            {
                handler.OnDeath -= OnEnd;
                handler.RemoveTickDamage(this);
            }
            if (particles != null) GameObject.Destroy(particles.gameObject);
        }

        public float RemainingTimeInSeconds()
            => RemainingTicks() * TickManager.TICK_TIMER_MAX;

        public int RemainingTicks()
           => Data.TicksLifetime - currentTicks;

        private void SetDamagesData()
        {
            bool isCrit = RandomExtensions.PercentageChance(critChances * Data.CritChancesModifier);
            damagesData.SetIsCrit(isCrit);
            float finalDamages = damages * Data.DamagesModifier;

            if (!isCrit) damagesData.SetDamages(finalDamages);
            else
            {
                damagesData.SetDamages(finalDamages * (critMultiplier * Data.CritMultiplierModifier));
            }
        }
    } 
}