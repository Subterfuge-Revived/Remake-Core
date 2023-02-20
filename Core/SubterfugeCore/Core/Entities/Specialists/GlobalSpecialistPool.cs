using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Core.Entities.Specialists
{
    public class GlobalSpecialistPool
    {
        private static readonly SimpleUser _globalCreator = new SimpleUser()
        {
            Id = "1",
            Username = "Subterfuge"
        };
        
        private readonly Dictionary<GameVersion, List<SpecialistConfiguration>> SpecialistConfigurationMapping =
            new Dictionary<GameVersion, List<SpecialistConfiguration>>()
            {
                {
                    // Version 1 Configuration
                    GameVersion.V1, new List<SpecialistConfiguration>()
                    {
                        new SpecialistConfiguration()
                        {
                            Id = "Advisor",
                            Priority = 1,
                            SpecialistName = "Advisor",
                            Creator = _globalCreator,
                            SpecialistEffects = new List<SpecialistEffectConfiguration>(),
                            PromotesFromSpecialistId = null,
                        },
                        new SpecialistConfiguration()
                        {
                            Id = "Queen",
                            Priority = 1,
                            SpecialistName = "Queen",
                            Creator = _globalCreator,
                            SpecialistEffects = new List<SpecialistEffectConfiguration>()
                            {
                                new SpecialistEffectConfiguration()
                                {
                                    Value = 1,
                                    EffectModifier = EffectModifier.KillPlayer,
                                    EffectTarget = EffectTarget.Friendly,
                                    EffectTrigger = EffectTrigger.CombatLoss,
                                },
                                new SpecialistEffectConfiguration()
                                {
                                    Value = 20,
                                    EffectTrigger = EffectTrigger.SubArrive,
                                    EffectTarget = EffectTarget.Friendly,
                                    EffectModifier = EffectModifier.ShieldMaxValue,
                                },
                                new SpecialistEffectConfiguration
                                {
                                    Value = -20,
                                    EffectTrigger = EffectTrigger.SubLaunch,
                                    EffectTarget = EffectTarget.Friendly,
                                    EffectModifier = EffectModifier.ShieldMaxValue,
                                },
                            },
                            PromotesFromSpecialistId = null,
                        }
                    }

                },
            };

        public List<SpecialistConfiguration> GetPool()
        {
            return SpecialistConfigurationMapping[Game.GameConfiguration.GameVersion];
        }
    }
}