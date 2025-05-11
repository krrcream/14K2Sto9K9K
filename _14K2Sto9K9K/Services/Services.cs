using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WpfApp2;
using System.Linq;
using System.Text;

namespace _14k2sTO9K9K.Services
{
    public class Services
    {
        private ProgressWindow _progressWindow;
        private int _totalFiles;
        private int _processedFiles;
        private SemaphoreSlim _semaphore;
        private readonly OsuProcess _osuProcess;
        public Services(OsuProcess osuProcess)
        {
            _osuProcess = osuProcess;
            int maxDegreeOfParallelism = Environment.ProcessorCount * 4;
            _semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
            Console.OutputEncoding = Encoding.UTF8;
        }

        public async Task ProcessFilesAsync(string[] files)
        {
            _progressWindow = new ProgressWindow();
            _progressWindow.Show();

            _totalFiles = CountFiles(files);
            _progressWindow.UpdateTotalFiles(_totalFiles);

            var tasks = new List<Task>();
            foreach (var file in files)
            {
                if (File.Exists(file) && Path.GetExtension(file) == ".osu")
                {
                    tasks.Add(ProcessFileWithSemaphoreAsync(file));
                }
                else if (Directory.Exists(file))
                {
                    tasks.Add(ProcessDirectoryAsync(file));
                }
            }

            await Task.WhenAll(tasks);
            
      
            
            // ---------
            
            _progressWindow.Close();
            Console.WriteLine($"文件处理完成，共处理了 {_processedFiles} 个文件。");
        }

        private async Task ProcessFileWithSemaphoreAsync(string file)
        {
            await _semaphore.WaitAsync();
            try
            {
                await ProcessFileAsync(file);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ProcessFileAsync(string file)
        {
            await _osuProcess.MainProcess(file); // 调用 MainProcess
            _processedFiles++;
            _progressWindow.UpdateProcessedFiles(_processedFiles);
            Console.WriteLine($"已处理文件: {file}");
        }

        private async Task ProcessDirectoryAsync(string directory)
        {
            var files = Directory.GetFiles(directory, "*.osu", SearchOption.AllDirectories);
            var tasks = files.Select(f => ProcessFileWithSemaphoreAsync(f));
            await Task.WhenAll(tasks);
        }

        
        private int CountFiles(string[] paths)
        {
            return paths.Sum(path => 
                File.Exists(path) && Path.GetExtension(path) == ".osu" ? 1 :
                Directory.Exists(path) ? Directory.GetFiles(path, "*.osu", SearchOption.AllDirectories).Length : 0
            );
        }

    }
}