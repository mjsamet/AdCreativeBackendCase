namespace AdCreativeBackendCase;

public class ImageDownloader
{
    private int _numberOfImagesToDownload = 0;
    private int _maxParallelDownloadLimit = 0;
    private int _downloadedNumberOfImages = 0;
    private int _progressNumberOfImagess = 0;
    private string _savePath = null;
    private readonly HttpClient _client = new();
    private object lockDownloadednumberOfImages = new();
    private object lockStartNewTask = new();
    private bool exit = false;


    public delegate void DownloadedImageHandler(ImageDownloader sender, int downloadedNumberOfImages);
    public delegate void DownloadFinishedHandler();

    public event DownloadedImageHandler DownloadedImage;
    public event DownloadFinishedHandler DownloadFinished;

    public ImageDownloader(int numberOfImagesToDownload, int maxParallelDownloadLimit, string savePath)
    {
        _numberOfImagesToDownload = numberOfImagesToDownload;
        _maxParallelDownloadLimit = maxParallelDownloadLimit;
        _savePath = savePath;
    }

    public int NumberOfImagesToDownload => _numberOfImagesToDownload;

    public int MaxParallelDownloadLimit => _maxParallelDownloadLimit;

    async Task DownloadImage(int order)
    {
        var response = await _client.GetAsync("https://picsum.photos/200/300");
        var filePath = Path.Combine(_savePath, order + ".jpg");
        FileStream fs = new FileStream(filePath, FileMode.Create);

        await fs.WriteAsync(await response.Content.ReadAsByteArrayAsync());
        bool fireNext;
        lock (lockDownloadednumberOfImages)
        {
            _downloadedNumberOfImages++;
            fireNext = order < _numberOfImagesToDownload - _maxParallelDownloadLimit + 1;
        }

        if (DownloadedImage != null)
            DownloadedImage(this, _downloadedNumberOfImages);
        if (exit)
        {
            File.Delete(filePath);
            return;
        }
        if (fireNext)
            DownloadNextImages();
        if (order == _numberOfImagesToDownload && DownloadFinished != null)
            DownloadFinished();

    }

    void DownloadNextImages()
    {
        if (exit)
            return;
        Task.Factory.StartNew(async () =>
        {
            var order = 0;
            lock (lockStartNewTask)
            {
                order = ++_progressNumberOfImagess;
            }

            await DownloadImage(order);
        });
    }

    public void Start()
    {
        Task.Factory.StartNew(() =>
        {
            for (int i = 0; i < _maxParallelDownloadLimit; i++)
            {
                var order = i + 1;
                Task.Factory.StartNew(async () => { await DownloadImage(order); });
            }

            _progressNumberOfImagess = _maxParallelDownloadLimit;
        });
    }

    public void Stop()
    {
        exit = true;
        for (int i = 1; i <= _downloadedNumberOfImages; i++)
        {
            var filePath = Path.Combine(_savePath, i + ".jpg");
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}