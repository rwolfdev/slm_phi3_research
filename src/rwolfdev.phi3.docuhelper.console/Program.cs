using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using rwolfdev.phi3.docuhelper.generator;

namespace rwolfdev.phi3.docuhelper.console
{
    internal enum ScenarioType
    {
        Unknown = 0,
        Class = 1,
        Function = 2,
        Project = 3
    }

    internal sealed class Scenario
    {
        public string DirectoryPath { get; }
        public string Input { get; }
        public string Language { get; }
        public ScenarioType Type { get; }

        public Scenario(string directoryPath, string input, string language, ScenarioType type)
        {
            this.DirectoryPath = directoryPath;
            this.Input = input;
            this.Language = language;
            this.Type = type;
        }
    }

    internal static class CsvLogger
    {
        public const string CsvPath = "./results/results.csv";
        private const char Separator = ';';

        public static void EnsureWithHeader()
        {
            string? directory = Path.GetDirectoryName(CsvPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(CsvPath))
            {
                using (StreamWriter writer = new StreamWriter(CsvPath, false, new UTF8Encoding(true)))
                {
                    // Header
                    writer.WriteLine(string.Join(Separator, new[]
                    {
                        "Szenarioname",
                        "Prompt Name",
                        "Programminglanguage",
                        "Language Info",
                        "ScenarioType"
                    }));
                }
            }
        }

        public static void AppendRow(string scenarioName, string promptName, string language, bool withLanguageInfo, ScenarioType type)
        {
            using (StreamWriter writer = new StreamWriter(CsvPath, true, new UTF8Encoding(true)))
            {
                string line = string.Join(Separator, new[]
                {
                    Escape(scenarioName),
                    Escape(promptName),
                    Escape(language),
                    withLanguageInfo ? "true" : "false",
                    Escape(type.ToString())
                });
                writer.WriteLine(line);
            }
        }

        private static string Escape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            bool mustQuote = value.IndexOfAny(new[] { Separator, '"', '\n', '\r' }) >= 0 || value.StartsWith(" ") || value.EndsWith(" ");
            if (!mustQuote)
            {
                return value;
            }

