using System;
using System.Collections.Generic;
using System.Text;

namespace rwolfdev.phi3.docuhelper.generator
{
    public interface IPhi3DocuGenerator : IDisposable
    {
        string GenerateFunctionDocumentation(string functionDefinition, string language = "C#");
        string GenerateClassDocumentation(string classDefinition, string language = "C#");
        string GenerateProjectDocumentation(string projectDefinition, string language = "C#");
    }
}
