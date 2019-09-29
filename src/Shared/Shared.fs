namespace Shared

type Counter = { Value : int }

module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type QuestLineInfo = {
    Id : int
    Name : string
    Order : int
    Description : string
}

type Quest = {
    Id: int
    Name: string
    Description : string
    Prerequisites : int list
}

type QuestLineQuest = {
    Id : int
    Location : int * int
    Size : int * int
    Quest : Quest
}

type QuestLine = {
    QuestLineInfo : QuestLineInfo
    Quests : QuestLineQuest list
}

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type IQuestApi =
    { 
        quests : unit -> Async<Quest list>
        questLines : unit -> Async<QuestLineInfo list>
        questLineById : int -> Async<QuestLine>
    }
