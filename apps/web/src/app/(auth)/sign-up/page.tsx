'use client';

import { Eye, EyeOff, UserPlus, Loader2 } from 'lucide-react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useState } from 'react';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useAuthStore } from '@/hooks/useAuthStore';

const SignUpPage = () => {
  const router = useRouter();
  const { setToken } = useAuthStore();
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [error, setError] = useState('');

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSignUp = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    // Basic validation
    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match');
      setIsLoading(false);
      return;
    }

    if (formData.password.length < 6) {
      setError('Password must be at least 6 characters long');
      setIsLoading(false);
      return;
    }

    try {
      // In development, redirect to sign-in with success message
      if (process.env.NODE_ENV === 'development') {
        // Simulate API delay
        await new Promise((resolve) => setTimeout(resolve, 1000));
        router.push('/sign-in?message=account-created');
        return;
      }

      // In production, use real registration
      const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          password: formData.password,
        }),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Registration failed');
      }

      const { token } = await response.json();
      setToken(token);
      router.push('/boards');
    } catch (error) {
      console.error('Sign-up failed:', error);
      setError(error instanceof Error ? error.message : 'Sign-up failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className='min-h-screen bg-gradient-to-br from-green-50 to-emerald-100 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center px-4 sm:px-6 lg:px-8'>
      <div className='max-w-md w-full space-y-8'>
        <div className='text-center'>
          <div className='mx-auto h-16 w-16 bg-green-600 rounded-full flex items-center justify-center mb-4'>
            <UserPlus className='h-8 w-8 text-white' />
          </div>
          <h2 className='text-3xl font-bold text-gray-900 dark:text-gray-100'>
            Create your account
          </h2>
          <p className='mt-2 text-sm text-gray-600 dark:text-gray-400'>
            Get started with TaskHub today
          </p>
        </div>

        <form
          className='mt-8 space-y-6'
          onSubmit={handleSignUp}
        >
          <div className='bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 space-y-4'>
            {error && (
              <div className='bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md p-3'>
                <p className='text-sm text-red-600 dark:text-red-400'>{error}</p>
              </div>
            )}

            {process.env.NODE_ENV === 'development' && (
              <div className='bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-md p-3'>
                <p className='text-sm text-green-600 dark:text-green-400'>
                  <strong>Development Mode:</strong> Fill out the form and click "Sign Up" to
                  simulate account creation
                </p>
              </div>
            )}

            <div className='grid grid-cols-2 gap-4'>
              <div>
                <label
                  htmlFor='firstName'
                  className='block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'
                >
                  First name
                </label>
                <Input
                  id='firstName'
                  name='firstName'
                  type='text'
                  autoComplete='given-name'
                  required
                  value={formData.firstName}
                  onChange={handleChange}
                  placeholder='John'
                  className='w-full'
                />
              </div>

              <div>
                <label
                  htmlFor='lastName'
                  className='block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'
                >
                  Last name
                </label>
                <Input
                  id='lastName'
                  name='lastName'
                  type='text'
                  autoComplete='family-name'
                  required
                  value={formData.lastName}
                  onChange={handleChange}
                  placeholder='Doe'
                  className='w-full'
                />
              </div>
            </div>

            <div>
              <label
                htmlFor='email'
                className='block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'
              >
                Email address
              </label>
              <Input
                id='email'
                name='email'
                type='email'
                autoComplete='email'
                required
                value={formData.email}
                onChange={handleChange}
                placeholder='john.doe@example.com'
                className='w-full'
              />
            </div>

            <div>
              <label
                htmlFor='password'
                className='block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'
              >
                Password
              </label>
              <div className='relative'>
                <Input
                  id='password'
                  name='password'
                  type={showPassword ? 'text' : 'password'}
                  autoComplete='new-password'
                  required
                  value={formData.password}
                  onChange={handleChange}
                  placeholder='Create a password'
                  className='w-full pr-12'
                />
                <button
                  type='button'
                  className='absolute inset-y-0 right-0 pr-3 flex items-center'
                  onClick={() => {
                    setShowPassword(!showPassword);
                  }}
                >
                  {showPassword ? (
                    <EyeOff className='h-4 w-4 text-gray-400' />
                  ) : (
                    <Eye className='h-4 w-4 text-gray-400' />
                  )}
                </button>
              </div>
            </div>

            <div>
              <label
                htmlFor='confirmPassword'
                className='block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1'
              >
                Confirm password
              </label>
              <div className='relative'>
                <Input
                  id='confirmPassword'
                  name='confirmPassword'
                  type={showConfirmPassword ? 'text' : 'password'}
                  autoComplete='new-password'
                  required
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  placeholder='Confirm your password'
                  className='w-full pr-12'
                />
                <button
                  type='button'
                  className='absolute inset-y-0 right-0 pr-3 flex items-center'
                  onClick={() => {
                    setShowConfirmPassword(!showConfirmPassword);
                  }}
                >
                  {showConfirmPassword ? (
                    <EyeOff className='h-4 w-4 text-gray-400' />
                  ) : (
                    <Eye className='h-4 w-4 text-gray-400' />
                  )}
                </button>
              </div>
            </div>

            <div className='flex items-center'>
              <input
                id='terms'
                name='terms'
                type='checkbox'
                required
                className='h-4 w-4 text-green-600 focus:ring-green-500 border-gray-300 rounded'
              />
              <label
                htmlFor='terms'
                className='ml-2 block text-sm text-gray-700 dark:text-gray-300'
              >
                I agree to the{' '}
                <Link
                  href='/terms'
                  className='text-green-600 hover:text-green-500 dark:text-green-400 dark:hover:text-green-300'
                >
                  Terms of Service
                </Link>{' '}
                and{' '}
                <Link
                  href='/privacy'
                  className='text-green-600 hover:text-green-500 dark:text-green-400 dark:hover:text-green-300'
                >
                  Privacy Policy
                </Link>
              </label>
            </div>

            <Button
              type='submit'
              disabled={isLoading}
              className='w-full flex justify-center items-center space-x-2 bg-green-600 hover:bg-green-700'
            >
              {isLoading ? (
                <>
                  <Loader2 className='w-4 h-4 animate-spin' />
                  <span>Creating account...</span>
                </>
              ) : (
                <>
                  <UserPlus className='w-4 h-4' />
                  <span>Sign up</span>
                </>
              )}
            </Button>
          </div>

          <div className='text-center'>
            <p className='text-sm text-gray-600 dark:text-gray-400'>
              Already have an account?{' '}
              <Link
                href='/sign-in'
                className='text-green-600 hover:text-green-500 dark:text-green-400 dark:hover:text-green-300 font-medium'
              >
                Sign in
              </Link>
            </p>
          </div>
        </form>
      </div>
    </div>
  );
};

export default SignUpPage;
