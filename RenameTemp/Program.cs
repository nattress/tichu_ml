using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace tichulogparse
{
    class Program
    {
        private static readonly int BatchSize = 1000;
        private static readonly int DelayBetweenBatches = 2000; // milliseconds
        private static readonly int PerRequestDelay = 5; // milliseconds
        static void PrintUsage()
        {
            Console.WriteLine("Usage: tichulogparse <out dir> <start id> <end id>");
        }

        static void Main(string[] args)
        {
            // Usage: tichulogparse <out dir> <start id> <end id>

            if (args.Length != 3)
            {
                PrintUsage();
                return;
            }

            string outDir = args[0];
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            if (!int.TryParse(args[1], out int startId))
            {
                Console.Error.WriteLine("Bad start id");
                return;
            }

            if (!int.TryParse(args[2], out int endId))
            {
                Console.Error.WriteLine("Bad id id");
                return;
            }
            
            int totalToDownload = endId - startId + 1;
            int downloaded = 0;
            string baseUrl = @"http://tichulog.brettspielwelt.de/{0}.tch";
            
            
            HttpClient client = new HttpClient();
            Stopwatch sw = new Stopwatch();
            for (int batchStartId = startId; batchStartId <= endId; batchStartId += BatchSize)
            {
                sw.Restart();
                int batchEndId = batchStartId + Math.Min(BatchSize, totalToDownload - downloaded);
                Parallel.For(batchStartId, batchEndId, (id) => 
                {
                    string url = string.Format(baseUrl, id);
                    string localFile = Path.Combine(outDir, $"{id}.txt");
                    Interlocked.Increment(ref downloaded);
                    
                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        try
                        {
                            using (HttpContent content = response.Content)
                            using (StreamWriter sw = File.CreateText(localFile))
                            {
                                string result = content.ReadAsStringAsync().Result;
                                sw.Write(result);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error downloading {url} -> {localFile}");
                            Console.WriteLine(e.ToString());
                            return;
                        }
                    }

                    Thread.Sleep(PerRequestDelay);
                });

                if (downloaded < totalToDownload)
                {
                    // There's more left to download. Wait a few seconds to give the server a break
                    Thread.Sleep(DelayBetweenBatches);
                }

                sw.Stop();

                int batchNumber = (batchStartId - startId) / BatchSize + 1;
                int totalBatches = (int)Math.Ceiling((double)totalToDownload / (double)BatchSize);
                int remainingMsEta = downloaded < totalToDownload ? (int)((double)sw.ElapsedMilliseconds / (double)(batchEndId - batchStartId) * (double)(totalToDownload - downloaded)) : 0;
                Console.WriteLine($"Batch [{batchNumber,4} / {totalBatches}] Ok - Batch time {sw.ElapsedMilliseconds}ms. ETA {remainingMsEta}ms");
            }
        }
    }
}
