import { auth } from "@/auth"
import { NextResponse } from "next/server"
import type { NextRequest } from "next/server"

export default auth((req) => {
  const { nextUrl, auth: session } = req
  const isLoggedIn = !!session?.user
  
  // Define protected routes
  const protectedPaths = ["/board", "/boards"]
  const isProtectedRoute = protectedPaths.some(path => 
    nextUrl.pathname.startsWith(path)
  )
  
  // Redirect to sign in if accessing protected route without authentication
  if (isProtectedRoute && !isLoggedIn) {
    const redirectUrl = new URL("/auth/signin", nextUrl.origin)
    redirectUrl.searchParams.set("callbackUrl", nextUrl.href)
    return NextResponse.redirect(redirectUrl)
  }

  // Redirect authenticated users away from auth pages
  const authPaths = ["/auth/signin", "/auth/signup"]
  const isAuthRoute = authPaths.some(path => nextUrl.pathname.startsWith(path))
  
  if (isAuthRoute && isLoggedIn) {
    return NextResponse.redirect(new URL("/boards", nextUrl.origin))
  }

  return NextResponse.next()
})

export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - api/auth (NextAuth.js routes)
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     */
    "/((?!api/auth|_next/static|_next/image|favicon.ico).*)",
  ],
} 