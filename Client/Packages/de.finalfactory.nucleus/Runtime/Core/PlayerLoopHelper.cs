using System;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace FinalFactory.Core
{
    public static class PlayerLoopHelper
    {
        /// <summary>
        ///     Insert a subsystem into the player loop
        /// </summary>
        /// <param name="system">The subsystem to insert</param>
        /// <typeparam name="T">
        ///     Declare in which main system you want to insert the subsystem. Only types allowed unter the
        ///     namespace UnityEngine.PlayerLoop
        /// </typeparam>
        public static void InsertSubsystem<T>(IPlayerLoopSystem system)
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            // Create a new subsystem
            var newSubsystem = new PlayerLoopSystem
            {
                type = system.GetType(),
                updateDelegate = system.Update
            };

            // Insert the new subsystem into the Update phase of the player loop
            for (var i = 0; i < playerLoop.subSystemList.Length; i++)
                if (playerLoop.subSystemList[i].type == typeof(Update))
                {
                    var playerLoopSystems = playerLoop.subSystemList[i].subSystemList;
                    Array.Resize(ref playerLoopSystems, playerLoopSystems.Length + 1);
                    playerLoopSystems[^1] = newSubsystem;
                    playerLoop.subSystemList[i].subSystemList = playerLoopSystems;
                    break;
                }

            // Set the modified player loop as the current player loop
            PlayerLoop.SetPlayerLoop(playerLoop);
        }
    }
}