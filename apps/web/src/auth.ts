import NextAuth from 'next-auth';
import type { NextAuthConfig } from 'next-auth';
import Credentials from 'next-auth/providers/credentials';
import Google from 'next-auth/providers/google';

export const config = {
  providers: [
    Credentials({
      name: 'credentials',
      credentials: {
        emailOrUsername: { label: 'Email or Username', type: 'text' },
        password: { label: 'Password', type: 'password' },
      },
      async authorize(credentials) {
        if (!credentials?.emailOrUsername || !credentials?.password) {
          return null;
        }

        try {
          const response = await fetch(
            `${process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5205/api'}/auth/login`,
            {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
              },
              body: JSON.stringify({
                emailOrUsername: credentials.emailOrUsername,
                password: credentials.password,
              }),
            }
          );

          if (!response.ok) {
            return null;
          }

          const userData = await response.json();

          if (userData?.user) {
            return {
              id: userData.user.id,
              email: userData.user.email,
              name: userData.user.username,
              role: userData.user.role || 'User',
            };
          }

          return null;
        } catch (error) {
          console.error('Authentication error:', error);
          return null;
        }
      },
    }),
    Google({
      clientId: process.env.AUTH_GOOGLE_ID!,
      clientSecret: process.env.AUTH_GOOGLE_SECRET!,
    }),
  ],
  session: {
    strategy: 'jwt',
    maxAge: 30 * 24 * 60 * 60, // 30 days
  },
  cookies: {
    sessionToken: {
      name: `next-auth.session-token`,
      options: {
        httpOnly: true,
        sameSite: 'lax',
        path: '/',
        secure: process.env.NODE_ENV === 'production',
      },
    },
  },
  pages: {
    signIn: '/sign-in',
    error: '/error',
  },
  callbacks: {
    async signIn({ user, account, profile }) {
      if (account?.provider === 'google' && user.email) {
        try {
          // Call our API to handle Google OAuth user
          const response = await fetch(
            `${process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5205/api'}/nextauth/google-oauth`,
            {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
              },
              body: JSON.stringify({
                sub: user.id,
                email: user.email,
                name: user.name,
                image: user.image,
                emailVerified: (user as any).emailVerified || false,
              }),
            }
          );

          if (response.ok) {
            const userData = await response.json();
            // Store user data in token for session
            user.id = userData.id;
            return true;
          } else {
            console.error('Failed to process Google OAuth user');
            return false;
          }
        } catch (error) {
          console.error('Error during Google OAuth sign in:', error);
          return false;
        }
      }
      return true;
    },
    async session({ session, token }) {
      if (session.user && token.sub) {
        session.user.id = token.sub;
        // Add role to session for RBAC
        if (token.role) {
          session.user.role = token.role as string;
        }
      }
      return session;
    },
    async jwt({ token, user, account }) {
      if (user) {
        token.id = user.id;
        // Store role in token for credentials provider
        if (user.role) {
          token.role = user.role;
        }
      }
      return token;
    },
  },
} satisfies NextAuthConfig;

export const { handlers, signIn, signOut, auth } = NextAuth(config);
