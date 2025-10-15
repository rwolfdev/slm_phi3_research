using System.Text;

Console.WriteLine("ProjectSource2TxtInput");

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("Please type in the project path which should be flattened:");
string? projectPath = Console.ReadLine();

if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
{
    Console.WriteLine("Invalid directory path.");
    return;
}

Console.WriteLine("Please type the output file which should be generated (e.g. output.txt):");
string? outputFileName = Console.ReadLine();

if (string.IsNullOrWhiteSpace(outputFileName))
{
    outputFileName = "output.txt";
}

string outputPath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);

Console.WriteLine($"Creating flattened file: {outputPath}");

StringBuilder sb = new StringBuilder();
ProcessDirectory(projectPath, sb, projectPath);

File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
Console.WriteLine("Created output file.");



static void ProcessDirectory(string dirPath, StringBuilder sb, string rootPath)
{
    string relativePath = Path.GetRelativePath(rootPath, dirPath);

    foreach (string file in Directory.GetFiles(dirPath))
    {
        if (ShouldSkipFile(file)) continue;

        sb.AppendLine($"===== File: {relativePath}\\{Path.GetFileName(file)} =====");
        try
        {
            string content = File.ReadAllText(file);
            sb.AppendLine(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        sb.AppendLine();
    }

    foreach (string subDir in Directory.GetDirectories(dirPath))
    {
        ProcessDirectory(subDir, sb, rootPath);
    }
}

static bool ShouldSkipFile(string filePath)
{
    string extension = Path.GetExtension(filePath).ToLowerInvariant();
    string[] skipExtensions = { ".exe", ".dll", ".pdb", ".png", ".jpg", ".jpeg", ".gif", ".ico" };
    return Array.Exists(skipExtensions, ext => ext == extension);
}