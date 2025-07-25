using SwarmUI.Core;
using SwarmUI.Utils;
using SwarmUI.Text2Image;
using SwarmUI.Builtin_ComfyUIBackend;
using Newtonsoft.Json.Linq;

namespace DAAMSwarmExtension;

/// <summary>
/// DAAM (Diffusion Attentive Attribution Maps) Extension for SwarmUI
/// Provides cross-attention heatmap visualization for Stable Diffusion models
/// </summary>
public class DAAMSwarm : Extension
{
    /// <summary>DAAM Analysis Enable parameter</summary>
    public static T2IRegisteredParam<bool> DAAMEnabled;
    
    /// <summary>Words to analyze for attention heatmaps</summary>
    public static T2IRegisteredParam<string> DAAMWords;
    
    /// <summary>Positive prompt heatmap visualization</summary>
    public static T2IRegisteredParam<bool> DAAMShowPositive;
    
    /// <summary>Negative prompt heatmap visualization</summary>
    public static T2IRegisteredParam<bool> DAAMShowNegative;
    
    /// <summary>Heatmap opacity for overlay</summary>
    public static T2IRegisteredParam<double> DAAMOpacity;
    
    /// <summary>DAAM parameter group</summary>
    public static T2IParamGroup DAAMGroup;

    public override void OnInit()
    {
        // set extension metadata
        ExtensionName = "DAAM";
        Version = "1.0.0";
        ExtensionAuthor = "jakstein";
        Description = "Diffusion Attentive Attribution Maps for visualizing cross-attention heatmaps in Stable Diffusion models";
        License = "MIT";
        Tags = ["parameters"];
        
        Logs.Init("DAAM Extension initializing...");
        
        // register the ComfyUI DAAM nodes as an installable feature
        InstallableFeatures.RegisterInstallableFeature(new("DAAM Analysis", "comfyui_daam",
            "https://github.com/nisaruj/comfyui-daam", "nisaruj", 
            "This will install DAAM (Diffusion Attentive Attribution Maps) support.\nDo you wish to install?"));
        
        // map comfy nodes to the feature flag
        ComfyUIBackendExtension.NodeToFeatureMap["CLIPTextEncodeWithTokens"] = "comfyui_daam";
        ComfyUIBackendExtension.NodeToFeatureMap["KSamplerDAAM"] = "comfyui_daam";
        ComfyUIBackendExtension.NodeToFeatureMap["DAAMAnalyzer"] = "comfyui_daam";
        
        // create parameter group (note: Toggles should be true to make group work properly with install buttons)
        DAAMGroup = new("DAAM Analysis", Toggles: true, Open: false, IsAdvanced: true);
        
        // register parameters
        DAAMEnabled = T2IParamTypes.Register<bool>(new("Enable DAAM Analysis", 
            "Enable attention heatmap analysis for this generation",
            "false", Toggleable: true, Group: DAAMGroup, FeatureFlag: "comfyui_daam"));
            
        DAAMWords = T2IParamTypes.Register<string>(new("DAAM Analysis Words", 
            "Comma-separated list of words to generate attention heatmaps for (e.g., 'dog,hat,park')",
            "", Group: DAAMGroup, FeatureFlag: "comfyui_daam"));
            
        DAAMShowPositive = T2IParamTypes.Register<bool>(new("Show Positive Heatmaps", 
            "Display attention heatmaps for positive prompt words",
            "true", Group: DAAMGroup, FeatureFlag: "comfyui_daam"));
            
        DAAMShowNegative = T2IParamTypes.Register<bool>(new("Show Negative Heatmaps", 
            "Display attention heatmaps for negative prompt words",
            "false", Group: DAAMGroup, FeatureFlag: "comfyui_daam"));
            
        DAAMOpacity = T2IParamTypes.Register<double>(new("Heatmap Opacity", 
            "Opacity of the attention heatmap overlay (0.0 to 1.0)",
            "0.6", Min: 0.0, Max: 1.0, Step: 0.1, Group: DAAMGroup, FeatureFlag: "comfyui_daam"));
        
        // add JS and CSS assets
        ScriptFiles.Add("Assets/daam.js");
        StyleSheetFiles.Add("Assets/daam.css");
        
        // add workflow generation step for DAAM
        WorkflowGenerator.AddStep(GenerateDAAMWorkflow, 8.5); // Run before final save (priority 10)
        
        Logs.Init("DAAM Extension initialized successfully");
    }
    
