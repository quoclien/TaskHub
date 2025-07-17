# Common TaskHub Issues & Solutions

## C# LSP Issues
**Problem**: DTOs showing 0 references, no intellisense dropdown
**Causes**: 
- IDE indexing out of sync
- Build artifacts corruption
- Language server needs refresh

**Solutions**:
1. Restart C# language server
2. Clean and rebuild: `dotnet clean && dotnet build`
3. Delete bin/obj folders and rebuild
4. Restart IDE/OmniSharp

## Windows Development
- Use uppercase drive letters (D:\...) to avoid case-sensitivity issues
- Use PowerShell-compatible commands (avoid && operators)
- Redirect output to logs for troubleshooting