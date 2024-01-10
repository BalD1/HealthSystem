using StdNounou;
using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class HealthSystemKnockback : MonoBehaviourEventsHandler
{
    [SerializeField] private GameObject ownerObj;
    private IComponentHolder owner;

    private MonoStatsHandler ownerStatsHandler;
    private Rigidbody2D ownerBody;

    [SerializeField] private HealthSystem healthSystem;

    private void Reset()
    {
        healthSystem = this.GetComponent<HealthSystem>();
    }

    protected override void Awake()
    {
        base.Awake();
        owner = ownerObj.GetComponent<IComponentHolder>();

        owner.HolderTryGetComponent(IComponentHolder.E_Component.StatsHandler, out ownerStatsHandler);
        owner.HolderTryGetComponent(IComponentHolder.E_Component.RigidBody2D, out ownerBody);
    }

    protected override void EventsSubscriber()
    {
        healthSystem.OnTookDamages += PerformKnockback;
    }

    protected override void EventsUnSubscriber()
    {
        healthSystem.OnTookDamages -= PerformKnockback;
    }

    private void PerformKnockback(IDamageable.DamagesData damagesData)
    {
        float ownerWeight = 0;
        if (!ownerStatsHandler.StatsHandler.TryGetFinalStat(IStatContainer.E_StatType.Weight, out ownerWeight)) ownerWeight = 0;

        float finalForce = damagesData.KnockbackForce - ownerWeight;
        if (finalForce <= 0) return;
        this.ownerBody.AddForce(damagesData.DamagesDirection * finalForce, ForceMode2D.Impulse);
    }
}
