using System.Diagnostics.CodeAnalysis;

namespace ConsoleGUI
{
    public static partial class GUI
    {
        private static readonly TextBox logBox = new(0, 0, _guiWidth, 1, -1, string.Empty, ConsoleColor.Black, ConsoleColor.White);
        private static readonly List<TextBox> textBoxes = new();
        private static readonly List<TextBox> textFields = new();
        private static byte textBoxSelection = 0;

        public enum Interactivity
        {
            None,
            ScrollOnly,
            ScrollAndSelect
        }

        #region TextBox
        private class TextBox
        {
            private readonly int left;
            private readonly int top;
            private readonly int width;
            private readonly int height;
            protected bool scrollBar;

            private string      textFull;
            private string[]    textFormatted;
            private int startLine;
            private int selectedLine;

            private readonly ConsoleColor bgColor;
            private readonly ConsoleColor textColor;
            private readonly ConsoleColor inactiveColor;

            public string this[int lineIndex] => textFormatted[lineIndex].TrimEnd();

            /// <summary>
            /// Gets and sets the text of a textbox, and also formats the text to properly fit into the box.
            /// </summary>
            public string Text
            {
                [MemberNotNull(nameof(textFull), nameof(textFormatted))]
                set
                {
                    textFull = value;
                    string[] newTextFormatted = FormatText(textFull, width);
                    scrollBar = (newTextFormatted.Length > height) && height >= 3;

                    if (scrollBar)  // Adjust the text to a reduced width if there's need of a scrollbar
                        newTextFormatted = FormatText(textFull, width - 1);

                    if (newTextFormatted.Length < textFormatted?.Length)
                    {
                        if (selectedLine > newTextFormatted.Length - 1)
                        {
                            selectedLine = newTextFormatted.Length - 1;

                            if (selectedLine < 0)
                                selectedLine = 0;
                        }

                        for (int line = 0; line < textFormatted.Length; line++)
                            textFormatted[line] = string.Empty.PadRight(width);

                        Render(true, false);
                    }

                    textFormatted = newTextFormatted;

                    Render();
                }

                get => textFull;
            }

            public int SelectedLine => selectedLine;

            /// <summary>
            /// TextBox constructor
            /// </summary>
            public TextBox(int left, int top, int width, int height, int selectedLine, string text = "",
                ConsoleColor bgColor = _guiColor, ConsoleColor textColor = _guiTextColor, ConsoleColor inactiveColor = _guiInactiveColor)
            {
                this.left = left;
                this.top = top;
                this.width = width;
                this.height = height;

                this.startLine = 0;
                this.selectedLine = selectedLine;

                this.bgColor = bgColor;
                this.textColor = textColor;
                this.inactiveColor = inactiveColor;

                this.Text = text;   // sets textFull, textFormatted, and scrollBar
            }

