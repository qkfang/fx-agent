(function () {
    var toggleBtn = document.getElementById('chatkit-toggle');
    var panel = document.getElementById('chatkit-panel');
    var initialized = false;

    toggleBtn.addEventListener('click', function () {
        var isOpen = panel.style.display === 'block';
        panel.style.display = isOpen ? 'none' : 'block';
        if (!isOpen && !initialized) {
            initialized = true;
            initChatKit();
        }
    });

    function initChatKit() {
        customElements.whenDefined('openai-chatkit').then(function () {
            var chatkit = document.getElementById('research-chatkit');
            if (!chatkit) return;
            chatkit.setOptions({
                api: {
                    url: '/chatkit',
                    domainKey: 'fx-research'
                },
                history: { enabled: false },
                startScreen: {
                    greeting: 'FX Research Assistant',
                    prompts: [
                        { label: 'Market outlook', prompt: 'What is the current market outlook for EUR/USD?' },
                        { label: 'Trading strategy', prompt: 'What trading strategies work best in current conditions?' },
                        { label: 'Currency analysis', prompt: 'Provide a technical analysis for USD/JPY.' }
                    ]
                },
                composer: { placeholder: 'Ask about market research...' }
            });
        });
    }
})();

