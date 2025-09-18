// Development configuration
export const developmentConfig = {
  // API endpoints for development
  api: {
    baseUrl: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5205',
    developmentTokenEndpoint: '/api/Development/generate-token',
  },

  // Development features
  features: {
    mockAuth: true, // Enable mock authentication when backend is not available
    mockToken:
      'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c',
  },

  // Development warnings and messages
  messages: {
    backendUnavailable: 'Backend is not available. Using mock authentication for development.',
    developmentMode: 'Development Mode: Click "Sign In" to get a development token',
  },
};

// Helper function to get the full API URL
export const getApiUrl = (endpoint: string): string => {
  return `${developmentConfig.api.baseUrl}${endpoint}`;
};

// Helper function to check if we're in development mode
export const isDevelopment = (): boolean => {
  return process.env.NODE_ENV === 'development';
};
