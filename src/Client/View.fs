module View

open Domain
open Elmish
open Elmish.Navigation
open Elmish.React
open Elmish.UrlParser
open Fable.FontAwesome
open Fable.FontAwesome.Free
open Fable.React
open Fable.React.Props
open Fulma
open Shared
open Thoth.Json

let stri = sprintf "%i" >> str

let navSource (model : Model) (dispatch : Msg -> unit) source =
    Navbar.Item.a [ Navbar.Item.Props [ OnClick(fun _ ->
                                            source
                                            |> Source
                                            |> NavigateTo
                                            |> dispatch) ] ] [ //(model.SelectedSource |> Option.defaultValue "" |> (=) source |> Navbar.Item.IsActive)
                                                               str source ]

let navBrand (model : Model) (dispatch : Msg -> unit) =
    Navbar.navbar [ Navbar.Color IsWhite ]
        [ Container.container []
              [ Navbar.Brand.div []
                    [ Navbar.Item.a [ Navbar.Item.CustomClass "brand-text" ]
                          [ str "SAFE Admin" ] ]

                Navbar.menu []
                    [ Navbar.Start.div []
                          (match model.QuestSources with
                           | None -> []
                           | Some sources ->
                               sources |> List.map (navSource model dispatch)) ] ] ]

let showQuestLine (dispatch : Msg -> unit) (ql : QuestLineInfo) =
    Menu.Item.a [ Menu.Item.OnClick(fun _ ->
                      ql.Id
                      |> QuestLineSelected
                      |> dispatch) ] [ ql.Name |> str ]

let menu (model : Model) dispatch =
    Menu.menu [] [ Menu.label [] []
                   (//model.SelectedSource |> (Option.defaultValue "Select a source on top") |> str
                    match None with //model.QuestLines with
                    | None -> Menu.label [] []
                    | Some ql ->
                        Menu.list [] (ql
                                      |> List.sortBy (fun ql -> ql.Order)
                                      |> List.map (showQuestLine dispatch))) ]

let qlInfo (ql : Shared.QuestLineInfo) =
    Hero.hero []
        [ Hero.body []
              [ Container.container []
                    [ Heading.h1 [] [ str ql.Name ]
                      Heading.h5 [ Heading.IsSubtitle ] [ str ql.Description ] ] ] ]

let showPrerequisite pr =
    a [ pr
        |> sprintf "#%i"
        |> Href ] [ Tag.tag [] [ pr
                                 |> string
                                 |> str ] ]

let showQuest (q : QuestLineQuest) =
    Card.card [ Props [ q.Id
                        |> string
                        |> Id ] ]
        [ Card.header []
              [ Card.Header.title [] [ str q.Quest.Name ]

                Card.Header.icon [ Props [ q.Id
                                           |> sprintf "#%i"
                                           |> Href ] ]
                    [ i [ ClassName "fa fa-hashtag" ] [] ] ]

          Card.content []
              [ Content.content []
                    (match q.Quest.Prerequisites with
                     | [] -> List.empty
                     | prereqs ->
                         [ (Tag.list [] (prereqs |> List.map showPrerequisite)) ]) ]

          Card.content []
              [ Content.content []
                    [ pre [ Style [ WhiteSpaceOptions.PreWrap |> WhiteSpace ] ]
                          [ str q.Quest.Description ] ] ] ]

let showQuestLineQuest (qlq : QuestLineQuest) =
    let (x, y) = qlq.Location
    let (sx, sy) = qlq.Size
    a [ qlq.Id
        |> sprintf "#%i"
        |> Href ] [ div [ Style [ Height sx
                                  Width sy
                                  Position PositionOptions.Absolute
                                  Top x
                                  Left y
                                  Background "#000" ] ] [] ]

let questLineView (model : Model) : ReactElement list =
    match None with //model.SelectedQuestLine with
    | None ->
        [ Hero.hero []
              [ Hero.body []
                    [ Container.container []
                          [ Heading.h1 []
                                [ str "Select a QuestLine on the left" ] ] ] ] ]
    | Some ql ->
        [ (ql.QuestLineInfo |> qlInfo)
          div [ Style [ Height(ql.Quests
                               |> List.map
                                      (fun q -> fst q.Location + fst q.Size)
                               |> List.max)
                        Width(ql.Quests
                              |> List.map (fun q -> snd q.Location + snd q.Size)
                              |> List.max)
                        Position PositionOptions.Relative
                        OverflowStyle OverflowOptions.Auto ]
                Class "box" ] (ql.Quests |> List.map showQuestLineQuest)
          div [] (ql.Quests |> List.map showQuest) ]

let view (model : Model) (dispatch : Msg -> unit) =
    div []
        [ navBrand model dispatch

          Container.container []
              [ Columns.columns []
                    [ Column.column [ Column.Width(Screen.All, Column.Is3) ]
                          [ menu model dispatch ]

                      Column.column [ Column.Width(Screen.All, Column.Is9) ]
                          [ div [] (questLineView model) ] ] ] ]
