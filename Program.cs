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
    private const string RemoteJsonUrl = "https://raw.githubusercontent.com/ldoubil/astral.github.io/fe34c79c91463485d0ceb4aaac015e7c88e8229a/public/server.json";
    private const string EmbeddedResourceName = "Assets\\server.json";  // Replace 'EasyTierTest' with your project namespace
    private const int TestCount = 4;  // Number of tests per server for stability
    private const int TestTimeoutMs = 2000;  // Timeout per test
    private const int TestIntervalMs = 500;  // Delay between tests
    private const int TopServersCount = 5;  // Number of top servers to select

    static async Task Main(string[] args) {
        Console.WriteLine("欢迎使用 EasyTier 服务器测试工具！");
        Console.WriteLine("这个工具会自动检测服务器的延迟和稳定性，适合游戏联机使用。");
        Console.WriteLine("如果远程 JSON 下载失败，会使用内置的本地 JSON 继续。");
        Console.WriteLine("开始测试，请稍等...");

        // Try to load JSON from remote URL first
        string? jsonContent = null;
        try {
            using HttpClient client = new HttpClient();
            jsonContent = await client.GetStringAsync(RemoteJsonUrl);
            Console.WriteLine("远程 JSON 下载成功！");
        }
        catch (Exception ex) {
            Console.WriteLine($"远程 JSON 下载失败：{ex.Message}");
            Console.WriteLine("正在切换到本地嵌入的 JSON...");
            jsonContent = LoadEmbeddedJson();
            if (jsonContent == null) {
                Console.WriteLine("本地 JSON 也加载失败，无法继续。");
                return;
            }
        }

        // Parse JSON into list of servers
        List<Server>? servers;
        try {
            servers = JsonSerializer.Deserialize<List<Server>>(jsonContent);
            Console.WriteLine($"成功解析 {servers?.Count} 个服务器。");
        }
        catch (Exception ex) {
            Console.WriteLine($"JSON 解析失败：{ex.Message}。可能是格式变化，请检查文件。");
            return;
        }

        // Test servers in parallel for faster execution
        Console.WriteLine("正在并行测试所有服务器，以加速过程...");
        var tasks = servers?.Select(s => TestServerAsync(s));

        if (tasks == null) {
            Console.WriteLine("好像哪里出错了!");
            Console.WriteLine("Json 好像有点问题.");
            return;
        }

        ServerResult[] resultsArray = await Task.WhenAll(tasks);
        List<ServerResult> results = resultsArray.ToList();

        // Print all servers' results
        Console.WriteLine("\n所有服务器测试结果：");
        foreach (var result in results) {
            string latencyStr = result.AverageLatency >= 0 ? $"{result.AverageLatency:F2} ms" : "无法连接";
            string stdDevStr = result.AverageLatency >= 0 ? $"{result.StdDev:F2}" : "N/A";
            Console.WriteLine($"服务器：{result.Server.name} ({result.Server.url}) - 平均延迟：{latencyStr}，稳定性：{stdDevStr}");
        }

        // Find and print top fastest and most stable servers
        var topServers = results
            .Where(r => r.AverageLatency >= 0)  // Exclude failed tests
            .OrderBy(r => r.AverageLatency)  // Sort by average latency ascending
            .ThenBy(r => r.StdDev)  // Then by stability (lower std dev is better)
            .Take(TopServersCount)
            .ToList();

        Console.WriteLine($"\n最快最稳定的 {TopServersCount} 个服务器（基于平均延迟和稳定性排序）：");
        if (topServers.Count == 0) {
            Console.WriteLine("没有成功的测试结果。可能是网络问题，请检查连接。");
        }
        else {
            foreach (var top in topServers) {
                Console.WriteLine($"服务器：{top.Server.name} ({top.Server.url}) - 平均延迟：{top.AverageLatency:F2} ms，稳定性：{top.StdDev:F2}");
            }
        }

        Console.WriteLine("\n测试结束！如果有问题，可以重新运行或检查网络。");
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
            catch {
                // Connection failed, ignore
            }
            finally {
                client.Close();
            }
            await Task.Delay(TestIntervalMs);
        }

        double avg = latencies.Any() ? latencies.Average() : -1;
        double stdDev = latencies.Count >= 2 ? CalculateStdDev(latencies, avg) : 0;
        return new ServerResult { Server = server, AverageLatency = avg, StdDev = stdDev };
    }

    // Calculate standard deviation for stability
    private static double CalculateStdDev(List<double> values, double mean) {
        double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / ( values.Count - 1 ));
    }
}