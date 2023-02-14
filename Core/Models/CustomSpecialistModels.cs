#nullable enable
using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    public class SpecialistConfiguration
    {
        public string Id { get; set; }
        public long Priority { get; set; }
        public string SpecialistName { get; set; }
        public SimpleUser Creator { get; set; }
        public List<SpecialistEffectConfiguration> SpecialistEffects { get; set; }
        public string PromotesFromSpecialistId { get; set; }
    }

    public class SpecialistEffectConfiguration
    {
        public long Value { get; set; }
        public EffectTrigger EffectTrigger { get; set; }
        public EffectTarget EffectTarget { get; set; }
        public EffectTriggerRange EffectTriggerRange { get; set; }
        public EffectModifier EffectModifier { get; set; }
        public SpecialistEffectScale EffectScale { get; set; }
    }
    
    public class SpecialistEffectScale {
        public EffectScale EffectScale { get; set; }
        public EffectTarget EffectScaleTarget { get; set; }
        public EffectTriggerRange EffectTriggerRange { get; set; }
    }

    public enum EffectTrigger
    {
        NoTrigger = 0,
        Hire = 1,
        Promote = 2,
        SubLaunch = 3,
        SubArrive = 4,
        OutpostCombat = 5,
        SubCombat = 6,
        CombatLoss = 7,
        CombatVictory = 8,
        FactoryProduce = 9,
        MineProduce = 10,
        SubEnterVision = 11,
    }

    public enum EffectTarget
    {
        NoTarget = 0,
        Friendly = 1,
        Enemy = 2,
        OnlyCombatParticipants = 3,
        Any = 4,
    }

    public enum EffectTriggerRange
    {
        None = 0,
        Self = 1,
        Local = 2,
        ConstantRange = 3,
        LocationVisionRange = 4,
        PlayerVisionRange = 5,
        Global = 6,
    }

    public enum EffectModifier
    {
        NoEffect = 0,
        Driller = 1,
        SpecialistCapacity = 2,
        ShieldValue = 3,
        ShieldRegenerationRate = 4,
        ShieldMaxValue = 5,
        VisionRange = 6,
        Speed = 7,
        KillPlayer = 8,
        VictoryPlayer = 9,
        NeptuniumGenerationRate = 10,
        KillSpecialist = 11,
        SwapSpecialistOwner = 12,
    }

    public enum EffectScale
    {
        NoScale = 0,
        ConstantValue = 1,
        PlayerOutpostCount = 2,
    }
    
    public class SpecialistPackage {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public SimpleUser Creator { get; set; }
        public string PackageName { get; set; }
        public List<string> SpecialistIds { get; set; }
        public List<string> PackageIds { get; set; }
        public long TimesUsedInGame { get; set; }
        public double AverageRating { get; set; }
    }

    public class SubmitCustomSpecialistRequest
    {
        public long Priority { get; set; }
        public string SpecialistName { get; set; }
        public List<SpecialistEffectConfiguration> SpecialistEffects { get; set; }
        public string PromotesFromSpecialistId { get; set; }
    }

    public class SubmitCustomSpecialistResponse
    {
        public string SpecialistConfigurationId { get; set; }
    }

    public class GetCustomSpecialistsRequest
    {
        public long Pagination { get; set; } = 1;
        public string? SearchTerm { get; set; }
        public string? PromotesFromSpecialistId { get; set; }
        public string? CreatedByPlayerId { get; set; }
    }

    public class GetCustomSpecialistsResponse
    {
        public List<SpecialistConfiguration> CustomSpecialists { get; set; }
    }

    public class CreateSpecialistPackageRequest
    {
        public string PackageName { get; set; }
        public List<string> SpecialistIds { get; set; }
        public List<string> PackageIds { get; set; }
    }

    public class CreateSpecialistPackageResponse
    {
        public string SpecialistPackageId { get; set; }
    }

    public class GetSpecialistPackagesRequest
    {
        public long Pagination { get; set; } = 1;
        public string? SearchTerm { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? ContainsSpecialistId { get; set; }
        public string? ContainsPackageId { get; set; }
    }

    public class GetSpecialistPackagesResponse
    {
        public List<SpecialistPackage> SpecialistPackages { get; set; }
    }

}