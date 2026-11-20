import type { AsyncState } from "./async-state.js";

interface JournalEntry {
  readonly id: number;
  readonly title: string;
}

function describeJournalState(
  state: AsyncState<readonly JournalEntry[], Error>,
): string {
  switch (state.status) {
    case "loading":
      return "Loading journal entries...";
    case "success":
      return state.data.length === 0
        ? "No journal entries found."
        : state.data.map((entry) => `- ${entry.title}`).join("\n");
    case "error":
      return `Could not load journal entries: ${state.error.message}`;
    default:
      return assertNever(state);
  }
}

function assertNever(value: never): never {
  throw new Error(`Unhandled async state: ${JSON.stringify(value)}`);
}

const journalStates: readonly AsyncState<readonly JournalEntry[], Error>[] = [
  { status: "loading" },
  {
    status: "success",
    data: [
      { id: 1, title: "A quiet morning walk" },
      { id: 2, title: "Dinner with a friend" },
    ],
  },
  { status: "error", error: new Error("The journal service is unavailable.") },
];

for (const state of journalStates) {
  console.log(describeJournalState(state));
}
