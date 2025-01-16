using System.IO.Compression;

namespace LimpezaDeBase.Extension
{
    public static class ZipFileExtension
    {
        public static byte[] CreateZip(IEnumerable<(string FileName, byte[] FileContent)> files)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var (fileName, fileContent) in files)
                {
                    var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    entryStream.Write(fileContent, 0, fileContent.Length);
                }
            }

            return memoryStream.ToArray();
        }
    }
}
