using Spectre.Console;

namespace jammer
{
    public class Start
    {
        static public void Run(string[] args)
        {
            TUI.InitScreen();
            //TUI.AskForUrl();
            //AnsiConsole.Write(new FigletText("jammer"));
                TUI.Draw();
        }
    }
}