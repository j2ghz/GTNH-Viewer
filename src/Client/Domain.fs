module Domain

type Remote<'a> =
    | Empty
    | Loading
    | LoadError of string
    | Body of 'a

type Route =
    | Source of string
    | SourceQuestLine of string * int
    | SourceQuestLineQuest of string * int * int

type Quests =
    { QuestLines: Shared.QuestLineInfo list option
      SelectedQuestLine: Shared.QuestLine option }

type Source =
    | Quests of Quests
    | Recipes

type Model =
    { CurrentRoute: Route option
      QuestSources: string list Remote }

type Msg =
    | QuestLineSelected of int
    | SourcesLoaded of string list
    //    | SourceSelected of string
    | QuestLineReceived of Shared.QuestLine
    | QuestLinesLoaded of Shared.QuestLineInfo list
    | NavigateTo of Route
    | Error of exn
