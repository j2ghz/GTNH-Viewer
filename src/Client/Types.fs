module Types

type Page =
    // Pages of App
    | Home
    // Sub pages of App
    | Quests of Quests.Types.Page
    | Recipes of Recipes.Types.Page

type AppModel =
    { //App's own state
      CurrentPage: Option<Page>
      //Children states
      Quests: Quests.Types.State
      Recipes: Recipes.Types.State }

type AppMsg =
    //Children messages
    | QuestsMsg of Quests.Types.Msg
    | RecipesMsg of Recipes.Types.Msg
    //App messages
    //Navigation
    | NavigateTo of Page
