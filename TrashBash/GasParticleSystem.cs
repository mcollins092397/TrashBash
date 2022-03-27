using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TrashBash
{
    public class GasParticleSystem : ParticleSystem
    {

        public GasParticleSystem(Game game, int maxExplosions) : base(game, maxExplosions * 45) { }

        protected override void InitializeConstants()
        {
            textureFilename = "gas";

            minNumParticles = 40;
            maxNumParticles = 45;

            blendState = BlendState.Additive;
            DrawOrder = AdditiveBlendDrawOrder;
        }

        protected override void InitializeParticle(ref Particle p, Vector2 where)
        {
            var velocity = RandomHelper.NextDirection() * RandomHelper.NextFloat(18, 80);

            var lifetime = RandomHelper.NextFloat(2.5f, 3.0f);

            var rotation = RandomHelper.NextFloat(0, MathHelper.TwoPi);

            var angularVelocity = RandomHelper.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);

            var acceleration = -velocity / lifetime;

            p.Initialize(where, velocity, acceleration, lifetime: lifetime, rotation: rotation, angularVelocity: angularVelocity);
        }

        protected override void UpdateParticle(ref Particle particle, float dt)
        {
            base.UpdateParticle(ref particle, dt);

            float normalizedLifetime = particle.TimeSinceStart / particle.Lifetime;

            float alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);
            particle.Color = Color.Green * alpha;

            particle.Scale = 1.0f + .25f * normalizedLifetime;
        }

        public void PlaceGas(Vector2 where)
        {
            AddParticles(where);
        }

        public void ClearGas()
        {
            int count = 0;
            while(count < Particles.Length)
            {
                Particles[count].Lifetime = 0;
                count++;
            }
        }
    }
}
