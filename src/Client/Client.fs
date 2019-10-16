module Client

open Types
open Elmish
open Elmish.Navigation
open Elmish.React
open Elmish.UrlParser



//#if DEBUG for debugging

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif


Program.mkProgram (fun _ -> Types.Page.Home |> State.init ) State.update View.view
#if DEBUG
|> Program.withConsoleTrace
#endif

|> Program.withReactBatched "elmish-app"
//|> Program.toNavigable State.route urlUpdate
#if DEBUG
|> Program.withDebugger
#endif

|> Program.run
