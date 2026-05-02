# XAML Language Server

XAML language support for WinUI / Windows App SDK projects, powered by the Microsoft XAML Compiler.

## Features

- Syntax highlighting for `.xaml` files
- Diagnostics (errors and warnings) via the XAML Compiler
- Hover information
- Code completion

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (for the language server backend)
- [Node.js](https://nodejs.org/) 18+
- [Bun](https://bun.sh/) (package manager)

## Development

```bash
# Install dependencies
bun install

# Build the extension
bun run compile

# Watch mode (auto-rebuild on changes)
bun run watch

# Run tests (requires .NET server to be built first)
bun run test

# Lint
bun run lint

# Package for distribution
bun run package
```

Press **F5** in VS Code to launch the extension in the Extension Development Host.

## Build Pipeline

The extension consists of two parts:

1. **TypeScript client** (`src/extension.ts`) — bundled with esbuild into `dist/extension.js`
2. **.NET LSP server** (`server/`) — built via `scripts/prepare-server.js`

The `vscode:prepublish` hook runs the full pipeline: .NET server build → type checking → linting → esbuild bundling.

## License

MIT
