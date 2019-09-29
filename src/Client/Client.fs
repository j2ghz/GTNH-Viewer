module Client

open Elmish
open Elmish.React
open Fable.FontAwesome
open Fable.FontAwesome.Free
open Fable.React
open Fable.React.Props
open Fulma
open Thoth.Json
open Shared

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model =
    { QuestLines : Shared.QuestLineInfo list option
      SelectedQuestLine : Shared.QuestLine option }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
    | QuestLineSelected of int
    | QuestLineReceived of Shared.QuestLine
    | InitialDataLoaded of Shared.QuestLineInfo list
    | Error of exn

module Server =
    open Shared
    open Fable.Remoting.Client
    
    /// A proxy you can use to talk to server directly
    let questAPI : IQuestApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.buildProxy<IQuestApi>

// defines the initial state and initial command (= side-effect) of the application
let init() : Model * Cmd<Msg> =
    let initialModel =
        { QuestLines = None
          SelectedQuestLine = None }
    
    let loadCountCmd =
        Cmd.OfAsync.either Server.questAPI.questLines () InitialDataLoaded Error
    initialModel, loadCountCmd

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel, msg with
    | m, InitialDataLoaded quests -> 
        { m with QuestLines = Some quests }, Cmd.none
    | m, QuestLineSelected i -> 
        { m with SelectedQuestLine = None }, 
        Cmd.OfAsync.either Server.questAPI.questLineById i QuestLineReceived 
            Error
    | m, QuestLineReceived ql -> 
        { m with SelectedQuestLine = (Some ql) }, Cmd.none
    | _, Error e -> 
        printf "%O" e
        currentModel, Cmd.none
    | _ -> currentModel, Cmd.none

let stri = sprintf "%i" >> str

//NAVBAR
let navBrand =
    Navbar.navbar [ Navbar.Color IsWhite ] 
        [ Container.container [] 
              [ Navbar.Brand.div [] 
                    [ Navbar.Item.a [ Navbar.Item.CustomClass "brand-text" ] 
                          [ str "SAFE Admin" ] ]
                
                Navbar.menu [] 
                    [ Navbar.Start.div [] [ Navbar.Item.a [] [ str "Home" ] ] ] ] ]

//MENU
let showQuestLine (dispatch : Msg -> unit) (ql : QuestLineInfo) =
    Menu.Item.a [ Menu.Item.OnClick(fun _ -> 
                      ql.Id
                      |> QuestLineSelected
                      |> dispatch) ] [ ql.Name |> str ]

let menu (model : Model) dispatch =
    Menu.menu [] [ Menu.label [] [ str "QuestLines" ]
                   Menu.list [] (model.QuestLines
                                 |> Option.defaultValue List.empty
                                 |> List.sortBy (fun ql -> ql.Order)
                                 |> List.map (showQuestLine dispatch)) ]

let qlInfo (ql : Shared.QuestLineInfo) =
    Hero.hero [] 
        [ Hero.body [] 
              [ Container.container [] 
                    [ Heading.h1 [] [ str ql.Name ]
                      Heading.h2 [ Heading.IsSubtitle ] [ str ql.Description ] ] ] ]

let showPrerequisite pr =
    a [ pr
        |> sprintf "#%i"
        |> Href ] [ Tag.tag [ Tag.Color IsInfo ] [ pr
                                                   |> string
                                                   |> str ] ]

let showQuest (q : QuestLineQuest) =
    Box.box' [] 
        [ a 
            [ q.Id |> sprintf "#%i" |> Href ] 
            [ Heading.h4 
                [ Heading.Props [ q.Id |> string |> Id ] ] 
                [ Tag.tag [ Tag.Size IsLarge; Tag.Color IsDark ] [ stri q.Id ]; str q.Quest.Name ] ]
          
          div [ ] 
              (match q.Quest.Prerequisites with
               | [] -> []
               | prereqs -> 
                   [ (Tag.list [] (prereqs |> List.map showPrerequisite)) ])
          pre [ Style [ WhiteSpace "pre-wrap" ] ] [ str q.Quest.Description ] ]

    Card.card [ Props [ q.Id |> string |> Id ] ]
        [ Card.header [ ]
            [ Card.Header.title [ ]
                [ str q.Quest.Name ]
              Card.Header.icon [ Props [ q.Id |> sprintf "#%i" |> Href ] ]
                [ i [ ClassName "fa fa-hashtag" ] [ ] ] ]
          Card.content [ ]
            [ Content.content [ ]
                 (match q.Quest.Prerequisites with
                   | [] -> List.empty
                   | prereqs -> [ (Tag.list [] (prereqs |> List.map showPrerequisite)) ]) ]]
          Card.content [ ]
            [ Content.content [ ]
                [ pre [ Style [ WhiteSpace "pre-wrap" ] ] [ str q.Quest.Description ] ] ]
          Card.footer [ ]
            [ Card.Footer.a [ ]
                [ str "Save" ]
              Card.Footer.a [ ]
                [ str "Edit" ]
              Card.Footer.a [ ]
                [ str "Delete" ] ] ]

    

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
    match model.SelectedQuestLine with
    | None -> 
        [ Hero.hero [ Hero.Color IsInfo ] 
              [ Hero.body [] 
                    [ Container.container [] 
                          [ Heading.h1 [] [ str "Select a QuestLine" ] ] ] ] ]
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
                        Overflow "auto" ]
                Class "box" ] (ql.Quests |> List.map showQuestLineQuest)
          div [] (ql.Quests |> List.map showQuest) ]

let view (model : Model) (dispatch : Msg -> unit) =
    div [] 
        [ navBrand
          
          Container.container [] 
              [ Columns.columns [] 
                    [ Column.column [ Column.Width(Screen.All, Column.Is3) ] 
                          [ menu model dispatch ]
                      
                      Column.column [ Column.Width(Screen.All, Column.Is9) ] 
                          [ div [] (questLineView model) ] ] ] ]
#if DEBUG

open Elmish.Debug
open Elmish.HMR
#endif


Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif

|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif

|> Program.run
