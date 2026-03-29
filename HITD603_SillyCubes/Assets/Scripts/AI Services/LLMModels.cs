using UnityEngine;

// Available LLM models for selection
public enum LLMModelType
{
    Gemma3_12B,      // gemma3:12b (8.1 GB)
    Llama3_Latest   // llama3.2:latest (2.0 GB)
}

public static class LLMModels
{    
    public static string GetModelName(LLMModelType modelType)
    {
        switch (modelType)
        {
            case LLMModelType.Gemma3_12B:
                return "gemma3:12b";
            case LLMModelType.Llama3_Latest:
                return "llama3.2:latest";
            default:
                return "gemma3:12b";
        }
    }
}
