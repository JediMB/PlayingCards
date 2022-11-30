
namespace PlayingCardsApp
{
    public sealed class Data
    {
        private const string configFilename = @"config.ini";
        private const string cardASCIIFilename = @".\data\cards.ascii";

        public string Name { get; init; } = "Playing Cards!";
        public string Tagline { get; init; } = "The odds will betray you, and I will replace you";
        public string Description { get; init; } = "A console application featuring games with traditional playing cards.";
        public string Logo { get; init; } =
            "    /////   //        ////   //  //  //  //    //   /////     /////   ////   /////   /////    /////   //\n" +
            "   //  //  //       //  //  //  //  //  ////  //  //        //      //  //  //  //  //  //  //       //\n" +
            "  /////   //       //////   ////   //  // // //  //  ///   //      //////  /////   //  //   ////    //\n" +
            " //      //       //  //    //    //  //  ////  //   //   //      //  //  // //   //  //      //     \n" +
            "//      ///////  //  //    //    //  //    //   /////     /////  //  //  //  //  /////   /////    //";
        public string Copyright { get; init; } = "(c) Copyright 2022, M Berntson";

        public string[,] cardASCII;

        #region Singleton Instantiation
        public static readonly Data instance = new();
        static Data() { }
        public static Data Instance => instance;
        #endregion

        #region Data Initialization
        private Data()
        {
            string line;

            // External data is imported through here
            if (File.Exists(configFilename))
            {
                using StreamReader configReader = new(configFilename);

                while ((line = configReader.ReadLine() ?? string.Empty) != string.Empty)
                {
                    if (line.Contains('=') is not true)
                        continue;

                    string key = line.Split('=')[0];
                    string value = line.Split('=')[1];

                    switch (key.ToLower())
                    {
                        case "name":
                            Name = value;
                            break;

                        case "tagline":
                            Tagline = value;
                            break;

                        case "description":
                            Description = value;
                            break;

                        case "logo":
                            Logo = value.Replace(@"\n", "\n");
                            break;

                        case "copyright":
                            Copyright = value;
                            break;
                    }
                }
            }
            else
            {
                string newConfigText =
                    "name=" + Name + "\n" +
                    "tagline=" + Tagline + "\n" +
                    "description=" + Description + "\n" +
                    "logo=" + Logo.Replace("\n", @"\n") + "\n" +
                    "copyright=" + Copyright;

                File.WriteAllText(configFilename, newConfigText);
            }

            cardASCII = new string[4, 13];

            if(!File.Exists(cardASCIIFilename))
                throw new FileNotFoundException(cardASCIIFilename);

            using StreamReader asciiReader = new(cardASCIIFilename);

            for (int suit = 0; suit < 4; suit++)
            {
                for (int srcLine = 0; srcLine < 6; srcLine++)
                {
                    line = asciiReader.ReadLine() ?? string.Empty;

                    int cardWidth = line.Length / 13;

                    for (int rank = 0; rank < 13; rank++)
                        cardASCII[suit, rank] += line.Substring(rank * cardWidth, cardWidth) + (srcLine == 5 ? "" : "\n");
                }
            }
        }
        #endregion
    }
}
