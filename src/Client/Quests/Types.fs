module Quests.Types
open Shared

type Page =
    | SelectedSource of source : Source

type Msg =
    | LoadSources
    | LoadQuestLines of Source

type State =
    {
        Sources : Shared.Source list Remote
        Source : Source option
    }