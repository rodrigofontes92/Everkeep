import { useState, type FormEvent } from "react";
import type { ConnectionDraft } from "../types/connection";

interface ConnectionFormProps {
  onAdd: (connection: ConnectionDraft) => void;
  onSimulateLoadError: () => void;
  isLoading: boolean;
}

type SubmissionState =
  | { status: "idle" }
  | { status: "saving" }
  | { status: "success"; message: string }
  | { status: "error"; message: string };

const saveDelay = (milliseconds: number) =>
  new Promise<void>((resolve) => window.setTimeout(resolve, milliseconds));

export function ConnectionForm({
  onAdd,
  onSimulateLoadError,
  isLoading,
}: ConnectionFormProps) {
  const [name, setName] = useState("");
  const [reason, setReason] = useState("");
  const [simulateSaveError, setSimulateSaveError] = useState(false);
  const [submission, setSubmission] = useState<SubmissionState>({ status: "idle" });

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedName = name.trim();
    const trimmedReason = reason.trim();

    if (!trimmedName || !trimmedReason) {
      setSubmission({
        status: "error",
        message: "Enter a name and a reason before adding to your list.",
      });
      return;
    }

    setSubmission({ status: "saving" });
    await saveDelay(450);

    if (simulateSaveError) {
      setSubmission({
        status: "error",
        message: "The demo could not save this entry. Turn off the simulated error and try again.",
      });
      return;
    }

    onAdd({ name: trimmedName, reason: trimmedReason });
    setName("");
    setReason("");
    setSimulateSaveError(false);
    setSubmission({ status: "success", message: `${trimmedName} was added to your list.` });
  }

  const feedback =
    submission.status === "saving"
      ? "Saving your entry…"
      : submission.status === "success" || submission.status === "error"
        ? submission.message
        : "";

  return (
    <section className="form-panel" aria-labelledby="form-heading">
      <div className="section-heading section-heading--form">
        <div>
          <p className="eyebrow">Make a small plan</p>
          <h2 id="form-heading">Who comes to mind?</h2>
        </div>
        <span className="heading-mark" aria-hidden="true">↗</span>
      </div>

      <form className="connection-form" onSubmit={handleSubmit}>
        <div className="field-group">
          <label htmlFor="connection-name">Their name</label>
          <input
            autoComplete="name"
            id="connection-name"
            maxLength={70}
            name="name"
            onChange={(event) => setName(event.currentTarget.value)}
            placeholder="For example, Alex"
            required
            value={name}
          />
        </div>

        <div className="field-group">
          <label htmlFor="connection-reason">What would you like to remember?</label>
          <textarea
            id="connection-reason"
            maxLength={180}
            name="reason"
            onChange={(event) => setReason(event.currentTarget.value)}
            placeholder="A shared laugh, a kind message, a catch-up you keep meaning to plan…"
            required
            rows={3}
            value={reason}
          />
          <span className="field-hint">Keep it personal and just for you.</span>
        </div>

        <button
          className="button button--primary"
          disabled={submission.status === "saving"}
          type="submit"
        >
          {submission.status === "saving" ? "Saving…" : "Add to my list"}
          <span aria-hidden="true">＋</span>
        </button>

        <p
          className={`form-feedback${submission.status === "error" ? " form-feedback--error" : ""}`}
          aria-atomic="true"
          aria-live={submission.status === "error" ? "assertive" : "polite"}
          role={submission.status === "error" ? "alert" : "status"}
        >
          {feedback}
        </p>
      </form>

      <details className="demo-controls">
        <summary>Explore the interface states</summary>
        <div className="demo-options">
          <label className="checkbox-label" htmlFor="simulate-save-error">
            <input
              checked={simulateSaveError}
              id="simulate-save-error"
              onChange={(event) => setSimulateSaveError(event.currentTarget.checked)}
              type="checkbox"
            />
            Simulate a save error
          </label>
          <button
            className="text-button"
            disabled={isLoading}
            onClick={onSimulateLoadError}
            type="button"
          >
            Show the list error state
          </button>
        </div>
      </details>
    </section>
  );
}
