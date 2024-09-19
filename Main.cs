using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;

class Program
{
    public class FileInfo
    {
        public required string Url { get; set; }
        public required string FileName { get; set; }
    }
    public class FileList
    {
        public required FileInfo[] Files { get; set; }
    }

    static async Task Main()
    {
        string jsonUrl = "https://example.com/files.json";
        string jsonPath = AppDomain.CurrentDomain.BaseDirectory + "files.json";
        await DownloadFileWithProgressAsync(jsonUrl, jsonPath);

        bool? success = true;
        Console.WriteLine("Unesi putanju do Minecraft root foldera: ");
        string? UserPath = Console.ReadLine();

        FileList fileList = await LoadFilesFromJsonAsync(jsonPath);

        foreach (var file in fileList.Files) {

            string filePath = Path.Combine(UserPath + "/mods", file.FileName);
            if (Directory.Exists(UserPath + "/versions"))
            {
                if (!Directory.Exists(UserPath + "/mods")) Directory.CreateDirectory(UserPath + "/mods");
                await DownloadFileWithProgressAsync(file.Url, filePath);
            }
            else
            {
                Console.WriteLine("Nepravilna putanja");
                success = false;
                break;
            }
        }

        if (success == true)
        {
            Console.WriteLine("Svi fajlovi uspešno preuzeti.\nSkinuti verziju Minecrafta: Forge 1.20.1");
        }
        Console.ReadLine();
    }

    static async Task<string> ReadEmbeddedJsonAsync(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
                                   .FirstOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName != null)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        throw new FileNotFoundException($"Resource '{fileName}' not found.");
    }

    static async Task<FileList> LoadFilesFromJsonAsync(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            return await JsonSerializer.DeserializeAsync<FileList>(fs);
        }
    }

    static async Task DownloadFileWithProgressAsync(string url, string filePath)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            double totalMb = Math.Round((double)totalBytes / 1024 / 1024, 2);

            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                          fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                var buffer = new byte[8192];  // Buffer size (8 KB)
                long totalRead = 0;
                double totalMbRead;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    totalMbRead = Math.Round((double)totalRead / 1024 / 1024, 2);

                    if (totalBytes.HasValue)
                    {
                        // Display progress as a percentage
                        Console.Write($"\rPreuzimanje {Path.GetFileName(filePath)}: {totalMbRead} od {totalMb} Mb. ({(totalRead * 100.0 / totalBytes.Value):0.00}%)");
                    }
                    else
                    {
                        // If content length is not known, just display total bytes read
                        Console.Write($"\rPreuzimanje {Path.GetFileName(filePath)}: {totalMbRead} Mb");
                    }
                }

                Console.WriteLine();  // To move to the next line after download is complete
            }
        }
    }
}
