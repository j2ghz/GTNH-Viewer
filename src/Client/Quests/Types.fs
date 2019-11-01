module Quests.Types

open Shared
open Elmish

type Page =
    | Home
    | SelectedSource of source: Source
    | QuestLine of Source * int
    | Search of Source * string

type Msg =
    //Sources
    | LoadSources
    | LoadSourcesFinished of Source list
    | LoadSourcesError of exn
    //QuestLines
    | LoadQuestLines of Source
    | LoadQuestLinesFinished of Shared.QuestLineInfo list
    | LoadQuestLinesError of exn
    //QuestLine
    | LoadQuestLine of Source * int
    | LoadQuestLineFinished of Shared.QuestLine
    | LoadQuestLineError of exn
    //Search
    | LoadSearchResults of Source * string
    | LoadSearchResultsFinished of QuestSearchResult list
    | LoadSearchResultsError of exn

type State =
    { Sources: Shared.Source list Remote
      QuestLines: Shared.QuestLineInfo list Remote
      QuestLine: Shared.QuestLine Remote
      SearchResults: Shared.QuestSearchResult list Remote }
