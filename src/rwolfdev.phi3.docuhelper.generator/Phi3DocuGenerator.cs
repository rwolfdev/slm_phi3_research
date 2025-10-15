using System;
using System.Collections.Generic;
using System.Text;

namespace rwolfdev.phi3.docuhelper.generator
{
    public class Phi3DocuGenerator : IPhi3DocuGenerator
    {
        private readonly Phi3Environment _environment;
        private PromptTemplate _template;

        public Phi3DocuGenerator(Phi3Environment environment)
        {
            _environment = environment;
        }

        public void UseTemplate(PromptTemplate template)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template), "Template cannot be null");
        }

        public string GenerateFunctionDocumentation(string functionDefinition, string language = "C#")
        {
            _ = _template ?? throw new InvalidOperationException("Template must be set before generating documentation.");

            string userContent = _template.IncludeLanguageInfo
                ? $"Create the function documentation for the following {language} code: {functionDefinition}"
                : $"Create the function documentation for the following code: {functionDefinition}";

            string prompt = $"<|system|>{_template.FunctionPrompt}<|end|><|user|>{userContent}<|end|><|assistant|>";
            return _environment.SubmitPrompt(prompt);
        }

        public string GenerateClassDocumentation(string classDefinition, string language = "C#")
        {
            _ = _template ?? throw new InvalidOperationException("Template must be set before generating documentation.");

            string languageInfo = _template.IncludeLanguageInfo ? language : "";

            string prompt = $"<|system|>{_template.ClassPrompt}<|end|><|user|>Create the class documentation for the following {languageInfo} code: {classDefinition}<|end|><|assistant|>";
            return _environment.SubmitPrompt(prompt);
        }

        public string GenerateProjectDocumentation(string projectDefinition, string language = "C#")
        {
            _ = _template ?? throw new InvalidOperationException("Template must be set before generating documentation.");

            string languageInfo = _template.IncludeLanguageInfo ? language : "";

            string prompt = $"<|system|>{_template.ProjectPrompt}<|end|><|user|>Create a project documentation for the following {languageInfo} code: {projectDefinition}<|end|><|assistant|>";
            return _environment.SubmitPrompt(prompt);
        }

        public void Dispose() => _environment.Dispose();
    }
}
