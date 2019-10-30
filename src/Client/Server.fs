module Server

open Fable.Remoting.Client
open Shared

let API: IApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IApi>
