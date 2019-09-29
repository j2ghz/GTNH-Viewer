module Parsers

module BQv2 =
    open Shared
    
    // must be pre-processed with:
    // jq 'walk( if type == "object" then with_entries( .key |= sub( ":.+$"; "") ) else . end ) | walk( if type == "object" and has("0") then . | to_entries | map_values(.value) else . end)' DefaultQuests.json > DefaultQuests-cleaned.json
    type private BetterQuestingDB = FSharp.Data.JsonProvider<"../Shared/SampleData/DefaultQuests-master-cleaned.json">
    
    // Replace by actual file
    let questDB = BetterQuestingDB.GetSample()

    let mapQuestLine (ql : BetterQuestingDB.QuestLine) =
        { Id = ql.LineId
          Name = ql.Properties.Betterquesting.Name
          Order = ql.Order
          Description = ql.Properties.Betterquesting.Desc }
    let getQuestLines =
        questDB.QuestLines
        |> Array.map mapQuestLine
        |> Array.toList

    let mapQuest (q : BetterQuestingDB.QuestDatabase) =
        { Id = q.QuestId
          Name = q.Properties.Betterquesting.Name
          Description = q.Properties.Betterquesting.Desc
          Prerequisites = q.PreRequisites |> Array.toList }
    let getQuests =
        questDB.QuestDatabase
        |> Array.map mapQuest
        |> Array.toList

    let mapQuestLineQuests (qs : Quest list) (ql : BetterQuestingDB.QuestLine) =
        query {
            for q in ql.Quests do
            join x in qs
                on (q.Id = x.Id)
            select {
                    Id= q.Id
                    Location= (q.X,q.Y)
                    Size=(q.SizeX,q.SizeY)
                    Quest= x
                   }
        } |> Seq.toList

    let getQuestLineQuests =
        questDB.QuestLines
        |> Array.map (fun ql -> {
            QuestLineInfo=mapQuestLine ql
            Quests=mapQuestLineQuests getQuests ql
        })
        |> Array.toList

    let getQuestLineById id = getQuestLineQuests |> List.find (fun ql -> ql.QuestLineInfo.Id = id)
