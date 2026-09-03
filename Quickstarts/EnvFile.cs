namespace Quickstart.Common;

public static class EnvFile
{
    public static void Load(string path = ".env")
    {
        if (!File.Exists(path)) return;

        foreach (var sourceLine in File.ReadLines(path))
        {
            var line = sourceLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            var separator = line.IndexOf('=');
            if (separator <= 0) continue;

            var name = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (value.Length >= 2 && value[0] == value[^1] && value[0] is '\'' or '"')
                value = value[1..^1];

            if (Environment.GetEnvironmentVariable(name) is null)
                Environment.SetEnvironmentVariable(name, value);
        }
    }
}
