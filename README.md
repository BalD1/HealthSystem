This Health System is based on a StatsHandler, from which it will search a MaxHealth stat.

Aside from basic damaging, it can handle TickDamages (see below) and is modulable, as shows some exemples:
- Popup Texts, which will display lost or gained health amount, which are fully customable following context (if the damages were criticals for exemple)
- Basic knockback, which can take a Weight stat to make some entities less or more pushable than others

The StatHandler it is based on can hold any type of stat and is also modulable. 
It uses a dictionary populable with keys from an Enum, directly from the Unity Inspector. 
It also is able to hold StatModifiers, which can be additive, multiplicative, temporary or permanent, stackable or not, and will send an event when one is applied or removed.
