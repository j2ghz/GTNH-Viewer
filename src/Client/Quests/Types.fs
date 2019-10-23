module Quests.Types

open Shared

type Page =
    | Home
    | SelectedSource of source: Source
    | QuestLine of Source * int

type Msg =
    | LoadSources
    | LoadSourcesFinished of Source list
    | LoadSourcesError of exn
    | LoadQuestLines of Source
    | LoadQuestLinesFinished of Shared.QuestLineInfo list
    | LoadQuestLinesError of exn
    | LoadQuestLine of Source * int
    | LoadQuestLineFinished of Shared.QuestLine
    | LoadQuestLineError of exn

type State =
    { Sources: Shared.Source list Remote
      QuestLines: Shared.QuestLineInfo list Remote
      QuestLine: Shared.QuestLine Remote }
