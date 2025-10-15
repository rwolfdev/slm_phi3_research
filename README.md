# Automated Code and Project Documentation with Phi-3  
**Research Repository – Small Language Model (SLM) Evaluation**

This repository contains all research artifacts, datasets, and results from the study  
**“Automated Code and Project Documentation with Phi-3: Development and Evaluation of a Local AI Assistance System.”**

The research evaluates Microsoft’s **Phi-3 Small Language Model (SLM)** in its ability to generate function, class, and project-level documentation for C# code.  
The dataset and analysis aim to provide transparency, reproducibility, and comparability for future research in automated software documentation.

---

## 📁 Repository Structure
```
slm_phi3_research/
│
├── diagrams/ # Visualization of evaluation results
│ ├── correctness.png
│ ├── correllation_cc_correctness.png
│ ├── errors.png
│ ├── hallucinations.png
│ ├── readability.png
│ └── relevance.png
│
├── results/
│ ├── evaluated/ # Consolidated and processed evaluation results
│ │ ├── Complete_Resultset.xlsx
│ │ ├── correctness.csv
│ │ ├── correllation_cc_correctness.csv
│ │ ├── errors.csv
│ │ ├── hallucinations.csv
│ │ ├── readability.csv
│ │ ├── relevance.csv
│ │ └── szenario_overview.csv
│ │
│ └── raw/ # Raw test and prompt results
│ ├── prompts/ # Prompt definitions per documentation type
│ │ ├── .class/
│ │ ├── .function/
│ │ └── .project/
│ │
│ └── szenarios/ # Scenario-based input and generated outputs
│ ├── SZ001/ … SZ00X/
│ │ ├── code_input.txt
│ │ ├── szenario_info.txt
│ │ ├── prompt_XXX.txt_result.txt
│ │ └── prompt_XXX.txt_result_language.txt
│
└── README.md # (this file)
```


---

## 🧪 Research Overview

### Objective
To evaluate whether **Phi-3**, as a *locally executable Small Language Model*, can generate  
**accurate, readable, and context-relevant** documentation for software projects without relying on cloud-based LLMs.

### Evaluation Setup
- **Model:** Phi-3-mini (local, CPU-based inference)
- **Language:** C#
- **Documentation types:** Function, Class, Project
- **Prompt variations:** 6 for functions, 4 for classes, 4 for projects  
- **Total test runs:** 132

Each scenario includes:
- Original code input (`code_input.txt`)
- Model outputs with and without language specification
- Associated prompt definition
- Evaluation metrics

### Evaluation Metrics
- **Correctness**
- **Readability**
- **Relevance**
- **Error Rate**
- **Hallucination Frequency**
- **Correlation between Cyclomatic Complexity and Correctness**

---

## 📊 Results Summary

Key results can be found in `/results/evaluated/`:
- **`Complete_Resultset.xlsx`** – aggregated dataset of all evaluations  
- **`correllation_cc_correctness.png`** – shows the relationship between complexity and correctness  
- **`hallucinations.png`** – illustrates model hallucination frequency by documentation type  

---

## 🔍 How to Use the Data

You can reproduce or extend the analysis as follows:

1. **Explore scenarios** in `/results/raw/szenarios/`  
   Each folder (e.g., `SZ001`, `SZ004`) contains both prompt input and model output.
2. **Inspect evaluated metrics** via the `.csv` files in `/results/evaluated/`.
3. **Review visualizations** in `/diagrams/` to understand the performance patterns.
4. **Use `szenario_overview.csv`** to match scenario identifiers with documentation types.

---

## 🧠 License

This dataset and associated scripts are released under the **MIT License**.  
You are free to use, modify, and share the data with proper attribution.
