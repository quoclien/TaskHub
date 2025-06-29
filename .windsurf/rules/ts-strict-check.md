---
trigger: always_on
---

on: codegen
run: tsc --noEmit
onError: "@cascade Explain and Fix TypeScript errors"