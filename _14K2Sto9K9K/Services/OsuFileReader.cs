using System.IO;
using System.Threading.Tasks;
using OsuParsers.Beatmaps;
using OsuParsers.Decoders;


namespace _14k2sTO9K9K.Services
{
    public class OsuFileReader : IOsuFileReader
    {
        private static readonly object DecodeLock = new object();

        public async Task<Beatmap> ReadAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                lock (DecodeLock)
                {
                    return BeatmapDecoder.Decode(filePath);
                }
            });
        }
    }
}