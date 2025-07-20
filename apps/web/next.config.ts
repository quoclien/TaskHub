import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  transpilePackages: ['@taskhub/shared'],
  eslint: {
    // Disable Next.js built-in ESLint to use our custom configuration
    ignoreDuringBuilds: true,
  },
};

export default nextConfig;
