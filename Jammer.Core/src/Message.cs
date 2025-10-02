using Jammer;
using JRead;
using Spectre.Console;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Jammer
{

    public class CustomSelectInputSettings
    {
        public int ItemsPerPage { get; set; } = 10;
        public int StartIndex { get; set; } = 0;
    }
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
        // Helper method to calculate visual width of text containing multi-byte characters
        private static int GetVisualWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int width = 0;
            foreach (char c in text)
            {
                // Check if character is wide (CJK characters, etc.)
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.OtherLetter || // Most CJK characters
                    category == UnicodeCategory.OtherSymbol ||  // CJK symbols
                    (c >= 0x1100 && c <= 0x11FF) ||  // Hangul Jamo
                    (c >= 0x2E80 && c <= 0x2EFF) ||  // CJK Radicals Supplement
                    (c >= 0x2F00 && c <= 0x2FDF) ||  // Kangxi Radicals
                    (c >= 0x3000 && c <= 0x303F) ||  // CJK Symbols and Punctuation
                    (c >= 0x3040 && c <= 0x309F) ||  // Hiragana
                    (c >= 0x30A0 && c <= 0x30FF) ||  // Katakana
                    (c >= 0x3100 && c <= 0x312F) ||  // Bopomofo
                    (c >= 0x3130 && c <= 0x318F) ||  // Hangul Compatibility Jamo
                    (c >= 0x3190 && c <= 0x319F) ||  // Kanbun
                    (c >= 0x31A0 && c <= 0x31BF) ||  // Bopomofo Extended
                    (c >= 0x31C0 && c <= 0x31EF) ||  // CJK Strokes
                    (c >= 0x31F0 && c <= 0x31FF) ||  // Katakana Phonetic Extensions
                    (c >= 0x3200 && c <= 0x32FF) ||  // Enclosed CJK Letters and Months
                    (c >= 0x3300 && c <= 0x33FF) ||  // CJK Compatibility
                    (c >= 0x3400 && c <= 0x4DBF) ||  // CJK Unified Ideographs Extension A
                    (c >= 0x4E00 && c <= 0x9FFF) ||  // CJK Unified Ideographs
                    (c >= 0xA000 && c <= 0xA48F) ||  // Yi Syllables
                    (c >= 0xA490 && c <= 0xA4CF) ||  // Yi Radicals
                    (c >= 0xAC00 && c <= 0xD7AF) ||  // Hangul Syllables
                    (c >= 0xF900 && c <= 0xFAFF) ||  // CJK Compatibility Ideographs
                    (c >= 0xFE30 && c <= 0xFE4F) ||  // CJK Compatibility Forms
                    (c >= 0xFF00 && c <= 0xFFEF))    // Halfwidth and Fullwidth Forms
                {
                    width += 2; // Wide characters take 2 columns
                }
                else
                {
                    width += 1; // Regular characters take 1 column
                }
            }
            return width;
        }

        // Custom menu that supports ESC to cancel
        public static string? CustomMenuSelect(CustomSelectInput[] options, string title, CustomSelectInputSettings? settings = null)
        {
            if (options == null || options.Length == 0)
                return "__CANCELLED__";

            settings ??= new CustomSelectInputSettings();
            
            int selected = settings.StartIndex;
            int scrollOffset = 0;
            int itemsPerPage = settings.ItemsPerPage;
            ConsoleKeyInfo keyInfo;
            bool previousHadInfo = false;
            bool firstRun = true;

            while (true)
            {
                // Check if current selection has description (author is now inline)
                var currentSel = options[selected];
                bool currentHasDescription = !string.IsNullOrEmpty(currentSel?.Description);
                
                // Only clear if first run or description state changed
                bool needsClear = firstRun || previousHadInfo != currentHasDescription;
                
                if (needsClear)
                {
                    AnsiConsole.Clear();
                }
                else
                {
                    // Just move cursor to top to overwrite
                    AnsiConsole.Cursor.SetPosition(0, 0);
                }

                var selectionTable = new Table();
                selectionTable.AddColumn(new TableColumn(Themes.sColor(title + " [i](Use arrows, Enter to select, ESC to cancel, PgUp/PgDn to scroll)[/]", Themes.CurrentTheme.InputBox.TitleColor ?? "")));
                selectionTable.Width(Start.consoleWidth);

                // Calculate visible range
                int startIndex = scrollOffset;
                int endIndex = Math.Min(startIndex + itemsPerPage, options.Length);

                // Ensure selected item is visible
                if (selected < startIndex)
                {
                    scrollOffset = selected;
                    startIndex = scrollOffset;
                    endIndex = Math.Min(startIndex + itemsPerPage, options.Length);
                }
                else if (selected >= endIndex)
                {
                    scrollOffset = selected - itemsPerPage + 1;
                    if (scrollOffset < 0) scrollOffset = 0;
                    startIndex = scrollOffset;
                    endIndex = Math.Min(startIndex + itemsPerPage, options.Length);
                }

                // Show page info if there are more items than can fit on one page
                if (options.Length > itemsPerPage)
                {
                    int currentPage = (selected / itemsPerPage) + 1;
                    int totalPages = (int)Math.Ceiling((double)options.Length / itemsPerPage);
                    selectionTable.AddRow(Themes.sColor($"[dim]Page {currentPage} of {totalPages} | Items {startIndex + 1}-{endIndex} of {options.Length}[/]", Themes.CurrentTheme.InputBox.InputTextColor ?? ""));
                }

                for (int i = startIndex; i < endIndex; i++)
                {
                    var opt = options[i];
                    string titleText = opt?.Title ?? "";
                    string authorText = opt?.Author ?? "";

                    // Create display text with author right-aligned
                    string displayText;
                    if (!string.IsNullOrEmpty(authorText))
                    {
                        string escapedTitle = string.IsNullOrEmpty(titleText) ? "" : Markup.Escape(titleText);
                        string escapedAuthor = Markup.Escape(authorText);
                        
                        // Calculate space for right alignment using visual width
                        int availableWidth = Start.consoleWidth - 8; // Account for table borders and padding
                        int contentWidth = GetVisualWidth(escapedTitle) + GetVisualWidth(escapedAuthor);
                        
                        if (contentWidth < availableWidth)
                        {
                            int spacePadding = availableWidth - contentWidth;
                            displayText = $"{escapedTitle}{new string(' ', spacePadding)}{escapedAuthor}";
                        }
                        else
                        {
                            displayText = $"{escapedTitle}";
                        }
                    }
                    else
                    {
                        displayText = string.IsNullOrEmpty(titleText) ? "" : Markup.Escape(titleText);
                    }

                    // Highlight selected row
                    if (i == selected)
                    {
                        selectionTable.AddRow(Themes.sColor($"> [b]{displayText}[/] <", Themes.CurrentTheme.InputBox.InputTextColor ?? ""));
                    }
                    else
                    {
                        selectionTable.AddRow(displayText);
                    }
                }

                AnsiConsole.Write(selectionTable);

                // Description table for selected item (only description now)
                var sel = options[selected];
                string selDesc = sel?.Description ?? "";

                if (!string.IsNullOrEmpty(selDesc))
                {
                    var descTable = new Table();
                    descTable.AddColumn(Themes.sColor("Description", Themes.CurrentTheme.InputBox.InputTextColor ?? ""));
                    descTable.Width(Start.consoleWidth);
                    
                    string escapedDesc = Markup.Escape(selDesc);
                    descTable.AddRow(escapedDesc);
                    AnsiConsole.Write(descTable);
                }

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
                else if (keyInfo.Key == ConsoleKey.PageUp)
                {
                    selected = Math.Max(0, selected - itemsPerPage);
                }
                else if (keyInfo.Key == ConsoleKey.PageDown)
                {
                    selected = Math.Min(options.Length - 1, selected + itemsPerPage);
                }
                else if (keyInfo.Key == ConsoleKey.Home)
                {
                    selected = 0;
                }
                else if (keyInfo.Key == ConsoleKey.End)
                {
                    selected = options.Length - 1;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return options[selected]?.DataURI ?? null;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return null;
                }

                // Update state for next iteration
                previousHadInfo = currentHasDescription;
                firstRun = false;
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

                // Start.Sanitize(input, false);
                return input;
            }
            else
            {
                // string input = ReadLineWithEscSupportAndPrefill(inputSaying + " ", prefillText);
                string input = JRead.JRead.Read(inputSaying, prefillText, options);
                if (!string.IsNullOrEmpty(input)) // Only add non-empty input to history
                    JRead.JRead.History.Add(input);
                // Start.Sanitize(input, true);
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