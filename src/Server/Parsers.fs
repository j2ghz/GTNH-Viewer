module Parsers

// must be preprocessed with:
// jq 'walk( if type == "object" then with_entries( .key |= sub( ":.+$"; "") ) else . end ) | walk( if type == "object" and has("0") then . | to_entries | map_values(.value) else . end)' DefaultQuests.json > DefaultQuests-cleaned.json
type BetterQuestingDB = FSharp.Data.JsonProvider<"../Shared/SampleData/DefaultQuests-master-cleaned.json">