# Simple.API — Usage Guide (for Agents)

How to **consume** the `Simple.API` NuGet package from application code. This is a REST/HTTP client library (JSON via Newtonsoft.Json) with a low-level client, a typed interface builder, JWT parsing, and a WebSocket wrapper.

Namespace: `Simple.API` (attributes in `Simple.API.ClientBuilderAttributes`, WebSocket processors in `Simple.API.WebSocketProcessors`).

```bash
dotnet add package Simple.API
```

## Compatibility (affects which APIs you can call)
- **`ClientInfo`, `Response<T>`, JWT, extensions**: all TFMs (net Framework 4.5 / netstandard1.1 → net9).
- **`ClientBuilder`** (typed interface proxy, `DispatchProxy`): **only** `net6.0+` or `netstandard2.1+`.
- **`WebSocket<,>`**: everything **except** `netstandard1.1`.
- **`WebSocket.DisposeAsync` (`IAsyncDisposable`)**: everything except `netstandard2.0` (which keeps only sync `Dispose`).
- Query-string building from an object (`GetAsync(service, object)`, `OptionsAsync(service, object)`) uses reflection and **throws on `netstandard1.1`**.

---

## 1. `ClientInfo` — low-level client

```csharp
using Simple.API;

var client = new ClientInfo("https://api.example.com/"); // trailing '/' added if missing; base for all calls
```
- `BaseUri` is combined with each `service` via `new Uri(BaseUri, service)`. **Do not** start `service` with `/` unless you mean to drop the base path (standard `Uri` behavior).
- Sends `Accept: application/json` by default; auto-decompresses gzip/deflate.

### Verbs (all `async`, return `Task<Response<T>>` / `Task<Response>`)
```csharp
Response<Dto> r = await client.GetAsync<Dto>("things/42");
Response<Dto> p = await client.PostAsync<Dto>("things", new { name = "x" });
await client.PutAsync<Dto>("things/42", dto);
await client.PatchAsync<Dto>("things/42", dto);
await client.DeleteAsync("things/42");            // non-generic: Response (no body)
await client.DeleteAsync<Dto>("things/42");       // generic: Response<Dto>
await client.GetAsync("ping");                    // non-generic GET: Response (no deserialization)
await client.OptionsAsync("things", new { a = 1 });
```
Body handling for POST/PUT/PATCH `value`:
- `byte[]` → `application/octet-stream`; `Stream` → `application/octet-stream`; anything else → JSON.
- `HttpContent` overloads exist for POST when you need full control.

### Query params, ids, forms (extension methods)
```csharp
await client.GetAsync<Dto>("things", 42);                       // -> things/42
await client.GetAsync<Dto>("things", guid);                     // -> things/{guid}
await client.GetAsync<Dto>("search", new { q = "hi", page = 2 });// -> search?q=hi&page=2
await client.PostAsync<Dto>("things", dto, 42);                 // -> things/42
await client.FormUrlEncodedPostAsync<Dto>("login", new { user, pass });
await client.MultipartFormPostAsync<Dto>("upload", fileStream, "application/pdf", "file", "doc.pdf");
```
- Numeric query values are formatted with `InvariantCulture`. Nulls are dropped when `client.NullParameterHandlingPolicy_IgnoreNulls == true` (default).

### Auth & headers
```csharp
client.SetAuthorizationBearer(jwt);          // Authorization: Bearer <jwt>
client.SetAuthorizationBasic(user, password);// Authorization: Basic <base64>
client.SetAuthorization("Bearer " + t);      // raw Authorization value
client.SetHeader("X-Api-Key", key);          // replaces any existing value
client.RemoveAuthorization();
client.Timeout = TimeSpan.FromSeconds(30);
```

---

## 2. `Response<T>` and error handling

`Response<T> : Response`. Key members: `Data`, `IsSuccessStatusCode`, `StatusCode`, `ReasonPhrase`, `Headers`, `ContentHeaders`, `Duration`, `ErrorResponseData` (raw body on non-2xx).

Three ways to handle failures — pick per call:
```csharp
// (a) inspect manually
var r = await client.GetAsync<Dto>("things/42");
if (!r.IsSuccessStatusCode)
{
    var err = r.ParseErrorResponseData<ApiError>();      // or r.TryParseErrorResponseData(out ApiError e)
}

// (b) throw-on-failure and get data
Dto dto = (await client.GetAsync<Dto>("things/42")).GetSuccessfulData(); // throws UnsuccessfulStatusCodeException

// (c) typed error on throw
r.EnsureSuccessStatusCode<ApiError>(); // throws UnsuccessfulStatusCodeException<ApiError> with .ErrorInformation
```
Exceptions: `UnsuccessfulStatusCodeException` (has `.Response`) and `UnsuccessfulStatusCodeException<TError>` (adds `.ErrorInformation`).

