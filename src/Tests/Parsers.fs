module Tests.Parsers

open Expecto
open Parsers

let parser = lazy (BQv1.parser "./SampleData/DefaultQuests-2.0.7.5-cleaned-minified.json")

[<Tests>]
let tests =
    testList "BQv1"
        [ test "Should return some quests" { "getQuests not empty" |> Expect.isNonEmpty parser.Value.getQuests }
          test "Should return some questlines"
              { "getQuestLines not empty" |> Expect.isNonEmpty parser.Value.getQuestLines }
          test "Should return a questline by ID"
              {
              "getQuestLineById 0 has a name"
              |> Expect.isNotEmpty (parser.Value.getQuestLineById 0).QuestLineInfo.Name } ]
