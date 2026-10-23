import type { AsyncState, Connection } from "../types/connection";

interface ConnectionListProps {
  state: AsyncState<Connection[]>;
  onRetry: () => void;
  onRemove: (id: string) => void;
}

export function ConnectionList({
  state,
  onRetry,
  onRemove,
}: ConnectionListProps) {
  if (state.status === "loading") {
    return (
      <div className="state-card" role="status" aria-live="polite">
        <span className="state-indicator" aria-hidden="true" />
        <div>
          <p className="eyebrow">Loading your list</p>
          <p className="state-copy">Gathering the people you want to remember.</p>
        </div>
      </div>
    );
  }

  if (state.status === "error") {
    return (
      <div className="state-card state-card--error" role="alert">
        <span className="state-icon" aria-hidden="true">!</span>
        <div className="state-content">
          <p className="eyebrow">The list could not be loaded</p>
          <p className="state-copy">{state.message}</p>
          <button className="button button--small" type="button" onClick={onRetry}>
            Try again
          </button>
        </div>
      </div>
    );
  }

  if (state.data.length === 0) {
    return (
      <div className="empty-state" role="status">
        <p className="eyebrow">A fresh start</p>
        <p>Your list is empty. Add someone you would like to reconnect with.</p>
      </div>
    );
  }

  return (
    <ul className="connection-list" aria-label="People to reconnect with">
      {state.data.map((connection, index) => (
        <li className="connection-card" key={connection.id}>
          <span className="connection-number" aria-hidden="true">
            {String(index + 1).padStart(2, "0")}
          </span>
          <div className="connection-details">
            <h3>{connection.name}</h3>
            <p>{connection.reason}</p>
          </div>
          <button
            className="remove-button"
            type="button"
            aria-label={`Remove ${connection.name} from the list`}
            onClick={() => onRemove(connection.id)}
          >
            Remove
          </button>
        </li>
      ))}
    </ul>
  );
}
