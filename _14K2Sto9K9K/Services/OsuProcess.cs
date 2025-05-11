using System;
using System.IO;
using System.Numerics;
using System.Linq; 
using System.Threading.Tasks;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects.Mania;


namespace _14k2sTO9K9K.Services
{

    // OsuFileReader 接口
    public interface IOsuFileReader
    {
        Task<Beatmap> ReadAsync(string filePath);
    }

// OsuFileProcessor 接口（原 OsuFileGenerator）
    public interface IOsuFileProcessor
    {
        Task ProcessAsync(Beatmap beatmap, string filePath);
    }

    public class OsuProcess
    {
        private readonly IOsuFileReader _fileReader;
        private readonly IOsuFileProcessor _fileProcessor;

        // 通过构造函数注入依赖
        public OsuProcess(IOsuFileReader fileReader, IOsuFileProcessor fileProcessor)
        {
            _fileReader = fileReader;
            _fileProcessor = fileProcessor;
        }

        // 处理单个文件的 MainProcess 方法
        public async Task MainProcess(string filePath)
        {
            try
            {
                var beatmap = await _fileReader.ReadAsync(filePath);
                await _fileProcessor.ProcessAsync(beatmap, filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理文件 {filePath} 失败: {ex.Message}");
            }
        }

        // 批量处理多个文件的方法（可选）
        public async Task ProcessFilesAsync(IEnumerable<string> filePaths)
        {
            var tasks = filePaths.Select(file => MainProcess(file));
            await Task.WhenAll(tasks);
        }

    }
}   