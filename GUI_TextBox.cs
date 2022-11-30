using System.Diagnostics.CodeAnalysis;

namespace ConsoleGUI
{
    public static partial class GUI
    {
        private static readonly TextBox logBox = TextBox.Factory.InstantiateLogBox();
        private static TextBox? selectedTextBox = null;

        public static class LogBox
        {
            /// <summary>
            /// Adds text to the textbox at the top of the console. Useful for error messages, instructions, etc.
            /// </summary>
            public static void Print(string output)
            {
                string appendedText = (logBox.Text != string.Empty)
                    ? "\n" + logBox.Text
                    : string.Empty;

                logBox.Text = $"({_logEntries:00}) : {output}{appendedText}";
                _logEntries++;

                logBox.Render();
            }

            public static void ScrollUp() => logBox.ScrollUp();
            public static void ScrollDown() => logBox.ScrollDown();
        }

        public class TextBox
        {
            public enum Interactivity
            {
                None,
                ScrollOnly,
                ScrollAndSelect
            }

            private readonly int _left;
            private readonly int _top;
            private readonly int _width;
            private readonly int _height;
            private bool _scrollBar;

            private string      _textFull;
            private string[]    _textFormatted;
            private int _renderStartLine;
            private int _selectedLine;

            private ConsoleColor _bgColor;
            private ConsoleColor _textColor;
            private ConsoleColor _inactiveColor;

            public string this[int lineIndex] => _textFormatted[lineIndex].TrimEnd();
            public int SelectedLine => _selectedLine;
            public ConsoleColor BackgroundColor { get => _bgColor; set => _bgColor = value; }
            public ConsoleColor TextColor { get => _textColor; set => _textColor = value; }
            public ConsoleColor InactiveColor { get => _inactiveColor; set => _inactiveColor = value; }

            /// <summary>
            /// Gets and sets the text of a textbox, and also formats the text to properly fit into the box.
            /// </summary>
            public string Text
            {
                [MemberNotNull(nameof(_textFull), nameof(_textFormatted))]
                set
                {
                    _textFull = value;
                    string[] newTextFormatted = FormatText(_textFull, _width);
                    _scrollBar = (newTextFormatted.Length > _height) && _height >= 3;

                    if (_scrollBar)  // Adjust the text to a reduced width if there's need of a scrollbar
                        newTextFormatted = FormatText(_textFull, _width - 1);

                    if (newTextFormatted.Length < _textFormatted?.Length)
                    {
                        if (_selectedLine > newTextFormatted.Length - 1)
                        {
                            _selectedLine = newTextFormatted.Length - 1;

                            if (_selectedLine < 0)
                                _selectedLine = 0;
                        }

                        for (int line = 0; line < _textFormatted.Length; line++)
                            _textFormatted[line] = string.Empty.PadRight(_width);

                        Render(true, false);
                    }

                    _textFormatted = newTextFormatted;

                    Render();
                }

                get => _textFull;
            }

            /// <summary>
            /// TextBox constructor
            /// </summary>
            private TextBox(int left, int top, int width, int height, int selectedLine, string text = "",
                ConsoleColor bgColor = _guiColor, ConsoleColor textColor = _guiTextColor, ConsoleColor inactiveColor = _guiInactiveColor)
            {
                this._left = left;
                this._top = top;
                this._width = width;
                this._height = height;

                this._renderStartLine = 0;
                this._selectedLine = selectedLine;

                this._bgColor = bgColor;
                this._textColor = textColor;
                this._inactiveColor = inactiveColor;

                this.Text = text;   // sets textFull, textFormatted, and scrollBar
            }

            public static class Factory
            {
                public static TextBox InstantiateLogBox()
                {
                    if (logBox is not null)
                        return logBox;

                    return new TextBox(0, 0, _guiWidth, 1, -1, string.Empty, ConsoleColor.Black, ConsoleColor.White);
                }

                public static TextBox CreateTextBox(int left, int top, int width, int height, string text = "",
                    ConsoleColor? bgColor = null, ConsoleColor? textColor = null, Interactivity interactivity = Interactivity.None)
                {
                    // Throw an exception if a parameter value would lead to drawing outside the console buffer...
                    if (left < 0 || left > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(left), "Origin point is beyond horizontal buffer bounds.");
                    if (top < 0 || top > GetGUIHeight) throw new ArgumentOutOfRangeException(nameof(top), "Origin point is beyond vertical buffer bounds.");
                    if (left + width > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(left) + "', '" + nameof(width), "Too wide.");
                    if (top + height > GetGUIHeight) throw new ArgumentOutOfRangeException(nameof(top) + "', '" + nameof(height), "Too tall.");

                    top++;  // Push GUI down from row 0

                    // Assigns default colors if none are provided
                    bgColor ??= _guiColor;
                    textColor ??= _guiTextColor;
                    int selectedLine = (interactivity == Interactivity.ScrollAndSelect ? 0 : -1);

                    TextBox textBox = new(left, top, width, height, selectedLine, text, bgColor.Value, textColor.Value);

                    return textBox;
                }
            }

