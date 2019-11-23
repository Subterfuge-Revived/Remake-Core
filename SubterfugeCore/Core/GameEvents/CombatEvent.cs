using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SubterfugeCore.Core.Components.Outpost;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Timing;

namespace SubterfugeCore.GameEvents
{
    // It is considered a 'combat' if you are arriving at any outpost, even your own!
    public class CombatEvent : GameEvent
    {
        private GameTick eventTick;
        private Vector2 combatLocation;
        private ICombatable combatant1;
        private ICombatable combatant2;

        public CombatEvent(ICombatable combatant1, ICombatable combatant2, GameTick tick, Vector2 combatLocation)
        {
            this.combatant1 = combatant1;
            this.combatant2 = combatant2;
            this.eventTick = tick;
            this.combatLocation = combatLocation;
        }
        public override void eventBackwardAction()
        {
            if (this.eventSuccess)
            {
                if (combatant1.getOwner() != combatant2.getOwner())
                {
                }
            }
        }

        public override void eventForwardAction()
        {
            // Check if everything in combat exists.
            Sub sub1 = combatant1 as Sub;
            Sub sub2 = combatant2 as Sub;
            if(sub1 != null && !GameServer.timeMachine.getState().subExists(sub1))
            {
                return;
            }
            if (sub2 != null && !GameServer.timeMachine.getState().subExists(sub2))
            {
                return;
            }
            if (combatant1.getOwner() != combatant2.getOwner())
            {
                this.preSpecialistPhase();
                this.specialistPhase();
                this.postSpecialistPhase();
                this.preDrillerCombat();
                this.drillerCombat();
                this.postDrillerCombat();
            } else
            {
                // Arrive at friendly location
                Sub arrivingSub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
                Outpost outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);

                outpost.addDrillers(arrivingSub.getDrillerCount());
                outpost.getSpecialistManager().addSpecialists(arrivingSub.getSpecialistManager().getSpecialists());
                GameServer.timeMachine.getState().removeSub(arrivingSub);
            }
            this.eventSuccess = true;
        }

        public void preSpecialistPhase()
        {

        }

        public void specialistPhase()
        {
            // Get a list of the player's global effects and apply them here.

            List<Specialist> specialists = new List<Specialist>();
            specialists.AddRange(combatant1.getSpecialistManager().getSpecialists());
            specialists.AddRange(combatant2.getSpecialistManager().getSpecialists());

            while (specialists.Count > 0)
            {
                Specialist topPriority = null;
                foreach (Specialist s in specialists)
                {
                    if (topPriority == null || s.getPriority() < topPriority.getPriority())
                    {
                        topPriority = s;
                    }
                }
                // Apply the specialist effect to the enemey.
                ICombatable enemy = combatant1.getOwner() == topPriority.getOwner() ? combatant2 : combatant1;
                topPriority.applyEffect(enemy);
            }
        }

        public void postSpecialistPhase() { }

        public void preDrillerCombat() { }

        public void drillerCombat()
        {
            int drillers1 = combatant1.getDrillerCount();
            int drillers2 = combatant2.getDrillerCount();
            combatant1.removeDrillers(drillers2);
            combatant2.removeDrillers(drillers1);
        }

        public void postDrillerCombat()
        {
            // Change ownership
            if(combatant1.getDrillerCount() != combatant2.getDrillerCount())
            {
                // No tie, decisive winner.
                ICombatable winner = combatant1.getDrillerCount() > combatant2.getDrillerCount() ? combatant1 : combatant2;
                ICombatable loser = combatant1.getDrillerCount() > combatant2.getDrillerCount() ? combatant2 : combatant1;

                if(loser is Outpost)
                {
                    loser.setOwner(winner.getOwner());
                    loser.setDrillerCount(winner.getDrillerCount());
                    loser.getSpecialistManager().addSpecialists(winner.getSpecialistManager().getSpecialists());
                    GameServer.timeMachine.getState().removeSub((Sub)winner);
                } else
                {
                    // Loser is a sub
                    GameServer.timeMachine.getState().removeSub((Sub)loser);
                }

            } else
            {
                int specs1 = combatant1.getSpecialistManager().getSpecialistCount();
                int specs2 = combatant2.getSpecialistManager().getSpecialistCount();
                // tie. Determine tiebreaker.
                if (specs1 != specs2)
                {
                    ICombatable winner = specs1 > specs2 ? combatant1 : combatant2;
                    ICombatable loser = specs1 > specs2 ? combatant2 : combatant1;

                    if (loser is Outpost)
                    {
                        loser.setOwner(winner.getOwner());
                        loser.setDrillerCount(winner.getDrillerCount());
                        loser.getSpecialistManager().addSpecialists(winner.getSpecialistManager().getSpecialists());
                        GameServer.timeMachine.getState().removeSub((Sub)winner);
                    }
                    else
                    {
                        // Loser is a sub
                        GameServer.timeMachine.getState().removeSub((Sub)loser);
                    }
                } else
                {
                    // Keep outposts, otherwise destroy both subs
                    if(combatant1 is Outpost || combatant2 is Outpost)
                    {
                        Outpost outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
                        Sub sub = (Sub)(combatant1 is Outpost ? combatant2 : combatant1);

                        outpost.setDrillerCount(0);
                        GameServer.timeMachine.getState().removeSub(sub);
                    } else
                    {
                        GameServer.timeMachine.getState().removeSub((Sub)combatant1);
                        GameServer.timeMachine.getState().removeSub((Sub)combatant2);
                    }
                }
            }
        }

        public void undoSpecialistEffects()
        {

            List<Specialist> specialists = new List<Specialist>();
            specialists.AddRange(combatant1.getSpecialistManager().getSpecialists());
            specialists.AddRange(combatant2.getSpecialistManager().getSpecialists());

            while (specialists.Count > 0)
            {
                Specialist lowPriority = null;
                foreach (Specialist s in specialists)
                {
                    if (lowPriority == null || s.getPriority() >= lowPriority.getPriority())
                    {
                        lowPriority = s;
                    }
                }
                // Apply the specialist effect to the enemey.
                ICombatable enemy = combatant1.getOwner() == lowPriority.getOwner() ? combatant2 : combatant1;
                lowPriority.undoEffect(enemy);
            }


            // Get a list of the player's global effects and undo them here.
        }

        public override GameTick getTick()
        {
            return this.eventTick;
        }

        public Vector2 GetCombatLocation()
        {
            return this.combatLocation;
        }
    }
}
