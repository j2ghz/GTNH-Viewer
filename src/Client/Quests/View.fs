module Quests.View

open Fable.React.Standard
open Fable.React.Helpers
open Fable.React.Props
open Fulma
open Shared
open Fulma.Extensions.Wikiki

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

let menu s (model: Types.State) urlMaker =
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
                       [ Menu.Item.Props
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
          Tooltip.dataTooltip qlq.Quest.Name
          Class(Tooltip.ClassName + " " + Tooltip.IsTooltipBottom) ]
        [ rect
            [ SVGAttr.Width sx
              SVGAttr.Height sy
              SVGAttr.X x
              SVGAttr.Y y
              SVGAttr.Stroke "black" ] [] ]

let questLineView ql =
    let h =
        ql.Quests
        |> List.map (fun q -> fst q.Location + fst q.Size)
        |> List.max

    let w =
        ql.Quests
        |> List.map (fun q -> snd q.Location + snd q.Size)
        |> List.max

    [ (ql.QuestLineInfo |> questLineInfo)
      div [ Class "box" ]
          [ svg
              [ SVGAttr.Height h
                SVGAttr.Width w ] (ql.Quests |> List.map questLineQuestGridItem) ]
      div [] (ql.Quests |> List.map questCard) ]

let view (currentPage: Types.Page) urlMaker (model: Types.State) (dispatch: Types.Msg -> unit) =
    [ Columns.columns []
          [ Column.column [ Column.Width(Screen.All, Column.Is3) ]
                [ Menu.menu []
                      (match currentPage with
                       | Types.Page.Home -> [ Menu.label [] [ str "Select a source" ] ]
                       | Types.Page.SelectedSource s -> menu s model urlMaker
                       | Types.Page.QuestLine(s, i) -> menu s model urlMaker) ]
            Column.column [ Column.Width(Screen.All, Column.Is9) ]
                [ yield! (match model.QuestLine with
                          | Empty -> [ str "Select a source and a questline" ]
                          | Loading -> [ str "Loading QuestLine" ]
                          | LoadError e ->
                              [ sprintf "Loading QuestLine failed:\n%s" e
                                |> str ]
                          | Body ql -> questLineView ql) ] ] ]
