using System;

namespace AirportCEO_Mod_Loader_Installer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Installer installer = new Installer();
            installer.UpdateLoop();
            Console.WriteLine("Press enter to close");
            Console.ReadLine();
        }
    }
}
