import { useEffect, useState } from "react";
import { ConnectionForm } from "./components/ConnectionForm";
import { ConnectionList } from "./components/ConnectionList";
import type { AsyncState, Connection, ConnectionDraft } from "./types/connection";

const sampleConnections: Connection[] = [
  {
    id: "person-01",
    name: "Maya Chen",
    reason: "She always made the long train ride feel shorter.",
  },
  {
    id: "person-02",
    name: "Theo Martin",
    reason: "We promised to trade book recommendations.",
  },
  {
    id: "person-03",
    name: "Samira Costa",
    reason: "I still smile when I think about that rainy afternoon.",
  },
];

const wait = (milliseconds: number) =>
  new Promise<void>((resolve) => window.setTimeout(resolve, milliseconds));

export default function App() {
  const [connections, setConnections] = useState(sampleConnections);
  const [listState, setListState] = useState<AsyncState<Connection[]>>({
    status: "loading",
  });

  useEffect(() => {
    const initialLoad = window.setTimeout(() => {
      setListState({ status: "success", data: sampleConnections });
    }, 650);

    return () => window.clearTimeout(initialLoad);
  }, []);

  async function loadConnections(simulateError = false) {
    setListState({ status: "loading" });
    await wait(650);

    if (simulateError) {
      setListState({
        status: "error",
        message: "Your saved connections are temporarily unavailable. Try loading them again.",
      });
      return;
    }

    setListState({ status: "success", data: connections });
  }

  function addConnection(draft: ConnectionDraft) {
    const nextConnections = [{ id: crypto.randomUUID(), ...draft }, ...connections];
    setConnections(nextConnections);
    setListState({ status: "success", data: nextConnections });
  }

  function removeConnection(id: string) {
    const nextConnections = connections.filter((connection) => connection.id !== id);
    setConnections(nextConnections);
    setListState({ status: "success", data: nextConnections });
  }

  return (
    <main className="page-shell">
      <header className="topbar">
        <a className="wordmark" href="#top" aria-label="Keep in touch home">
          <span className="wordmark-icon" aria-hidden="true">e</span>
          <span>everkeep</span>
        </a>
        <span className="topbar-note">A little space for what matters</span>
      </header>

      <section className="hero" id="top" aria-labelledby="page-title">
        <div className="hero-copy">
          <p className="eyebrow">A gentle reminder</p>
          <h1 id="page-title">Some people are worth <em>coming back to.</em></h1>
          <p className="hero-description">
            Keep a small list of people you would love to reconnect with, and the
            moments that make you think of them.
          </p>
        </div>
        <div className="hero-art" aria-hidden="true">
          <span className="orbit orbit--one" />
          <span className="orbit orbit--two" />
          <span className="orbit orbit--three" />
          <span className="hero-spark">✳</span>
          <span className="hero-caption">made of moments</span>
        </div>
      </section>

      <div className="content-grid">
        <section className="list-panel" aria-labelledby="list-heading">
          <div className="section-heading">
            <div>
              <p className="eyebrow">The people on your mind</p>
              <h2 id="list-heading">A note to reconnect</h2>
            </div>
            <span className="list-count" aria-label={`${connections.length} people`}>
              {String(connections.length).padStart(2, "0")}
            </span>
          </div>
          <ConnectionList
            onRemove={removeConnection}
            onRetry={() => void loadConnections()}
            state={listState}
          />
        </section>

        <ConnectionForm
          isLoading={listState.status === "loading"}
          onAdd={addConnection}
          onSimulateLoadError={() => void loadConnections(true)}
        />
      </div>

      <footer className="footer-note">
        <span aria-hidden="true">✳</span>
        <p>A quiet practice in remembering what connects us.</p>
      </footer>
    </main>
  );
}
