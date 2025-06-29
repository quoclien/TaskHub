---
trigger: always_on
---

1. ❗️ **Monorepo dependency sync**  
After any change to `package.json`, always run:
`pnpm install`
This ensures new dependencies are fetched and TypeScript & ESLint pick them up.

2. ✅ **Strict-mode type checks**  
Always run:
`pnpm run typecheck`
or:
`tsc --noEmit`
to enforce TypeScript strict mode before merging or writing further code.

3. ⚠️ **Lint & auto-fix**  
Ensure ESLint is set up for all packages with strict rules. Cascade will auto-fix lint errors, but running:
`pnpm run lint --fix`
manually helps catch audit issues.

4. 🧪 **Vitest usage**  
- Vitest watches files by default (`pnpm run test` remains in watch mode).
- Use `pnpm run test:run` to execute tests once and exit with pass/fail — ideal for CI and pre-commit.

5. 💾 **Test-driven development**  
- Always write a failing test before implementing code.
- Follow Red → Green → Refactor for all new features.

6. ⚙️ **Cascade triggers**  
On code generation or workspace changes:
- Run typecheck (2).
- If type errors occur, Cascade should “Explain and Fix” them.
- Run lint and auto-fix before proceeding.

7. 👁️‍🗨️ **Stay lean & consistent**  
- No new packages without explicit approval.
- Maintain consistent versions across packages (`pnpm up -r` if needed).

8. 🔁 **CI enforcement**  
Ensure GitHub Actions or equivalent runs `pnpm install`, typecheck, lint, and `pnpm run test:run` across all packages before merging.