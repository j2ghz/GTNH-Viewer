namespace Shared

type Remote<'a> =
    | Empty
    | Loading
    | LoadError of string
    | Body of 'a

module Route =
    let builder typeName methodName = sprintf "/api/%s/%s" typeName methodName

type Source = string

type QuestLineInfo =
    { Id: int
      Name: string
      Order: int
      Description: string }

type Quest =
    { Id: int
      Name: string
      Description: string
      Prerequisites: int list
      Icon: string
      Main: bool }

type QuestLineQuest =
    { Id: int
      Location: int * int
      Size: int * int
      Quest: Quest }

type QuestLine =
    { QuestLineInfo: QuestLineInfo
      Quests: QuestLineQuest list }

type GregtechDetails =
    { MachineName: string }

type RecipeType =
    | Shaped
    | Shapeless
    | Gregtech of GregtechDetails

type Item =
    { rawName: string option
      Name: string option }

type ItemAmount =
    { Item: Item
      Amount: int }

type Recipe =
    { Input: ItemAmount list
      Output: ItemAmount list
      Details: RecipeType }

type IApi =
    { questSources: unit -> Async<Source list>
      quests: string -> Async<Quest list>
      questLines: string -> Async<QuestLineInfo list>
      questLineById: string -> int -> Async<QuestLine>
      recipeSources: unit -> Async<Source list>
      recipes: string -> Async<Recipe list>
      items: string -> Async<Item list> }
