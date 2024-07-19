using Jammer;
using Spectre.Console;
using System.Text.RegularExpressions;


namespace Jammer
{
    public static class Message
    {
        public static string Input(string inputSaying, string title, bool oneChar = false)
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
            
            // replace inputSaying every character inside of [] @"\[.*?\]
            string pattern = @"\[.*?\]";
            string replacement = "";
            Regex rgx = new Regex(pattern);
            int len = rgx.Replace(inputSaying, replacement).Length;
            len += 6;
            AnsiConsole.Cursor.SetPosition(len, 5);

            if (oneChar)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); // 'intercept: true' prevents the key from being displayed
                return keyInfo.KeyChar.ToString(); // Return the character as a string
            }
            else
            {
                string input = Console.ReadLine() ?? string.Empty;
                return input;
            }
        }

        public static void Data(string data, string title, bool isError = false, bool readKey = true) {
            var mainTable = new Table();
            var messageTable = new Table();
            mainTable.Border = Themes.bStyle(Themes.CurrentTheme.InputBox.BorderStyle);
            mainTable.BorderColor(Themes.bColor(Themes.CurrentTheme.InputBox.BorderColor));
            if (isError)
            {
                mainTable.AddColumn(new TableColumn(Themes.sColor(title, Themes.CurrentTheme.InputBox.TitleColorIfError))).Centered().Width(Start.consoleWidth);
                messageTable.AddColumn(new TableColumn(Themes.sColor(data, Themes.CurrentTheme.InputBox.InputTextColorIfError))).Centered().Width(Start.consoleWidth);
                messageTable.Border = Themes.bStyle(Themes.CurrentTheme.InputBox.InputBorderStyleIfError);
                messageTable.BorderColor(Themes.bColor(Themes.CurrentTheme.InputBox.InputBorderColorIfError));
            }
            else
            {
                mainTable.AddColumn(new TableColumn(Themes.sColor(title, Themes.CurrentTheme.InputBox.TitleColor))).Centered().Width(Start.consoleWidth);
                messageTable.AddColumn(new TableColumn(Themes.sColor(data, Themes.CurrentTheme.InputBox.InputTextColor))).Centered().Width(Start.consoleWidth);
                messageTable.Border = Themes.bStyle(Themes.CurrentTheme.InputBox.InputBorderStyle);
                messageTable.BorderColor(Themes.bColor(Themes.CurrentTheme.InputBox.InputBorderColor));
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

        public static string MultiSelect(string[] options, string title)
        {

            AnsiConsole.Cursor.Show();
            AnsiConsole.Cursor.SetPosition(0,0);
            
            var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText(Themes.sColor("(Move up and down to reveal more options)", Themes.CurrentTheme.InputBox.MultiSelectMoreChoicesTextColor))
                .AddChoices(options));

            return selection;
        }
        
    }
}
