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
    { QuestLines : Shared.QuestLine [] option
      SelectedQuestLine : Shared.QuestLineWithQuests option }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
    | QuestLineSelected of int
    | QuestLineReceived of Shared.QuestLineWithQuests
    | InitialDataLoaded of Shared.QuestLine []
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
        Cmd.OfAsync.perform Server.questAPI.questLines () InitialDataLoaded
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

let navBrand =
    Navbar.navbar [ Navbar.Color IsWhite ] 
        [ Container.container [] 
              [ Navbar.Brand.div [] 
                    [ Navbar.Item.a [ Navbar.Item.CustomClass "brand-text" ] 
                          [ str "SAFE Admin" ] ]
                
                Navbar.menu [] 
                    [ Navbar.Start.div [] [ Navbar.Item.a [] [ str "Home" ] ] ] ] ]

let showQuestLine (dispatch : Msg -> unit) (ql : QuestLine) =
    Menu.Item.a [ Menu.Item.OnClick(fun _ -> 
                      ql.Id
                      |> QuestLineSelected
                      |> dispatch) ] [ ql.Name |> str ]

let menu (model : Model) dispatch =
    Menu.menu [] [ Menu.label [] [ str "QuestLines" ]
                   Menu.list [] (model.QuestLines
                                 |> Option.defaultValue Array.empty
                                 |> Array.sortBy (fun ql -> ql.Order)
                                 |> Array.map (showQuestLine dispatch)
                                 |> Array.toList) ]

let qlInfo (ql : Shared.QuestLine) =
    Hero.hero [] 
        [ Hero.body [] 
              [ Container.container [] 
                    [ Heading.h1 [] [ str ql.Name ]
                      Heading.h2 [ Heading.IsSubtitle ] [ str ql.Description ] ] ] ]

let showQuest (q : Quest) =
    Box.box' [] 
        [ a [ q.Id |> sprintf "#%i" |> Href ] [ Heading.h6 [ Heading.Props [q.Id |> string |> Id ] ] [ str q.Name ] ]
          
          Heading.h6 [ Heading.IsSubtitle ] 
              (match q.Prerequisites with
               | [] -> []
               | prereqs -> 
                   ((span [] [ str "Prerequisites: " ]) 
                    :: [ Tag.list [] 
                             (prereqs
                              |> List.map (fun pr -> 
                                     Tag.tag [ Tag.Color IsInfo ] [ pr
                                                                    |> string
                                                                    |> str ])) ]))
          pre [ Style [ WhiteSpace "pre-wrap" ] ] [ str q.Description ] ]

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
        [ (ql.QuestLine |> qlInfo)
          div [ Style [ 
                  Height (ql.QuestLineQuests |> List.map (fun q -> fst q.Location + fst q.Size) |> List.max)
                  Width (ql.QuestLineQuests |> List.map (fun q -> snd q.Location + snd q.Size) |> List.max)
                  Position PositionOptions.Relative
                  Overflow "auto" ]; 
              Class "box" ]
              (ql.QuestLineQuests |> List.map showQuestLineQuest)
          
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