            /// <summary>
            /// Adds more text to a textbox, with options for linebreaks, padding, and slow rendering of the new text
            /// </summary>
            public void AddText(string newText, byte linebreaks = 1, bool addToTop = false, bool autoScroll = false, bool renderSlow = false, int milliSecDelay = 10)
            {
                newText = addToTop ? newText.PadRight(newText.Length + linebreaks, '\n') : newText.PadLeft(newText.Length + linebreaks, '\n');

                string[] newTextFormatted = FormatText(newText, width - (scrollBar ? 1 : 0), false);

                if (!scrollBar && newTextFormatted.Length + textFormatted.Length > height && height >= 3)
                {
                    newTextFormatted = FormatText(newText, width - 1, false);
                    textFormatted = FormatText(textFull, width - 1, false);
                    scrollBar = true;
                }

                int newStartLine = addToTop ? 0 : textFormatted.Length;
                int newEndLine = addToTop ? newTextFormatted.Length - 1 : textFormatted.Length + newTextFormatted.Length + 1;

                if (textFull == string.Empty)
                {
                    textFull = newText;
                    textFormatted = newTextFormatted;
                }
                else
                {
                    textFull = addToTop ? newText + "\n" + textFull : textFull + "\n" + newText;
                    textFormatted = addToTop ? newTextFormatted.Concat(textFormatted).ToArray() : textFormatted.Concat(newTextFormatted).ToArray();
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
                if (selectedLine > 0)
                    selectedLine--;
                else
                    ScrollUp(render: false);

                Render();
            }

            /// <summary>
            /// Moves the selection one line down in the current textbox. Scrolls down if necessary.
            /// </summary>
            public void NextLine()
            {
                if (selectedLine < textFormatted.Length - 1)
                {
                    if (selectedLine < height - 1 && selectedLine >= 0)
                        selectedLine++;
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
                if (textFormatted.Length > height)
                {
                    startLine--;

                    if (startLine < 0)
                        startLine = 0;

                    if (render)
                        Render();
                }
            }

            /// <summary>
            /// Handles scrolling a textbox downwards.
            /// </summary>
            public void ScrollDown(bool render = true)
            {
                if (textFormatted.Length > height)
                {
                    startLine++;

                    if (startLine > (textFormatted.Length - height))
                        startLine = textFormatted.Length - height;

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
                    startLine = 0;

                    if (scrollToBottom && textFormatted.Length > height)
                        startLine = textFormatted.Length - height;
                }

                bool renderAllSlow = renderSlow && (slowStartLine == -1 && slowEndLine == -1);
                bool fastPass = true;

                // Calculations used for moving the scrollbar
                float scrollPercentage;
                int scrollbarPosition = 0;

                if (scrollBar)
                {
                    scrollPercentage = ((float)startLine + height / 2) / (float)textFormatted.Length;
                    scrollbarPosition = startLine == 0 ? 1
                        : (startLine == textFormatted.Length - height ? height - 2
                        : 1 + (int)((height - 2) * scrollPercentage));

                    Console.BackgroundColor = bgColor;
                    Console.ForegroundColor = textColor;
                    Console.SetCursorPosition(left + width - 1, top + scrollbarPosition);
                    Console.Write("█");
                }

                for (int i = 0; i <= height; i++)
                {
                    // If the renderer has surpassed the final line in either the text or the box...
                    if (i + startLine >= textFormatted.Length || i >= height)
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
                    if (GUI.textBoxes?.Count != 0 && i == selectedLine) {
                        if (this == textBoxes?[GUI.textBoxSelection])    // Different colors depending on if it's the active textbox or not
                            (Console.BackgroundColor, Console.ForegroundColor) = (textColor, bgColor);
                        else
                            (Console.BackgroundColor, Console.ForegroundColor) = (inactiveColor, textColor);
                    }
                    else
                        (Console.BackgroundColor, Console.ForegroundColor) = (bgColor, textColor);

                    Console.SetCursorPosition(left, top + i);

                    if (fastPass)
                    {
                        if (renderAllSlow || (renderSlow && i + startLine >= slowStartLine && i + startLine <= slowEndLine))
                            Console.Write(String.Empty.PadRight(width - (scrollBar ? 1 : 0)));
                        else
                            Console.Write(textFormatted[i + startLine]);
                    }
                    else
                    {
                        if (renderAllSlow || (i + startLine >= slowStartLine && i + startLine <= slowEndLine))
                        {
                            string trimmedText = textFormatted[i + startLine].TrimEnd();

                            foreach (char c in trimmedText)
                            {
                                Console.Write(c);
                                Thread.Sleep(millisecDelay);
                            }
                        }
                        else if (i + startLine > slowEndLine)
                            break;
                    }

                    if (scrollBar)
                    {
                        Console.BackgroundColor = bgColor;
                        Console.ForegroundColor = textColor;
                        Console.CursorLeft = left + width - 1;

                        if (i == 0 && startLine > 0)
                            Console.Write("▲");
                        else if (i == height - 1 && startLine + height < textFormatted.Length)
                            Console.Write("▼");
                        else if (i != scrollbarPosition)
                            Console.Write("░");
                    }
                }

                Console.SetCursorPosition(0, 0);
            }
        }
        #endregion

        /// <summary>
        /// Creates a textbox and adds it to the textBoxes or textFields lists, depending on if it's set as interactable or not
        /// </summary>
        public static void CreateTextbox(int left, int top, int width, int height, string text = "",
            ConsoleColor? bgColor = null, ConsoleColor? textColor = null, Interactivity interactivity = Interactivity.None)
        {
            if (textBoxes.Count < byte.MaxValue || interactivity == Interactivity.None)
            {
                try
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

                    TextBox textBox = new(left, top, width, height, selectedLine ,text, bgColor.Value, textColor.Value);

                    if (interactivity == Interactivity.None)
                    {
                        textFields.Add(textBox);
                        return;
                    }

                    textBoxes.Add(textBox);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    PrintInfo("GUI.CreateTextbox error: " + ex.Message);
                }
            }
        }

