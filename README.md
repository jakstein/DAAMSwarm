# DAAM Extension for SwarmUI

A SwarmUI extension that provides **Diffusion Attentive Attribution Maps (DAAM)** functionality, allowing users to visualize cross-attention heatmaps in Stable Diffusion models. This extension helps you understand exactly which parts of your prompt influence specific regions of the generated image.

## Features

- Cross-attention heatmap visualization for positive and negative prompts
- Multi-model support: SD1.5, SDXL, SD3, and Flux Dev (beta)  
- Adjustable heatmap opacity for overlay control
- Seamless SwarmUI integration with native parameter groups
- Word-specific attention analysis via comma-separated word lists

### Installation

1. Install via **Swarm Server** -> **Extensions**.

2. Find DAAMSwarm and press install.

3. Restart SwarmUI to load the extension

3. Restart both ComfyUI and SwarmUI

## Usage

### Basic Usage

1. **Enable DAAM Analysis**: 
   - In the Text2Image tab, expand "Advanced Options"
   - Find the "DAAM Analysis" parameter group
   - Toggle "Enable DAAM Analysis"

2. **Specify Analysis Words**:
   - Enter comma-separated words in "DAAM Analysis Words" field
   - Example: `dog,hat,park,skateboard`
   - Choose words that appear in your prompt

3. **Configure Display Options**:
   - Toggle "Show Positive Heatmaps" for positive prompt analysis
   - Toggle "Show Negative Heatmaps" for negative prompt analysis  
   - Adjust "Heatmap Opacity" (0.0 to 1.0)

4. **Generate Image**:
   - Generate your image as usual
   - Results will include attention heatmap overlays

### Understanding Results

The generated heatmaps show:

- **Hot regions (red/yellow)**: Areas where the model focused most attention for that word
- **Cool regions (blue/purple)**: Areas with minimal attention for that word
- **Intensity**: Brighter colors indicate stronger attention weights


## License

This extension is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Credits

- **Original DAAM Research**: [castorini/daam](https://github.com/castorini/daam)
- **ComfyUI DAAM Implementation**: [nisaruj/comfyui-daam](https://github.com/nisaruj/comfyui-daam)
- **SwarmUI Framework**: [mcmonkeyprojects/SwarmUI](https://github.com/mcmonkeyprojects/SwarmUI)
