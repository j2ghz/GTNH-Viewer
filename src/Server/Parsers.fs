module Parsers

open Shared

type IQuestParser =
    { getQuests: Quest list
      getQuestLines: QuestLineInfo list
      getQuestLineById: int -> QuestLine }

type IRecExParser =
    { getRecipes: Recipe list
      getItems: Item list }

module RecEx =
    type private RecExDB = FSharp.Data.JsonProvider<"./SampleData/v2.0.7.5-gt-shaped-shapeless-cleaned-minified.json">

    let toString (i: RecExDB.FloatOrString) =
        if (Option.isSome i.String) then i.String
        else if (Option.isSome i.Number) then i.Number |> Option.map string
        else None

    let makeItemA a u l =
        { Amount = a
          Item =
              { rawName = u
                Name = l } }

    let mapII2 (item: RecExDB.II2) = makeItemA item.A item.UN (Some item.LN)
    let mapII (item: RecExDB.II) = makeItemA item.A (toString item.UN) item.LN
    let mapFI (item: RecExDB.FI) = makeItemA item.A (Some item.UN) (Some item.LN)
    let mapIO (item: RecExDB.IO) = makeItemA item.A (toString item.UN) (Some item.LN)

    let makeRecipe t i o =
        { Details = t
          Input = i
          Output = o }

    let mapShaped (recipe: RecExDB.Recipe) =
        makeRecipe Shared.Shaped
            (recipe.II
             |> Array.map mapII2
             |> Array.toList) ([ recipe.O |> mapFI ])

    let mapShapeless (recipe: RecExDB.Recipe2) =
        makeRecipe Shared.Shapeless
            (recipe.II
             |> Array.map mapFI
             |> Array.toList) ([ recipe.O |> mapFI ])

    let mapGregtech machineName (recipe: RecExDB.Rec) =
        makeRecipe (Shared.Gregtech { MachineName = machineName })
            [ yield! recipe.II |> Array.map mapII
              yield! recipe.FI |> Array.map mapFI ]
            [ yield! recipe.IO |> Array.map mapIO
              yield! recipe.FO |> Array.map mapFI ]

    let recipes (source: RecExDB.Root) =
        [ yield! source.Sources.Shaped.Recipes |> Array.map mapShaped
          yield! source.Sources.Shapeless.Recipes |> Array.map mapShapeless
          yield! source.Sources.Gregtech.Machines |> Array.collect (fun m -> m.Recs |> Array.map (mapGregtech m.N)) ]

    let items (recipes: Recipe list) =
        let recipeItems r =
            [ yield! r.Input
              yield! r.Output ]
            |> List.map (fun r -> r.Item)
            |> List.distinct
        recipes
        |> List.collect recipeItems
        |> List.distinct

    let parser (path: string): Async<IRecExParser> =
        async {
            let! source = RecExDB.AsyncLoad path
            let recipes = recipes source
            return { getRecipes = recipes
                     getItems = (items recipes) }
        }

module BQv3 =
    open Shared

    type private BetterQuestingDB = FSharp.Data.JsonProvider<"./SampleData/DefaultQuests-2.0.7.6c-dev-cleaned-minified.json">

    let mapQuestLine id (ql: BetterQuestingDB.QuestLine) =
        { Id = ql.LineId
          Name = ql.Properties.Betterquesting.Name
          Order = ql.Order
          Description = ql.Properties.Betterquesting.Desc }

    let mapQuest (q: BetterQuestingDB.QuestDatabase) =
        { Id = q.QuestId
          Name = q.Properties.Betterquesting.Name
          Description = q.Properties.Betterquesting.Desc
          Prerequisites = q.PreRequisites |> Array.toList
          Icon = q.Properties.Betterquesting.Icon.Id
          Main = q.Properties.Betterquesting.IsMain }

    let mapQuestLineQuests (qs: Quest list) (ql: BetterQuestingDB.QuestLine) =
        query {
            for q in ql.Quests do
                join x in qs on (q.Id = x.Id)
                select
                    { Id = q.Id
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
        (getQuestLineQuests qls mappedQuests) |> List.find (fun ql -> ql.QuestLineInfo.Id = id)

    let parser (path: string) =
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

    type private BetterQuestingDB = FSharp.Data.JsonProvider<"./SampleData/DefaultQuests-2.0.7.5-cleaned-minified.json">

    let mapQuestLine id (ql: BetterQuestingDB.QuestLine) =
        { Id = id
          Name = ql.Name
          Order = id
          Description = ql.Description }

    let mapQuest (q: BetterQuestingDB.QuestDatabase) =
        { Id = q.QuestId
          Name = q.Name
          Description = q.Description
          Prerequisites = q.PreRequisites |> Array.toList
          Icon = q.Icon.Id
          Main = q.IsMain }

    let mapQuestLineQuests (qs: Quest list) (ql: BetterQuestingDB.QuestLine) =
        query {
            for q in ql.Quests do
                join x in qs on (q.Id = x.Id)
                select
                    { Id = q.Id
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
        (getQuestLineQuests qls mappedQuests) |> List.find (fun ql -> ql.QuestLineInfo.Id = id)

    let parser (path: string) =
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
