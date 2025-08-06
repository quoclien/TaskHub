'use client';

import { Eye, EyeOff, Loader2 } from 'lucide-react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useState, useEffect, Suspense } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useAuth } from '@/hooks/useAuth';
import { signIn } from 'next-auth/react';

const credentialsSchema = z.object({
  emailOrUsername: z.string().min(1, 'Email or username is required'),
  password: z.string().min(1, 'Password is required'),
});

type CredentialsForm = z.infer<typeof credentialsSchema>;

function SignInContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [isCredentialsLoading, setIsCredentialsLoading] = useState(false);
  const [error, setError] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CredentialsForm>({
    resolver: zodResolver(credentialsSchema),
  });
  
  const callbackUrl = searchParams.get('callbackUrl') || '/boards';

  useEffect(() => {
    if (isAuthenticated && !authLoading) {
      router.push(callbackUrl);
    }
  }, [isAuthenticated, authLoading, router, callbackUrl]);

  const handleCredentialsSubmit = async (data: CredentialsForm) => {
    setIsCredentialsLoading(true);
    setError('');
    
    try {
      const result = await signIn('credentials', {
        emailOrUsername: data.emailOrUsername,
        password: data.password,
        callbackUrl,
        redirect: false,
      });
      
      if (result?.error) {
        setError('Invalid email/username or password. Please try again.');
      } else if (result?.ok) {
        router.push(callbackUrl);
      }
    } catch (error) {
      console.error('Credentials sign-in failed:', error);
      setError('Failed to sign in. Please try again.');
    } finally {
      setIsCredentialsLoading(false);
    }
  };

  const handleGoogleSignIn = async () => {
    setIsLoading(true);
    setError('');
    
    try {
      const result = await signIn('google', { 
        callbackUrl,
        redirect: false 
      });
      
      if (result?.error) {
        setError('Failed to sign in with Google. Please try again.');
      }
    } catch (error) {
      console.error('Google sign-in failed:', error);
      setError('Failed to sign in with Google. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className='min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center px-4 sm:px-6 lg:px-8'>
      <div className='max-w-md w-full space-y-8'>
        <div className='text-center'>
          <div className='mx-auto h-16 w-16 bg-blue-600 rounded-full flex items-center justify-center mb-4'>
            <svg
              className='h-8 w-8 text-white'
              fill='none'
              stroke='currentColor'
              viewBox='0 0 24 24'
            >
              <path
                strokeLinecap='round'
                strokeLinejoin='round'
                strokeWidth={2}
                d='M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1'
              />
            </svg>
          </div>
          <h2 className='text-3xl font-bold text-gray-900 dark:text-gray-100'>Welcome back</h2>
          <p className='mt-2 text-sm text-gray-600 dark:text-gray-400'>
            Sign in to your TaskHub account
          </p>
        </div>

        <div className='mt-8 space-y-6'>
          <div className='bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 space-y-6'>
            {error && (
              <div className='bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-3'>
                <p className='text-sm text-red-600 dark:text-red-400'>{error}</p>
              </div>
            )}

            {/* Credentials Form */}
            <form onSubmit={handleSubmit(handleCredentialsSubmit)} className='space-y-4'>
              <div className='space-y-2'>
                <Label htmlFor='emailOrUsername' className='text-sm font-medium text-gray-700 dark:text-gray-300'>
                  Email or Username
                </Label>
                <Input
                  id='emailOrUsername'
                  type='text'
                  autoComplete='username'
                  required
                  className='w-full'
                  placeholder='Enter your email or username'
                  {...register('emailOrUsername')}
                />
                {errors.emailOrUsername && (
                  <p className='text-sm text-red-600 dark:text-red-400'>
                    {errors.emailOrUsername.message}
                  </p>
                )}
              </div>

              <div className='space-y-2'>
                <Label htmlFor='password' className='text-sm font-medium text-gray-700 dark:text-gray-300'>
                  Password
                </Label>
                <div className='relative'>
                  <Input
                    id='password'
                    type={showPassword ? 'text' : 'password'}
                    autoComplete='current-password'
                    required
                    className='w-full pr-10'
                    placeholder='Enter your password'
                    {...register('password')}
                  />
                  <button
                    type='button'
                    className='absolute inset-y-0 right-0 pr-3 flex items-center'
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? (
                      <EyeOff className='h-4 w-4 text-gray-400' />
                    ) : (
                      <Eye className='h-4 w-4 text-gray-400' />
                    )}
                  </button>
                </div>
                {errors.password && (
                  <p className='text-sm text-red-600 dark:text-red-400'>
                    {errors.password.message}
                  </p>
                )}
              </div>

              <Button
                type='submit'
                disabled={isCredentialsLoading || authLoading}
                className='w-full bg-blue-600 hover:bg-blue-700 text-white'
              >
                {isCredentialsLoading ? (
                  <>
                    <Loader2 className='w-4 h-4 mr-2 animate-spin' />
                    Signing in...
                  </>
                ) : (
                  'Sign in'
                )}
              </Button>
            </form>

            {/* Divider */}
            <div className='relative'>
              <div className='absolute inset-0 flex items-center'>
                <div className='w-full border-t border-gray-300 dark:border-gray-600' />
              </div>
              <div className='relative flex justify-center text-sm'>
                <span className='px-2 bg-white dark:bg-gray-800 text-gray-500 dark:text-gray-400'>
                  or continue with
                </span>
              </div>
            </div>

            {/* Google Sign In */}
            <Button
              onClick={handleGoogleSignIn}
              disabled={isLoading || authLoading}
              className='w-full flex justify-center items-center space-x-3 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 border border-gray-300 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-600'
              variant="outline"
            >
              {isLoading ? (
                <>
                  <Loader2 className='w-5 h-5 animate-spin' />
                  <span>Signing in...</span>
                </>
              ) : (
                <>
                  <svg className='w-5 h-5' viewBox='0 0 24 24'>
                    <path
                      fill='#4285F4'
                      d='M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z'
                    />
                    <path
                      fill='#34A853'
                      d='M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z'
                    />
                    <path
                      fill='#FBBC05'
                      d='M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z'
                    />
                    <path
                      fill='#EA4335'
                      d='M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z'
                    />
                  </svg>
                  <span>Continue with Google</span>
                </>
              )}
            </Button>


          </div>

          <div className='text-center'>
            <p className='text-sm text-gray-600 dark:text-gray-400'>
              By signing in, you agree to our terms of service and privacy policy.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default function SignInPage() {
  return (
    <Suspense fallback={
      <div className='min-h-screen flex items-center justify-center'>
        <div className='animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600'></div>
      </div>
    }>
      <SignInContent />
    </Suspense>
  );
}
