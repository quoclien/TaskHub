import { http, HttpResponse, delay } from 'msw';

// Mock data
let mockTasks = [
  { id: '1', title: 'Test Task 1', completed: false },
  { id: '2', title: 'Test Task 2', completed: true },
];

// Define request handlers
export const handlers = [
  // Get all tasks
  http.get('/api/tasks', async () => {
    await delay(150); // Simulate network delay
    return HttpResponse.json(mockTasks);
  }),
  
  // Create a new task
  http.post('/api/tasks', async ({ request }) => {
    const newTask = await request.json();
    
    // Ensure newTask is an object
    if (!newTask || typeof newTask !== 'object' || Array.isArray(newTask)) {
      return new Response(JSON.stringify({ error: 'Invalid task data' }), {
        status: 400,
        headers: {
          'Content-Type': 'application/json',
        },
      });
    }
    
    const task = {
      id: Math.random().toString(36).substr(2, 9),
      completed: false, // default value
      title: '', // default value
      ...newTask, // will override defaults with any provided values
    };
    mockTasks.push(task);
    await delay(150);
    return HttpResponse.json(task);
  }),
  
  // Update a task
  http.put('/api/tasks/:id', async ({ params, request }) => {
    const { id } = params;
    const taskIndex = mockTasks.findIndex((t) => t.id === id);
    
    if (taskIndex === -1) {
      return new HttpResponse(
        JSON.stringify({ error: 'Task not found' }), 
        { status: 404 }
      );
    }
    
    const updates = await request.json();
    
    // Ensure updates is a non-null object
    if (!updates || typeof updates !== 'object' || Array.isArray(updates)) {
      return new HttpResponse(
        JSON.stringify({ error: 'Invalid update data' }),
        { status: 400, headers: { 'Content-Type': 'application/json' } }
      );
    }
    
    // Create updated task safely
    const currentTask = mockTasks[taskIndex];
    const updatedTask = {
      ...currentTask,
      ...Object.fromEntries(
        Object.entries(updates).filter(([key]) => key in currentTask)
      )
    };
    
    mockTasks[taskIndex] = updatedTask;
    
    await delay(150);
    return HttpResponse.json(updatedTask);
  }),
  
  // Delete a task
  http.delete('/api/tasks/:id', async ({ params }) => {
    const { id } = params;
    const taskIndex = mockTasks.findIndex((t) => t.id === id);
    
    if (taskIndex === -1) {
      return new HttpResponse(
        JSON.stringify({ error: 'Task not found' }), 
        { status: 404 }
      );
    }
    
    mockTasks.splice(taskIndex, 1);
    await delay(150);
    return HttpResponse.json({ success: true });
  }),
];

// Browser-specific exports (only in browser environment)
if (typeof window !== 'undefined') {
  import('msw/browser').then(({ setupWorker }) => {
    const worker = setupWorker(...handlers);
    // @ts-ignore - Make worker available globally for browser tests
    window.msw = { worker };
  }).catch(console.error);
}

export const resetMockTasks = () => {
  mockTasks = [
    { id: '1', title: 'Test Task 1', completed: false },
    { id: '2', title: 'Test Task 2', completed: true },
  ];
};
