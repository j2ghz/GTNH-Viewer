module Parsers

open Shared

type IQuestParser =
    { getQuests : Quest list
      getQuestLines : QuestLineInfo list
      getQuestLineById : int -> QuestLine }

module BQv3 =
    open Shared

    type private BetterQuestingDB = FSharp.Data.JsonProvider<"./SampleData/DefaultQuests-2.0.7.6c-dev-cleaned.json">

    let mapQuestLine id (ql : BetterQuestingDB.QuestLine) =
        { Id = ql.LineId
          Name = ql.Properties.Betterquesting.Name
          Order = ql.Order
          Description = ql.Properties.Betterquesting.Desc }

    let mapQuest (q : BetterQuestingDB.QuestDatabase) =
        { Id = q.QuestId
          Name = q.Properties.Betterquesting.Name
          Description = q.Properties.Betterquesting.Desc
          Prerequisites = q.PreRequisites |> Array.toList
          Icon = q.Properties.Betterquesting.Icon.Id
          Main = q.Properties.Betterquesting.IsMain }

    let mapQuestLineQuests (qs : Quest list) (ql : BetterQuestingDB.QuestLine) =
        query {
            for q in ql.Quests do
                join x in qs on (q.Id = x.Id)
                select { Id = q.Id
                         Location = (q.X, q.Y)
                         Size = (q.SizeX, q.SizeY)
                         Quest = x }
        }
        |> Seq.toList

    let getQuestLineQuests qls mappedQuests =
        qls
        |> Array.mapi (fun i ql ->
               { QuestLineInfo = mapQuestLine i ql
                 Quests = mapQuestLineQuests mappedQuests ql })
        |> Array.toList

    let getQuestLineById qls mappedQuests id =
        (getQuestLineQuests qls mappedQuests)
        |> List.find (fun ql -> ql.QuestLineInfo.Id = id)

    let parser (path : string) =
        let questDB = BetterQuestingDB.Load path

        let quests =
            questDB.QuestDatabase
            |> Array.map mapQuest
            |> Array.toList
        { getQuests = quests
          getQuestLines =
              questDB.QuestLines
              |> Array.mapi mapQuestLine
              |> Array.toList
          getQuestLineById = (getQuestLineById questDB.QuestLines quests) }

module BQv1 =
    open Shared

    type private BetterQuestingDB = FSharp.Data.JsonProvider<"./SampleData/DefaultQuests-2.0.7.5-cleaned.json">

    let mapQuestLine id (ql : BetterQuestingDB.QuestLine) =
        { Id = id
          Name = ql.Name
          Order = id
          Description = ql.Description }

    let mapQuest (q : BetterQuestingDB.QuestDatabase) =
        { Id = q.QuestId
          Name = q.Name
          Description = q.Description
          Prerequisites = q.PreRequisites |> Array.toList
          Icon = q.Icon.Id
          Main = q.IsMain }

    let mapQuestLineQuests (qs : Quest list) (ql : BetterQuestingDB.QuestLine) =
        query {
            for q in ql.Quests do
                join x in qs on (q.Id = x.Id)
                select { Id = q.Id
                         Location = (q.X, q.Y)
                         Size = (24, 24)
                         Quest = x }
        }
        |> Seq.toList

    let getQuestLineQuests qls mappedQuests =
        qls
        |> Array.mapi (fun i ql ->
               { QuestLineInfo = mapQuestLine i ql
                 Quests = mapQuestLineQuests mappedQuests ql })
        |> Array.toList

    let getQuestLineById qls mappedQuests id =
        (getQuestLineQuests qls mappedQuests)
        |> List.find (fun ql -> ql.QuestLineInfo.Id = id)

    let parser (path : string) =
        let questDB = BetterQuestingDB.Load path

        let quests =
            questDB.QuestDatabase
            |> Array.map mapQuest
            |> Array.toList
        { getQuests = quests
          getQuestLines =
              questDB.QuestLines
              |> Array.mapi mapQuestLine
              |> Array.toList
          getQuestLineById = (getQuestLineById questDB.QuestLines quests) }
