using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SFML.Graphics;
using SFML.Window;
using System.Runtime.ConstrainedExecution;

namespace AirportCEO_Mod_Loader_Installer
{
    public enum ProgramState
    {
        looking,
        moving,
        success,
        failure
    }

    internal class Installer
    {
        ProgramState programState = ProgramState.looking;

        private RenderWindow window;
        readonly uint ScreenWidth = 600;
        readonly uint ScreenHieght = 200;
        Color windowColor;

        Text lookingText;
        Text movingText;
        Text successText;
        Text failureText;

        private string ExePath = "";
        private readonly string winReasourcePath = "../Mod Loader Core/Windows"; // ../../../../..

        public void UpdateLoop()
        {
            Console.WriteLine("Starting Loop.");
            while (window.IsOpen)
            {

                window.DispatchEvents();
                window.Clear(windowColor);

                window.Draw(TextOfProgramState());

                window.Display();

                if (programState == ProgramState.looking)
                {
                    TryFindPath();
                    continue;
                }
                if (programState == ProgramState.moving)
                {
                    TryMove();
                    continue;
                }
            }
        }

        public Installer()
        {
            window = new RenderWindow(new VideoMode(ScreenWidth, ScreenHieght), "AirportCEO Mod Loader Installer", Styles.Titlebar | Styles.Close);
            window.Closed += Close;
            window.SetFramerateLimit(60);
            windowColor = new Color(255, 255, 255);

            lookingText = CreateText("Looking for folder...");
            movingText = CreateText("Moving/Installing mod loader...");
            successText = CreateText("Mod Loader installed successfully!\nYou may close this window, and enjoy.");
            failureText = CreateText("Finding folder unsuccessful.\nPlease see \"ManualInstallationInstructions.txt\".\n" +
                "(If you are on a mac, auto installation is not available)");
        }
        void Close(object? sender, EventArgs? eventArgs)
        {
            window.Close();
        }

        Text TextOfProgramState()
        {
            switch (programState)
            {
                case ProgramState.looking:
                    return lookingText;
                case ProgramState.moving:
                    return movingText;
                case ProgramState.success:
                    return successText;
                case ProgramState.failure:
                    return failureText;
            }

            return failureText;
        }

        Text CreateText(string message)
        {
            Font font = new Font("./Source/AirportCEO Mod Loader Installer/Roboto-Regular.ttf"); //../../../Roboto-Regular.ttf

            Text text = new Text();
            text.DisplayedString = message;
            text.CharacterSize = 24;
            text.Font = font;
            text.FillColor = new Color(0, 0, 0);
            text.Position = new SFML.System.Vector2f(250, 125);

            // Centering 
            FloatRect rect = text.GetLocalBounds();
            text.Origin = new SFML.System.Vector2f(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
            text.Position = new SFML.System.Vector2f(ScreenWidth / 2, ScreenHieght / 2);

            return text;
        }

        public void TryFindPath()
        {
            Console.WriteLine("Starting path search");

            ExePath = FindExePath();

            if (string.IsNullOrEmpty(ExePath))
            {
                programState = ProgramState.failure;
                Console.WriteLine("Path not found. Program finished");
                return;
            }

            Console.WriteLine("Path search successful");
            programState = ProgramState.moving;
        }

        private string FindExePath()
        {
            string basePath1 = "C:/Program Files (x86)/Steam/steamapps/common/Airport CEO";
            string basePath2 = "C:/Program Files/Steam/steamapps/common/Airport CEO";

            string pathToUse;

            if (Directory.Exists(basePath1))
            {
                pathToUse = basePath1;
            }
            else if (Directory.Exists(basePath2))
            {
                pathToUse = basePath2;
            }
            else
            {
                return "";
            }

            if (File.Exists(Path.Combine(pathToUse, "Airport CEO.exe")))
            {
                return pathToUse;
            }

            return "";
        }

        public void TryMove()
        {
            Console.WriteLine("Starting moving");
            try
            {
                string[] directories = Directory.GetDirectories(winReasourcePath);
                for (int i = 0; i < directories.Length; i++)
                {
                    if (Directory.Exists(Path.Combine(ExePath, Path.GetFileName(directories[i]))))
                    {
                        programState = ProgramState.failure;
                        return;
                    }
                    Console.WriteLine(Path.GetDirectoryName(directories[i]));
                    Directory.Move(directories[i], Path.Combine(ExePath, Path.GetFileName(directories[i])));
                }

                string[] files = Directory.GetFiles(winReasourcePath);
                for (int i = 0; i < files.Length; i++)
                {
                    if (File.Exists(Path.Combine(ExePath, Path.GetFileName(files[i]))))
                    {
                        programState = ProgramState.failure;
                        return;
                    }
                    File.Move(files[i], Path.Combine(ExePath, Path.GetFileName(files[i])));
                }

                programState = ProgramState.success;
                Console.WriteLine("Moving successful");
            }
            catch (Exception ex)
            {
                programState = ProgramState.failure;
                Console.WriteLine($"Moving threw an exception! Error: {ex.ToString()}. Exe path: {ExePath}");
            }
        }
    }
}
