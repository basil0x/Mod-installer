using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string[] urls = {
            "https://www.curseforge.com/api/v1/mods/523860/files/5680361/download",
            "https://www.curseforge.com/api/v1/mods/567450/files/5314033/download",
            "https://www.curseforge.com/api/v1/mods/519759/files/5626093/download",
            "https://www.curseforge.com/api/v1/mods/450659/files/5566900/download",
            "https://www.curseforge.com/api/v1/mods/494654/files/5481257/download",
            "https://www.curseforge.com/api/v1/mods/499980/files/5702363/download",
            "https://www.curseforge.com/api/v1/mods/238222/files/5705580/download",
            "https://www.curseforge.com/api/v1/mods/908741/files/5681725/download",
            "https://www.curseforge.com/api/v1/mods/551736/files/5701571/download",
            "https://www.curseforge.com/api/v1/mods/665658/files/5089991/download",
            "https://www.curseforge.com/api/v1/mods/509041/files/5254836/download",
            "https://www.curseforge.com/api/v1/mods/419699/files/5137938/download",
            "https://www.curseforge.com/api/v1/mods/348521/files/4973441/download",
            "https://www.curseforge.com/api/v1/mods/581495/files/5299671/download"
        };
        string[] names =
        {
            "recruits","workers","map_atlases","small_ship","item_banning","moonlight_api","jei","embeddium","embeddium_lightning","canary",
            "epic_knights","architectury_api","cloth_api","oculus"
        };

        bool? success = true;
        Console.WriteLine("Unesi putanju do Minecraft root foldera: ");
        string? UserPath = Console.ReadLine();

        for (int i = 0; i < urls.Length; i++)
        {
            string url = urls[i];
            string filePath = $"{UserPath}/mods/{names[i]}.jar";
            if (Directory.Exists(UserPath + "/mods"))
            {
                await DownloadFileWithProgressAsync(url, filePath);
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
