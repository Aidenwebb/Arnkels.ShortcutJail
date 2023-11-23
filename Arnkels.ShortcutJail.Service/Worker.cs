namespace Arnkels.ShortcutJail.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var shortcutExtensions = new [] {".lnk", ".url"}; 
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
            
            // Check for and Create Jail folder if not exists
            
            string userDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string shortcutJailPath = Path.Join(userDesktopPath, "ShortcutJail");

            if (!Directory.Exists(shortcutJailPath))
            {
                Directory.CreateDirectory(shortcutJailPath);
            }
            
            _logger.LogInformation(shortcutJailPath);
            
            // Check user Desktop for shortcut files

            var shortcutFiles = Directory
                .GetFiles(userDesktopPath)
                .Where(file => shortcutExtensions.Any(file.ToLower().EndsWith))
                .ToList();

            // Move shortcut files to jail
            foreach (var shortcutFile in shortcutFiles)
            {
                var shortcutFileName = Path.GetFileName(shortcutFile);
                var shortcutDestination = Path.Join(shortcutJailPath, shortcutFileName);
                _logger.LogInformation($"Shortcut filename: {shortcutFileName}");
                if (File.Exists(shortcutDestination))
                {
                    var newShortcutFileName = string.Concat(Path.GetFileNameWithoutExtension(shortcutFile), "_",
                        DateTime.Now.ToFileTime(), Path.GetExtension(shortcutFile));
                    shortcutDestination = Path.Join(shortcutJailPath, newShortcutFileName);
                    
                    _logger.LogInformation($"Shortcut filename: {shortcutFileName}");
                }
                File.Move(shortcutFile, shortcutDestination);
                _logger.LogInformation(shortcutFile);
            }
        }
    }
}
