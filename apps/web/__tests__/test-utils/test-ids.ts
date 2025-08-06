// Test IDs for elements that are frequently interacted with in tests
// These should match the data-testid attributes in the application code

export const TestIds = {
  // Auth
  signInForm: 'sign-in-form',
  signUpForm: 'sign-up-form',
  emailInput: 'email-input',
  passwordInput: 'password-input',
  submitButton: 'submit-button',
  
  // Navigation
  mainNavigation: 'main-navigation',
  userMenu: 'user-menu',
  signOutButton: 'sign-out-button',
  
  // Boards
  boardList: 'board-list',
  createBoardButton: 'create-board-button',
  boardCard: (id: string) => `board-card-${id}`,
  boardTitle: 'board-title',
  
  // Lists
  listContainer: 'list-container',
  listTitle: 'list-title',
  addListButton: 'add-list-button',
  listForm: 'list-form',
  
  // Cards
  cardItem: (id: string) => `card-item-${id}`,
  cardTitle: 'card-title',
  cardDescription: 'card-description',
  addCardButton: 'add-card-button',
  cardForm: 'card-form',
  
  // Modals
  modal: 'modal',
  modalTitle: 'modal-title',
  modalCloseButton: 'modal-close-button',
  modalConfirmButton: 'modal-confirm-button',
  
  // Loading states
  loadingIndicator: 'loading-indicator',
  skeletonLoader: 'skeleton-loader',
  
  // Error states
  errorMessage: 'error-message',
  retryButton: 'retry-button',
  
  // Empty states
  emptyState: 'empty-state',
  emptyStateAction: 'empty-state-action',
} as const;

// Helper function to generate test IDs for dynamic content
export const getTestId = (baseId: string, suffix?: string | number) => {
  return suffix !== undefined ? `${baseId}-${suffix}` : baseId;
};

// Type-safe test ID accessor
type TestIdKey = keyof typeof TestIds;
type TestIdValue<T extends TestIdKey> = T extends keyof typeof TestIds 
  ? typeof TestIds[T] 
  : (suffix: string | number) => string;

export const getTestId = <T extends TestIdKey>(key: T, suffix?: string | number): TestIdValue<T> => {
  const value = TestIds[key];
  return (typeof value === 'function' && suffix !== undefined 
    ? value(suffix) 
    : value) as TestIdValue<T>;
};
