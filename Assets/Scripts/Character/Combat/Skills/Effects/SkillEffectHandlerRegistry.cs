using System.Collections.Generic;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Maps each <see cref="SkillEffectType"/> to the handler that resolves it. A type without a registered
    /// handler falls back to the default handler.
    /// </summary>
    public sealed class SkillEffectHandlerRegistry
    {
        private readonly Dictionary<SkillEffectType, ISkillEffectHandler> handlers = new Dictionary<SkillEffectType, ISkillEffectHandler>();
        private readonly ISkillEffectHandler fallback;

        /// <summary>Initializes the registry.</summary>
        /// <param name="fallback">The handler used for any effect type without its own handler.</param>
        public SkillEffectHandlerRegistry(ISkillEffectHandler fallback)
        {
            this.fallback = fallback;
        }

        /// <summary>
        /// Registers the handler for an effect type, replacing any previous one.
        /// </summary>
        /// <param name="type">The effect type.</param>
        /// <param name="handler">The handler resolving it.</param>
        public void Register(SkillEffectType type, ISkillEffectHandler handler)
        {
            handlers[type] = handler;
        }

        /// <summary>
        /// Gets the handler for an effect type.
        /// </summary>
        /// <param name="type">The effect type.</param>
        /// <returns>The registered handler, or the fallback.</returns>
        public ISkillEffectHandler Get(SkillEffectType type)
        {
            return handlers.TryGetValue(type, out var handler) ? handler : fallback;
        }

        /// <summary>
        /// Builds the registry with a handler for every effect type the game currently has.
        /// </summary>
        /// <returns>The default registry.</returns>
        public static SkillEffectHandlerRegistry CreateDefault()
        {
            var registry = new SkillEffectHandlerRegistry(new DamageEffectHandler());
            registry.Register(SkillEffectType.Heal, new HealEffectHandler());
            registry.Register(SkillEffectType.Buff, new BuffEffectHandler());
            registry.Register(SkillEffectType.Toggle, new ToggleEffectHandler());
            registry.Register(SkillEffectType.ToggleDrain, new ToggleDrainEffectHandler());
            registry.Register(SkillEffectType.Reveal, new RevealEffectHandler());
            registry.Register(SkillEffectType.Cleanse, new CleanseEffectHandler());
            registry.Register(SkillEffectType.Displacement, new DisplacementEffectHandler());
            registry.Register(SkillEffectType.Craft, new CraftEffectHandler());
            registry.Register(SkillEffectType.Teleport, new TeleportEffectHandler());
            return registry;
        }
    }
}
