namespace Beforeguard.Frostline.Cli

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Text.Json

type SanitizeResult =
    { Json: string
      /// JSON paths of string values that were redacted because their field is not known to be safe.
      Redacted: string list }

/// Deterministic, fail-closed sanitizer for Battle.net JSON responses.
module Sanitizer =

    type private Kind =
        | Character
        | Realm
        | Guild
        | Account

    // String fields known to carry non-identifying game data. Any other string
    // field is redacted so that new, unrecognized fields cannot leak data.
    let private safeStringKeys =
        set [ "name"; "type"; "display_string"; "description"; "title"; "locale"; "side"; "category" ]

    let private linkKeys = set [ "href"; "key" ]

    let redactedValue = "REDACTED"
    let private normalize (value: string) =
        value |> Seq.filter Char.IsLetterOrDigit |> Seq.map Char.ToLowerInvariant |> Seq.toArray |> String

    let private normalizeKey (value: string) =
        value |> Seq.filter Char.IsLetterOrDigit |> Seq.map Char.ToLowerInvariant |> Seq.toArray |> String

    let private label kind =
        match kind with
        | Character -> "Character"
        | Realm -> "Realm"
        | Guild -> "Guild"
        | Account -> "Account"

    let private idBase kind =
        match kind with
        | Character -> 900000000L
        | Guild -> 800000000L
        | Realm -> 700000L
        | Account -> 600000000L

    /// Allocates placeholders on first sight so the same input always maps to the same output within a file.
    type private Mapper() =
        let indexes = Dictionary<struct (Kind * string * string), int>()

        member _.Index(kind: Kind, space: string, key: string) =
            let k = struct (kind, space, key)

            match indexes.TryGetValue k with
            | true, i -> i
            | _ ->
                let i = (indexes |> Seq.filter (fun kv -> let struct (kd, sp, _) = kv.Key in kd = kind && sp = space) |> Seq.length) + 1
                indexes[k] <- i
                i

        member this.Name(kind, value: string) =
            sprintf "%s%d" (label kind) (this.Index(kind, "name", normalize value))

        member this.Slug(kind, value: string) =
            sprintf "%s-%d" ((label kind).ToLowerInvariant()) (this.Index(kind, "name", normalize value))

        member this.Id(kind, raw: string) = idBase kind + int64 (this.Index(kind, "id", raw))

        member this.UrlSegment(kind, value: string) =
            this.Slug(kind, Uri.UnescapeDataString value)

        member this.UrlId(kind, value: string) =
            match Int64.TryParse(Uri.UnescapeDataString value) with
            | true, _ -> string (this.Id(kind, value))
            | _ -> this.UrlSegment(kind, value)

    let private tryProp (name: string) (el: JsonElement) =
        match el.TryGetProperty name with
        | true, v -> Some v
        | _ -> None

    /// Which kind of entity an object describes, derived from the key that holds it or its shape.
    let private objectKind (parentKey: string) (el: JsonElement) =
        match parentKey with
        | "character" -> Some Character
        | "realm" -> Some Realm
        | "guild" -> Some Guild
        | _ when (tryProp "realm" el).IsSome ->
            if (tryProp "character_class" el).IsSome || (tryProp "level" el).IsSome then
                Some Character
            else
                Some Guild
        | _ -> None

    let private sanitizePath (mapper: Mapper) (path: string) =
        let segments = path.Split('/', StringSplitOptions.None)
        let mutable changed = false

        let replace index (value: string) =
            if segments[index] <> value then
                segments[index] <- Uri.EscapeDataString value
                changed <- true

        for i in 0 .. segments.Length - 2 do
            let marker = segments[i].ToLowerInvariant()

            match marker with
            | "character" when i + 2 < segments.Length ->
                replace (i + 1) (mapper.UrlSegment(Realm, segments[i + 1]))
                replace (i + 2) (mapper.UrlSegment(Character, segments[i + 2]))
            | "guild" when i + 2 < segments.Length ->
                replace (i + 1) (mapper.UrlSegment(Realm, segments[i + 1]))
                replace (i + 2) (mapper.UrlSegment(Guild, segments[i + 2]))
            | "realm" when segments[i + 1].ToLowerInvariant() <> "index" ->
                replace (i + 1) (mapper.UrlSegment(Realm, segments[i + 1]))
            | "connected-realm" when segments[i + 1].ToLowerInvariant() <> "index" ->
                replace (i + 1) (mapper.UrlId(Realm, segments[i + 1]))
            | _ -> ()

        String.Join('/', segments), changed

    let private sanitizeQuery (mapper: Mapper) (query: string) =
        let mutable changed = false

        let sanitizeValue (key: string) (value: string) =
            let decoded = Uri.UnescapeDataString(value.Replace("+", " "))

            match normalizeKey key with
            | "realm" | "realmslug" -> mapper.Slug(Realm, decoded)
            | "character" | "charactername" | "characterslug" -> mapper.Slug(Character, decoded)
            | "guild" | "guildname" | "guildslug" | "namesearch" -> mapper.Slug(Guild, decoded)
            | "account" | "accountid" ->
                match Int64.TryParse decoded with
                | true, _ -> string (mapper.Id(Account, decoded))
                | _ -> mapper.Name(Account, decoded)
            | "characterid" -> string (mapper.Id(Character, decoded))
            | "guildid" -> string (mapper.Id(Guild, decoded))
            | "realmid" -> string (mapper.Id(Realm, decoded))
            | _ -> decoded

        if String.IsNullOrEmpty query then
            query, false
        else
            let pairs =
                query.TrimStart('?').Split('&', StringSplitOptions.None)
                |> Array.map (fun pair ->
                    match pair.IndexOf '=' with
                    | -1 -> pair
                    | separator ->
                        let key = pair.Substring(0, separator)
                        let value = pair.Substring(separator + 1)
                        let sanitized = sanitizeValue (Uri.UnescapeDataString key) value

                        if sanitized = Uri.UnescapeDataString(value.Replace("+", " ")) then
                            pair
                        else
                            changed <- true
                            key + "=" + Uri.EscapeDataString sanitized)

            "?" + String.Join('&', pairs), changed

    let private sanitizeUrl (mapper: Mapper) (value: string) =
        match Uri.TryCreate(value, UriKind.Absolute) with
        | true, rawUri ->
            let uri = nonNull rawUri

            if uri.Scheme <> Uri.UriSchemeHttp && uri.Scheme <> Uri.UriSchemeHttps then
                null
            else
                let path, pathChanged = sanitizePath mapper uri.AbsolutePath
                let query, queryChanged = sanitizeQuery mapper uri.Query

                if pathChanged || queryChanged then
                    let builder = UriBuilder(uri)
                    builder.Path <- path
                    builder.Query <- query.TrimStart('?')
                    builder.Uri.AbsoluteUri
                else
                    value
        | _ -> null

    let sanitize (root: JsonElement) : SanitizeResult =
        let mapper = Mapper()
        let redacted = List<string>()
        use stream = new MemoryStream()

        let options = JsonWriterOptions(Indented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping)

        use writer = new Utf8JsonWriter(stream, options)

        let redact path (value: string) =
            redacted.Add path
            writer.WriteStringValue value

        // `entity` is the identifying kind of the object that directly contains the value, if any.
        let rec write (path: string) (key: string) (entity: Kind option) (el: JsonElement) =
            match el.ValueKind with
            | JsonValueKind.Object ->
                let own = objectKind key el
                writer.WriteStartObject()

                for p in el.EnumerateObject() do
                    writer.WritePropertyName p.Name
                    write (path + "." + p.Name) p.Name own p.Value

                writer.WriteEndObject()
            | JsonValueKind.Array ->
                writer.WriteStartArray()
                let mutable i = 0

                for item in el.EnumerateArray() do
                    write (sprintf "%s[%d]" path i) key None item
                    i <- i + 1

                writer.WriteEndArray()
            | JsonValueKind.String ->
                let s = el.GetString() |> nonNull

                match entity, key with
                | Some k, "name" -> writer.WriteStringValue(mapper.Name(k, s))
                | Some k, ("slug" | "name_search") -> writer.WriteStringValue(mapper.Slug(k, s))
                | _, ("account_id" | "account") -> writer.WriteStringValue(mapper.Name(Account, s))
                | _, k when linkKeys.Contains k ->
                    match sanitizeUrl mapper s with
                    | null when k = "key" -> writer.WriteStringValue s
                    | null -> redact path redactedValue
                    | url -> writer.WriteStringValue url
                | _, k when safeStringKeys.Contains k -> writer.WriteStringValue s
                | _ -> redact path redactedValue
            | JsonValueKind.Number ->
                match entity, key with
                | Some k, "id" -> writer.WriteNumberValue(mapper.Id(k, el.GetRawText()))
                | _, ("account_id" | "account") -> writer.WriteNumberValue(mapper.Id(Account, el.GetRawText()))
                | _ -> el.WriteTo writer
            | _ -> el.WriteTo writer

        write "$" "" None root
        writer.Flush()

        { Json = Encoding.UTF8.GetString(stream.ToArray())
          Redacted = redacted |> Seq.toList }
