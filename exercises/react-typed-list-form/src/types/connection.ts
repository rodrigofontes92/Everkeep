export interface Connection {
  id: string;
  name: string;
  reason: string;
}

export type ConnectionDraft = Omit<Connection, "id">;

export type AsyncState<T> =
  | { status: "loading" }
  | { status: "error"; message: string }
  | { status: "success"; data: T };
