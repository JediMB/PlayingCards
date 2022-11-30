using System.Security.Cryptography.X509Certificates;
using static ConsoleGUI.GUI;

namespace ConsoleGUI
{
    public partial class GUI
    {
        public class TextBoxGroup
        {
            private readonly List<TextBox> _textBoxes;
            private int _selectedIndex;
            public TextBox this[int index] => _textBoxes[index];
            public int Count => _textBoxes.Count;
            public TextBox.Interactivity Interactivity { get; init; }
            public int SelectedIndex
            {
                get => _selectedIndex;
                private set
                {
                    if (value >= _textBoxes.Count)
                    {
                        _selectedIndex = 0;
                        return;
                    }

                    if (value < 0)
                    {
                        _selectedIndex = _textBoxes.Count - 1;
                        return;
                    }

                    _selectedIndex = value;
                }
            }

            private TextBoxGroup(TextBox.Interactivity interactivity)
            {
                _textBoxes = new();
                this.Interactivity = interactivity;
            }

            public static class Factory
            {
                public static TextBoxGroup CreateLabelGroup() => new(TextBox.Interactivity.None);

                public static TextBoxGroup CreateScrollingGroup() => new(TextBox.Interactivity.ScrollOnly);

                public static TextBoxGroup CreateSelectableGroup() => new(TextBox.Interactivity.ScrollAndSelect);

                public static TextBoxGroup CreateLabelSequence(int count, bool horizontal, int left, int top, int width, int height, ConsoleColor? bgColor = null, ConsoleColor? textColor = null)
                {
                    TextBoxGroup group = new(TextBox.Interactivity.None);

                    for (int i = 0; i < count; i++)
                    {
                        group.Add(left, top, width, height, string.Empty, bgColor, textColor);

                        if (horizontal)
                            left += width;
                        else
                            top += height;
                    }
                    // TODO: Maybe implement code that locks the group from further additions?

                    return group;
                }
            }

            public void Add(int left, int top, int width, int height, string text = "", ConsoleColor? bgColor = null, ConsoleColor? textColor = null)
            {
                try
                {
                    this._textBoxes.Add(TextBox.Factory.CreateTextBox(left, top, width, height, text, bgColor, textColor, this.Interactivity));

                    selectedTextBox ??= this._textBoxes[0];
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    LogBox.Print("GUI.CreateTextbox error: " + ex.Message);
                }
            }
            
            /// <summary>
            /// Switches focus and controls to the previous item in the textBoxes list.
            /// </summary>
            public void PrevTextbox()
            {
                if (_textBoxes.Count < 1 || this.Interactivity == TextBox.Interactivity.None)
                    return;

                TextBox prevSelection = _textBoxes[SelectedIndex];

                SelectedIndex--;

                prevSelection.Render();
                //if (prevSelection != _textBoxes[SelectedBox])
                _textBoxes[SelectedIndex].Render();

                GUI.selectedTextBox = _textBoxes[SelectedIndex];
            }

            /// <summary>
            /// Switches focus and controls to the next item in the textBoxes list.
            /// </summary>
            public void NextTextbox()
            {
                if (_textBoxes.Count < 1 || this.Interactivity == TextBox.Interactivity.None)
                    return;

                TextBox prevSelection = _textBoxes[SelectedIndex];
                
                SelectedIndex++;

                prevSelection.Render();
                //if (prevSelection != _textBoxes[SelectedBox])
                _textBoxes[SelectedIndex].Render();

                GUI.selectedTextBox = _textBoxes[SelectedIndex];
            }

            public void PrevSelection()
            {
                if (_textBoxes.Count < 1 || this.Interactivity == TextBox.Interactivity.None)
                    return;

                _textBoxes[SelectedIndex].PrevLine();
            }

            public void NextSelection()
            {
                if (_textBoxes.Count < 1 || this.Interactivity == TextBox.Interactivity.None)
                    return;

                _textBoxes[SelectedIndex].NextLine();
            }

            /// <summary>
            /// Handles 'selection' of lines in a textbox
            /// </summary>
            public string ReadSelection()
            {
                if (_textBoxes.Count < 1 || this.Interactivity == TextBox.Interactivity.None)
                    return string.Empty;

                int selectedLine = _textBoxes[SelectedIndex].SelectedLine;

                if (selectedLine == -1)
                    return string.Empty;

                return _textBoxes[SelectedIndex][selectedLine];
            }

            public string ReadTextbox()
            {
                if (_textBoxes.Count < 1)
                    return string.Empty;

                return _textBoxes[SelectedIndex].Text;
            }

            public void WriteTextbox(string newText)
            {
                if (_textBoxes.Count < 1)
                    return;

                _textBoxes[SelectedIndex].Text = newText;
            }

            /// <summary>
            /// Refreshes the currently selected textbox
            /// </summary>
            public void RefreshTextbox()
            {
                if (_textBoxes.Count < 1)
                    return;

                _textBoxes[SelectedIndex].Render();
            }

            /// <summary>
            /// Clears the currently selected textbox
            /// </summary>
            public void ClearTextbox()
            {
                if (_textBoxes.Count < 1)
                    return;

                _textBoxes[SelectedIndex].Text = "";
            }

            /// <summary>
            /// Inserts new text into the selected textbox
            /// </summary>
            public void InsertIntoTextbox(string text)
            {
                if (_textBoxes.Count < 1)
                    return;

                _textBoxes[SelectedIndex].AddText(text, 2, true, false, true);
            }
        }
    }
}
