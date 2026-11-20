module ProbeOptionsTests

open Xunit
open Beforeguard.Frostline.Cli

[<Fact>]
let ``defaults to saving into samples`` () =
    let result = ProbeOptions.parse [ "/data/x" ]
    Assert.Equal(Ok { Path = "/data/x"; Output = SaveToSamples; Sanitize = false }, result)

[<Fact>]
let ``parses print, sanitize and fixture flags`` () =
    Assert.Equal(Ok { Path = "/p"; Output = Print; Sanitize = true }, ProbeOptions.parse [ "/p"; "--print"; "--sanitize" ])

    Assert.Equal(
        Ok { Path = "/p"; Output = Fixture "CharacterProfile"; Sanitize = true },
        ProbeOptions.parse [ "/p"; "--sanitize"; "--fixture"; "CharacterProfile" ]
    )

[<Fact>]
let ``print with fixture is an error`` () =
    match ProbeOptions.parse [ "/p"; "--print"; "--fixture"; "X" ] with
    | Error message -> Assert.Contains("cannot be used together", message)
    | Ok _ -> failwith "expected an error"

[<Fact>]
let ``positional output file is rejected`` () =
    Assert.True((ProbeOptions.parse [ "/p"; "out.json" ]).IsError)

[<Theory>]
[<InlineData("..\\evil")>]
[<InlineData("a/b")>]
[<InlineData(".hidden")>]
let ``invalid fixture names are rejected`` (name: string) =
    Assert.True((ProbeOptions.parse [ "/p"; "--fixture"; name ]).IsError)

[<Fact>]
let ``fixture requires a name`` () =
    Assert.True((ProbeOptions.parse [ "/p"; "--fixture" ]).IsError)
    Assert.True((ProbeOptions.parse [ "/p"; "--fixture"; "--sanitize" ]).IsError)
