﻿namespace FTGO.Restaurant.UseCases

open FTGO.Common.BaseTypes
open FTGO.Restaurant.BaseTypes

type CreateMenuItemArgs = {
    Name : NonEmptyString
    Price : decimal
}

type CreateRestaurantArgs = {
    Name : NonEmptyString
    Menu : CreateMenuItemArgs list
}

type Restaurant = {
    Id : Versioned<RestaurantId>
    Name : NonEmptyString
}

type CreateRestaurant = CreateRestaurantArgs -> Async<Restaurant>

type ReadRestaurant = RestaurantId -> Async<Restaurant option>
