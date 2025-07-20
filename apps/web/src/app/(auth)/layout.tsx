import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Authentication - TaskHub',
  description: 'Sign in or create your TaskHub account to start managing your tasks.',
};

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return children;
}
