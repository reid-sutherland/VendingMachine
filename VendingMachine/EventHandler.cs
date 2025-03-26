using Exiled.Events;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Features;
using System;
using Exiled.API.Enums;
using UnityEngine;
using Exiled.API.Features.Items;
using Exiled.Events.Commands.Reload;

namespace Scp914;

public class EventHandler
{
    public void Enable()
    {
        Exiled.Events.Handlers.Player.SearchingPickup += Interacted;
    }

    public void Disable()
    {
        Exiled.Events.Handlers.Player.SearchingPickup -= Interacted;
    }

    private void Interacted(SearchingPickupEventArgs ev)
    {
        Player player = ev.Player;
        if (ev.Pickup.GameObject.name.Equals("Vending"))
        {
            if (player.CurrentItem == null)
            {

                player.ShowHint(Config.InteractionFailedMessage, 5f);
                return;
            }

            if (player.CurrentItem.Type == ItemType.Coin)
            {
                player.RemoveItem(player.CurrentItem);
                var weightedChances = new WeightedChanceExecutor(
                    new WeightedChanceParam(() =>
                    {
                        System.Random random = new System.Random();
                        ItemType randomItem = Config.VendingMachineStock[random.Next(Config.VendingMachineStock.Count)];
                        player.AddItem(randomItem);
                        player.ShowHint(Config.InteractionSuccessfulItems, 5f);
                    }, Config.ItemChance),
                    new WeightedChanceParam(() =>
                    {
                        if (Config.EnableEffects == true)
                        {
                            System.Random random = new System.Random();
                            EffectType randomEffect = Config.VendingMachineEffects[random.Next(Config.VendingMachineEffects.Count)];
                            player.EnableEffect(randomEffect, random.Next(Config.MinEffectDuration, Config.MaxEffectDuration));
                            player.ShowHint(Config.InteractionSuccessfulEffects, 5f);
                        }
                        else
                        {
                            System.Random random = new System.Random();
                            ItemType randomItem = Config.VendingMachineStock[random.Next(Config.VendingMachineStock.Count)];
                            player.AddItem(randomItem);
                            player.ShowHint(Config.InteractionSuccessfulItems, 5f);
                        }
                    }, Config.EffectChance),
                    new WeightedChanceParam(() =>
                    {
                        if (Config.EnableSizeChange == true)
                        {
                            player.Scale = Config.GnomedSize;
                            player.ShowHint(Config.InteractionSuccessfulSize, 5f);
                        }
                        else
                        {
                            System.Random random = new System.Random();
                            ItemType randomItem = Config.VendingMachineStock[random.Next(Config.VendingMachineStock.Count)];
                            player.AddItem(randomItem);
                            player.ShowHint(Config.InteractionSuccessfulItems, 5f);
                        }
                    }, Config.SizeChangeChance),
                    new WeightedChanceParam(() =>
                    {
                        if (Config.EnableExplode == true)
                        {
                            player.Explode();
                            player.ShowHint(Config.InteractionSuccessfulExplode, 5f);
                        }
                        else
                        {
                            System.Random random = new System.Random();
                            ItemType randomItem = Config.VendingMachineStock[random.Next(Config.VendingMachineStock.Count)];
                            player.AddItem(randomItem);
                            player.ShowHint(Config.InteractionSuccessfulItems, 5f);
                        }
                    }, Config.ExplodeChance),
                    new WeightedChanceParam(() =>
                    {
                        if (Config.EnableFlashbang == true)
                        {
                            FlashGrenade flash = (FlashGrenade)Item.Create(ItemType.GrenadeFlash, player);
                            flash.FuseTime = 1f;
                            flash.SpawnActive(player.Position);
                            player.ShowHint(Config.InteractionSuccessfulFlashbang, 5f);
                        }
                        else
                        {
                            System.Random random = new System.Random();
                            ItemType randomItem = Config.VendingMachineStock[random.Next(Config.VendingMachineStock.Count)];
                            player.AddItem(randomItem);
                            player.ShowHint(Config.InteractionSuccessfulItems, 5f);
                        }
                    }, Config.FlashbangChance),
                     new WeightedChanceParam(() =>
                     {
                         if (Config.EnableTantrum == true)
                         {
                             player.PlaceTantrum();
                             player.ShowHint(Config.InteractionSuccessfulTantrum, 5f);
                         }
                         else
                         {
                             System.Random random = new System.Random();
                             ItemType randomItem = Config.VendingMachineStock[random.Next(Config.VendingMachineStock.Count)];
                             player.AddItem(randomItem);
                             player.ShowHint(Config.InteractionSuccessfulItems, 5f);
                         }
                     }, Config.TantrumChance)

             );
                weightedChances.Execute();
            }
            else
            {
                player.ShowHint(Config.InteractionFailedMessage, 5f);
            }
        }
    }
}