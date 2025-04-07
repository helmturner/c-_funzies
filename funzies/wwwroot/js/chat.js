document.addEventListener('DOMContentLoaded', () => {
    const chatForm = document.getElementById('chatForm');
    const chatInput = document.getElementById('chatInput');
    const chatBox = document.getElementById('chatBox');
    const errorMessage = document.getElementById('errorMessage');

    // Focus the input field when the page loads
    chatInput.focus();

    // Function to add a new message to the chat box
    function addMessage(text, className) {
        const messageDiv = document.createElement('div');
        messageDiv.classList.add('message', className);
        
        const paragraph = document.createElement('p');
        paragraph.textContent = text;
        
        messageDiv.appendChild(paragraph);
        chatBox.appendChild(messageDiv);
        
        // Scroll to the bottom of the chat box
        chatBox.scrollTop = chatBox.scrollHeight;
        
        return messageDiv;
    }

    // Function to show loading indicator
    function showLoading() {
        const loadingDiv = document.createElement('div');
        loadingDiv.classList.add('loading');
        loadingDiv.id = 'loadingIndicator';
        
        for (let i = 0; i < 3; i++) {
            const dot = document.createElement('span');
            loadingDiv.appendChild(dot);
        }
        
        chatBox.appendChild(loadingDiv);
        chatBox.scrollTop = chatBox.scrollHeight;
        
        return loadingDiv;
    }

    // Function to hide loading indicator
    function hideLoading() {
        const loadingIndicator = document.getElementById('loadingIndicator');
        if (loadingIndicator) {
            loadingIndicator.remove();
        }
    }

    // Function to show error message
    function showError(text) {
        errorMessage.textContent = text;
        errorMessage.style.opacity = 1;
        
        // Hide error after 5 seconds
        setTimeout(() => {
            errorMessage.style.opacity = 0;
            setTimeout(() => {
                errorMessage.textContent = '';
            }, 500);
        }, 5000);
    }

    // Handle form submission
    chatForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const message = chatInput.value.trim();
        
        // Clear the input field
        chatInput.value = '';
        
        // Clear any previous error messages
        errorMessage.textContent = '';
        
        // If the message is empty, show an error
        if (!message) {
            showError('Please enter a message');
            return;
        }
        
        // Add the user message to the chat
        addMessage(message, 'user');
        
        // Show loading indicator
        showLoading();
        
        try {
            // Send the message to the server
            const response = await fetch(`/chat?message=${encodeURIComponent(message)}`);
            
            // Hide loading indicator
            hideLoading();
            
            if (response.ok) {
                const data = await response.json();
                
                // Add the bot's response to the chat
                if (data?.text) {
                    addMessage(data.text, 'bot');
                } else {
                    addMessage('I received an empty response. Please try again.', 'bot');
                }
            } else {
                // If the server returns an error, show it
                const errorText = await response.text();
                addMessage(`Error: ${errorText || response.statusText}`, 'system');
                console.error('Error:', response);
            }
        } catch (error) {
            // Hide loading indicator
            hideLoading();
            
            // If there's a network error, show it
            addMessage('Network error. Please check your connection and try again.', 'system');
            console.error('Error:', error);
        }
        
        // Focus the input field again
        chatInput.focus();
    });

    // Add keyboard shortcut (Ctrl+Enter or Cmd+Enter) to submit
    chatInput.addEventListener('keydown', (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
            const submitEvent = new Event('submit', { cancelable: true });
            chatForm.dispatchEvent(submitEvent);
        }
    });
});
