using Spectre.Console;

namespace jammer
{
    public class Start
    {
        static public void Run(string[] args)
        {
            AnsiConsole.Write(new FigletText("jammer"));
        }
    }
}