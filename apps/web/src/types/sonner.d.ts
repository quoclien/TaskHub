declare module 'sonner' {
  export interface ToastOptions {
    duration?: number;
    position?:
      | 'top-left'
      | 'top-center'
      | 'top-right'
      | 'bottom-left'
      | 'bottom-center'
      | 'bottom-right';
    className?: string;
    style?: React.CSSProperties;
    icon?: React.ReactNode;
  }

  export interface ToastPromiseOptions<T> extends ToastOptions {
    loading: string | React.ReactNode;
    success: string | ((data: T) => string | React.ReactNode);
    error: string | ((error: Error) => string | React.ReactNode);
  }

  export interface ToastFunction {
    (message: string | React.ReactNode, options?: ToastOptions): string | number;
    success: (message: string | React.ReactNode, options?: ToastOptions) => string | number;
    error: (message: string | React.ReactNode, options?: ToastOptions) => string | number;
    promise: <T>(promise: Promise<T>, options: ToastPromiseOptions<T>) => Promise<T>;
    dismiss: (toastId?: string | number) => void;
    remove: (toastId?: string | number) => void;
  }

  export interface ToasterProps {
    position?:
      | 'top-left'
      | 'top-center'
      | 'top-right'
      | 'bottom-left'
      | 'bottom-center'
      | 'bottom-right';
    richColors?: boolean;
    expand?: boolean;
    visibleToasts?: number;
    closeButton?: boolean;
    toastOptions?: ToastOptions;
    className?: string;
    style?: React.CSSProperties;
    offset?: string | number;
    theme?: 'light' | 'dark' | 'system';
    invert?: boolean;
    pauseWhenPageIsHidden?: boolean;
    cn?: (...classes: any[]) => string;
  }

  export const Toaster: React.ComponentType<ToasterProps>;
  export const toast: ToastFunction;
}
