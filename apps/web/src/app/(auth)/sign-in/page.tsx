"use client";

import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/hooks/useAuthStore';

const SignInPage = () => {
  const router = useRouter();
  const { setToken } = useAuthStore();

  const handleSignIn = async () => {
    try {
      // In a real app, you'd send credentials. Here, we just get a dev token.
      const response = await fetch('http://localhost:5204/api/Development/generate-token');
      if (!response.ok) {
        throw new Error('Failed to get token');
      }
      const { token } = await response.json();
      setToken(token);
      router.push('/boards');
    } catch (error) {
      console.error('Sign-in failed:', error);
      // Handle error state in UI
    }
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen">
      <h1 className="text-2xl font-bold mb-4">Sign In</h1>
      <button
        onClick={handleSignIn}
        className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
      >
        Sign In (Dev)
      </button>
    </div>
  );
};

export default SignInPage;