Content types auto-detected by `GetAsync<T>` etc.: JSON (default), raw `string`, `byte[]`, `Stream`, `JObject`, `JWT`, XML (body starting with `<`), and URL-encoded JSON.

---

## 3. `ClientBuilder` — typed interface client (net6+/netstandard2.1+)

Declare an interface with route attributes; the builder implements it at runtime.
```csharp
using Simple.API;
using Simple.API.ClientBuilderAttributes;

[Timeout(30)] // seconds, optional, on the interface
public interface IThings : IBuildedClientInternalFunctions   // inherit for auth/header/internal helpers
{
    [Get("things/{id}")]    Task<Response<Dto>> GetRaw([InRoute] int id);  // full Response<T>
    [Get("things/{id}")]    Task<Dto>           Get([InRoute] int id);     // unwrapped (throws on non-2xx)
    [Get("search")]         Task<Response<Dto[]>> Search(Query query);     // non-[InRoute] arg -> query string
    [Post("things")]        Task<Dto>           Create(Dto body);
    [Put("things/{id}")]    Task<Response>      Replace([InRoute] int id, Dto body); // Response only, no body deserialize
    [Patch("things/{id}")]  Task<Dto>           Update([InRoute] int id, Dto body);
    [Delete("things/{id}")] Task                Delete([InRoute] int id);  // fire-and-forget (throws on non-2xx)
}

var api = ClientBuilder.Create<IThings>("https://api.example.com/");
api.SetAuthorizationBearer(jwt);                 // from IBuildedClientInternalFunctions
var dto = await api.Get(42);
```
Return-type semantics (choose deliberately):
- `Task<Response<T>>` → full response, body deserialized to `T`, **no** throw.
- `Task<T>` → body deserialized, **throws** `UnsuccessfulStatusCodeException` on non-2xx (via `GetSuccessfulData`).
- `Task<Response>` → status/headers only, **no body deserialization**, **no** throw (inspect `IsSuccessStatusCode`).
- `Task` → fire-and-forget; throws on non-2xx, body ignored.

Rules:
- `{token}` in a route **requires** a matching `[InRoute]` param, and vice-versa (validated at call time).
- One non-`[InRoute]` argument on GET becomes the query string; on POST/PUT/PATCH it becomes the body.
- `IBuildedClientInternalFunctions` exposes `GetInternalClient()`, `SetAuthorizationBearer(string)`, `SetHeader(string,string)`. HTTP attributes take precedence, so a mapped method won't be mistaken for an internal one.

---

## 4. JWT parsing
```csharp
var jwt = JWT.Parse(tokenString);
bool active = jwt.Content.GetExp > DateTime.UtcNow; // GenericJwt: iss/exp/iat/nbf/sub/aud + GetExp/GetIat/GetNbf
var typed = JWT<MyClaims>.Parse(tokenString);       // payload bound to your model in .Content
```

---

## 5. `WebSocket<TSend, TReceive>` (not on netstandard1.1)
```csharp
using Simple.API;
using Simple.API.WebSocketProcessors;

var ws = new WebSocket<OutMsg, InMsg>("wss://host/ws", new JsonDataProcessor<OutMsg, InMsg>());
ws.OnMessageReceived  += (_, msg)    => Handle(msg);
ws.OnConnectionClosed += (_, status) => Log(status); // fired on server-initiated close
ws.OnError            += (_, ex)     => Log(ex);     // then the socket auto-closes/cleans up

await ws.ConnectAsync();               // starts a background receive loop
await ws.SendMessageAsync(new OutMsg{...});
await ws.SendCloseMessageAsync();
await ws.DisconnectAsync();            // or: await using var ws = ...;  (IAsyncDisposable, except netstandard2.0)
```
- Processors: `JsonDataProcessor<TSend,TReceive>` (JSON, UTF-8; raw text if `T` is `string`), `StringDataProcessor` (string↔string), `GenericDataProcessor<,>` (supply your own send/receive/close delegates).
- Dispose via `await ws.DisposeAsync()` (preferred) or `ws.Dispose()`. Safe to dispose a never-connected instance.
- `SendMessageAsync`/`SendCloseMessageAsync` throw `InvalidOperationException` if called before connect / after disconnect.
- On a server-initiated close the client completes the close handshake, raises `OnConnectionClosed`, then releases resources (`InternalClient` becomes null).

---

## 6. Customization hooks (events on `ClientInfo`)
- `BeforeSend` — inspect/modify the `HttpRequestMessage` before it is sent.
- `ResponseDataReceived` — telemetry for every response (uri, status, content, timing).
- `JsonSerializeOverride` — replace request-body serialization (set `args.Value` + `args.Handled = true`), e.g. request signing.
- `DeserializeJObjectOverride` / `DeserializeJValueOverride` — mutate the parsed JSON before it is bound to `T`.

`ConfigureHttpClient(Action<HttpClient>)` gives raw access; `ClientInfo.GlobalDefaultTimeout` (static) seeds the timeout of new instances.
