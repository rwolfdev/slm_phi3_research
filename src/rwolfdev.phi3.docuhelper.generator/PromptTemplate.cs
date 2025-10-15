using System;
using System.Collections.Generic;
using System.Text;

namespace rwolfdev.phi3.docuhelper.generator
{
    public class PromptTemplate
    {
        public string FunctionPrompt { get; set; }
        public string ClassPrompt { get; set; }
        public string ProjectPrompt { get; set; }
        public bool IncludeLanguageInfo { get; set; } = false;
    }
}
