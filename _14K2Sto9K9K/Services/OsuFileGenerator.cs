using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Decoders;
using System.Text.Json;

namespace _14k2sTO9K9K.Services
{
    public class OsuFileGenerator : IOsuFileProcessor
{
    private static readonly object _saveLock = new object();
    // 主过程
    public async Task ProcessAsync(Beatmap beatmap, string filePath)
    {

        int keys = (int)beatmap.DifficultySection.CircleSize;
        
        if (keys == 14 || keys == 16)
        {
            // 并行处理 HitObjects（后台线程）
            await Task.Run(() => ProcessHitObjects(beatmap, keys));
            
        }
        savetoorgdir(beatmap, filePath);
    }   
    
    private void savetoorgdir(Beatmap beatmap, string filePath)
    {
        UpdateMetadataORGdir(beatmap);
        // 生成新文件路径（原目录）
        string newFilePath = GenerateNewFilePath(beatmap, filePath);
        beatmap.Save(newFilePath);
    }
    

    private void UpdateMetadataORGdir(Beatmap beatmap)
    {
        beatmap.DifficultySection.CircleSize = 18;
        beatmap.MetadataSection.Version = "[9K9K conv.]"+beatmap.MetadataSection.Version ;
        beatmap.MetadataSection.Creator = "krr conv.";
        beatmap.MetadataSection.Tags.Concat(new[] {"krrcream converter 9k9k"});
    }
    
    


    private void sortHitObjects(Beatmap beatmap)
    {
        beatmap.HitObjects.Sort((a, b) => { int timeCompare = a.StartTime.CompareTo(b.StartTime); return timeCompare != 0 ? timeCompare : a.Position.X.CompareTo(b.Position.X); }); 
    }
    
    private void ProcessHitObjects(Beatmap beatmap, int keys)
    {
        sortHitObjects(beatmap);   
        
        int currentEndingtime = 0;
        PingPongCounter leftiter = new PingPongCounter(8);
        PingPongCounter rightiter = new PingPongCounter(8);
        int convertNum = 32;
        int convertFlag = 0;
        int[] convertArray = { leftiter.Current, rightiter.Current };
        
       
        
        for (int i = 0; i < beatmap.HitObjects.Count; i++)
        {
            currentEndingtime = Math.Max(currentEndingtime, beatmap.HitObjects[0].EndTime);
            Vector2 position = beatmap.HitObjects[i].Position;
            if (convertFlag >= convertNum && beatmap.HitObjects[i].StartTime - 80 > currentEndingtime)
            {
                convertFlag = 0;
                leftiter.MoveNext();
                rightiter.MoveNext();
                convertArray[0] = leftiter.Current;
                convertArray[1] = rightiter.Current;
            }
         
            convertFlag += 1;
            
            position.X = Function.ConvertColumn(convertArray, (int)position.X, keys);
            beatmap.HitObjects[i].Position = position;
        }

        
        
    }

    private string GenerateFilename(Beatmap beatmap)
    {
        string sanitizedName = SanitizeFileName(
            $"{beatmap.MetadataSection.Artist} - {beatmap.MetadataSection.Title} ({beatmap.MetadataSection.Creator}) [{beatmap.MetadataSection.Version}]"
        );
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            sanitizedName = sanitizedName.Replace(invalidChar.ToString(), "");
        }
        return sanitizedName;
    }
    
    private string GenerateNewFilePath(Beatmap beatmap, string originalPath)
    {
        string sanitizedName = GenerateFilename(beatmap);

        return Path.Combine(
            Path.GetDirectoryName(originalPath),
            $"{sanitizedName}.osu"
        );
    }
    
    
    private string SanitizeFileName(string unsafeName)
    {
        return new string(
            unsafeName.Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                .ToArray()
        );
    }

    private async Task SaveAsync(Beatmap beatmap, string filePath)
    {
        lock (_saveLock)
        {
            beatmap.Save(filePath);
        }
    }
}
    
    // 迭代器
    public class PingPongCounter
    {
        private int _current;
        private readonly int _max;
        private bool _isIncreasing;

        public PingPongCounter(int max)
        {
            _max = max;
            var random = new Random();
            _current = random.Next(0, _max);
            _isIncreasing = random.Next(2) == 1;
        }

        public int Current => _current;

        public void MoveNext()
        {
            if (_isIncreasing)
            {
                _current++;
                if (_current >= _max)
                {
                    _current = _max - 2;
                    _isIncreasing = false;
                }
            }
            else
            {
                _current--;
                if (_current <= 0)
                {
                    _current = 0;
                    _isIncreasing = true;
                }
            }
        }
    }
    
    
    
    
    public static class Function
    {
        public static int KeyValueToColunm(double value, int keys)
        {
            int result;
            if (keys == 14)
            {
                result = (int)Math.Floor(value * 14 / 512.0)+2;
                return result;
            }
            else 
            {
                result = (int)Math.Floor(value * 16 / 512.0)+1;
                return result;
            }
        }
        public static int KeyValueToColunmGeneral(double value, int keys)
        {
            int result;
            result = (int)Math.Floor(value * keys / 512.0);
            return result;
        }
        
        public static int SetPositionX(int index)
        {
            int[] positionX = { 16, 48, 80, 112, 128, 144, 176, 208, 240, 272, 304, 336, 368, 384, 400, 432, 464, 496 };

            if (index < 0 || index >= positionX.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index out of array range");

            return positionX[index];
        }
        
        public static int ConvertColumn(int[] array, int dpbmsKeyValue, int keys)
        {
            int result = 0;
            int dpbmsColunm = KeyValueToColunm(dpbmsKeyValue, keys);
            if (array == null || array.Length != 2)
                throw new ArgumentException("Array must be of length 2", nameof(array));
            
            if (dpbmsColunm <= 8)
            {
                if (dpbmsColunm == 1)
                {
                    result = SetPositionX(array[0]);
                }
                else if (dpbmsColunm <= array[0]+1)
                {
                    result = SetPositionX(dpbmsColunm - 2);
                }
                else
                {
                    result = SetPositionX(dpbmsColunm);
                }
            }else if (dpbmsColunm >= 9)
            {
                if (dpbmsColunm == 16)
                {
                    result = SetPositionX(10 + array[1]);
                }
                else if (dpbmsColunm < array[1] + 9 )
                {
                    result = SetPositionX(dpbmsColunm);
                }
                else
                {
                    result = SetPositionX(dpbmsColunm + 2);
                }
                
            }
            
            return result;
        }
    }
    

}