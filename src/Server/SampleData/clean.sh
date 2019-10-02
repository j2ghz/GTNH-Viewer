#!/bin/bash
# Beacuse of the wierdness of BQ json files, they must be pre-processed with this jq script:
jq 'walk( if type == "object" then with_entries( .key |= sub( ":.+$"; "") ) else . end ) | walk( if type == "object" and has("0") then . | to_entries | map_values(.value) else . end)' DefaultQuests.json > DefaultQuests-cleaned.json
# The walk function applies to every object recursively. There are two of them:
# 1. Cut a sequence of colon and number form ends of strings. No idea what they mean.
# 2. If an object has a property with key "0", change it to an array. No idea why the original json uses objects for arrays.
# Both these issues maade it impossible to autogenerate a model for the parser from the raw json.
# Thankfully, JQ exists.