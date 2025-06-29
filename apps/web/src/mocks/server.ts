import { setupServer } from 'msw/node';
import { handlers } from './handlers';

// Setup requests interception using the given request handlers
export const server = setupServer(...handlers);

// Export the server instance
export default server;

// Export a function to reset the mock data between tests
export { resetMockTasks } from './handlers';
