using System.Runtime.InteropServices;

namespace ConsoleGUI
{
    [SupportedOSPlatform("windows")]

    public static partial class GUI
    {
        private static readonly int _guiWidth = 128;
        private static readonly int _guiHeight = 48;
        private static int _logEntries = 0;
        private const ConsoleColor _guiColor = ConsoleColor.DarkBlue;
        private const ConsoleColor _guiTextColor = ConsoleColor.Yellow;
        private const ConsoleColor _guiInactiveColor = ConsoleColor.DarkYellow;

        private const string _lineH = "─═";
        private const string _lineV = "│║";
        private const string _cornerTL = "┌╔├╠┬╦┼╬";
        private const string _cornerTR = "┐╗┤╣┬╦┼╬";
        private const string _cornerBL = "└╚├╠┴╩┼╬";
        private const string _cornerBR = "┘╝┤╣┴╩┼╬";
        //private const string _block = "■█▀▄";
        //private const string _halftone = "░▒▓";
        //private const string _triangleT = "▵▴△▲";
        //private const string _triangleB = "▿▾▽▼";
        //private const string _triangleL = "◃◂◁◀";
        //private const string _triangleR = "▹▸▷▶";
        //private const string _triangleTL = "◸◤";
        //private const string _triangleTR = "◹◥";
        //private const string _triangleBL = "◺◣";
        //private const string _triangleBR = "◿◢";


        public enum BorderStyle
        {
            Single,
            Double
        }

        public enum EdgeStyle
        {
            None = 0,
            VerticalJunction = 2,
            HorizontalJunction = 4,
            Crossing = 6
        }

        public enum CornerStyle
        {
            Corner = 0,
            VerticalJunction = 2,
            HorizontalJunction = 4,
            Crossing = 6
        }

        public static int GetGUIWidth { get => _guiWidth; }
        public static int GetGUIHeight { get => _guiHeight; }

        /// <summary>
        /// Sets up necessary console settings for the application and its GUI.
        /// </summary>
        public static void Initialize(string windowTitle)
        {
            Console.BufferWidth = GetGUIWidth;
            Console.BufferHeight = GetGUIHeight + 1;
            Console.WindowWidth = GetGUIWidth;
            Console.WindowHeight = GetGUIHeight + 1;

            Console.CursorVisible = false;
            Console.Title = windowTitle;
            Console.BackgroundColor = _guiColor;
            Console.ForegroundColor = _guiTextColor;
            Console.Clear();

            DisableResize();
        }

        static void DisableResize()
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                int dmMsg = DeleteMenu(sysMenu, 0xF000, 0x00000000);

