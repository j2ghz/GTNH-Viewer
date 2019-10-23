module Quests.Types

open Shared

type Page =
    | Home
    | SelectedSource of source: Source

type Msg =
    | LoadSources
    | LoadSourcesFinished of Source list
    | LoadSourcesError of exn
    | LoadQuestLines of Source
    | LoadQuestLinesFinished of Shared.QuestLineInfo list
    | LoadQuestLinesError of exn

type State =
    { Sources: Shared.Source list Remote }
