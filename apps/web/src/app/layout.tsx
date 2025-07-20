import type { Metadata } from 'next';
import { Geist, Geist_Mono } from 'next/font/google';

import './globals.css';
import { QueryProvider } from '@/components/providers/QueryProvider';

const geistSans = Geist({
  variable: '--font-geist-sans',
  subsets: ['latin'],
});

const geistMono = Geist_Mono({
  variable: '--font-geist-mono',
  subsets: ['latin'],
});

export const metadata: Metadata = {
  title: 'TaskHub - Modern Task Management',
  description:
    'Organize your projects and tasks efficiently with TaskHub, a modern Trello-like task management application with real-time collaboration features.',
  openGraph: {
    title: 'TaskHub - Modern Task Management',
    description:
      'Organize your projects and tasks efficiently with TaskHub, a modern Trello-like task management application with real-time collaboration features.',
    type: 'website',
    images: [
      {
        url: '/vercel.svg',
        width: 1200,
        height: 630,
        alt: 'TaskHub',
      },
    ],
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang='en'>
      <body className={`${geistSans.variable} ${geistMono.variable} antialiased`}>
        <QueryProvider>{children}</QueryProvider>
      </body>
    </html>
  );
}
