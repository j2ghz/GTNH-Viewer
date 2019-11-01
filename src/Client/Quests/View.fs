module Quests.View

open Fable.React.Standard
open Fable.React.Helpers
open Fable.React.Props
open Fulma
open Types
open Shared
open Fulma.Extensions.Wikiki
open Fable.Core.JsInterop
open Elmish.Navigation

let navbarSource urlMaker s =
    Navbar.Item.a
        [ Navbar.Item.Props
            [ s
              |> Types.Page.SelectedSource
              |> urlMaker
              |> Fable.React.Props.Href ] ] [ str s ]

let navbarItem (model: Types.State) currentPage urlMaker =
    match model.Sources with
    | Empty
    | Loading -> [ Navbar.Item.a [] [ str "Quests" ] ]
    | LoadError e ->
        [ Navbar.Item.div [ Navbar.Item.HasDropdown; Navbar.Item.IsHoverable ]
              [ Navbar.Link.a [] [ str "Quests" ]
                Navbar.Dropdown.div [] [ str e ] ] ]
    | Body sources ->
        [ Navbar.Item.div [ Navbar.Item.HasDropdown; Navbar.Item.IsHoverable ]
              [ Navbar.Link.a [] [ str "Quests" ]
                Navbar.Dropdown.div [] (sources |> List.map (navbarSource urlMaker)) ] ]

let menu s (model: Types.State) urlMaker i =
    [ Menu.label [] [ str s ]
      Menu.list []
          (match model.QuestLines with
           | Empty -> [ Menu.Item.li [] [ str "Empty" ] ]
           | Loading -> [ Menu.Item.li [] [ str "Loading" ] ]
           | LoadError e -> [ Menu.Item.li [] [ str e ] ]
           | Body qlis ->
               qlis
               |> List.map (fun qli ->
                   Menu.Item.li
                       [ Menu.Item.IsActive(qli.Id = i)
                         Menu.Item.Props
                             [ (s, qli.Id)
                               |> Types.Page.QuestLine
                               |> urlMaker
                               |> Href ] ] [ str qli.Name ])) ]

let questLineInfo (ql: Shared.QuestLineInfo) =
    Hero.hero []
        [ Hero.body []
              [ Container.container []
                    [ Heading.h1 [] [ str ql.Name ]
                      Heading.h5 [ Heading.IsSubtitle ] [ str ql.Description ] ] ] ]

let prerequisiteBadge pr =
    a
        [ pr
          |> sprintf "#/Quest/%i"
          |> Href ]
        [ Tag.tag []
              [ pr
                |> string
                |> str ] ]

let questCard (q: QuestLineQuest) =
    Card.card []
        [ Card.header []
              [ Card.Header.title [] [ str q.Quest.Name ]
                Card.Header.icon
                    [ Props
                        [ q.Id
                          |> sprintf "#/Quest/%i"
                          |> Href ] ] [ i [ ClassName "fa fa-hashtag" ] [] ] ]

          Card.content []
              [ Content.content []
                    (match q.Quest.Prerequisites with
                     | [] -> List.empty
                     | prereqs -> [ (Tag.list [] (prereqs |> List.map prerequisiteBadge)) ]) ]

          Card.content []
              [ Content.content []
                    [ pre [ Style [ WhiteSpaceOptions.PreWrap |> WhiteSpace ] ] [ str q.Quest.Description ] ] ] ]

let questLineQuestGridItem (qlq: QuestLineQuest) =
    let (x, y) = qlq.Location
    let (sx, sy) = qlq.Size
    a
        [ qlq.Id
          |> sprintf "#/Quest/%i"
          |> Href
          Tooltip.dataTooltip qlq.Quest.Name //FIX
          Class(Tooltip.ClassName + " " + Tooltip.IsTooltipBottom) ]  //FIX
        [ rect
            [ SVGAttr.Width sx
              SVGAttr.Height sy
              SVGAttr.X x
              SVGAttr.Y y
              SVGAttr.Stroke "black" ] [] ]

let questLineQuestGridConnections (qlqs: QuestLineQuest list) =
    let questById id = List.tryFind (fun (q: QuestLineQuest) -> q.Id = id) qlqs
    let prereqQuests qlq = qlq.Quest.Prerequisites |> List.choose questById

    let questCenter q =
        let x, y = q.Location
        let sx, sy = q.Size
        (x + (sx / 2), y + (sy / 2))

    let line fromx fromy tox toy =
        line
            [ X1 fromx
              Y1 fromy
              X2 tox
              Y2 toy
              SVGAttr.Stroke "black" ] []

    qlqs
    |> List.collect (fun qlq -> prereqQuests qlq |> List.map (fun pre -> (pre, qlq)))
    |> List.map (fun (from, qlq) ->
        let fromx, fromy = questCenter from
        let (x, y) = questCenter qlq
        line fromx fromy x y)

let questLineView ql =
    let w =
        ql.Quests
        |> List.map (fun q -> fst q.Location + fst q.Size)
        |> List.max

    let h =
        ql.Quests
        |> List.map (fun q -> snd q.Location + snd q.Size)
        |> List.max

    [ (ql.QuestLineInfo |> questLineInfo)
      div
          [ Class "box"
            [ OverflowY OverflowOptions.Auto ] |> Style ]
          [ svg
              [ SVGAttr.Height h
                SVGAttr.Width w ]
                [ yield! ql.Quests |> List.map questLineQuestGridItem
                  yield! questLineQuestGridConnections ql.Quests ] ]
      div [] (ql.Quests |> List.map questCard) ]

let search s (dispatch: Types.Msg -> unit) value =
    Input.search
        [ Input.Placeholder "Search Quests"
          Input.ValueOrDefault value
          Input.OnChange(fun e ->
              (s, !!e.target?value)
              |> LoadSearchResults
              |> dispatch) ]

let view (currentPage: Types.Page) urlMaker (model: Types.State) (dispatch: Types.Msg -> unit) =
    match currentPage with
    | Home -> [ str "Select a source" ]
    | SelectedSource s ->
        search s dispatch "" :: [ Columns.columns []
                                      [ Column.column [ Column.Width(Screen.All, Column.Is3) ]
                                            [ Menu.menu [] (menu s model urlMaker -1) ]
                                        Column.column [ Column.Width(Screen.All, Column.Is9) ] [] ] ]
    | QuestLine(s, i) ->
        [ Columns.columns []
              [ Column.column [ Column.Width(Screen.All, Column.Is2) ] [ Menu.menu [] (menu s model urlMaker i) ]
                Column.column [ Column.Width(Screen.All, Column.Is10) ]
                    [ yield! (match model.QuestLine with
                              | Empty -> [ str "Select a source and a questline" ]
                              | Loading -> [ str "Loading QuestLine" ]
                              | LoadError e ->
                                  [ sprintf "Loading QuestLine failed:\n%s" e
                                    |> str ]
                              | Body ql -> questLineView ql) ] ] ]
    | Search(s, st) ->
        search s dispatch st :: match model.SearchResults with
                                | Empty -> [ str "Search for something" ]
                                | Loading -> [ str "Searching" ]
                                | LoadError e ->
                                    [ str "Searching error:"
                                      str e ]
                                | Body results ->
                                    [ div []
                                          (results
                                           |> List.map (fun r ->
                                               Box.box' []
                                                   [ Heading.h2 [] [ str r.Name ]
                                                     p [] [ str r.Description ] ])) ]
