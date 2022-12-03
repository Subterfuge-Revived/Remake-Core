using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    public interface INetworkCustomSpeicliastManager
    {

        SubmitCustomSpecialistResponse SubmitCustomSpecialist(
            SubmitCustomSpecialistRequest submitCustomSpecialistRequest);

        GetCustomSpecialistsResponse GetCustomSpecialists(GetCustomSpecialistsRequest getCustomSpecialistsRequest);

        CreateSpecialistPackageResponse CreateSpecialistPackage(CreateSpecialistPackageRequest createSpecialistPackageRequest);

        GetSpecialistPackagesResponse GetSpecialistPackages(GetSpecialistPackagesRequest getSpecialistPackagesRequest);
    }

    public class SpecialistConfiguration
    {
        public string Id { get; set; }
        public long Priority { get; set; }
        public string SpecialistName { get; set; }
        public User Creator { get; set; }
        public List<SpecialistEffectConfiguration> SpecialistEffects { get; set; }
        public string PromotesFromSpecialistId { get; set; }
        public List<SpecialistClass> SpecialistClasses { get; set; }
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

    public enum SpecialistClass
    {
        Unknown,
        Warrier,
        Protector,
        Theif,
        Zoner,
        Investor,
        Controller,
        Producer,
        Influencer,
        Leecher,
        Zoomer,
        Piercer,
    }

    public enum EffectTrigger
    {
        NoTrigger,
        Hire,
        Promote,
        SubLaunch,
        SubArrive,
        OutpostCombat,
        SubCombat,
        CombatLoss,
        CombatVictory,
        FactoryProduce,
        MineProduce,
        SubEnterVision,
    }

    public enum EffectTarget
    {
        NoTarget,
        Friendly,
        Enemy,
        OnlyCombatParticipants,
        Any,
    }

    public enum EffectTriggerRange
    {
        Self,
        Local,
        ConstantRange,
        LocationVisionRange,
        PlayerVisionRange,
        Global,
    }

    public enum EffectModifier
    {
        NoEffect,
        Driller,
        SpecialistCapacity,
        ShieldValue,
        ShieldRegenerationRate,
        ShieldMaxValue,
        VisionRange,
        Speed,
        KillPlayer,
        VictoryPlayer,
        NeptuniumGenerationRate,
        KillSpecialist,
        SwapSpecialistOwner,
    }

    public enum EffectScale
    {
        NoScale,
        ConstantValue,
        PlayerOutpostCount
    }
    
    public class SpecialistPackage {
        public string Id { get; set; }
        public User Creator { get; set; }
        public string PackageName { get; set; }
        public List<string> SpecialistIds { get; set; }
        public long TimesUsedInGame { get; set; }
    }

    public class SubmitCustomSpecialistRequest
    {
        public SpecialistConfiguration SpecialistConfiguration { get; set; }
    }

    public class SubmitCustomSpecialistResponse : NetworkResponse
    {
        public string SpecialistConfigurationId { get; set; }
    }

    public class GetCustomSpecialistsRequest
    {
        public long PageNumber { get; set; }
        public string SearchTerm { get; set; }
        public string PromotesFromSpecialistId { get; set; }
        public string CreatedByPlayerId { get; set; }
    }

    public class GetCustomSpecialistsResponse : NetworkResponse
    {
        public List<SpecialistConfiguration> CustomSpecialists { get; set; }
    }

    public class CreateSpecialistPackageRequest
    {
        public SpecialistPackage SpecialistPackage { get; set; }
    }

    public class CreateSpecialistPackageResponse : NetworkResponse
    {
        public string SpecialistPackageId { get; set; }
    }

    public class GetSpecialistPackagesRequest
    {
        public long PageNumber { get; set; }
        public string SearchTerm { get; set; }
        public string IsPackageCreatedById { get; set; }
        public string PackageContainsSpecialistId { get; set; }
    }

    public class GetSpecialistPackagesResponse : NetworkResponse
    {
        public List<SpecialistPackage> SpecialistPackages { get; set; }
    }

}