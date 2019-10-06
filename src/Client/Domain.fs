module Domain

type Route =
    | Source of string
    | SourceQuestLine of string * int
    | SourceQuestLineQuest of string * int * int

type Model =
    { QuestSources : string list option
      SelectedSource : string option
      QuestLines : Shared.QuestLineInfo list option
      SelectedQuestLine : Shared.QuestLine option }

type Msg =
    | QuestLineSelected of int
    | SourcesLoaded of string list
    | SourceSelected of string
    | QuestLineReceived of Shared.QuestLine
    | QuestLinesLoaded of Shared.QuestLineInfo list
    | Error of exn