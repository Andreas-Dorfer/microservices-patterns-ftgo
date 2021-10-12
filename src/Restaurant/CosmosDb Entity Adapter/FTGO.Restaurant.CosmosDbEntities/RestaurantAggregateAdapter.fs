namespace FTGO.Restaurant.CosmosDbEntities

open System
open FTGO.Common.BaseTypes
open FTGO.Restaurant.BaseTypes
open FTGO.Restaurant.Entities
open FTGO.Restaurant.CosmosDbEntities.Core

module RestaurantAggregateAdapter =

    let private ofCosmosEntity (entity : Restaurant) = {
        Id = entity.Id |> Guid.Parse |> RestaurantId.create |> Option.get
        Name = entity.Name |> NonEmptyString.create |> Option.get
        Menu = entity.Menu |> Seq.map (fun m -> { Id = m.Id |> Guid.Parse |> MenuItemId.create |> Option.get; Name = m.Name |> NonEmptyString.create |> Option.get; Price = m.Price }) |> Seq.toList
    }

    let create container : CreateRestaurantEntity =
        let dataAccess = container |> RestaurantDataAccess
        fun (restaurant, created) -> async {

            let entity = Restaurant ()
            entity.Id <- restaurant.Id.Value.ToString()
            entity.Name <- restaurant.Name.Value
            restaurant.Menu
            |> Seq.map (fun m -> MenuItem (m.Id.Value.ToString(), m.Name.Value, m.Price))
            |> entity.Menu.AddRange

            let event = RestaurantCreated ()
            event.Id <- created.Id.ToString()
            event.Name <- created.Name.Value

            let! entity = (entity, event) |> dataAccess.Create |> Async.AwaitTask

            return Versioned (entity |> ofCosmosEntity, ETag entity.ETag)
        }
