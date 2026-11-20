module Fixtures

open System.IO
open System.Reflection

/// Loads an embedded JSON fixture from the Fixtures folder by name (without extension).
let load (name: string) : string =
    let assembly = Assembly.GetExecutingAssembly()
    let suffix = ".Fixtures." + name + ".json"

    match assembly.GetManifestResourceNames() |> Array.tryFind (fun n -> n.EndsWith suffix) with
    | Some resource ->
        use stream = assembly.GetManifestResourceStream resource |> nonNull
        use reader = new StreamReader(stream)
        reader.ReadToEnd()
    | None -> failwithf "Fixture '%s' not found. Run: frostline probe \"<path>\" --sanitize --fixture %s" name name
