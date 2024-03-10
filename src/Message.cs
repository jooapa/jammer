using jammer;
using Spectre.Console;


namespace jammer
{
    public static class Message
    {
        public static string Input(string inputSaying, string title)
        {
            var mainTable = new Table();
            var messageTable = new Table();
            mainTable.AddColumn(new TableColumn("[bold]" + title + "[/]")).Centered().Width(Start.consoleWidth);
            messageTable.AddColumn(new TableColumn(inputSaying)).Centered().Width(Start.consoleWidth);
            mainTable.AddRow(messageTable);
            AnsiConsole.Cursor.SetPosition(0,0);
            AnsiConsole.Write(mainTable);
            AnsiConsole.Cursor.SetPosition(inputSaying.Length + 6, 5);
            string input = Console.ReadLine() ?? string.Empty;
            return input;
        }

        public static void Data(string data, string title, bool isError = false) {
            var mainTable = new Table();
            var messageTable = new Table();
            if (isError)
            {
                mainTable.AddColumn(new TableColumn("[bold][red]" + title + "[/][/]")).Centered().Width(Start.consoleWidth);
                messageTable.AddColumn(new TableColumn("[red]" + data + "[/]")).Centered().Width(Start.consoleWidth);
            }
            else
            {
                mainTable.AddColumn(new TableColumn("[bold]" + title + "[/]")).Centered().Width(Start.consoleWidth);
                messageTable.AddColumn(new TableColumn(data)).Centered().Width(Start.consoleWidth);
            }
            mainTable.AddRow(messageTable);
            AnsiConsole.Cursor.SetPosition(0,0);
            AnsiConsole.Write(mainTable);
            Console.ReadLine();
        }
    }
}