            /// <summary>
            /// Adds more text to a textbox, with options for linebreaks, padding, and slow rendering of the new text
            /// </summary>
            public void AddText(string newText, byte linebreaks = 1, bool addToTop = false, bool autoScroll = false, bool renderSlow = false, int milliSecDelay = 10)
            {
                newText = addToTop ? newText.PadRight(newText.Length + linebreaks, '\n') : newText.PadLeft(newText.Length + linebreaks, '\n');

                string[] newTextFormatted = FormatText(newText, _width - (_scrollBar ? 1 : 0), false);

                if (!_scrollBar && newTextFormatted.Length + _textFormatted.Length > _height && _height >= 3)
                {
                    newTextFormatted = FormatText(newText, _width - 1, false);
                    _textFormatted = FormatText(_textFull, _width - 1, false);
                    _scrollBar = true;
                }

                int newStartLine = addToTop ? 0 : _textFormatted.Length;
                int newEndLine = addToTop ? newTextFormatted.Length - 1 : _textFormatted.Length + newTextFormatted.Length + 1;

                if (_textFull == string.Empty)
                {
                    _textFull = newText;
                    _textFormatted = newTextFormatted;
                }
                else
                {
                    _textFull = addToTop ? newText + "\n" + _textFull : _textFull + "\n" + newText;
                    _textFormatted = addToTop ? newTextFormatted.Concat(_textFormatted).ToArray() : _textFormatted.Concat(newTextFormatted).ToArray();
                }

                Render(autoScroll, !addToTop, renderSlow, milliSecDelay, newStartLine, newEndLine);
            }

            /// <summary>
            /// Handles formatting of a text string to fit within a block of the specified width.
            /// </summary>
            private static string[] FormatText(string text, int width, bool removeTrailingLinebreaks = true)
            {
                if (string.IsNullOrWhiteSpace(text))
                    return new string[] { "".PadRight(width) };

                text = text.Replace('\t', ' ');

                if (removeTrailingLinebreaks)
                    text = text.TrimEnd('\n', '\r');

                string[] textLines = text.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None); // Splits the text at linebreak characters

                SplitLinesByLength(ref textLines, width);
                                
                return textLines;
            }

            /// <summary>
            /// Identifies at what point to split a line in order to fit it into a textbox.
            /// </summary>
            private static void SplitLinesByLength(ref string[] lines, int maxLength)
            {
                Queue<string> newLines = new();

                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    // If a string fits the textbox width, just pad and enqueue it
                    if (lines[lineIndex].Length <= maxLength)
                    {
                        newLines.Enqueue(lines[lineIndex].PadRight(maxLength));
                        continue;
                    }

                    // Otherwise, find an appropriate character to split it at
                    for (int i = maxLength; i >= 0; i--)
                    {
                        if (i != maxLength && lines[lineIndex][i] == '-')
                        {
                            newLines.Enqueue(lines[lineIndex][..(i + 1)].PadRight(maxLength));
                            lines[lineIndex] = lines[lineIndex][(i + 1)..];
                            lineIndex--;
                            break;
                        }

                        if (lines[lineIndex][i] == ' ')
                        {
                            newLines.Enqueue(lines[lineIndex][..i].PadRight(maxLength));
                            lines[lineIndex] = lines[lineIndex][(i + 1)..];
                            lineIndex--;
                            break;
                        }

                        // If no split point can be found, just split it at the final character
                        if (i == 0)
                        {
                            newLines.Enqueue(lines[lineIndex][..maxLength]);
                            lines[lineIndex] = lines[lineIndex][maxLength..];
                            lineIndex--;
                            break;
                        }
                    }
                }

                lines = newLines.ToArray();
            }
                        
            /// <summary>
            /// Moves the selection one line up in the current textbox. Scrolls up if necessary.
            /// </summary>
            public void PrevLine()
            {
                if (_selectedLine > 0)
                    _selectedLine--;
                else
                    ScrollUp(render: false);

                Render();
            }

            /// <summary>
            /// Moves the selection one line down in the current textbox. Scrolls down if necessary.
            /// </summary>
            public void NextLine()
            {
                if (_selectedLine < _textFormatted.Length - 1)
                {
                    if (_selectedLine < _height - 1 && _selectedLine >= 0)
                        _selectedLine++;
                    else
                        ScrollDown(render: false);

                    Render();
                }
            }


