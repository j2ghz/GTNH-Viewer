module Server
open Fable.Remoting.Client
open Shared

let questAPI : IQuestApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IQuestApi>