    /// <summary>
    /// Generates the DAAM workflow nodes when DAAM analysis is enabled
    /// </summary>
    private void GenerateDAAMWorkflow(WorkflowGenerator generator)
    {
        // check if DAAM is enabled
        if (!generator.UserInput.TryGet(DAAMEnabled, out bool isEnabled) || !isEnabled)
        {
            return;
        }
        
        // gget DAAM configuration
        string analysisWords = generator.UserInput.Get(DAAMWords, "");
        bool showPositive = generator.UserInput.Get(DAAMShowPositive, true);
        bool showNegative = generator.UserInput.Get(DAAMShowNegative, false);
        double opacity = generator.UserInput.Get(DAAMOpacity, 0.6);
        
        if (string.IsNullOrWhiteSpace(analysisWords))
        {
            Logs.Warning("DAAM enabled but no analysis words specified");
            return;
        }
        
        Logs.Verbose($"Generating DAAM workflow for words: {analysisWords}");
        
        try
        {
            // check if comfyui-daam feature is available
            if (!generator.Features.Contains("comfyui_daam"))
            {
                Logs.Warning("DAAM analysis requested but ComfyUI DAAM nodes not available");
                return;
            }
            
            // create DAAM-enabled text encoders and sampler
            string posTokensNode = "";
            string negTokensNode = "";
            
            // replace positive prompt encoding
            if (generator.UserInput.TryGet(T2IParamTypes.Prompt, out string positivePrompt) && !string.IsNullOrEmpty(positivePrompt))
            {
                posTokensNode = generator.CreateNode("CLIPTextEncodeWithTokens", new JObject()
                {
                    ["text"] = positivePrompt,
                    ["clip"] = generator.FinalClip
                });
            }
            
            // replace negative prompt encoding if needed
            if (showNegative && generator.UserInput.TryGet(T2IParamTypes.NegativePrompt, out string negativePrompt) && !string.IsNullOrEmpty(negativePrompt))
            {
                negTokensNode = generator.CreateNode("CLIPTextEncodeWithTokens", new JObject()
                {
                    ["text"] = negativePrompt,
                    ["clip"] = generator.FinalClip
                });
            }
            
            // create DAAM sampler to replace regular sampler
            string daaamSamplerNode = generator.CreateNode("KSamplerDAAM", new JObject()
            {
                ["model"] = generator.FinalModel,
                ["positive"] = !string.IsNullOrEmpty(posTokensNode) ? new JArray { posTokensNode, 0 } : generator.FinalPrompt,
                ["negative"] = !string.IsNullOrEmpty(negTokensNode) ? new JArray { negTokensNode, 0 } : generator.FinalNegativePrompt,
                ["latent_image"] = generator.FinalLatentImage,
                ["seed"] = generator.UserInput.Get(T2IParamTypes.Seed),
                ["steps"] = generator.UserInput.Get(T2IParamTypes.Steps),
                ["cfg"] = generator.UserInput.Get(T2IParamTypes.CFGScale),
                ["sampler_name"] = generator.UserInput.Get(ComfyUIBackendExtension.SamplerParam, "euler"),
                ["scheduler"] = generator.UserInput.Get(ComfyUIBackendExtension.SchedulerParam, "normal"),
                ["denoise"] = generator.UserInput.Get(T2IParamTypes.InitImageCreativity, 1.0)
            });
            
            // update final latent to use DAAM sampler output
            generator.FinalLatentImage = new JArray { daaamSamplerNode, 0 };
            
            // create DAAM analyzer nodes
            if (showPositive && !string.IsNullOrEmpty(posTokensNode))
            {
                string analyzerNodePos = generator.CreateNode("DAAMAnalyzer", new JObject()
                {
                    ["clip"] = generator.FinalClip,
                    ["tokens"] = new JArray { posTokensNode, 1 }, // tokens output
                    ["heatmaps"] = new JArray { daaamSamplerNode, 1 }, // pos_heatmaps output
                    ["attentions"] = analysisWords,
                    ["caption"] = true,
                    ["alpha"] = opacity,
                    ["images"] = generator.FinalImageOut
                });
                
                generator.FinalImageOut = new JArray { analyzerNodePos, 0 };
            }
            
            if (showNegative && !string.IsNullOrEmpty(negTokensNode))
            {
                string analyzerNodeNeg = generator.CreateNode("DAAMAnalyzer", new JObject()
                {
                    ["clip"] = generator.FinalClip,
                    ["tokens"] = new JArray { negTokensNode, 1 }, // tokens output
                    ["heatmaps"] = new JArray { daaamSamplerNode, 2 }, // neg_heatmaps output
                    ["attentions"] = analysisWords,
                    ["caption"] = true,
                    ["alpha"] = opacity,
                    ["images"] = generator.FinalImageOut
                });
                
                generator.FinalImageOut = new JArray { analyzerNodeNeg, 0 };
            }
        }
        catch (Exception ex)
        {
            Logs.Error($"Error generating DAAM workflow: {ex.Message}");
        }
    }
}
