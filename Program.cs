using System.Diagnostics;
using System.Text.Json;

if (args.Length == 0)
    return;

string slnPath = args[0];
string exeDir = AppContext.BaseDirectory;
string configPath = Path.Combine(exeDir, "config.json");

if (!File.Exists(configPath))
{
    MessageBox.Show($"Config nicht gefunden:\n{configPath}", "SlnLauncher", MessageBoxButtons.OK, MessageBoxIcon.Error);
    return;
}

var config = JsonSerializer.Deserialize<LauncherConfig>(
    File.ReadAllText(configPath),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

if (config is null)
{
    MessageBox.Show("Config konnte nicht gelesen werden.", "SlnLauncher", MessageBoxButtons.OK, MessageBoxIcon.Error);
    return;
}

string targetKey = config.DefaultTarget;

foreach (var line in File.ReadLines(slnPath).Take(5))
{
    var match = config.Markers.FirstOrDefault(m =>
        line.Contains(m.Contains, StringComparison.OrdinalIgnoreCase));

    if (match is not null)
    {
        targetKey = match.Target;
        break;
    }
}

string? targetPath = targetKey switch
{
    "devenvPath" => config.DevenvPath,
    "tcXaeShellPath" => config.TcXaeShellPath,
    _ => null
};

if (targetPath is null || !File.Exists(targetPath))
{
    MessageBox.Show($"Zielanwendung nicht gefunden:\n{targetPath ?? targetKey}", "SlnLauncher", MessageBoxButtons.OK, MessageBoxIcon.Error);
    return;
}

Process.Start(new ProcessStartInfo
{
    FileName = targetPath,
    Arguments = $"\"{slnPath}\"",
    UseShellExecute = true
});

class LauncherConfig
{
    public string DevenvPath { get; set; } = "";
    public string TcXaeShellPath { get; set; } = "";
    public List<MarkerRule> Markers { get; set; } = new();
    public string DefaultTarget { get; set; } = "devenvPath";
}

class MarkerRule
{
    public string Contains { get; set; } = "";
    public string Target { get; set; } = "";
}