            string quoted = value.Replace("\"", "\"\"");
            return "\"" + quoted + "\"";
        }
    }

    internal class Program
    {
        public const string Phi3ModelPath = @"C:\phi3\models\Phi-3-mini-4k-instruct-onnx\cpu_and_mobile\cpu-int4-rtn-block-32-acc-level-4";

        private const string PromptsClassDir = "./prompts/.class/";
        private const string PromptsFunctionDir = "./prompts/.function/";
        private const string PromptsProjectDir = "./prompts/.project/";
        private const string ScenariosRootDir = "./szenarios";

        private const string MarkerClass = ".class";
        private const string MarkerFunction = ".function";
        private const string MarkerProject = ".project";
        private const string MarkerLanguage = ".language";
        private const string CodeInputFile = "code_input.txt";

        private static void Main(string[] args)
        {
            PrintApplicationInfo();

            Console.WriteLine("Initializing Phi-3 environment...");
            Phi3Environment environment = new Phi3Environment(Phi3ModelPath);
            Console.WriteLine("Initialized Phi-3 environment.");

            // 1) Prompts laden
            Console.WriteLine("Loading prompts...");
            Dictionary<string, string> classPrompts = LoadPromptFolder(PromptsClassDir, "class");
            Dictionary<string, string> functionPrompts = LoadPromptFolder(PromptsFunctionDir, "function");
            Dictionary<string, string> projectPrompts = LoadPromptFolder(PromptsProjectDir, "project");
            Console.WriteLine("Prompts loaded successfully.");

            // 2) Szenarien laden
            Console.WriteLine("Loading scenarios...");
            List<Scenario> scenarios = LoadScenarios(ScenariosRootDir);
            Console.WriteLine("Scenarios loaded: " + scenarios.Count);

            CsvLogger.EnsureWithHeader();

            // 3) Verarbeiten
            Phi3DocuGenerator generator = new Phi3DocuGenerator(environment);

            ProcessScenarios(
                FilterScenariosByType(scenarios, ScenarioType.Function),
                functionPrompts,
                ScenarioType.Function,
                generator
            );

            ProcessScenarios(
                FilterScenariosByType(scenarios, ScenarioType.Class),
                classPrompts,
                ScenarioType.Class,
                generator
            );

            ProcessScenarios(
                FilterScenariosByType(scenarios, ScenarioType.Project),
                projectPrompts,
                ScenarioType.Project,
                generator
            );

            Console.WriteLine("All scenarios processed successfully.");
            environment.Dispose();
        }

        private static void ProcessScenarios(
            List<Scenario> scenarios,
            Dictionary<string, string> prompts,
            ScenarioType type,
            Phi3DocuGenerator generator)
        {
            foreach (Scenario scenario in scenarios)
            {
                Console.WriteLine("Processing " + type + " scenario: " + scenario.DirectoryPath);

                foreach (KeyValuePair<string, string> promptKvp in prompts)
                {
                    string promptName = promptKvp.Key;
                    string promptContent = promptKvp.Value;

                    Console.WriteLine("Using prompt: " + promptName);

                    PromptTemplate template = new PromptTemplate();
                    switch (type)
                    {
                        case ScenarioType.Function:
                            template.FunctionPrompt = promptContent;
                            break;
                        case ScenarioType.Class:
                            template.ClassPrompt = promptContent;
                            break;
                        case ScenarioType.Project:
                            template.ProjectPrompt = promptContent;
                            break;
                        default:
                            continue;
                    }

                    generator.UseTemplate(template);

                    if (string.IsNullOrEmpty(scenario.Input))
                    {
                        Console.WriteLine("Skipped (empty input): " + scenario.DirectoryPath);
                        continue;
                    }

                    // Run ohne Sprache
                    string outputNoLang = GenerateByType(generator, type, scenario.Input, scenario.Language);
                    WriteResultFile(scenario.DirectoryPath, promptName + "_result.txt", outputNoLang);
                    CsvLogger.AppendRow(
                        GetScenarioName(scenario.DirectoryPath),
                        promptName,
                        scenario.Language,
                        false,
                        type
                    );


                    // Run mit Sprache (falls vorhanden)
                    template.IncludeLanguageInfo = true;
                    // Falls dein PromptTemplate eine Sprach-Property unterstützt, hier setzen:
                    // template.Language = scenario.Language;

                    string outputWithLang = GenerateByType(generator, type, scenario.Input, scenario.Language);
                    WriteResultFile(scenario.DirectoryPath, promptName + "_result_language.txt", outputWithLang);
                    CsvLogger.AppendRow(
                        GetScenarioName(scenario.DirectoryPath),
                        promptName,
                        scenario.Language,
                        true,
                        type
                    );


                    Console.WriteLine("Saved results to: " + Path.Combine(scenario.DirectoryPath, promptName + "_result*.txt"));
                }
            }
        }

        private static string GenerateByType(Phi3DocuGenerator generator, ScenarioType type, string input, string language = null)
        {
            switch (type)
            {
                case ScenarioType.Function:
                    return generator.GenerateFunctionDocumentation(input, language);
                case ScenarioType.Class:
                    return generator.GenerateClassDocumentation(input, language);
                case ScenarioType.Project:
                    return generator.GenerateProjectDocumentation(input, language);
                default:
                    return string.Empty;
            }
        }

        private static void WriteResultFile(string directory, string fileName, string content)
        {
            string path = Path.Combine(directory, fileName);
            File.WriteAllText(path, content ?? string.Empty);
        }

        private static Dictionary<string, string> LoadPromptFolder(string folder, string label)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (!Directory.Exists(folder))
            {
                Console.WriteLine("Prompt folder not found (" + label + "): " + folder);
                return result;
            }

            string[] files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string content = File.ReadAllText(file);
                result[name] = content;
                Console.WriteLine("Loaded " + label + " prompt: " + name);
            }

            return result;
        }

        private static List<Scenario> LoadScenarios(string root)
        {
            List<Scenario> scenarios = new List<Scenario>();

            if (!Directory.Exists(root))
            {
                Console.WriteLine("Scenarios root not found: " + root);
                return scenarios;
            }

            string[] scenarioDirs = Directory.GetDirectories(root);
            foreach (string dir in scenarioDirs)
            {
                ScenarioType type = ScenarioType.Unknown;
                string language = string.Empty;
                string input = string.Empty;

                string[] files = Directory.GetFiles(dir);
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    string fileName = Path.GetFileName(file);

                    if (string.Equals(fileName, MarkerClass, StringComparison.OrdinalIgnoreCase))
                    {
                        type = ScenarioType.Class;
                    }
                    else if (string.Equals(fileName, MarkerFunction, StringComparison.OrdinalIgnoreCase))
                    {
                        type = ScenarioType.Function;
                    }
                    else if (string.Equals(fileName, MarkerProject, StringComparison.OrdinalIgnoreCase))
                    {
                        type = ScenarioType.Project;
                    }
                    else if (string.Equals(fileName, MarkerLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        language = File.ReadAllText(file).Trim();
                    }
                    else if (string.Equals(fileName, CodeInputFile, StringComparison.OrdinalIgnoreCase))
                    {
                        input = File.ReadAllText(file);
                    }
                }

                if (type == ScenarioType.Unknown)
                {
                    Console.WriteLine("Skipped scenario (no marker): " + dir);
                    continue;
                }

                scenarios.Add(new Scenario(dir, input, language, type));
                Console.WriteLine("Loaded " + type + " scenario: " + dir);
            }

            return scenarios;
        }

        private static List<Scenario> FilterScenariosByType(List<Scenario> scenarios, ScenarioType type)
        {
            List<Scenario> list = new List<Scenario>();
            for (int i = 0; i < scenarios.Count; i++)
            {
                if (scenarios[i].Type == type)
                {
                    list.Add(scenarios[i]);
                }
            }
            return list;
        }

        private static string GetScenarioName(string directoryPath)
        {
            return new DirectoryInfo(directoryPath).Name;
        }

        private static void PrintApplicationInfo()
        {
            Console.WriteLine("rwolfdev.phi3.docuhelper - Console");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Developed as part of a Master of Engineering research project at Wilhelm Büchner Hochschule.");
            Console.WriteLine("Exploring local AI-assisted code and project documentation with Microsoft Phi-3.");
            Console.WriteLine("----------------------------------");
        }
    }
}
