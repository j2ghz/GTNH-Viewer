namespace Shared

type Counter = { Value : int }

module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

// must be preprocessed with:
// jq 'walk( if type == "object" then with_entries( .key |= sub( ":.+$"; "") ) else . end )' DefaultQuests.json
type BetterQuestingDB = FSharp.Data.JsonProvider<"../Shared/DefaultQuests-cleaned.json">

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type IQuestApi =
    { questList : unit -> Async<BetterQuestingDB.QuestDatabase> }