            /// <summary>
            /// Handles scrolling a textbox upwards.
            /// </summary>
            public void ScrollUp(bool render = true)
            {
                if (_textFormatted.Length > _height)
                {
                    _renderStartLine--;

                    if (_renderStartLine < 0)
                        _renderStartLine = 0;

                    if (render)
                        Render();
                }
            }

            /// <summary>
            /// Handles scrolling a textbox downwards.
            /// </summary>
            public void ScrollDown(bool render = true)
            {
                if (_textFormatted.Length > _height)
                {
                    _renderStartLine++;

                    if (_renderStartLine > (_textFormatted.Length - _height))
                        _renderStartLine = _textFormatted.Length - _height;

                    if (render)
                        Render();
                }
            }

            /// <summary>
            /// Renders all the visible text lines in a textbox
            /// </summary>
            public void Render(bool autoScroll = false, bool scrollToBottom = true,
                bool renderSlow = false, int millisecDelay = 10, int slowStartLine = -1, int slowEndLine = -1)
            {
                if (autoScroll)
                {
                    _renderStartLine = 0;

                    if (scrollToBottom && _textFormatted.Length > _height)
                        _renderStartLine = _textFormatted.Length - _height;
                }

                bool renderAllSlow = renderSlow && (slowStartLine == -1 && slowEndLine == -1);
                bool fastPass = true;

                // Calculations used for moving the scrollbar
                float scrollPercentage;
                int scrollbarPosition = 0;

                if (_scrollBar)
                {
                    scrollPercentage = ((float)_renderStartLine + _height / 2) / (float)_textFormatted.Length;
                    scrollbarPosition = _renderStartLine == 0 ? 1
                        : (_renderStartLine == _textFormatted.Length - _height ? _height - 2
                        : 1 + (int)((_height - 2) * scrollPercentage));

                    Console.BackgroundColor = _bgColor;
                    Console.ForegroundColor = _textColor;
                    Console.SetCursorPosition(_left + _width - 1, _top + scrollbarPosition);
                    Console.Write("█");
                }

                for (int i = 0; i <= _height; i++)
                {
                    // If the renderer has surpassed the final line in either the text or the box...
                    if (i + _renderStartLine >= _textFormatted.Length || i >= _height)
                    {
                        if (fastPass && renderSlow)     // ...proceed with the slow pass if applicable...
                        {
                            fastPass = false;
                            i = -1;
                            continue;
                        }
                        else                            // ...or stop the rendering process if not
                            break;
                    }

                    // This handles showing what line you have marked
                    if (GUI.selectedTextBox is not null && i == _selectedLine) {
                        if (this == selectedTextBox)    // Different colors depending on if it's the active textbox or not
                            (Console.BackgroundColor, Console.ForegroundColor) = (_textColor, _bgColor);
                        else
                            (Console.BackgroundColor, Console.ForegroundColor) = (_inactiveColor, _textColor);
                    }
                    else
                        (Console.BackgroundColor, Console.ForegroundColor) = (_bgColor, _textColor);

                    Console.SetCursorPosition(_left, _top + i);

                    if (fastPass)
                    {
                        if (renderAllSlow || (renderSlow && i + _renderStartLine >= slowStartLine && i + _renderStartLine <= slowEndLine))
                            Console.Write(String.Empty.PadRight(_width - (_scrollBar ? 1 : 0)));
                        else
                            Console.Write(_textFormatted[i + _renderStartLine]);
                    }
                    else
                    {
                        if (renderAllSlow || (i + _renderStartLine >= slowStartLine && i + _renderStartLine <= slowEndLine))
                        {
                            string trimmedText = _textFormatted[i + _renderStartLine].TrimEnd();

                            foreach (char c in trimmedText)
                            {
                                Console.Write(c);
                                Thread.Sleep(millisecDelay);
                            }
                        }
                        else if (i + _renderStartLine > slowEndLine)
                            break;
                    }

                    if (_scrollBar)
                    {
                        Console.BackgroundColor = _bgColor;
                        Console.ForegroundColor = _textColor;
                        Console.CursorLeft = _left + _width - 1;

                        if (i == 0 && _renderStartLine > 0)
                            Console.Write("▲");
                        else if (i == _height - 1 && _renderStartLine + _height < _textFormatted.Length)
                            Console.Write("▼");
                        else if (i != scrollbarPosition)
                            Console.Write("░");
                    }
                }

                Console.SetCursorPosition(0, 0);
            }
        }
    }
}
