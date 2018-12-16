﻿using AmeisenBot.Clients;
using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotDB;
using AmeisenBotUtilities;
using System.Collections.Generic;
using static AmeisenBotFSM.Objects.Delegates;

namespace AmeisenBotFSM.Actions
{
    public class ActionDead : ActionMoving
    {
        public override Start StartAction { get { return Start; } }
        public override DoThings StartDoThings { get { return DoThings; } }
        public override Exit StartExit { get { return Stop; } }
        private Unit ActiveUnit { get; set; }
        private List<Unit> ActiveUnits { get; set; }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }
        private AmeisenDBManager AmeisenDBManager { get; set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }

        public ActionDead(AmeisenDataHolder ameisenDataHolder,
            AmeisenDBManager ameisenDBManager,
            AmeisenNavmeshClient ameisenNavmeshClient) : base(ameisenDataHolder, ameisenDBManager, ameisenNavmeshClient)
        {
            AmeisenDataHolder = ameisenDataHolder;
            AmeisenDBManager = ameisenDBManager;
        }

        public override void DoThings()
        {
            GoToCorpseAndRevive();
            base.DoThings();
        }

        public override void Start() => base.Start();

        public override void Stop() => base.Stop();

        private void GoToCorpseAndRevive()
        {
            Vector3 corpsePosition = AmeisenCore.GetCorpsePosition();

            if (corpsePosition.X != 0 && corpsePosition.Y != 0 && corpsePosition.Z != 0)
            {
                if (!WaypointQueue.Contains(corpsePosition))
                {
                    WaypointQueue.Enqueue(corpsePosition);
                }
            }

            if (Utils.GetDistance(Me.pos, corpsePosition) < 10.0)
            {
                AmeisenCore.RetrieveCorpse(true);
            }
        }
    }
}