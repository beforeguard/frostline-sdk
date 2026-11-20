namespace Beforeguard.Frostline.Cli

open System
open System.IO

type ProbeOutput =
    | SaveToSamples
    | Print
    | Fixture of name: string

type ProbeOptions =
    { Path: string
      Output: ProbeOutput
      Sanitize: bool }

module ProbeOptions =

    let fixturesRelativeDir = Path.Combine("tests", "Beforeguard.Frostline.WoW.Tests", "Fixtures")

    let private isValidFixtureName (name: string) =
        name.Length > 0
        && name |> Seq.forall (fun c -> Char.IsLetterOrDigit c || c = '_' || c = '-' || c = '.')
        && not (name.StartsWith ".")

    /// Parses the arguments that follow `probe`.
    let parse (args: string list) : Result<ProbeOptions, string> =
        let rec loop path print sanitize fixture rest =
            match rest with
            | [] -> Ok(path, print, sanitize, fixture)
            | "--print" :: tail -> loop path true sanitize fixture tail
            | "--sanitize" :: tail -> loop path print true fixture tail
            | "--fixture" :: name :: tail when not (name.StartsWith "--") -> loop path print sanitize (Some name) tail
            | "--fixture" :: _ -> Error "--fixture requires a fixture name, e.g. --fixture CharacterProfile."
            | arg :: _ when arg.StartsWith "--" -> Error(sprintf "Unknown option: %s" arg)
            | arg :: tail ->
                match path with
                | None -> loop (Some arg) print sanitize fixture tail
                | Some _ ->
                    Error(sprintf "Unexpected argument: %s. Output files are no longer supported; use --print or --fixture <Name>." arg)

        loop None false false None args
        |> Result.bind (fun (path, print, sanitize, fixture) ->
            match path, print, fixture with
            | None, _, _ -> Error "probe requires a path."
            | _, true, Some _ -> Error "--print and --fixture cannot be used together."
            | Some _, _, Some name when not (isValidFixtureName name) ->
                Error(sprintf "Invalid fixture name '%s'. Use letters, digits, '_', '-' or '.'." name)
            | Some p, true, None -> Ok { Path = p; Output = Print; Sanitize = sanitize }
            | Some p, false, Some name -> Ok { Path = p; Output = Fixture name; Sanitize = sanitize }
            | Some p, false, None -> Ok { Path = p; Output = SaveToSamples; Sanitize = sanitize })
