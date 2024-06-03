using Jammer;
using Spectre.Console;


namespace Jammer
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
            AnsiConsole.Cursor.Show();
            AnsiConsole.Write(mainTable);
            AnsiConsole.Cursor.SetPosition(inputSaying.Length + 6, 5);
            string input = Console.ReadLine() ?? string.Empty;
            return input;
        }

        public static void Data(string data, string title, bool isError = false, bool readKey = true) {
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
            AnsiConsole.Cursor.Show();
            AnsiConsole.Cursor.SetPosition(0,0);
            AnsiConsole.Write(mainTable);
            if (readKey)
            {
                Console.ReadKey();
            }
        }

        public static int MultiSelect(string[] options, string title, string message, bool isError = false)
        {
            var mainTable = new Table();
            var messageTable = new Table();
            if (isError)
            {
                mainTable.AddColumn(new TableColumn("[bold][red]" + title + "[/][/]")).Centered().Width(Start.consoleWidth);
                messageTable.AddColumn(new TableColumn("[red]" + message + "[/]")).Centered().Width(Start.consoleWidth);
            }
            else
            {
                mainTable.AddColumn(new TableColumn("[bold]" + title + "[/]")).Centered().Width(Start.consoleWidth);
                messageTable.AddColumn(new TableColumn(message)).Centered().Width(Start.consoleWidth);
            }
            mainTable.AddRow(messageTable);

            AnsiConsole.Cursor.Show();
            AnsiConsole.Cursor.SetPosition(0,0);
            AnsiConsole.Write(mainTable);
            
            var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Select an option")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .AddChoices(options));
            return selection.IndexOf(selection);
        }
        
    }
}
