module Tests.Parsers

open Expecto
open Parsers

[<Tests>]
let tests =
    testList "parsers"
        [ test "BQv1" {
              let parser = BQv1.parser "./SampleData/DefaultQuests-2.0.7.5-cleaned-minified.json"
              "Should return some quests" |> Expect.isNonEmpty parser.getQuests
              "Should return some questlines" |> Expect.isNonEmpty parser.getQuestLines
              "Should return a questline by ID" |> Expect.isNotEmpty (parser.getQuestLineById 0).QuestLineInfo.Name
          }
          //test "I am (should fail)" { "╰〳 ಠ 益 ಠೃ 〵╯" |> Expect.equal true false }
         ]
