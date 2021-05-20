﻿namespace FTGO.Restaurant

open FTGO.Restaurant.Entities
open FTGO.Restaurant.Events
open FTGO.Restaurant.UseCases
open FTGO.Common.Operators.OptionOperators

module RestaurantService =

    let private entityCreated : RestaurantEntityCreated =
        fun entity -> {
            Name = entity.Name
        }

    let create (createEntity : CreateRestaurantEntity) : CreateRestaurant =
        let createEntity = createEntity entityCreated
        fun args -> async {
            let! entity = args |> CreateRestaurantArgs.toEntityArgs |> createEntity
            return entity |> Restaurant.fromEntity
        }

    let find (findEntity : FindRestaurantEntity) : FindRestaurant =
        fun id -> async {
            let! entity = id |> findEntity
            return entity |>> Restaurant.fromEntity
        }