        public static class Controls
        {
            public static class LogBox
            {
                public static void ScrollUp() => logBox.ScrollUp();
                public static void ScrollDown() => logBox.ScrollDown();
            }

            /// <summary>
            /// Switches focus and controls to the previous item in the textBoxes list.
            /// </summary>
            public static void PrevTextbox()
            {
                if (textBoxes.Count < 1)
                    return;

                TextBox prevSelection = textBoxes[textBoxSelection];

                textBoxSelection--;

                if (textBoxSelection == byte.MaxValue)
                    textBoxSelection = (byte)(textBoxes.Count - 1);

                prevSelection.Render();

                if (prevSelection != textBoxes[textBoxSelection])
                    textBoxes[textBoxSelection].Render();
            }

            /// <summary>
            /// Switches focus and controls to the next item in the textBoxes list.
            /// </summary>
            public static void NextTextbox()
            {
                if (textBoxes.Count < 1)
                    return;

                TextBox prevSelection = textBoxes[textBoxSelection];

                textBoxSelection++;

                if (textBoxSelection >= textBoxes.Count)
                    textBoxSelection = 0;

                prevSelection.Render();
                if (prevSelection != textBoxes[textBoxSelection])
                    textBoxes[textBoxSelection].Render();
            }

            public static void PrevSelection()
            {
                if (textBoxes.Count < 1)
                    return;

                textBoxes[textBoxSelection].PrevLine();
            }

            public static void NextSelection()
            {
                if (textBoxes.Count < 1)
                    return;

                textBoxes[textBoxSelection].NextLine();
            }

            /// <summary>
            /// Handles 'selection' of lines in a textbox
            /// </summary>
            public static string ReadSelection()
            {
                if (textBoxes.Count < 1)
                    return string.Empty;

                int selectedLine = textBoxes[textBoxSelection].SelectedLine;

                if (selectedLine == -1)
                    return string.Empty;

                return textBoxes[textBoxSelection][selectedLine];
            }

            public static string ReadTextbox()
            {
                if (textBoxes.Count < 1)
                    return string.Empty;

                return textBoxes[textBoxSelection].Text;
            }

            public static void WriteTextbox(string newText)
            {
                if (textBoxes.Count < 1)
                    return;

                textBoxes[textBoxSelection].Text = newText;
            }

            /// <summary>
            /// Refreshes the currently selected textbox
            /// </summary>
            public static void RefreshTextbox()
            {
                if (textBoxes.Count < 1)
                    return;

                textBoxes[textBoxSelection].Render();
            }

            /// <summary>
            /// Clears the currently selected textbox
            /// </summary>
            public static void ClearTextbox()
            {
                if (textBoxes.Count < 1)
                    return;

                textBoxes[textBoxSelection].Text = "";
            }

            /// <summary>
            /// Inserts new text into the selected textbox
            /// </summary>
            public static void InsertIntoTextbox(string text)
            {
                if (textBoxes.Count < 1)
                    return;
                
                textBoxes[textBoxSelection].AddText(text, 2, true, false, true);
            }
        }
    }
}
