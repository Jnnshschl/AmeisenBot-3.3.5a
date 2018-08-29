﻿using AmeisenData;
using AmeisenUtilities;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenAI
{
    public class AmeisenFollowManager
    {
        #region Private Fields

        private static readonly object padlock = new object();
        private static AmeisenFollowManager instance;
        private readonly Thread mainWorker;
        private List<Unit> followUnitList = new List<Unit>();
        private bool stop = false;

        #endregion Private Fields

        #region Singleton stuff

        private AmeisenFollowManager()
        {
            mainWorker = new Thread(new ThreadStart(DoWork));
        }

        /// <summary>
        /// Initialize/Get the instance of our singleton
        /// </summary>
        /// <returns>AmeisenFollowManager instance</returns>
        public static AmeisenFollowManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new AmeisenFollowManager();
                    return instance;
                }
            }
        }

        #endregion Singleton stuff

        #region Private Properties

        private Me Me
        {
            get { return AmeisenDataHolder.Instance.Me; }
            set { AmeisenDataHolder.Instance.Me = value; }
        }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        /// Register a unit to be followed
        /// </summary>
        /// <param name="unit">unit object to follow</param>
        public void AddPlayerToFollow(Unit unit)
        {
            followUnitList.Add(unit);
        }

        /// <summary>
        /// Unregister all units to be no longer followed
        /// </summary>
        public void RemoveAllPlayersToFollow()
        {
            followUnitList.Clear();
        }

        /// <summary>
        /// Unregister a single unit to be no longer followed
        /// </summary>
        /// <param name="unit"></param>
        public void RemovePlayerToFollow(Unit unit)
        {
            followUnitList.Remove(unit);
        }

        /// <summary>
        /// Start the follow engine
        /// </summary>
        public void Start()
        {
            mainWorker.Start();
        }

        /// <summary>
        /// Stop the follow engine
        /// </summary>
        public void Stop()
        {
            stop = true; mainWorker.Join();
        }

        #endregion Public Methods

        #region Private Methods

        private void DoWork()
        {
            while (!stop)
            {
                if (Me.NeedToRevive || !AmeisenAIManager.Instance.DoFollow)
                {
                    Thread.Sleep(25);
                    continue;
                }

                if (AmeisenCoreUtils.AmeisenCore.IsGhost(LUAUnit.player))
                {
                    //TODO: Handle death follow GO_TO_CORPSE_AND_REVIVE

                    AmeisenAction ameisenAction = new AmeisenAction(
                    AmeisenActionType.GO_TO_CORPSE_AND_REVIVE, null);

                    AmeisenAIManager.Instance.AddActionToQueue(ref ameisenAction);

                    while (!ameisenAction.IsActionDone())
                        Thread.Sleep(50);
                }

                if (followUnitList.Count > 0)
                {
                    Unit activeUnit = null;

                    foreach (Unit u in followUnitList)
                    {
                        if (u == null)
                            continue;
                        u.Update();
                        activeUnit = u;
                        break;
                    }

                    if (activeUnit != null && AmeisenAIManager.Instance.DoFollow)
                    {
                        AmeisenAction ameisenAction = new AmeisenAction(
                            AmeisenActionType.MOVE_NEAR_POSITION,
                            new object[] {
                                activeUnit.pos,
                                4.0 } // Follow distance
                            );

                        AmeisenAIManager.Instance.AddActionToQueue(ref ameisenAction);

                        while (!ameisenAction.IsActionDone())
                            Thread.Sleep(50);
                    }
                }

                Thread.Sleep(50);
            }
        }

        #endregion Private Methods
    }
}