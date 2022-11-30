using System.Diagnostics.CodeAnalysis;
using ConsoleGUI;
using static PlayingCardsApp.PlayingCards;

namespace PlayingCardsApp
{
    [SupportedOSPlatform("windows")]

    public sealed partial class Logic
    {
        private enum MenuState
        {
            MainMenu,
            Blackjack,
            PlayingBlackjack
        }

        private bool contentLoaded = false;
        private bool contentCreated = false;
        private bool guiDrawn = false;
        private bool quit = false;

        private MenuState menuState = MenuState.MainMenu;
        private bool menuLineSelected = false;
        private string menuLine = string.Empty;

        private Data data;

        private GUI.TextBoxGroup labelBoxes;
        private GUI.TextBoxGroup menuBoxes;
        private GUI.TextBoxGroup scrollBoxes;

        private GUI.TextBoxGroup cardsPlayer;
        
        #region Singleton Instantiation
        private static readonly Logic instance = new();
        static Logic() { }
        public static Logic Instance => instance;
        #endregion

        #region Logic Initialization
        private Logic()
        {
            // Initialization of app logic here
            LoadContent();
            GUI.Initialize(data.Name);

            DrawGUI();

            CreateContent();

            menuBoxes.RefreshTextbox();
        }

        /// <summary>
        /// Loads any necessary data from external files
        /// </summary>
        [MemberNotNull(nameof(data))]
        private void LoadContent()
        {
            if (contentLoaded)
                throw new Exception("Tried to run LoadContent more than once.");

            contentLoaded = true;

            data = Data.Instance;
        }

        /// <summary>
        /// Draws static/non-updating GUI elements such as boxes, lines, and columns.
        /// </summary>
        private void DrawGUI()
        {
            if (guiDrawn)
                GUI.LogBox.Print("GUI refreshed.");

            guiDrawn = true;

            // Draw static GUI elements here

            GUI.DrawBox(left:0, top:0, width:GUI.GetGUIWidth, height:9, GUI.BorderStyle.Double);
            GUI.DrawLine(startX:1, y:6, width:GUI.GetGUIWidth-2);

            GUI.DrawBox(0, 9, GUI.GetGUIWidth, 15, GUI.BorderStyle.Single, 0, 0, 0, 0, ConsoleColor.Gray, ConsoleColor.Black);
            GUI.DrawLine(0, 16, GUI.GetGUIWidth, GUI.BorderStyle.Single, GUI.EdgeStyle.VerticalJunction, GUI.EdgeStyle.VerticalJunction, ConsoleColor.Gray, ConsoleColor.Black);
            
            GUI.DrawBox(0, 24, GUI.GetGUIWidth, 17, GUI.BorderStyle.Single);
            GUI.DrawColumn(17, 24, 17, GUI.BorderStyle.Single, GUI.EdgeStyle.HorizontalJunction, GUI.EdgeStyle.HorizontalJunction);
           
            GUI.DrawBox(left: 0, top: GUI.GetGUIHeight - 7, width: GUI.GetGUIWidth, height: 7, GUI.BorderStyle.Double);
        }

