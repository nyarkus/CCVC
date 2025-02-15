using CCVC.Encoder;
using CCVC.Players;

namespace CCVC;

public class Program
{
    internal static int Counter = 0;
    internal static object Locker = new object();
    public static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("commands:");
            Console.WriteLine("convert");
            Console.WriteLine("play");

            switch (Console.ReadLine())
            {
                case "convert":
                    Console.Clear();
                    Convert();
                    Console.Clear();
                    continue;
                case "play":
                    Console.Clear();
                    PlayInConsole();
                    Console.Clear();
                    continue;
                default:
                    Console.WriteLine("Unknown command");
                    continue;
            }
        }
    }
    
    private static int ReadInt(string text, int max = int.MaxValue)
    {
        while (true)
        {
            Console.WriteLine(text);
            if(int.TryParse(Console.ReadLine(), out var value))
            {
                if (value > max)
                    continue;
                return value;
            }
        }
    }

    public static void Convert()
    {
        Console.WriteLine("Enter a source video:");
        string source = Console.ReadLine().Trim('"');

        byte colors = (byte)ReadInt($"How many colors do you want to use? (max is {byte.MaxValue})");
        int width = ReadInt($"Specify the width of the video (in number of characters. Your max is {ConsolePlayer.GetMaxWidth()})");
        int height = ReadInt($"Specify the heigh of the video (in number of characters. Your max is {ConsolePlayer.GetMaxHeight()})");

        Console.WriteLine("Where to save the .ccv file?");
        string output = Console.ReadLine().Trim('"');
        output = Path.ChangeExtension(output, ".ccv");

        Console.WriteLine("Converting...");
        var video = Converter.ConvertFromVideo(source, width, height, colors);

        Console.WriteLine("Saving...");
        video.Save(output);
    }
    public static void PlayInConsole()
    {
        Console.WriteLine("Enter a path to .ccv file:");
        string source = Console.ReadLine().Trim('"');

        var video = CVideo.Load(source);

        Console.Beep();
        Console.WriteLine("Ready to play! Press enter to start playing video");
        Console.ReadLine();
        ConsolePlayer.Play(video);
        GC.Collect();
    }
}