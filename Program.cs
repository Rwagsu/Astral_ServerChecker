using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Astral_ServerChecker.Classes;

class Program {
    private static readonly string[] tipText = {
        "Tips: Tips: Tips: Tips: Tips: Tips: Tips: Tips!",
        "写 C++ s路一条!",
        "你知道吗? 向石英门倒水上面也不会发生任何事.\n 嗯, 你应该知道()",
        "现在唯一没崩过的应用就是 CLIP STUDIO PAINT...... 什么意思? o(TヘTo)",
        "累了困了玩会 Changed / Minecraft / CLIP STUDIO PAINT / Blender / Visual Studio / Logic Pro / Davinci Resolve / Godot / Final Cut Pro / Apple Motion / Aseprite / Pixel Composer / Pixelmator Pro. \n很好玩的awa",
        "1 + 1 = 4",
        "憋用中文写代码! (╯‵□′)╯︵┻━┻"
    };

    private const string RemoteJsonUrl = "https://raw.githubusercontent.com/ldoubil/astral.github.io/fe34c79c91463485d0ceb4aaac015e7c88e8229a/public/server.json";
    private const string EmbeddedResourceName = "Assets\\server.json";  // Replace 'EasyTierTest' with your project namespace
    private const int TestCount = 12;  // Number of tests per server for stability
    private const int TestTimeoutMs = 2000;  // Timeout per test
    private const int TestIntervalMs = 500;  // Delay between tests
    private const int TopServersCount = 5;  // Number of top servers to select

    static async Task Main(string[] args) {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Welcome text
        Console.WriteLine(@" █████╗ ███████╗████████╗██████╗  █████╗ ██╗     
██╔══██╗██╔════╝╚══██╔══╝██╔══██╗██╔══██╗██║     
███████║███████╗   ██║   ██████╔╝███████║██║     
██╔══██║╚════██║   ██║   ██╔══██╗██╔══██║██║     
██║  ██║███████║   ██║   ██║  ██║██║  ██║███████╗
╚═╝  ╚═╝╚══════╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝");
        Console.WriteLine(@"                                                 __ 
 _____                     _____ _           _  |  |
|   __|___ ___ _ _ ___ ___|     | |_ ___ ___| |_|  |
|__   | -_|  _| | | -_|  _|   --|   | -_|  _| '_|__|
|_____|___|_|  \_/|___|_| |_____|_|_|___|___|_,_|__|
                                                    ");

        Console.WriteLine("等待测试完毕......");
        Console.WriteLine($"大概 {TestCount} 秒, 将很快完成.");

        // Try to load JSON from remote URL first
        string? jsonContent = null;
        if (args.Contains("--local")) {
            Console.WriteLine("因为 --local 参数, 等待测试本地 JSON......");
            jsonContent = LoadEmbeddedJson();
            if (jsonContent == null) {
                Console.WriteLine("好像哪里出错了! 本地 JSON 未能加载.");
                Console.WriteLine("已经无法继续测试. :(");
                return;
            }
        }
        else {
            try {
                using HttpClient client = new HttpClient();
                jsonContent = await client.GetStringAsync(RemoteJsonUrl);
                Console.WriteLine("从 GitHub 成功下载 JSON.");
            }
            catch (Exception ex) {
                Console.WriteLine($"未能从 GitHub 下载 JSON: {ex.Message}");
                Console.WriteLine("等待测试本地 JSON......");
                jsonContent = LoadEmbeddedJson();
                if (jsonContent == null) {
                    Console.WriteLine("好像哪里出错了! 本地 JSON 也未能加载.");
                    Console.WriteLine("已经无法继续测试. :(");
                    return;
                }
            }
        }

        // Parse JSON into list of servers
        List<Server>? servers;
        try {
            servers = JsonSerializer.Deserialize<List<Server>>(jsonContent);
            Console.WriteLine($"解析了 {servers?.Count} 个服务器.");
        }
        catch (Exception ex) {
            Console.WriteLine($"JSON 无法解析: {ex.Message}");
            Console.WriteLine($"你可以使用 --local 参数重新运行应用, 这将直接解析本地 json.");
            return;
        }

        // Good tip awa
        var random = new Random();
        string tip = tipText[random.Next(tipText.Length)];
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"\n======================Tip======================");
        Console.WriteLine(tip);
        Console.WriteLine($"=====================EndTip=====================\n");
        Console.ResetColor();

        // Test servers in parallel for faster execution
        var tasks = servers?.Select(s => TestServerAsync(s));

        if (tasks == null) {
            Console.WriteLine("好像哪里出错了!");
            Console.WriteLine("Json 好像有点问题.");
            return;
        }

        ServerResult[] resultsArray = await Task.WhenAll(tasks);
        List<ServerResult> results = resultsArray.ToList();

        // Print all servers' results
        Console.WriteLine("\n已经全部完成!");
        foreach (var result in results) {
            string latencyStr = result.AverageLatency >= 0 ? $"{result.AverageLatency:F2} ms" : "出错了! 🥳";
            string stdDevStr = result.AverageLatency >= 0 ? $"{result.StdDev:F2}" : "N/A";
            if (result.AverageLatency >= 0 && result.AverageLatency >= 0 && string.IsNullOrWhiteSpace(result.ErrMessage)) {
                // If success
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"😋✅ 服务器：{result.Server.name} ({result.Server.url}) - 平均延迟：{latencyStr}，稳定性：{stdDevStr}");

                // Reset console color
                Console.ResetColor();

                Console.WriteLine();
            }
            else {
                // If error
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"🥳❌ 服务器：{result.Server.name} ({result.Server.url}) - 出错了! [{result.ErrMessage}]");

                // Reset console color
                Console.ResetColor();

                Console.WriteLine();
            }
        }

