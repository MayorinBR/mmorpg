namespace Project.Combat
{
    /// <summary>
    /// A generalized set of stat bonuses a buff/debuff or persistent
    /// modifier (see <see cref="BuffController"/>) can carry: the original
    /// ATK%/DEF%/MDEF trio, plus ASPD%, flat Max HP, and the six base
    /// stats (STR/AGI/VIT/INT/DEX/LUK) — enough to cover skills like
    /// Blessing, Increase AGI, Angelus and Improve Concentration without a
    /// new payload shape per skill. Carries raw stat fields rather than
    /// <see cref="Project.Items.StatModifiers"/> directly, since
    /// Project.Combat cannot reference Project.Items without creating an
    /// assembly cycle (Project.Items already references Project.Combat) —
    /// the conversion happens in <see cref="Character.Combat.PlayerStatsController"/>,
    /// which references both.
    /// </summary>
    public readonly struct BuffPayload
    {
        /// <summary>
        /// Initializes a new payload. Every component defaults to zero, so
        /// a caller only needs to pass the components its buff actually has.
        /// </summary>
        public BuffPayload(float atkPercent = 0f, float defPercent = 0f, int mdefFlat = 0, float aspdPercent = 0f, int maxHealthFlat = 0, int strength = 0, int agility = 0, int vitality = 0, int intelligence = 0, int dexterity = 0, int luck = 0, float incomingDamageReductionPercent = 0f)
        {
            AtkPercent = atkPercent;
            DefPercent = defPercent;
            MdefFlat = mdefFlat;
            AspdPercent = aspdPercent;
            MaxHealthFlat = maxHealthFlat;
            Strength = strength;
            Agility = agility;
            Vitality = vitality;
            Intelligence = intelligence;
            Dexterity = dexterity;
            Luck = luck;
            IncomingDamageReductionPercent = incomingDamageReductionPercent;
        }

        /// <summary>Gets the attack power multiplier bonus, e.g. 0.32 for +32%.</summary>
        public float AtkPercent { get; }

        /// <summary>Gets the physical defense multiplier bonus, e.g. -0.55 for -55%.</summary>
        public float DefPercent { get; }

        /// <summary>Gets the flat magical defense bonus.</summary>
        public int MdefFlat { get; }

        /// <summary>Gets the attack speed multiplier bonus, e.g. 0.25 for +25%.</summary>
        public float AspdPercent { get; }

        /// <summary>Gets the flat Max HP bonus.</summary>
        public int MaxHealthFlat { get; }

        /// <summary>Gets the flat Strength bonus.</summary>
        public int Strength { get; }

        /// <summary>Gets the flat Agility bonus.</summary>
        public int Agility { get; }

        /// <summary>Gets the flat Vitality bonus.</summary>
        public int Vitality { get; }

        /// <summary>Gets the flat Intelligence bonus.</summary>
        public int Intelligence { get; }

        /// <summary>Gets the flat Dexterity bonus.</summary>
        public int Dexterity { get; }

        /// <summary>Gets the flat Luck bonus.</summary>
        public int Luck { get; }

        /// <summary>Gets the incoming-damage multiplier reduction, e.g. 0.3 for -30% incoming damage after defense mitigation (see <see cref="BuffController.IncomingDamageMultiplier"/>) — e.g. Energy Coat.</summary>
        public float IncomingDamageReductionPercent { get; }

        /// <summary>Gets whether every component of this payload is zero.</summary>
        public bool IsEmpty =>
            AtkPercent == 0f && DefPercent == 0f && MdefFlat == 0 && AspdPercent == 0f && MaxHealthFlat == 0 &&
            Strength == 0 && Agility == 0 && Vitality == 0 && Intelligence == 0 && Dexterity == 0 && Luck == 0 &&
            IncomingDamageReductionPercent == 0f;

        /// <summary>
        /// Combines two payloads by summing each component.
        /// </summary>
        public static BuffPayload operator +(BuffPayload a, BuffPayload b)
        {
            return new BuffPayload(
                a.AtkPercent + b.AtkPercent,
                a.DefPercent + b.DefPercent,
                a.MdefFlat + b.MdefFlat,
                a.AspdPercent + b.AspdPercent,
                a.MaxHealthFlat + b.MaxHealthFlat,
                a.Strength + b.Strength,
                a.Agility + b.Agility,
                a.Vitality + b.Vitality,
                a.Intelligence + b.Intelligence,
                a.Dexterity + b.Dexterity,
                a.Luck + b.Luck,
                a.IncomingDamageReductionPercent + b.IncomingDamageReductionPercent);
        }
    }
}
