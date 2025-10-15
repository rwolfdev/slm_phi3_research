using Microsoft.ML.OnnxRuntimeGenAI;

namespace rwolfdev.phi3.docuhelper.generator
{
    public class Phi3Environment : IDisposable
    {
        private readonly string _modelPath;
        private readonly Model _model;
        private readonly Tokenizer _tokenizer;

        /// <summary>
        /// Initializes the Phi3Environment with the specified model path.
        /// </summary>
        /// <param name="modelPath">The path containing the phi3 onnx model</param>
        public Phi3Environment(string modelPath)
        {

            // inspired from: https://github.com/microsoft/onnxruntime-genai/blob/main/examples/csharp/HelloPhi/Program.cs
            _modelPath = modelPath;

            Config config = new Config(modelPath);
            _model = new Model(config);

            _tokenizer = new Tokenizer(_model);


        }


        /// <summary>
        /// Submits a prompt to the Phi-3 model and returns the generated response.
        /// </summary>
        /// <param name="prompt">The prompt which should be processed by the phi3 model.</param>
        /// <returns></returns>
        public string SubmitPrompt(string prompt)
        {
            try
            {
                string promptOutput = string.Empty;
                using (Sequences tokens = _tokenizer.Encode(prompt))
                using (GeneratorParams generatorParams = new GeneratorParams(_model))
                {
                    generatorParams.SetSearchOption("max_length", 4096);
                    using (Generator generator = new Generator(_model, generatorParams))
                    {
                        generator.AppendTokenSequences(tokens);
                        while (!generator.IsDone())
                        {
                            generator.GenerateNextToken();
                            ReadOnlySpan<int> outputTokens = generator.GetSequence(0);
                            ReadOnlySpan<int> newToken = outputTokens.Slice(outputTokens.Length - 1, 1);
                            string decodedToken = _tokenizer.Decode(newToken);
                            promptOutput += decodedToken;
                        }
                    }
                }
                return promptOutput;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public void Dispose()
        {
            _model.Dispose();
            _tokenizer.Dispose();
        }

    }
}
