using Microsoft.Xna.Framework;
using SubterfugeCore.Components;
using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Players;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Specialist : IOwnable
    {
        int priority;
        String specialistName;
        Player owner;
        SpecialistEffectListener effectListener = new SpecialistEffectListener();
        List<ISpecialistEffect> specialistEffects = new List<ISpecialistEffect>();


        // Event delegates for Effects to listen to
        public event EventHandler Combat;

        public Specialist(String name, int priority, Player owner)
        {
            this.specialistName = name;
            this.priority = priority;
            this.owner = owner;
        }

        public List<ISpecialistEffect> getSpecialistEffects()
        {
            return this.specialistEffects;
        }

        public void removeSpecialistEffect(ISpecialistEffect effect)
        {
            if(specialistEffects.Contains(effect))
            {
                specialistEffects.Remove(effect);
            }
        }

        public void addSpecialistEffect(ISpecialistEffect effect)
        {
            specialistEffects.Add(effect);
        }

        public void applyEffect(ICombatable combatable)
        {
            foreach(ISpecialistEffect effect in this.specialistEffects)
            {
                effect.forwardEffect(combatable);
            }
        }

        public void undoEffect(ICombatable combatable)
        {
            foreach (ISpecialistEffect effect in this.specialistEffects)
            {
                effect.backwardEffect(combatable);
            }
        }

        public int getPriority()
        {
            return this.priority;
        }

        public Player getOwner()
        {
            return this.owner;
        }

        public void setOwner(Player newOwner)
        {
            this.owner = newOwner;
        }

        public void invoke(EffectTrigger trigger)
        {
            effectListener.invoke(trigger);
        }
    }
}
