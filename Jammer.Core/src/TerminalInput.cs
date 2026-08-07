using System.Diagnostics;

namespace Jammer
{
    /// <summary>
    /// Normalizes terminal escape sequences that Console.ReadKey may leave undecoded.
    /// </summary>
    public static class TerminalInput
    {
        private const int EscapeSequenceTimeoutMs = 20;

        public static ConsoleKeyInfo ReadKey(bool intercept = true)
        {
            ConsoleKeyInfo first = Console.ReadKey(intercept);
            if (first.Key != ConsoleKey.Escape || first.Modifiers != 0)
            {
                return first;
            }

            string sequence = ReadEscapeSequence();
            return TryDecodeAnsiSequence(sequence, out ConsoleKeyInfo decoded) ? decoded : first;
        }

        public static bool TryDecodeAnsiSequence(string sequence, out ConsoleKeyInfo keyInfo)
        {
            keyInfo = default;
            if (string.IsNullOrEmpty(sequence))
            {
                return false;
            }

            if (sequence.Length == 1 && char.IsAsciiLetter(sequence[0]) &&
                Enum.TryParse(sequence.ToUpperInvariant(), out ConsoleKey altKey))
            {
                keyInfo = new ConsoleKeyInfo(sequence[0], altKey, false, true, false);
                return true;
            }

            if (sequence[0] == 'O' && sequence.Length == 2)
            {
                return TryCreateKey(sequence[1] switch
                {
                    'A' => ConsoleKey.UpArrow,
                    'B' => ConsoleKey.DownArrow,
                    'C' => ConsoleKey.RightArrow,
                    'D' => ConsoleKey.LeftArrow,
                    'H' => ConsoleKey.Home,
                    'F' => ConsoleKey.End,
                    _ => ConsoleKey.NoName,
                }, 1, out keyInfo);
            }

            if (sequence[0] != '[' || sequence.Length < 2)
            {
                return false;
            }

            char final = sequence[^1];
            string parameters = sequence[1..^1];
            int modifier = ParseModifier(parameters);
            ConsoleKey key = final switch
            {
                'A' => ConsoleKey.UpArrow,
                'B' => ConsoleKey.DownArrow,
                'C' => ConsoleKey.RightArrow,
                'D' => ConsoleKey.LeftArrow,
                'H' => ConsoleKey.Home,
                'F' => ConsoleKey.End,
                '~' => ParseTildeKey(parameters),
                _ => ConsoleKey.NoName,
            };

            return TryCreateKey(key, modifier, out keyInfo);
        }

        private static string ReadEscapeSequence()
        {
            var sequence = new System.Text.StringBuilder();
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < EscapeSequenceTimeoutMs && !Console.KeyAvailable)
            {
                Thread.Sleep(1);
            }

            if (!Console.KeyAvailable)
            {
                return string.Empty;
            }

            sequence.Append(Console.ReadKey(true).KeyChar);
            if (sequence[0] is not ('[' or 'O'))
            {
                return sequence.ToString();
            }

            while (sequence.Length < 16)
            {
                stopwatch.Restart();
                while (stopwatch.ElapsedMilliseconds < EscapeSequenceTimeoutMs && !Console.KeyAvailable)
                {
                    Thread.Sleep(1);
                }

                if (!Console.KeyAvailable)
                {
                    break;
                }

                sequence.Append(Console.ReadKey(true).KeyChar);
                char last = sequence[^1];
                if (sequence.Length >= 2 && (last == '~' || last is >= 'A' and <= 'Z' || last is >= 'a' and <= 'z'))
                {
                    break;
                }
            }

            return sequence.ToString();
        }

        private static ConsoleKey ParseTildeKey(string parameters)
        {
            string number = parameters.Split(';')[0];
            return number switch
            {
                "1" or "7" => ConsoleKey.Home,
                "4" or "8" => ConsoleKey.End,
                "5" => ConsoleKey.PageUp,
                "6" => ConsoleKey.PageDown,
                _ => ConsoleKey.NoName,
            };
        }

        private static int ParseModifier(string parameters)
        {
            string[] parts = parameters.Split(';');
            return parts.Length > 1 && int.TryParse(parts[^1], out int modifier) ? modifier : 1;
        }

        private static bool TryCreateKey(ConsoleKey key, int modifier, out ConsoleKeyInfo keyInfo)
        {
            keyInfo = default;
            if (key == ConsoleKey.NoName)
            {
                return false;
            }

            bool shift = modifier is 2 or 4 or 6 or 8;
            bool alt = modifier is 3 or 4 or 7 or 8;
            bool control = modifier is 5 or 6 or 7 or 8;
            keyInfo = new ConsoleKeyInfo('\0', key, shift, alt, control);
            return true;
        }
    }
}
