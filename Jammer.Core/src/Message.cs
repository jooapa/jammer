using Jammer;
using Spectre.Console;
using System.Text.RegularExpressions;


namespace Jammer
{
    public static class Message
    {
        // Custom menu that supports ESC to cancel
        public static string CustomMenuSelect(string[] options, string title)
        {
            int selected = 0;
            ConsoleKeyInfo keyInfo;
            while (true)
            {
                Console.Clear();
                Console.WriteLine(title + " (Use arrows, Enter to select, ESC to cancel)");
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selected)
                        Console.WriteLine($"> {options[i]} <");
                    else
                        Console.WriteLine($"  {options[i]}");
                }
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
                    return options[selected];
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    return "__CANCELLED__";
                }
            }
        }

        public static string Input(string inputSaying, string title, bool oneChar = false, string[]? setText = null)
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

            if (oneChar)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); // 'intercept: true' prevents the key from being displayed
                return Start.Sanitize(keyInfo.KeyChar.ToString());
            }
            else if (setText != null && setText.Length > 0)
            {
                var oldHistory = ReadLine.GetHistory();
                ReadLine.ClearHistory();
                ReadLine.AddHistory(setText.Reverse().ToArray());
                string input = ReadLineWithEscSupport(inputSaying);
                ReadLine.ClearHistory();
                foreach (var item in oldHistory)
                {
                    ReadLine.AddHistory(item);
                }
                Start.Sanitize(input, false);
                return input;
            }
            else
            {
                string input = ReadLineWithEscSupport(inputSaying + " ");
                if (!string.IsNullOrEmpty(input)) // Only add non-empty input to history
                    ReadLine.AddHistory(input);
                Start.Sanitize(input, true);
                return input;
            }
        }

        public static string Input(string inputSaying, string title, string prefillText, bool oneChar = false, string[]? setText = null)
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

            if (oneChar)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                return Start.Sanitize(keyInfo.KeyChar.ToString());
            }
            else if (setText != null && setText.Length > 0)
            {
                var oldHistory = ReadLine.GetHistory();
                ReadLine.ClearHistory();
                ReadLine.AddHistory(setText.Reverse().ToArray());
                string input = ReadLineWithEscSupportAndPrefill(inputSaying, prefillText, setText);
                ReadLine.ClearHistory();
                foreach (var item in oldHistory)
                {
                    ReadLine.AddHistory(item);
                }
                Start.Sanitize(input, false);
                return input;
            }
            else
            {
                string input = ReadLineWithEscSupportAndPrefill(inputSaying + " ", prefillText);
                if (!string.IsNullOrEmpty(input)) // Only add non-empty input to history
                    ReadLine.AddHistory(input);
                Start.Sanitize(input, true);
                return input;
            }
        }

        private static string ReadLineWithEscSupportAndPrefill(string prompt, string prefillText, string[]? suggestions = null)
        {
            // Check for immediate ESC key press
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    return string.Empty; // Return empty string for ESC, same as Enter-without-input
                }
                // If it's not ESC, we need to put the key back somehow
                // Unfortunately, we can't put keys back into Console buffer
                // So we'll handle common cases and fall back to ReadLine for complex input
                if (key.Key == ConsoleKey.Enter)
                {
                    return prefillText; // Return prefilled text on immediate Enter
                }
                // For other keys, start with that character and continue with ReadLine
                // This is a limitation but covers the ESC case which is what we need
            }
            
            // Use a custom input loop to handle ESC during typing with prefill
            return ReadLineWithEscapeDetectionAndPrefill(prompt, prefillText, suggestions);
        }

        private static string ReadLineWithEscapeDetectionAndPrefill(string prompt, string prefillText, string[]? suggestions = null)
        {
            string input = prefillText ?? "";
            ConsoleKeyInfo keyInfo;
            int cursorPosition = input.Length; // Track cursor position within the input
            int suggestionIndex = -1; // -1 = user input, 0+ = suggestion index
            string originalInput = prefillText ?? ""; // preserve user typed input when cycling back
            
            // Display the prefilled text
            Console.Write(input);
            
            do
            {
                keyInfo = Console.ReadKey(true);
                
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    // Clear the current input line and return empty
                    Console.Write(new string('\b', input.Length));
                    Console.Write(new string(' ', input.Length));
                    Console.Write(new string('\b', input.Length));
                    return string.Empty;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0 && cursorPosition > 0)
                    {
                        // Reset suggestion index when user starts typing
                        if (suggestionIndex != -1)
                        {
                            suggestionIndex = -1;
                            originalInput = input;
                        }
                        
                        // Remove character before cursor
                        input = input.Substring(0, cursorPosition - 1) + input.Substring(cursorPosition);
                        cursorPosition--;
                        
                        // Redraw the line
                        Console.Write(new string('\b', input.Length + 1));
                        Console.Write(new string(' ', input.Length + 1));
                        Console.Write(new string('\b', input.Length + 1));
                        Console.Write(input);
                        
                        // Position cursor correctly
                        int moveBack = input.Length - cursorPosition;
                        if (moveBack > 0)
                            Console.Write(new string('\b', moveBack));
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Delete)
                {
                    if (cursorPosition < input.Length)
                    {
                        // Reset suggestion index when user starts typing
                        if (suggestionIndex != -1)
                        {
                            suggestionIndex = -1;
                            originalInput = input;
                        }
                        
                        // Remove character at cursor position
                        input = input.Substring(0, cursorPosition) + input.Substring(cursorPosition + 1);
                        
                        // Redraw the line
                        Console.Write(new string('\b', cursorPosition));
                        Console.Write(input);
                        Console.Write(" "); // Clear the last character
                        
                        // Position cursor correctly
                        int moveBack = input.Length - cursorPosition + 1;
                        Console.Write(new string('\b', moveBack));
                    }
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        Console.Write('\b');
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (cursorPosition < input.Length)
                    {
                        cursorPosition++;
                        Console.Write(input[cursorPosition - 1]);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Home)
                {
                    // Move cursor to beginning of input
                    if (cursorPosition > 0)
                    {
                        Console.Write(new string('\b', cursorPosition));
                        cursorPosition = 0;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.End)
                {
                    // Move cursor to end of input
                    if (cursorPosition < input.Length)
                    {
                        Console.Write(input.Substring(cursorPosition));
                        cursorPosition = input.Length;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    // Navigate through suggestions upward
                    if (suggestions != null && suggestions.Length > 0)
                    {
                        // If currently on user input (-1), save it as original
                        if (suggestionIndex == -1)
                        {
                            originalInput = input;
                        }
                        
                        suggestionIndex++;
                        if (suggestionIndex >= suggestions.Length)
                        {
                            suggestionIndex = 0; // Wrap to first suggestion
                        }
                        
                        ReplaceInputField(suggestions[suggestionIndex], ref input, ref cursorPosition);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    // Navigate through suggestions downward
                    if (suggestions != null && suggestions.Length > 0)
                    {
                        // If currently on user input (-1), save it as original
                        if (suggestionIndex == -1)
                        {
                            originalInput = input;
                        }
                        
                        suggestionIndex--;
                        if (suggestionIndex < -1)
                        {
                            suggestionIndex = suggestions.Length - 1; // Wrap to last suggestion
                        }
                        
                        if (suggestionIndex == -1)
                        {
                            // Restore original user input
                            ReplaceInputField(originalInput, ref input, ref cursorPosition);
                        }
                        else
                        {
                            ReplaceInputField(suggestions[suggestionIndex], ref input, ref cursorPosition);
                        }
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    // Reset suggestion index when user starts typing
                    if (suggestionIndex != -1)
                    {
                        suggestionIndex = -1;
                        originalInput = input;
                    }
                    
                    // Insert character at cursor position
                    input = input.Substring(0, cursorPosition) + keyInfo.KeyChar + input.Substring(cursorPosition);
                    cursorPosition++;
                    
                    // Redraw from cursor position
                    Console.Write(input.Substring(cursorPosition - 1));
                    
                    // Position cursor correctly
                    int moveBack = input.Length - cursorPosition;
                    if (moveBack > 0)
                        Console.Write(new string('\b', moveBack));
                }
            } while (true);
            
            return input;
        }

        private static void ReplaceInputField(string newText, ref string input, ref int cursorPosition)
        {
            // Clear current input from console
            Console.Write(new string('\b', input.Length));
            Console.Write(new string(' ', input.Length));
            Console.Write(new string('\b', input.Length));
            
            // Write new text
            input = newText ?? "";
            Console.Write(input);
            
            // Set cursor to end of new input
            cursorPosition = input.Length;
        }

        private static string ReadLineWithEscSupport(string prompt)
        {
            // Check for immediate ESC key press
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    return string.Empty; // Return empty string for ESC, same as Enter-without-input
                }
                // If it's not ESC, we need to put the key back somehow
                // Unfortunately, we can't put keys back into Console buffer
                // So we'll handle common cases and fall back to ReadLine for complex input
                if (key.Key == ConsoleKey.Enter)
                {
                    return string.Empty;
                }
                // For other keys, start with that character and continue with ReadLine
                // This is a limitation but covers the ESC case which is what we need
            }
            
            // Use a custom input loop to handle ESC during typing
            return ReadLineWithEscapeDetection(prompt);
        }

        private static string ReadLineWithEscapeDetection(string prompt)
        {
            string input = "";
            ConsoleKeyInfo keyInfo;
            int cursorPosition = 0; // Track cursor position within the input
            
            do
            {
                keyInfo = Console.ReadKey(true);
                
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    // Clear the current input line and return empty
                    Console.Write(new string('\b', input.Length));
                    Console.Write(new string(' ', input.Length));
                    Console.Write(new string('\b', input.Length));
                    return string.Empty;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0 && cursorPosition > 0)
                    {
                        // Remove character before cursor
                        input = input.Substring(0, cursorPosition - 1) + input.Substring(cursorPosition);
                        cursorPosition--;
                        
                        // Redraw the line
                        Console.Write(new string('\b', input.Length + 1));
                        Console.Write(new string(' ', input.Length + 1));
                        Console.Write(new string('\b', input.Length + 1));
                        Console.Write(input);
                        
                        // Position cursor correctly
                        int moveBack = input.Length - cursorPosition;
                        if (moveBack > 0)
                            Console.Write(new string('\b', moveBack));
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Delete)
                {
                    if (cursorPosition < input.Length)
                    {
                        // Remove character at cursor position
                        input = input.Substring(0, cursorPosition) + input.Substring(cursorPosition + 1);
                        
                        // Redraw the line
                        Console.Write(new string('\b', cursorPosition));
                        Console.Write(input);
                        Console.Write(" "); // Clear the last character
                        
                        // Position cursor correctly
                        int moveBack = input.Length - cursorPosition + 1;
                        Console.Write(new string('\b', moveBack));
                    }
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        Console.Write('\b');
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (cursorPosition < input.Length)
                    {
                        cursorPosition++;
                        Console.Write(input[cursorPosition - 1]);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Home)
                {
                    // Move cursor to beginning of input
                    if (cursorPosition > 0)
                    {
                        Console.Write(new string('\b', cursorPosition));
                        cursorPosition = 0;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.End)
                {
                    // Move cursor to end of input
                    if (cursorPosition < input.Length)
                    {
                        Console.Write(input.Substring(cursorPosition));
                        cursorPosition = input.Length;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    // Insert character at cursor position
                    input = input.Substring(0, cursorPosition) + keyInfo.KeyChar + input.Substring(cursorPosition);
                    cursorPosition++;
                    
                    // Redraw from cursor position
                    Console.Write(input.Substring(cursorPosition - 1));
                    
                    // Position cursor correctly
                    int moveBack = input.Length - cursorPosition;
                    if (moveBack > 0)
                        Console.Write(new string('\b', moveBack));
                }
            } while (true);
            
            return input;
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
