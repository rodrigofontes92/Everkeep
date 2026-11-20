# React Typed List and Form Exercise

This standalone React and TypeScript exercise practices typed component props, a small form, and accessible interface states. The example keeps a local list of people to reconnect with; it does not call a backend or change the Everkeep MVC application.

## Requirements

- Node.js 22.12 or newer
- npm

## Run

From this directory, install the locked dependencies and start the Vite development server:

```powershell
npm ci
npm run dev
```

Run the type check and production build with:

```powershell
npm run check
npm run build
```

## What to explore

- `ConnectionListProps` and `ConnectionFormProps` type each component's inputs and callbacks.
- `AsyncState<T>` models loading, error, and success as a discriminated union.
- The form uses associated labels, native required-field validation, keyboard-operable controls, and a live status message.
- Expand **Explore the interface states** to trigger example save and loading errors. Retry the list request to return to its success state.
- The list supports adding and removing entries in memory; reloading the page restores the sample data.

For React's TypeScript guidance, see the [official React documentation](https://react.dev/learn/typescript). The app is built with [Vite](https://vite.dev/guide/).
