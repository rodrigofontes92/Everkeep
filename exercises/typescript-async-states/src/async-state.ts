export type AsyncState<TData, TError = Error> =
  | { readonly status: "loading" }
  | { readonly status: "success"; readonly data: TData }
  | { readonly status: "error"; readonly error: TError };
