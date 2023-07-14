namespace AdCreativeBackendCase;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter the number of images to download:");
        if (!int.TryParse(Console.ReadLine(), out var numberOfImagesToDownload))
        {
            Console.WriteLine("Wrong format");
            return;
        }

        Console.Write("Enter the maximum parallel download limit:");
        if (!int.TryParse(Console.ReadLine(), out var maxParallelDownloadLimit))
        {
            Console.WriteLine("Wrong format");
            return;
        }

        if (maxParallelDownloadLimit <= 0)
        {
            Console.WriteLine("Parallel download limit must be greater than zero");
            return;
        }

        if (maxParallelDownloadLimit > numberOfImagesToDownload)
        {
            Console.WriteLine("Parallel download limit must be lower than number of images to download");
            return;
        }

        Console.Write("Enter the save path (default: ./outputs):");
        var temp = Console.ReadLine();
        var savePath = string.IsNullOrWhiteSpace(temp) ? "./outputs" : temp;
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        var clearLine = "".PadRight(Console.BufferWidth);

        var lastLineForProgress = Console.CursorTop;
        Console.CursorVisible = false;

        object lockScreen = new();
        ImageDownloader downloader = new ImageDownloader(numberOfImagesToDownload, maxParallelDownloadLimit, savePath);
        downloader.DownloadedImage += (sender, progress) =>
        {
            lock (lockScreen)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = lastLineForProgress;
                Console.WriteLine(clearLine);
                Console.WriteLine(clearLine);
                Console.WriteLine(clearLine);
                Console.CursorLeft = 0;
                Console.CursorTop = lastLineForProgress;
                Console.WriteLine(
                    $"Downloading {sender.NumberOfImagesToDownload} images ({sender.MaxParallelDownloadLimit} parallel downloads at most)");
                Console.WriteLine();
                Console.WriteLine($"Progress: {progress}/{sender.NumberOfImagesToDownload}");
            }
        };
        downloader.DownloadFinished += () =>
        {
            Console.WriteLine();
            Console.Write("Download finished...");
            Environment.Exit(0);
        };

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Stoping and deleting...");
            downloader.Stop();
            Environment.Exit(0);
        };
        
        downloader.Start();
        
        while (true)
        {
            if (!Console.KeyAvailable)
            {
                
            }
        }
    }
}