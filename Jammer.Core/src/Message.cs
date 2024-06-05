using Jammer;
using Spectre.Console;


namespace Jammer
{
    public static class Message
    {
        public static string Input(string inputSaying, string title)
        {
            var mainTable = new Table();
            mainTable.Border = Themes.bStyle(Themes.CurrentTheme.InputBox.BorderStyle);
            mainTable.BorderColor(Themes.bColor(Themes.CurrentTheme.InputBox.BorderColor));

            var messageTable = new Table();
            messageTable.Border = Themes.bStyle(Themes.CurrentTheme.InputBox.InputBorderStyle);
            messageTable.BorderColor(Themes.bColor(Themes.CurrentTheme.InputBox.InputBorderColor));

            mainTable.AddColumn(new TableColumn(Themes.sColor(title, Themes.CurrentTheme.InputBox.TitleColor))).Centered().Width(Start.consoleWidth);
            messageTable.AddColumn(new TableColumn(Themes.sColor(inputSaying, Themes.CurrentTheme.InputBox.InputTextColor))).Centered().Width(Start.consoleWidth);
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
                mainTable.AddColumn(new TableColumn(Themes.sColor(title, Themes.CurrentTheme.InputBox.TitleColorIfError)));
                messageTable.AddColumn(new TableColumn(Themes.sColor(data, Themes.CurrentTheme.InputBox.InputTextColorIfError)));
            }
            else
            {
                mainTable.AddColumn(new TableColumn(Themes.sColor(title, Themes.CurrentTheme.InputBox.TitleColor)));
                messageTable.AddColumn(new TableColumn(Themes.sColor(data, Themes.CurrentTheme.InputBox.InputTextColor)));
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

        public static int MultiSelect(string[] options, string title, string message)
        {

            AnsiConsole.Cursor.Show();
            AnsiConsole.Cursor.SetPosition(0,0);
            
            var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText(Themes.sColor("(Move up and down to reveal more options)", Themes.CurrentTheme.InputBox.MultiSelectMoreChoicesTextColor))
                .AddChoices(options));
            return selection.IndexOf(selection);
        }
        
    }
}
