using System.Diagnostics;
using System.Text;
using LMKit.Model;
using LMKit.TextGeneration;
using MailKitSimplified.Receiver.Extensions;
using MailKitSimplified.Receiver.Services;

namespace EmailSummary
{
    internal class Program
    {
        private static bool _isDownloading;

        static async Task Main(string[] args)
        {
            // Set an optional license key here if available.
            // A free community license can be obtained from: https://lm-kit.com/products/community-edition/
            LMKit.Licensing.LicenseManager.SetLicenseKey("");
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            Console.Clear();
            Console.WriteLine("Please select the model you want to use for LM-Kit.NET:\n");
            Console.WriteLine("0 - MiniCPM o 4.5 9B       (~5.9 GB VRAM)");
            Console.WriteLine("1 - Alibaba Qwen 3.5 2B    (~2 GB VRAM)");
            Console.WriteLine("2 - Alibaba Qwen 3.5 4B    (~3.5 GB VRAM)");
            Console.WriteLine("3 - Alibaba Qwen 3.5 9B    (~7 GB VRAM)");
            Console.WriteLine("5 - Google Gemma 4 E4B      (~6 GB VRAM)");
            Console.WriteLine("6 - Alibaba Qwen 3.6 27B   (~18 GB VRAM)");
            Console.WriteLine("7 - Alibaba Qwen 3.6 35B-A3B (~22 GB VRAM)");
            Console.WriteLine("8 - Google Gemma 4 26B-A4B   (~18 GB VRAM)");
            Console.Write("\nOther entry: A custom model URI\n\n> ");

            var input = Console.ReadLine()?.Trim() ?? "0";
            Console.WriteLine(input);
            LM model = LoadModel(input);

            Console.Clear();
            Summarizer summarizer = new(model)
            {
                GenerateTitle = true,
                GenerateContent = true,
                MaxContentWords = 100,
                Guidance = ""
            };

            using var imapReceiver = ImapReceiver.Create("localhost");
            var reader = imapReceiver.ReadMail
                .Take(1, continuous: true)
                .ItemsForMimeMessages();

            while (true)
            {
                Console.Write("Press any key to get the next email.\n");
                _ = Console.ReadKey();
                Console.WriteLine("Fetching...\n");

                try
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    var mimeMessages = await reader.GetMimeMessagesAsync();
                    var mimeMessage = mimeMessages?.FirstOrDefault();
                    var messageText = mimeMessage?.ToString();
                    if (mimeMessage == null || string.IsNullOrWhiteSpace(messageText))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nNo email found to summarize.\n");
                        Console.ResetColor();
                        break;
                    }
                    sw.Stop();
                    Console.WriteLine("MimeMessage download took {0}\n```", sw.Elapsed);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(messageText);
                    Console.ResetColor();
                    Console.WriteLine("```\n");

                    sw.Reset();
                    var result = summarizer.Summarize(messageText);
                    sw.Stop();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Title: {0}", result.Title);
                    Console.WriteLine("Summary took {0}: {1}", sw.Elapsed, result.Summary);
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: Unable to read the email. Details: {e.Message}");
                    Console.ResetColor();
                    Console.WriteLine("Please ensure email dev container is running, e.g. ");
                    Console.WriteLine("`docker run -d -p 80:80 -p 25:25 -p 143:143 rnwood/smtp4dev`, ");
                    Console.WriteLine("and set it up with some sample emails.");
                    Console.WriteLine();
                    break;
                }
            }

            Console.WriteLine("Demo ended. Press any key to exit.");
            _ = Console.ReadKey();
        }

        private static LM LoadModel(string input)
        {
            string? modelId = input switch
            {
                "0" => "minicpm-o-45",
                "1" => "qwen3.5:2b",
                "2" => "qwen3.5:4b",
                "3" => "qwen3.5:9b",
                "5" => "gemma4:e4b",
                "6" => "qwen3.6:27b",
                "7" => "qwen3.6:35b-a3b",
                "8" => "gemma4:26b-a4b",
                _ => null
            };

            if (modelId != null)
            {
                return LM.LoadFromModelID(
                    modelId,
                    downloadingProgress: OnDownloadProgress,
                    loadingProgress: OnLoadProgress);
            }

            return new LM(
                new Uri(input.Trim('"')),
                downloadingProgress: OnDownloadProgress,
                loadingProgress: OnLoadProgress);
        }

        private static bool OnDownloadProgress(string path, long? contentLength, long bytesRead)
        {
            _isDownloading = true;
            if (contentLength.HasValue)
            {
                double percent = (double)bytesRead / contentLength.Value * 100;
                Console.Write($"\rDownloading: {percent:F1}%   ");
            }
            else
            {
                Console.Write($"\rDownloading: {bytesRead / 1024.0 / 1024.0:F1} MB   ");
            }
            return true;
        }

        private static bool OnLoadProgress(float progress)
        {
            if (_isDownloading) { Console.WriteLine(); _isDownloading = false; }
            Console.Write($"\rLoading: {progress * 100:F0}%   ");
            return true;
        }
    }
}
