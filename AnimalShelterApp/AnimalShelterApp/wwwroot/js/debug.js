window.blazorDebug = {
    logAuthToken: function() {
        const token = localStorage.getItem('authToken');
        if (token) {
            console.log('Auth token found:', token.substring(0, 20) + '...');
            return true;
        } else {
            console.log('No auth token found in localStorage');
            return false;
        }
    },
    
    logUserProfile: function() {
        const profile = localStorage.getItem('userProfile');
        if (profile) {
            console.log('User profile found:', JSON.parse(profile));
            return true;
        } else {
            console.log('No user profile found in localStorage');
            return false;
        }
    },
    
    clearAuth: function() {
        localStorage.removeItem('authToken');
        localStorage.removeItem('userProfile');
        console.log('Auth data cleared');
    },
    
    logFirebaseError: function(error) {
        console.error('Firebase Error Details:', error);
    }
};

// Global error handler for fetch requests
const originalFetch = window.fetch;
window.fetch = function(...args) {
    return originalFetch.apply(this, args)
        .then(response => {
            if (!response.ok && args[0].includes('firestore.googleapis.com')) {
                console.error('Firestore request failed:', {
                    url: args[0],
                    status: response.status,
                    statusText: response.statusText
                });
            }
            return response;
        });
};
