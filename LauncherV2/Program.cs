using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Utilities;

class StarkFN
{
    static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StarkFN");

    static async Task Main()
    {
        Directory.CreateDirectory(AppDataPath);
        await DownloadFiles();
        StartMenu();
    }

    static async Task DownloadFiles()
    {
        var files = new (string name, string url)[]
        {
            ("Backend.dll", "https://raw.githubusercontent.com/Project-Stark-FN/upload/refs/heads/main/Cobalt.dll")
        };

        using var client = new HttpClient();
        foreach (var file in files)
        {
            string localPath = Path.Combine(AppDataPath, file.name);
            if (File.Exists(localPath)) File.Delete(localPath);

            try
            {
                var bytes = await client.GetByteArrayAsync(file.url);
                await File.WriteAllBytesAsync(localPath, bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Download fehlgeschlagen für {file.name}: {e.Message}");
            }
        }
    }

    static void StartMenu()
    {
        Console.WriteLine("-----------------------------");
        Console.WriteLine("-> 1 - Settings");
        Console.WriteLine("-> 2 - Start Fortnite");
        Console.WriteLine("-----------------------------");
        Console.Write("What is your Choice: ");
        string option = Console.ReadLine();

        if (option == "1") Settings();
        else if (option == "2") StartFortnite();
        else
        {
            Console.WriteLine("Please enter a valid number");
            StartMenu();
        }
    }

    static void Settings()
    {
        Directory.CreateDirectory(AppDataPath);
        string filePath = Path.Combine(AppDataPath, "starkfn.txt");
        if (File.Exists(filePath)) File.Delete(filePath);

        string email;
        while (true)
        {
            Console.Write("Enter your E-Mail: ");
            email = Console.ReadLine();
            if (email.Contains("@")) break;
            Console.WriteLine("Invalid E-Mail.");
        }

        Console.Write("Enter your Password: ");
        string password = Console.ReadLine();

        string fortnitePath;
        while (true)
        {
            Console.Write("Enter the File Path of Fortnite: ");
            fortnitePath = Console.ReadLine();
            string expectedFile = Path.Combine(fortnitePath, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");
            if (File.Exists(expectedFile)) break;
            Console.WriteLine($"Error: File not found at {expectedFile}");
        }

        File.WriteAllLines(filePath, new[] { email, password, fortnitePath });
        Console.WriteLine("Saved!");
        StartMenu();
    }

    static void StartFortnite()
    {
        string filePath = Path.Combine(AppDataPath, "starkfn.txt");
        string backendDll = Path.Combine(AppDataPath, "Backend.dll");
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Settings not found! Please configure first.");
            StartMenu();
            return;
        }

        var lines = File.ReadAllLines(filePath);
        string email = lines[0];
        string password = lines[1];
        string fortnitePath = lines[2];

        string exePath = Path.Combine(fortnitePath, "FortniteGame", "Binaries", "Win64", "FortniteClient-Win64-Shipping.exe");
        if (!File.Exists(exePath))
        {
            Console.WriteLine($"Error: Fortnite executable not found at {exePath}");
            StartMenu();
            return;
        }

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo(exePath)
            {
                Arguments = $"-log -epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -skippatchcheck -nobe -fromfl=eac -AUTH_TYPE=epic -AUTH_LOGIN={email} -AUTH_PASSWORD={password} -fltoken=3db3ba5dcbd2e16703f3978d -caldera=eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2NvdW50X2lkIjoiYmU5ZGE1YzJmYmVhNDQwN2IyZjQwZWJhYWQ4NTlhZDQiLCJnZW5lcmF0ZWQiOjE2Mzg3MTcyNzgsImNhbGRlcmFHdWlkIjoiMzgxMGI4NjMtMmE2NS00NDU3LTliNTgtNGRhYjNiNDgyYTg2IiwiYWNQcm92aWRlciI6IkVhc3lBbnRpQ2hlYXQiLCJub3RlcyI6IiIsImZhbGxiYWNrIjpmYWxzZX0.VAWQB67RTxhiWOxx7DBjnzDnXyyEnX7OljJm-j2d88G_WgwQ9wrE6lwMEHZHjBd1ISJdUO1UVUqkfLdU5nofBQ",
                UseShellExecute = true
            };
            Process process = Process.Start(psi);
            Console.WriteLine("Fortnite wird gestartet...");
            Injector.Inject(process.Id, backendDll);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fehler beim Starten von Fortnite: {e.Message}");
        }
    }
}