        // Find and print top fastest and most stable servers
        var topServers = results
            .Where(r => r.AverageLatency >= 0)  // Exclude failed tests
            .OrderBy(r => r.AverageLatency)  // Sort by average latency ascending
            .ThenBy(r => r.StdDev)  // Then by stability (lower std dev is better)
            .Take(TopServersCount)
            .ToList();

        Console.WriteLine($"\n最好的 {TopServersCount} 个服务器 (基于平均延迟和稳定性排序):");
        if (topServers.Count == 0) {
            Console.WriteLine("好像哪里出错了!");
            Console.WriteLine("没有任何成功的连接.");
        }
        else {
            foreach (var top in topServers) {
                Console.WriteLine($"服务器：{top.Server.name} ({top.Server.url}) - 平均延迟：{top.AverageLatency:F2} ms，稳定性：{top.StdDev:F2}\n");
            }
        }

        Console.WriteLine("\n测试结束啦! 给个 star 嘛awa(雾)");
    }

    // Load embedded JSON as fallback
    private static string? LoadEmbeddedJson() {
        try {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(EmbeddedResourceName);
            if (stream == null)
                return null;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        catch {
            return null;
        }
    }

    // Test a single server: attempt TCP connections multiple times and calculate average and std dev
    private static async Task<ServerResult> TestServerAsync(Server server) {
        string[] parts = server.url.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[1], out int port)) {
            return new ServerResult { Server = server, AverageLatency = -1, StdDev = 0 };  // Invalid URL
        }
        string host = parts[0];

        List<double> latencies = new List<double>();

        string? errMsg = null;

        for (int i = 0; i < TestCount; i++) {
            using TcpClient client = new TcpClient();
            Stopwatch sw = Stopwatch.StartNew();
            try {
                Task connectTask = client.ConnectAsync(host, port);
                if (await Task.WhenAny(connectTask, Task.Delay(TestTimeoutMs)) == connectTask) {
                    await connectTask;  // Ensure completion
                    sw.Stop();
                    latencies.Add(sw.ElapsedMilliseconds);
                }
            }
            catch (Exception ex) {
                // Connection failed, record error message
                errMsg = ex.Message;
            }
            finally {
                client.Close();
            }
            await Task.Delay(TestIntervalMs);
        }

        double avg = latencies.Any() ? latencies.Average() : -1;
        double stdDev = latencies.Count >= 2 ? CalculateStdDev(latencies, avg) : 0;
        return new ServerResult { Server = server, AverageLatency = avg, StdDev = stdDev, ErrMessage = errMsg };
    }

    // Calculate standard deviation for stability
    private static double CalculateStdDev(List<double> values, double mean) {
        double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / ( values.Count - 1 ));
    }
}