using Jammer;
using JRead;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Jammer
{

    // custom type for the custom select input.
    public class CustomSelectInput
    {
        public string? DataURI { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
    }

    public static class Message
    {
        // Custom menu that supports ESC to cancel
        public static string? CustomMenuSelect(CustomSelectInput[] options, string title)
        {
            if (options == null || options.Length == 0)
                return "__CANCELLED__";

            int selected = 0;
            ConsoleKeyInfo keyInfo;

            while (true)
            {
                AnsiConsole.Clear();

                var selectionTable = new Table();
                selectionTable.AddColumn(new TableColumn(Themes.sColor(title + " [i](Use arrows, Enter to select, ESC to cancel)[/]", Themes.CurrentTheme.InputBox.TitleColor ?? "")));
                selectionTable.Width(Start.consoleWidth);

                for (int i = 0; i < options.Length; i++)
                {
                    var opt = options[i];
                    string? titleText = opt?.Title ?? null;

                    // Highlight selected row
                    if (i == selected)
                    {
                        selectionTable.AddRow(Themes.sColor($"> [b]{Markup.Escape(titleText)}[/] <", Themes.CurrentTheme.InputBox.InputTextColor ?? ""));
                    }
                    else
                    {
                        selectionTable.AddRow(Markup.Escape(titleText));
                    }
                }

                AnsiConsole.Write(selectionTable);

                // Info table for selected item (author + description)
                var infoTable = new Table();
                infoTable.AddColumn(Themes.sColor("Author", Themes.CurrentTheme.InputBox.InputTextColor ?? ""));
                infoTable.AddColumn(Themes.sColor("Description", Themes.CurrentTheme.InputBox.InputTextColor ?? ""));
                infoTable.Width(Start.consoleWidth);

                var sel = options[selected];
                string selAuthor = sel?.Author ?? "";
                string selDesc = sel?.Description ?? "";

                infoTable.AddRow(Markup.Escape(selAuthor), Markup.Escape(selDesc));
                // dont write if no author or desc
                if (!string.IsNullOrEmpty(selAuthor) || !string.IsNullOrEmpty(selDesc))
                    AnsiConsole.Write(infoTable);

                // Read input
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selected = (selected - 1 + options.Length) % options.Length;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selected = (selected + 1) % options.Length;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return options[selected]?.DataURI ?? null;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return "__CANCELLED__";
                }
            }
        }

        public static string Input(string inputSaying, string title, bool oneChar = false, string[]? setText = null, JReadOptions? options = null)
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
            // Clear the input line to ensure it starts empty for all prompts
            // This prevents leftover text from previous prompts appearing in the input area.
            // Prompts with prefill or suggestions will still show their intended text after clearing.
            Console.Write(new string(' ', Start.consoleWidth - 10));
            AnsiConsole.Cursor.SetPosition(5, 5 + count); // Reset cursor after clearing

            // add JReadOptions
            options ??= new JReadOptions();
            options.SubtractFromAvailableSpace = true;
            options.MaxDisplayLength = 4;

            if (oneChar)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); // 'intercept: true' prevents the key from being displayed
                return Start.Sanitize(keyInfo.KeyChar.ToString());
            }
            else if (setText != null && setText.Length > 0)
            {
                var oldHistory = JRead.JRead.History.GetAll();
                JRead.JRead.History.Clear();
                for (int i = 0; i < setText.Length; i++)
                {
                    JRead.JRead.History.Add(setText[i]);
                }
                // string input = ReadLineWithEscSupport(inputSaying);
                string input = JRead.JRead.Read(inputSaying, options);
                JRead.JRead.History.Clear();
                JRead.JRead.History.AddRange(oldHistory);

                Start.Sanitize(input, false);
                return input;
            }
            else
            {
                string input = JRead.JRead.Read(inputSaying, options);
                if (!string.IsNullOrEmpty(input)) // Only add non-empty input to history
                                                  // ReadLine.AddHistory(input);
                    JRead.JRead.History.Add(input);

                Start.Sanitize(input, true);
                return input;
            }
        }

        public static string Input(string inputSaying, string title, string prefillText, bool oneChar = false, string[]? setText = null, JReadOptions? options = null)
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

            // count how many \n are in the inputSaying
            int count = Regex.Matches(title, @"\n").Count;
            count += Regex.Matches(inputSaying, @"\n").Count;

            AnsiConsole.Cursor.SetPosition(5, 5 + count);
            // Clear the input line to ensure it starts empty for all prompts
            // This prevents leftover text from previous prompts appearing in the input area.
            // Prompts with prefill or suggestions will still show their intended text after clearing.
            Console.Write(new string(' ', Start.consoleWidth - 10));
            AnsiConsole.Cursor.SetPosition(5, 5 + count); // Reset cursor after clearing

            // add JReadOptions
            options ??= new JReadOptions();
            options.SubtractFromAvailableSpace = true;
            options.MaxDisplayLength = 4;

            if (oneChar)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                return Start.Sanitize(keyInfo.KeyChar.ToString());
            }
            else if (setText != null && setText.Length > 0)
            {
                var oldHistory = JRead.JRead.History.GetAll();
                JRead.JRead.History.Clear();
                for (int i = 0; i < setText.Length; i++)
                {
                    JRead.JRead.History.Add(setText[i]);
                }
                // string input = ReadLineWithEscSupportAndPrefill(inputSaying, prefillText, setText);
                string input = JRead.JRead.Read(inputSaying, prefillText, options);
                JRead.JRead.History.Clear();
                JRead.JRead.History.AddRange(oldHistory);

                Start.Sanitize(input, false);
                return input;
            }
            else
            {
                // string input = ReadLineWithEscSupportAndPrefill(inputSaying + " ", prefillText);
                string input = JRead.JRead.Read(inputSaying, prefillText, options);
                if (!string.IsNullOrEmpty(input)) // Only add non-empty input to history
                    JRead.JRead.History.Add(input);
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

            // Add a Cancel option if it doesn't already exist
            bool hasCancelOption = options.Contains("Cancel");
            string[] optionsWithCancel = hasCancelOption ? options : new[] { "Cancel" }.Concat(options).ToArray();

            try
            {
                var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title(title + " (Press ESC to cancel)")
                    .MoreChoicesText(Themes.sColor("(Move up and down to reveal more options)", Themes.CurrentTheme.InputBox.MultiSelectMoreChoicesTextColor))
                    .AddChoices(optionsWithCancel));

                // Check if the selection is the first item (Cancel) and there are keys available
                // This might indicate the user pressed ESC and the default was selected
                if (selection == "Cancel" && Console.KeyAvailable)
                {
                    // Peek at the next key to see if it's ESC
                    // Note: This is a workaround since we can't actually peek without consuming
                }

                return selection;
            }
            catch (Exception)
            {
                // Return a special value to indicate cancellation
                return "__CANCELLED__";
            }
        }


    }
}