                PrintInfo($"Window resizing has{(dmMsg == 1 ? "" : " NOT")} been disabled.");
            }
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool pRevert);
        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        /// <summary>
        /// Adds text to the textbox at the top of the console. Useful for error messages, instructions, etc.
        /// </summary>
        public static void PrintInfo(string output)
        {
            string appendedText = (logBox.Text != string.Empty)
                ? "\n" + logBox.Text
                : string.Empty;

            logBox.Text = $"({_logEntries:00}) : {output}{appendedText}";
            _logEntries++;

            logBox.Render();
        }

        /// <summary>
        /// Writes a single char at a the specified position in the console buffer
        /// </summary>
        private static void CharAtPosition(char output, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(output);
        }

        /// <summary>
        /// Draws a zigzag pattern as a horizontal line
        /// </summary>
        /// <param name="startX">X position for the origin point</param>
        /// <param name="topY">Y position for the top half of the line</param>
        /// <param name="width">Width of the line</param>
        /// <param name="borderStyle">Single or double-line border style</param>
        /// <param name="zigzagStyle">Option to flip the zigzag pattern vertically</param>
        /// <param name="straightEdge">Option to have the pattern starting and ending with straight lines</param>
        /// <param name="bgColor">Background color</param>
        /// <param name="textColor">Foreground/text color</param>
        public static void DrawLineZigzag(int startX, int topY, int width, BorderStyle borderStyle = 0,
            bool straightEdge = false, bool flipped = false,
            ConsoleColor? bgColor = null, ConsoleColor? textColor = null)
        {
            try
            {
                // Throw an exception if a parameter value would lead to drawing outside the console buffer...
                if (startX < 0 || startX > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(startX), "Origin point is beyond buffer bounds.");
                if (topY < 0 || topY > GetGUIHeight - 2) throw new ArgumentOutOfRangeException(nameof(topY), "Y position is beyond buffer bounds.");
                if (startX + width > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(startX) + "', '" + nameof(width), "Too long.");
                // ...or if the specified size is too small for the element to be drawn properly
                if (width < 2 /*4*/) throw new ArgumentOutOfRangeException(nameof(width), "Length can't be less than 2.");

                topY++; // Push GUI down from line 0

                Console.BackgroundColor = bgColor ?? _guiColor;
                Console.ForegroundColor = textColor ?? _guiTextColor;

                int numberOfZags = width / 4;
                int remainder = width % 4;      // can be 0, 1, 2, or *3* (standard)
                bool midPointsUp = (numberOfZags % 2 == 0);

                string[] fragment = new string[2];
                if (flipped)
                {
                    fragment[0] = $"{_cornerBL[(int)borderStyle]}{_lineH[(int)borderStyle]}{_cornerBR[(int)borderStyle]} ";
                    fragment[1] = $"{_cornerTR[(int)borderStyle]} {_cornerTL[(int)borderStyle]}{_lineH[(int)borderStyle]}";
                }
                else {
                    fragment[0] = $"{_cornerTL[(int)borderStyle]}{_lineH[(int)borderStyle]}{_cornerTR[(int)borderStyle]} ";
                    fragment[1] = $"{_cornerBR[(int)borderStyle]} {_cornerBL[(int)borderStyle]}{_lineH[(int)borderStyle]}";
                }

                int lineIndex;
                string line1 = string.Empty, line2 = string.Empty;
                int fragmentIndex = 1;

                line1 += straightEdge ? _lineH[(int)borderStyle] : fragment[0][0];

                for (lineIndex = 1; lineIndex < width - 1; lineIndex++)
                {
                    if (lineIndex == width / 2 && remainder != 3)   // Special behavior to adjust the zigzag pattern at the halfway point
                    {
                        if (midPointsUp)
                            fragmentIndex++;

                        if (width > 2)
                        {
                            switch (remainder)
                            {
                                case 0:
                                    line1 += line1[^1];
                                    line2 += line2[^1];
                                    break;

                                case 1:
                                    line1 = line1[..^1];
                                    line2 = line2[..^1];

                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (fragmentIndex >= fragment[0].Length)
                                            fragmentIndex = 0;

                                        line1 += fragment[0][fragmentIndex];
                                        line2 += fragment[1][fragmentIndex];
                                        fragmentIndex++;
                                    }

                                    lineIndex++;
                                    fragmentIndex++;
                                    break;

                                case 2:
                                    if (!midPointsUp)
                                        fragmentIndex++;

                                    if (fragmentIndex >= fragment[0].Length)
                                        fragmentIndex = 0;

                                    line1 += fragment[0][fragmentIndex];
                                    line2 += fragment[1][fragmentIndex];
                                    fragmentIndex++;
                                    break;
                            }

                            continue;
                        }
                    }

                    // Regular loop behavior here
                    if (fragmentIndex >= fragment[0].Length)
                        fragmentIndex = 0;

                    line1 += fragment[0][fragmentIndex];
                    line2 += fragment[1][fragmentIndex];

                    fragmentIndex++;
                }

                line1 += straightEdge ? _lineH[(int)borderStyle] : fragment[0][2];

                Console.SetCursorPosition(startX, topY + (flipped ? 1 : 0));
                Console.Write(line1);

                Console.SetCursorPosition(startX + 1, topY + (flipped ? 0 : 1));
                Console.Write(line2);
            }
            catch (ArgumentException ex)
            {
                PrintInfo("GUI.DrawLine error: " + ex.Message);
            }
        }

        /// <summary>
        /// Draws a horizontal line
        /// </summary>
        /// <param name="startX">X position for the origin point</param>
        /// <param name="y">Y position for the line</param>
        /// <param name="width">Width of the line</param>
        /// <param name="borderStyle">Single or double-line border style</param>
        /// <param name="edgeStyleLeft">Sets the left-most char as a straight line or a junction</param>
        /// <param name="edgeStyleRight">Sets the right-most char as a straight line or a junction</param>
        /// <param name="bgColor">Background color</param>
        /// <param name="textColor">Foreground/text color</param>
        public static void DrawLine(int startX, int y, int width, BorderStyle borderStyle = 0,
            EdgeStyle edgeStyleLeft = EdgeStyle.None, EdgeStyle edgeStyleRight = EdgeStyle.None,
            ConsoleColor? bgColor = null, ConsoleColor? textColor = null)
        {
            try
            {
                // Throw an exception if a parameter value would lead to drawing outside the console buffer...
                if (startX < 0 || startX > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(startX), "Origin point is beyond buffer bounds.");
                if (y < 0 || y > GetGUIHeight) throw new ArgumentOutOfRangeException(nameof(y), "Y position is beyond buffer bounds.");
                if (startX + width > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(startX) + "', '" + nameof(width), "Too long.");
                // ...or if the specified size is too small for the element to be drawn properly
                if (width < 3) throw new ArgumentOutOfRangeException(nameof(width), "Length can't be less than 3 (<3).");

                y++; // Push GUI down from line 0

                Console.BackgroundColor = bgColor ?? _guiColor;
                Console.ForegroundColor = textColor ?? _guiTextColor;

                if (edgeStyleLeft == EdgeStyle.None)
                    CharAtPosition(_lineH[(int)borderStyle], startX, y);
                else
                    CharAtPosition(_cornerTL[(int)borderStyle + (int)edgeStyleLeft], startX, y);

                for (int i = startX + 1; i < startX + width - 1; i++)
                    Console.Write(_lineH[(int)borderStyle]);

                if (edgeStyleRight == EdgeStyle.None)
                    Console.Write(_lineH[(int)borderStyle]);
                else
                    Console.Write(_cornerTR[(int)borderStyle + (int)edgeStyleRight]);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                PrintInfo("GUI.DrawLine error: " + ex.Message);
            }
        }

        /// <summary>
        /// Draws a zigzag pattern as a vertical column
        /// </summary>
        /// <param name="leftX">X position for the left-hand side of the column</param>
        /// <param name="startY">Y position for the origin point</param>
        /// <param name="height">Height of the column</param>
        /// <param name="borderStyle">Single or double-line border style</param>
        /// <param name="zigzagStyle">Option to mirror the zigzag pattern</param>
        /// <param name="straightEdge">To be deprecated? (Whether or not the ends of the line are straight rather than corners)</param>
        /// <param name="bgColor">Background color</param>
        /// <param name="textColor">Foreground/text color</param>
        public static void DrawColumnZigzag(int leftX, int startY, int height, BorderStyle borderStyle = 0,
            bool straightEdge = false, bool mirrored = false,
            ConsoleColor ? bgColor = null, ConsoleColor? textColor = null)
        {
            try
            {
                // Throw an exception if a parameter value would lead to drawing outside the console buffer...
                if (startY < 0 || startY > GetGUIHeight) throw new ArgumentOutOfRangeException(nameof(startY), "Origin point is beyond buffer bounds.");
                if (leftX < 0 || leftX > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(leftX), "X position is beyond buffer bounds.");
                if ((mirrored && leftX < 2) || (!mirrored && leftX > GetGUIWidth - 3)) throw new ArgumentOutOfRangeException(nameof(leftX), "Column is partially out of horizontal buffer bounds.");
                if (startY + height > GetGUIHeight) throw new ArgumentOutOfRangeException(nameof(startY) + "', '" + nameof(height), "Too long.");
                // ...or if the specified size is too small for the element to be drawn properly
                if (height < 4) throw new ArgumentOutOfRangeException(nameof(height), "Length can't be less than 3 (<3).");

                startY++; // Push GUI down from line 0

                Console.BackgroundColor = bgColor ?? _guiColor;
                Console.ForegroundColor = textColor ?? _guiTextColor;

                int line;
                bool oddHeight = (height % 2 == 1);
                bool oddLine = false;

                string[] fragment = {
                    $"{_cornerTL[(int)borderStyle]}{_lineH[(int)borderStyle]}{_cornerBR[(int)borderStyle]}",
                    $"{_cornerBL[(int)borderStyle]}{_lineH[(int)borderStyle]}{_cornerTR[(int)borderStyle]}"
                };

                CharAtPosition(straightEdge ? _lineV[(int)borderStyle] : (mirrored ? fragment[1][2] : fragment[0][0]), leftX + (mirrored ? 2 : 0), startY);

                for (line = 0; line < (height - 2)/2; line++)
                {
                    oddLine = (line % 2 == 1);

                    Console.SetCursorPosition(leftX, line + startY + 1);
                    Console.Write(fragment[mirrored ^ oddLine ? 0 : 1]);
                }

                if (oddHeight)
                {
                    bool oddZags = ((height - 5) % 4 == 0);
                    Console.SetCursorPosition(leftX, line + startY + 1);
                    Console.Write((mirrored ^ oddZags ? "  " + _lineV[(int)borderStyle] : _lineV[(int)borderStyle] + "  "));
                    line++;
                }

                for (int i = line; line < height - 2; line++)
                {
                    oddLine = (line % 2 == 1);

                    Console.SetCursorPosition(leftX, line + startY + 1);
                    Console.Write(fragment[mirrored ^ (!oddLine ^ !oddHeight) ? 0 : 1]);
                }

                CharAtPosition(straightEdge ? _lineV[(int)borderStyle] : (mirrored ? fragment[0][2] : fragment[1][0]), leftX + (mirrored ? 2 : 0), startY + height - 1);
            }
            catch (ArgumentException ex)
            {
                PrintInfo("GUI.DrawColumn error: " + ex.Message);
            }

        }

        /// <summary>
        /// Draws a vertical column
        /// </summary>
        /// <param name="x">X position for the line</param>
        /// <param name="startY">Y position for the origin point</param>
        /// <param name="height">Height of the line</param>
        /// <param name="borderStyle">Single or double-line border style</param>
        /// <param name="edgeStyleTop">Sets the top-most char as a straight line or a junction</param>
        /// <param name="edgeStyleBottom">Sets the bottom-most char as a straight line or a junction</param>
        /// <param name="bgColor">Background color</param>
        /// <param name="textColor">Foreground/text color</param>
        public static void DrawColumn(int x, int startY, int height, BorderStyle borderStyle = 0,
            EdgeStyle edgeStyleTop = EdgeStyle.None, EdgeStyle edgeStyleBottom = EdgeStyle.None,
            ConsoleColor? bgColor = null, ConsoleColor? textColor = null)
        {
            try
            {
                // Throw an exception if a parameter value would lead to drawing outside the console buffer...
                if (startY < 0 || startY > GetGUIHeight) throw new ArgumentOutOfRangeException(nameof(startY), "Origin point is beyond buffer bounds.");
                if (x < 0 || x > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(x), "X position is beyond buffer bounds.");
                if (startY + height > GetGUIHeight) throw new ArgumentOutOfRangeException(nameof(startY) + "', '" + nameof(height), "Too long.");
                // ...or if the specified size is too small for the element to be drawn properly
                if (height < 3) throw new ArgumentOutOfRangeException(nameof(height), "Length can't be less than 3 (<3).");

                startY++; // Push GUI down from line 0

                Console.BackgroundColor = bgColor ?? _guiColor;
                Console.ForegroundColor = textColor ?? _guiTextColor;

                if (edgeStyleTop == EdgeStyle.None)
                    CharAtPosition(_lineV[(int)borderStyle], x, startY);
                else
                    CharAtPosition(_cornerTL[(int)borderStyle + (int)edgeStyleTop], x, startY);
                
                for (int i = startY + 1; i < startY + height - 1; i++)
                    CharAtPosition(_lineV[(int)borderStyle], x, i);

                if (edgeStyleBottom == EdgeStyle.None)
                    CharAtPosition(_lineV[(int)borderStyle], x, startY + height - 1);
                else
                    CharAtPosition(_cornerBL[(int)borderStyle + (int)edgeStyleBottom], x, startY + height - 1);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                PrintInfo("GUI.DrawColumn error: " + ex.Message);
            }
        }

        /// <summary>
        /// Draws a content box
        /// </summary>
        /// <param name="left">X coordinate for the left-hand side of the box</param>
        /// <param name="top">Y coordinate for the top of the box</param>
        /// <param name="width">Width of the box</param>
        /// <param name="height">Height of the box</param>
        /// <param name="borderStyle">Single or double-line border style</param>
        /// <param name="cornerStyleTL">Sets the top-left char as a corner or a junction</param>
        /// <param name="cornerStyleTR">Sets the top-right char as a corner or a junction</param>
        /// <param name="cornerStyleBL">Sets the bottom-left char as a corner or a junction</param>
        /// <param name="cornerStyleBR">Sets the bottom-right char as a corner or a junction</param>
        /// <param name="bgColor">Background color</param>
        /// <param name="textColor">Foreground/text color</param>
        public static void DrawBox(int left, int top, int width, int height, BorderStyle borderStyle = 0,
            CornerStyle cornerStyleTL = 0, CornerStyle cornerStyleTR = 0, CornerStyle cornerStyleBL = 0, CornerStyle cornerStyleBR = 0,
            ConsoleColor? bgColor = null, ConsoleColor? textColor = null)
        {
            try
            {
                // Throw an exception if a parameter value would lead to drawing outside the console buffer...
                if (left < 0 || left > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(left), "Origin point is beyond horizontal buffer bounds.");
                if (top < 0 || top > GetGUIHeight) throw new ArgumentOutOfRangeException(nameof(top), "Origin point is beyond vertical buffer bounds.");
                if (left + width > GetGUIWidth) throw new ArgumentOutOfRangeException(nameof(left) + "', '" + nameof(width), "Too wide.");
                if (top + height > GetGUIHeight) throw new ArgumentOutOfRangeException(nameof(top) + "', '" + nameof(height), "Too tall.");
                // ...or if the specified size is too small for the element to be drawn properly
                if (width < 2 || height < 2) throw new ArgumentOutOfRangeException(nameof(width) + ", " + nameof(height), "Can't be smaller than 2 by 2.");

                top++; // Push GUI down from line 0

                Console.BackgroundColor = bgColor ?? _guiColor;
                Console.ForegroundColor = textColor ?? _guiTextColor;

                // Construct the horizontal lines here so you don't have to do it multiple times
                string backgroundFiller = " ";
                string horizontalLine = string.Empty;
                string horizontalChar = _lineH[(int)borderStyle].ToString();

                for (int i = left + 1; i < left + width - 1; i++)
                {
                    backgroundFiller += " ";
                    horizontalLine += horizontalChar;
                }

                // Write the top line of the box, including corners
                CharAtPosition(_cornerTL[(int)borderStyle + (int)cornerStyleTL], left, top);
                Console.Write(horizontalLine);
                Console.Write(_cornerTR[(int)borderStyle + (int)cornerStyleTR]);

                // Write the vertical lines of the box
                for (int i = top + 1; i < top + height - 1; i++)
                {
                    CharAtPosition(_lineV[(int)borderStyle], left, i);

                    if (bgColor != null)
                    {
                        Console.Write(backgroundFiller);
                    }

                    CharAtPosition(_lineV[(int)borderStyle], left + width - 1, i);
                }

                // Write the bottom line of the box, including corners
                CharAtPosition(_cornerBL[(int)borderStyle + (int)cornerStyleBL], left, top + height - 1);
                Console.Write(horizontalLine);
                Console.Write(_cornerBR[(int)borderStyle + (int)cornerStyleBR]);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                PrintInfo("GUI.DrawBox error: " + ex.Message);
            }
        }
    }
}
