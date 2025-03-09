using UniverseSimulation;
using static UniverseSimulation.Constants;

namespace UniverseSimulationUi;

public partial class GridForm : Form
{
    public const int RenderCanvasWidth = 1000;
        public const int RenderCanvasHeight = 1000;
        public const int ParticleSize = 2;
        
        private readonly Bitmap canvas;

        public GridForm()
        {
            this.Text = "4D Particle Simulation";
            this.Size = new Size(RenderCanvasWidth, RenderCanvasHeight);
            this.DoubleBuffered = true;
            canvas = new Bitmap(RenderCanvasWidth, RenderCanvasHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Simulator.SimulateFrame();
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.Black);
                for (var x = 0; x < XInfinity; x++)
                for (var y = 0; y < YInfinity; y++)
                for (var z = 0; z < ZInfinity; z++)
                for (var tau = 0; tau < TauInfinity; tau++)
                    DrawParticle(g, x, y, z, tau);
            }

            e.Graphics.DrawImageUnscaled(canvas, 0, 0);
            Invalidate();
        }

        private void DrawParticle(Graphics g, int x, int y, int z, int tau)
        {
            var particle = Simulator.Space.GetParticle(x, y, z, tau);
            if ((0, 0, 0, 0) ==
                (particle.A().ToInt(), particle.C().ToInt(), particle.T().ToInt(), particle.G().ToInt()))
            {
                return;
            }

            var scaleFactorX = RenderCanvasWidth / XInfinity;
            var scaleFactorY = RenderCanvasHeight / YInfinity;

            var color = GetParticleColor(particle, z, tau);
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, x * scaleFactorX, y * scaleFactorY, ParticleSize, ParticleSize);
        }

        private Color GetParticleColor(short particle, int z, int tau)
        {
            // Extract the nucleotide values (expected to be -1, 0, or 1)
            int a = particle.A().ToInt(); // A nucleotide
            int c = particle.C().ToInt(); // C nucleotide
            int t = particle.T().ToInt(); // T nucleotide
            int g = particle.G().ToInt(); // G nucleotide

            // Scale each nucleotide from [-1, 1] to [0, 255]
            // Formula: (value + 1) * 127.5
            int red = (int)Math.Round((a + 1) * 127.5);
            int green = (int)Math.Round((c + 1) * 127.5);

            // For blue, combine T and G by averaging their values, then scale.
            double averageTG = (t + g) / 2.0; // still in [-1, 1]
            int blue = (int)Math.Round((averageTG + 1) * 127.5);

            // Compute transparency from z and tau.
            // Cast to float to ensure proper division.
            int transparency = (int)Math.Round(255 * ((float)(z + tau) / (ZInfinity * TauInfinity)));

            // Clamp all channels to the range [0, 255]
            red = Math.Clamp(red, 0, 255);
            green = Math.Clamp(green, 0, 255);
            blue = Math.Clamp(blue, 0, 255);
            transparency = Math.Clamp(transparency, 0, 255);

            // Return the final color with the computed transparency and RGB values
            return Color.FromArgb(transparency, red, green, blue);
        }
}