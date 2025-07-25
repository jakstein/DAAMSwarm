/**
 * DAAM (Diffusion Attentive Attribution Maps) Extension JavaScript
 * provides frontend functionality for attention heatmap visualization
 */

// DAAM Extension namespace
window.DAAMSwarm = {
    
    /**
     * Initialize DAAM extension functionality
     */
    init: function() {
        console.log('DAAM Extension: Initializing...');
        
        // Add event listeners for DAAM parameters
        this.setupParameterValidation();
        
        console.log('DAAM Extension: Initialized successfully');
    },
    
    /**
     * setup parameter validation for DAAM inputs
     */
    setupParameterValidation: function() {
        // find DAAM words input
        const wordsInput = document.querySelector('input[data-param-id="daamamalysiswords"]');
        if (wordsInput) {
            wordsInput.addEventListener('input', this.validateWords.bind(this));
            wordsInput.addEventListener('blur', this.formatWords.bind(this));
        }
        
        // find DAAM enabled checkbox
        const enabledCheckbox = document.querySelector('input[data-param-id="enabledaamanalysis"]');
        if (enabledCheckbox) {
            enabledCheckbox.addEventListener('change', this.toggleDAAMControls.bind(this));
        }
    },
    
    /**
     * validate words input format
     */
    validateWords: function(event) {
        const input = event.target;
        const words = input.value.trim();
        
        if (words && !this.isValidWordsFormat(words)) {
            input.classList.add('daam-invalid');
            this.showTooltip(input, 'Please enter comma-separated words (e.g., "dog,hat,park")');
        } else {
            input.classList.remove('daam-invalid');
            this.hideTooltip(input);
        }
    },
    
    /**
     * format words input (remove extra spaces, normalize commas)
     */
    formatWords: function(event) {
        const input = event.target;
        const words = input.value.trim();
        
        if (words) {
            // clean up the format: remove extra spaces, normalize commas
            const formatted = words
                .split(',')
                .map(word => word.trim())
                .filter(word => word.length > 0)
                .join(',');
            
            if (formatted !== words) {
                input.value = formatted;
            }
        }
    },
    
    /**
     * check if words format is valid
     */
    isValidWordsFormat: function(words) {
        if (!words.trim()) return true;
        
        // check for valid comma-separated words
        const wordList = words.split(',');
        return wordList.every(word => {
            const trimmed = word.trim();
            return trimmed.length > 0 && /^[a-zA-Z0-9\s\-_]+$/.test(trimmed);
        });
    },
    
    /**
     * toggle DAAM control visibility based on enabled state
     */
    toggleDAAMControls: function(event) {
        const enabled = event.target.checked;
        const daaamGroup = document.querySelector('.param-group[data-group="daam-analysis"]');
        
        if (daaamGroup) {
            const controls = daaamGroup.querySelectorAll('.param-input:not([data-param-id="enabledaamanalysis"])');
            controls.forEach(control => {
                control.style.opacity = enabled ? '1' : '0.5';
                control.disabled = !enabled;
            });
        }
    },
    
    /**
     * show tooltip for validation errors
     */
    showTooltip: function(element, message) {
        let tooltip = element.parentNode.querySelector('.daam-tooltip');
        if (!tooltip) {
            tooltip = document.createElement('div');
            tooltip.className = 'daam-tooltip';
            element.parentNode.appendChild(tooltip);
        }
        tooltip.textContent = message;
        tooltip.style.display = 'block';
    },
    
    /**
     * hide tooltip
     */
    hideTooltip: function(element) {
        const tooltip = element.parentNode.querySelector('.daam-tooltip');
        if (tooltip) {
            tooltip.style.display = 'none';
        }
    }
};

// initialize DAAM extension when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    DAAMSwarm.init();
});

// also initialize if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        DAAMSwarm.init();
    });
} else {
    DAAMSwarm.init();
}
