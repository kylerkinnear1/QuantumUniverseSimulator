using static UniverseSimulation.Bit;
using static UniverseSimulation.Constants;

namespace UniverseSimulation
{
    // For a fixed size Universe
    public static class Constants
    {
        public const int XInfinity = 500;
        public const int YInfinity = 500;
        public const int ZInfinity = 2;
        public const int TauInfinity = 2;
    }

    // The universe is 4 for everything
    // 4 dimensions, 4 bits per particle
    public enum Bit
    {
        Zero,
        One,
        Minus1,
        Minus0
    }

    public static class BitExtensions
    {
        public static Bit Add(this Bit a, Bit b) =>
            (Bit)(((short)a + (short)b) % 4);

        // The universe has signed zeros, just
        // like a computer does. This works
        // around that optimization.
        public static int ToInt(this Bit a) =>
            a switch
            {
                Zero => 0,
                One => 1,
                Minus0 => 0,
                Minus1 => -1
            };
    }

    public static class Universe
    {
        public static void Randomize()
        {
            var random = new Random();
            for (var i = 0; i < Simulator.Space.Length; i++)
            {
                Simulator.Space[i] = Simulator.Space[i]
                    .SetA((Bit)random.Next(0, 4));
                Simulator.Space[i] = Simulator.Space[i]
                    .SetC((Bit)random.Next(0, 4));
                Simulator.Space[i] = Simulator.Space[i]
                    .SetT((Bit)random.Next(0, 4));
                Simulator.Space[i] = Simulator.Space[i]
                    .SetG((Bit)random.Next(0, 4));
            }
        }
        
        public static short GetParticle(
            this short[] particles,
            int x,
            int y,
            int z,
            int tau)
        {
            var index = GetIndex(x, y, z, tau);
            return particles[index];
        }

        public static void SetParticle(
            this short[] particles,
            int x,
            int y,
            int z,
            int tau,
            short particle)
        {
            var index = GetIndex(x, y, z, tau);
            particles[index] = particle;
        }

        private static int GetIndex(
            int x,
            int y,
            int z,
            int tau)
        {
            return
                x * (YInfinity * ZInfinity * TauInfinity) +
                y * (ZInfinity * TauInfinity) +
                z * TauInfinity +
                tau;
        }
    }

    // A particle is 2 bits, so using a single short 
    // and bit manipulation to save memory.
    public static class Particle
    {
        public static Bit A(this short value) =>
            (Bit)(value & 0b11);
        public static Bit C(this short value) => 
            (Bit)((value >> 2) & 0b11);
        public static Bit T(this short value) => 
            (Bit)((value >> 4) & 0b11);
        public static Bit G(this short value) => 
            (Bit)((value >> 6) & 0b11);

        public static short SetA(this short value, Bit bit) =>
            (short)((value & ~0b11) | (((byte)bit) & 0b11));

        public static short SetC(this short value, Bit bit) =>
            (short)((value & ~(0b11 << 2)) | ((((byte)bit) & 0b11) << 2));

        public static short SetT(this short value, Bit bit) =>
            (short)((value & ~(0b11 << 4)) | ((((byte)bit) & 0b11) << 4));

        public static short SetG(this short value, Bit bit) =>
            (short)((value & ~(0b11 << 6)) | ((((byte)bit) & 0b11) << 6));
    }

    public static class Simulator
    {
        // Space is just particles
        public static readonly short[] Space =
            new short[XInfinity * YInfinity * ZInfinity * TauInfinity];

        public static void SimulateFrame()
        {
            for (var x = 0; x < XInfinity; x++)
            for (var y = 0; y < YInfinity; y++)
            for (var z = 0; z < ZInfinity; z++)
            for (var tau = 0; tau < TauInfinity; tau++)
            {
                var particle = Space.GetParticle(x, y, z, tau);
                for (var axis = 0; axis < 4; axis++)
                {
                    particle = particle.Rotate((Bit)axis);
                }

                particle = particle.PropogateWave(x, y, z, tau);
                Space.SetParticle(x, y, z, tau, particle);
            }
        }

        public static short Rotate(this short particle, Bit axis) =>
            // It only increments. Source of right hand rule.
            axis switch
            {
                Zero => particle.SetA(particle.A().Add(One)),
                One => particle.SetC(particle.C().Add(One)),
                Minus1 => particle.SetT(particle.G().Add(One)),
                Minus0 => particle.SetG(particle.G().Add(One)),
                _ => throw new ArgumentException()
            };

        public static short PropogateWave(
            this short particle, 
            int x,
            int y, 
            int z, 
            int tau)
        {
            // Sum the particle's value with its neighbors
            for (var i = -1; i <= 1; i++)
            for (var j = -1; j <= 1; j++)
            for (var k = -1; k <= 1; k++)
            for (var l = -1; l <= 1; l++)
            {
                // Calculate the neighbor's position with wrapping
                var nx = (x + i + XInfinity) % XInfinity;
                var ny = (y + j + YInfinity) % YInfinity;
                var nz = (z + k + ZInfinity) % ZInfinity;
                var ntau = (tau + l + TauInfinity) % TauInfinity;

                var neighbor = Space.GetParticle(nx, ny, nz, ntau);
                particle.SetA(particle.A().Add(neighbor.A()));
                particle.SetC(particle.C().Add(neighbor.C()));
                particle.SetT(particle.T().Add(neighbor.T()));
                particle.SetG(particle.G().Add(neighbor.G()));
            }

            return particle;
        }
    }
}