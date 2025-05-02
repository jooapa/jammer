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
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Cursor.Show();
            AnsiConsole.Write(mainTable);

            // replace inputSaying every character inside of [] @"\[.*?\]
            //int len = Start.Purge(inputSaying).Length;
            //len += 6;

            // count how many \n are in the inputSaying
            int count = Regex.Matches(title, @"\n").Count;
            count += Regex.Matches(inputSaying, @"\n").Count;

            AnsiConsole.Cursor.SetPosition(5, 5 + count);

            if (oneChar)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); // 'intercept: true' prevents the key from being displayed
                return Start.Sanitize(keyInfo.KeyChar.ToString());
            }
            else
            {
                string input = ReadLine.Read(inputSaying + " ");
                ReadLine.AddHistory(input);
                Start.Sanitize(input, true);
                return input;
            }
        }

        public static void Data(string data, string title, bool isError = false, bool readKey = true)
        {
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
            AnsiConsole.Cursor.SetPosition(0, 0);
            AnsiConsole.Write(mainTable);
            if (readKey)
            {
                Console.ReadKey();
            }
        }

        public static string MultiSelect(string[] options, string title)
        {

            AnsiConsole.Cursor.Show();
            AnsiConsole.Cursor.SetPosition(0, 0);

            var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title(title)
                .MoreChoicesText(Themes.sColor("(Move up and down to reveal more options)", Themes.CurrentTheme.InputBox.MultiSelectMoreChoicesTextColor))
                .AddChoices(options));

            return selection;
        }


    }
}
