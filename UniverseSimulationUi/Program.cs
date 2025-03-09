using UniverseSimulation;

namespace UniverseSimulationUi;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Universe.Randomize();
        Application.Run(new GridForm());
    }
}