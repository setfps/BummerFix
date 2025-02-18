using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Security.Principal;

namespace BummerFix
{
    internal class Program
    {
        private static readonly string HostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void RestartAsAdministrator()
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("Перезапуск программы с правами администратора...");
                RestartAsAdministrator();
                return;
            }

            Console.WriteLine("Bummer fix by setfps (vk.com/setfps) v4");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;

            
            const string pastebinUrl = "https://pastebin.com/raw/UG2TukJa";

            Console.WriteLine("Остановка вредоносных служб...");
            StopMalwareServices();

            Console.WriteLine("Обработка разблокировки сайтов...");
            UnblockSitesFromPastebin(pastebinUrl);

           

          

            // Console.WriteLine("Готово, вирус от чурки устранен v4 (Перезагрузите компьютер)!");

            MessageBoxWinAPI.Show("Готово, вирус от чурки устранен v4 (Перезагрузите компьютер)!\n!НЕ ИГРАЙТЕ НА BUMMER RUST, ЕСЛИ НЕ ХОТИТЕ ВИРУСОВ!", "BummerFix", MessageBoxButtons.OK);
            Console.ReadLine();
        }

        private static void StopMalwareServices()
        {
            string[] services = { "dllhost32", "RpcSs", "dllhost", "svchost", "route" };
            string tempPath = Path.GetTempPath();
            string system32Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "System32");
            
            foreach (string service in services)
            {
                try
                {
                    Console.WriteLine($"Остановка сервиса: {service}");
                    ExecuteCommand($"sc stop {service}");
                    ExecuteCommand($"sc delete {service}");

                    string[] pathsToCheck = { 
                        Path.Combine(tempPath, $"{service}.exe"),
                        Path.Combine(system32Path, $"{service}.exe")
                    };

                    foreach (string filePath in pathsToCheck)
                    {
                        if (File.Exists(filePath))
                        {
                            Console.WriteLine($"Найден подозрительный файл: {filePath}");
                            Console.WriteLine("Проверка и восстановление файла...");
                            if (!filePath.Contains("System32"))
                            {
                                File.Delete(filePath);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке {service}: {ex.Message}");
                }
            }
        }

        private static void UpdateFirewallRules(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string response = client.DownloadString(url);
                    var firewallResponse = JsonConvert.DeserializeObject<FirewallResponse>(response);

                    ExecuteCommand("netsh advfirewall reset");
                    ExecuteCommand("route -f");
                    ExecuteCommand("ipconfig /renew");
                    Console.WriteLine("Сброшены все правила файрвола и маршрутизации");

                    if (firewallResponse?.Block != null)
                    {
                        foreach (string ip in firewallResponse.Block)
                        {
                            Console.WriteLine($"Блокировка IP: {ip}");
                            ExecuteCommand($"netsh advfirewall firewall add rule name=\"block {ip}\" dir=out action=block protocol=ANY remoteip={ip}");
                            ExecuteCommand($"route add {ip} mask 255.255.255.255 0.0.0.0 metric 1");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении правил: {ex.Message}");
            }
        }

        private static void UnblockSitesFromPastebin(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string jsonContent = client.DownloadString(url);
                    Data data = JsonConvert.DeserializeObject<Data>(jsonContent);

                    if (data != null && data.blockurls != null)
                    {
                        foreach (string site in data.blockurls)
                        {
                            Console.WriteLine("Разблокирован сайт: " + site);
                        }
                        UnblockSitesInHosts(data.blockurls);
                        Console.WriteLine("Все сайты разблокированы.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке данных для сайтов: {ex.Message}");
            }

            const string pbUrl = "https://raw.githubusercontent.com/setfps/testtt/refs/heads/main/block.json";
            Console.WriteLine("Обновление правил файрвола...");
            UpdateFirewallRules(pbUrl);
        }

        private static List<string> ReadHostsFile()
        {
            return File.ReadAllLines(HostsFilePath).ToList();
        }

        private static void UnblockSitesInHosts(IEnumerable<string> sites)
        {
            List<string> lines = ReadHostsFile();
            foreach (string site in sites)
            {
                lines.RemoveAll(line => line.Contains(site) || line.Contains("www." + site));
            }
            WriteHostsFile(lines);
        }

        private static void WriteHostsFile(List<string> lines)
        {
            File.WriteAllLines(HostsFilePath, lines);
        }

        private static void ExecuteCommand(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", $"/c {command}")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process process = Process.Start(processInfo))
            {
                process.WaitForExit();
            }
        }

        private class Data
        {
            public List<string> block { get; set; }
            public List<string> unblock { get; set; }
            public List<string> blockurls { get; set; }
            public List<string> unblockurls { get; set; }
        }

        private class FirewallResponse
        {
            [JsonProperty("block")]
            public List<string> Block { get; set; }

            [JsonProperty("unblock")]
            public List<string> Unblock { get; set; }
        }
    }
}
