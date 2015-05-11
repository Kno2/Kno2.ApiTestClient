using System;

namespace Kno2.ApiTestClient.Helpers
{
    public static class ConsoleHelper
    {
        public static string Header = " -------------------------------------------------------------------------------------------------------";

        public static void HeaderLine(bool extraLineFeed = false)
        {
            System.Console.WriteLine(Header);
            if (extraLineFeed)
                System.Console.WriteLine();
        }

        public static void HeaderLine(ConsoleColor color)
        {
            var current = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(Header);
            System.Console.ForegroundColor = current;
        }

        public static void AsBanner(this string message)
        {
            HeaderLine();
            System.Console.WriteLine("  " + message);
            HeaderLine();
        }

        public static void AsBanner(this string message, ConsoleColor color, bool previousLineFeed = false, bool resetColor = true)
        {
            if (previousLineFeed)
                Console.WriteLine();
            var current = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            AsBanner(message);
            if (resetColor)
                System.Console.ResetColor();
        }

        public static void AsOpeningBanner(this string message)
        {
            HeaderLine();
            System.Console.WriteLine("  " + message);
        }

        public static void AsInlineBanner(this string message, ConsoleColor color)
        {
            message.AsOpeningBanner(color, false, true, true, false);
        }

        public static void AsOpeningBanner(this string message, ConsoleColor color, bool resetColor = true, bool closeBanner = true, bool closePreviousBanner = false, bool extraLineFeed = true)
        {
            if (closePreviousBanner)
            {
                HeaderLine();
                Console.WriteLine();
            }

            var current = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            HeaderLine();
            System.Console.WriteLine("  " + message);
            if (closeBanner)
                HeaderLine();
            if (resetColor)
                System.Console.ForegroundColor = current;
            if (extraLineFeed)
                System.Console.WriteLine();
        }

        public static void AsClosingBanner(this string message)
        {
            System.Console.WriteLine("  " + message);
            HeaderLine();
        }

        public static void AsClosingBanner(this string message, ConsoleColor color)
        {
            var current = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine("  " + message);
            HeaderLine();
            System.Console.ForegroundColor = current;
        }

        public static void ToConsole(this string message)
        {
            System.Console.WriteLine("  " + message);
        }

        public static void ToConsole(this string message, ConsoleColor color, bool trailingReturn = true)
        {
            var current = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            if (trailingReturn)
                System.Console.WriteLine("  " + message);
            else
                System.Console.Write("  " + message);
            System.Console.ForegroundColor = current;
        }
    }
}