        /// <summary>
        /// Creates static text fields and interactable, scrolling text boxes on top of previously drawn GUI elements.
        /// </summary>
        [MemberNotNull(nameof(labelBoxes), nameof(menuBoxes), nameof(scrollBoxes), nameof(cardsPlayer))]
        private void CreateContent()
        {
            if (contentCreated)
                throw new Exception("Tried to run CreateContent more than once.");

            contentCreated = true;

            labelBoxes = GUI.TextBoxGroup.Factory.CreateLabelGroup();
            menuBoxes = GUI.TextBoxGroup.Factory.CreateSelectableGroup();
            scrollBoxes = GUI.TextBoxGroup.Factory.CreateScrollingGroup();

            cardsPlayer = GUI.TextBoxGroup.Factory.CreateLabelSequence(17, true, 3, 10, 7, 6, ConsoleColor.Gray, ConsoleColor.Black);

            // Create textboxes here

            labelBoxes.Add(12, 1, 104, 5, data.Logo, null, ConsoleColor.Cyan);
            labelBoxes.Add(GUI.GetGUIWidth/2 - data.Tagline.Length/2, 7, data.Tagline.Length, 1, data.Tagline);

            labelBoxes.Add(2, 9, 8, 1, " PLAYER ", ConsoleColor.Gray, ConsoleColor.Black);

            int suit = 0;
            for (int i = 0; i < cardsPlayer.Count; i++)
            {
                if (suit == 0 || suit == 3)
                    cardsPlayer[i].TextColor = ConsoleColor.Black;
                else
                    cardsPlayer[i].TextColor = ConsoleColor.DarkRed;

                cardsPlayer[i].Text = data.cardASCII[suit, 0];
                
                suit++;
                if (suit > 3) suit = 0;
            }

            //labelBoxes.Add(3, 10, 7, 6, data.cardASCII[0, 0], ConsoleColor.Gray, ConsoleColor.Black);
            //labelBoxes.Add(10, 10, 7, 6, data.cardASCII[1, 0], ConsoleColor.Gray, ConsoleColor.DarkRed);
            //labelBoxes.Add(17, 10, 7, 6, data.cardASCII[2, 0], ConsoleColor.Gray, ConsoleColor.DarkRed);
            //labelBoxes.Add(24, 10, 7, 6, data.cardASCII[3, 0], ConsoleColor.Gray, ConsoleColor.Black);

            labelBoxes.Add(2, 16, 11, 1, " THE HOUSE ", ConsoleColor.Gray, ConsoleColor.Black);

            menuBoxes.Add(1, 25, 16, 15, "New game");

            scrollBoxes.Add(18, 25, GUI.GetGUIWidth - 19, 15, "Would you like to play a game?");

            labelBoxes.Add(2, GUI.GetGUIHeight - 5, GUI.GetGUIWidth - 4, 3, data.Description + "\n\n" + data.Copyright);
        }
        #endregion

        #region Logic Loop
        public void RunLoop()
        {
            while (quit is not true)
            {
                WriteMenuText();

                Input();

                MenuResponse();
            }
        }

        private void WriteMenuText()
        {
            switch (menuState)
            {
                case MenuState.MainMenu:
                    menuBoxes.WriteTextbox("Blackjack\nPoker\nQuit");
                    break;

                case MenuState.Blackjack:
                    menuBoxes.WriteTextbox("Play\nHigh score\nMain menu\nQuit");
                    break;

                case MenuState.PlayingBlackjack:
                    menuBoxes.WriteTextbox("Hit me?");
                    break;
            }
        }

        private void MenuResponse()
        {
            if (!menuLineSelected)
                return;

            switch (menuLine)
            {
                case "Main menu":
                    menuState = MenuState.MainMenu;
                    return;

                case "Quit":
                    quit = true;
                    return;
            }

            switch (menuState)
            {
                case MenuState.MainMenu:
                    switch (menuLine)
                    {
                        case "Blackjack":
                            menuState = MenuState.Blackjack;
                            return;

                        default:
                            GUI.LogBox.Print($"Unknown menu command: {menuLine}");
                            break;
                    }
                    return;

                case MenuState.Blackjack:
                    switch (menuLine)
                    {
                        case "Play":
                            menuState = MenuState.PlayingBlackjack;
                            return;

                        case "High score":
                            return;

                        default:
                            GUI.LogBox.Print($"Unknown menu command: {menuLine}");
                            break;
                    }
                    return;
            }
        }

        /// <summary>
        /// Handles all user input.
        /// </summary>
        private void Input()
        {
            menuLineSelected = false;

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.Q:              quit = true; break;

                case ConsoleKey.PageUp:         GUI.LogBox.ScrollUp(); break;
                case ConsoleKey.PageDown:       GUI.LogBox.ScrollDown(); break;
                case ConsoleKey.UpArrow:        menuBoxes.PrevSelection(); break;
                case ConsoleKey.DownArrow:      menuBoxes.NextSelection(); break;
                case ConsoleKey.LeftArrow:      menuBoxes.PrevTextbox(); break;
                case ConsoleKey.RightArrow:     menuBoxes.NextTextbox(); break;
                case ConsoleKey.Enter:          MenuSelection(); break;
                //case ConsoleKey.Delete:         GUI.Controls.ClearTextbox(); break;
                //case ConsoleKey.Insert:         GUI.Controls.InsertIntoTextbox("Here comes a new challenger!"); break;
            }
        }

        private void MenuSelection()
        {
            menuLine = menuBoxes.ReadSelection();

            if (menuLine != string.Empty)
                menuLineSelected = true;
        }
        #endregion
    }
}
