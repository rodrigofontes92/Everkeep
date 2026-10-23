# TypeScript Async State Exercise

This standalone exercise models the three common outcomes of an asynchronous operation with a discriminated union:

- `loading`, before a result is available;
- `success`, with strongly typed data;
- `error`, with a typed error value.

The example narrows the union by its `status` field and handles every state exhaustively. It is independent of the Everkeep ASP.NET Core MVC application.

## Requirements

- Node.js 22 or newer
- npm

## Run

From this directory, install the locked dependencies, check the types, build the example, and run it:

```powershell
npm ci
npm run check
npm run build
npm start
```

The example prints loading, success, and error output for journal entries.
