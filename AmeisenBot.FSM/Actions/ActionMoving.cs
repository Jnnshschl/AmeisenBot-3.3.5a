﻿using AmeisenBot.Clients;
using AmeisenBotCore;
using AmeisenBotData;
using AmeisenBotDB;
using AmeisenBotFSM.Interfaces;
using AmeisenBotLogger;
using AmeisenBotMapping.objects;
using AmeisenBotUtilities;
using AmeisenBotUtilities.Structs;
using AmeisenPathCore;
using AmeisenPathCore.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using static AmeisenBotFSM.Objects.Delegates;

namespace AmeisenBotFSM.Actions
{
    public class ActionMoving : IAction
    {
        public virtual Start StartAction { get { return Start; } }
        public virtual DoThings StartDoThings { get { return DoThings; } }
        public virtual Exit StartExit { get { return Stop; } }
        public Queue<Vector3> WaypointQueue { get; set; }
        private AmeisenDataHolder AmeisenDataHolder { get; set; }
        private AmeisenDBManager AmeisenDBManager { get; set; }
        private AmeisenNavmeshClient AmeisenNavmeshClient { get; set; }
        private Vector3 LastPosition { get; set; }
        private double MovedSinceLastTick { get; set; }
        public Vector3 LastEnqued { get; internal set; }

        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }

        public ActionMoving(AmeisenDataHolder ameisenDataHolder, AmeisenDBManager ameisenDBManager, AmeisenNavmeshClient ameisenNavmeshClient)
        {
            AmeisenDataHolder = ameisenDataHolder;
            AmeisenDBManager = ameisenDBManager;
            AmeisenNavmeshClient = ameisenNavmeshClient;

            WaypointQueue = new Queue<Vector3>();
            LastPosition = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);
        }

        public virtual void DoThings()
        {
            if (WaypointQueue.Count > 0)
            {
                double distance = Utils.GetDistance(Me.pos, LastEnqued);

                if (distance > AmeisenDataHolder.Settings.useMountFollowDistance && AmeisenCore.IsOutdoors)
                {
                    if (!AmeisenCore.IsMounted)
                    {
                        AmeisenCore.MountRandomMount(AmeisenDataHolder.Settings.landMounts, AmeisenDataHolder.Settings.flyingMounts);
                        Thread.Sleep(1500);
                    }
                }

                if (distance < AmeisenDataHolder.Settings.useMountFollowDistance + 6)
                {
                    if (AmeisenCore.IsMounted)
                    {
                        AmeisenCore.MountRandomMount(AmeisenDataHolder.Settings.landMounts, AmeisenDataHolder.Settings.flyingMounts);
                    }
                }

                MoveToNode();
            }
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
        }

        /// <summary>
        /// Very basic Obstacle avoidance.
        ///
        /// Need to change this to a better waypoint system that uses our MapNode Database...
        /// </summary>
        /// <param name="initialPosition">initial position</param>
        /// <param name="activePosition">position now</param>
        /// <returns>if we havent moved 0.5m in the 2 vectors, jump and return true</returns>
        private bool CheckIfWeAreStuckIfYesJump(Vector3 initialPosition, Vector3 activePosition)
        {
            double oldMovedSinceLastTick = MovedSinceLastTick;
            MovedSinceLastTick = Utils.GetDistance(initialPosition, activePosition);

            // we are possibly stuck at a fence or something alike
            if (MovedSinceLastTick != 0 && MovedSinceLastTick < 1000)
            {
                if (MovedSinceLastTick - oldMovedSinceLastTick < AmeisenDataHolder.Settings.MovementJumpThreshold)
                {
                    AmeisenCore.CharacterJumpAsync();
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Jumping: {MovedSinceLastTick}", this);
                    return true;
                }
            }
            return false;
        }

        private void MoveToNode()
        {
            if (WaypointQueue.Count > 0)
            {
                Me.Update();
                Vector3 initialPosition = Me.pos;
                Vector3 targetPosition = WaypointQueue.Peek();

                if (WaypointQueue.Count < 2 && Utils.GetDistance(initialPosition, targetPosition) >= AmeisenDataHolder.Settings.PathfindingUsageThreshold)
                {
                    if (AmeisenDataHolder.Settings.usePathfinding
                        && AmeisenDataHolder.IsConnectedToDB)
                    {
                        UsePathfinding(initialPosition, targetPosition);
                    }
                    else
                    {
                        MoveToNode(targetPosition);
                    }
                }
                else
                {
                    MoveToNode(targetPosition);
                }
            }
            else { }
        }

        private void UsePathfinding(Vector3 initialPosition, Vector3 targetPosition)
        {
            List<Vector3> navmeshPath = AmeisenNavmeshClient.RequestPath(new PathRequest(initialPosition, targetPosition, Me.MapID));

            if (navmeshPath != null)
            {
                ProcessNavmeshPath(navmeshPath);
            }
            else
            {
                List<Node> path = FindWayToNode(initialPosition, targetPosition);

                if (path != null)
                {
                    ProcessPath(path);
                }
                else
                {
                    AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Path is null", this);
                    // retry with thicker path
                    path = FindWayToNode(initialPosition, targetPosition, true);

                    if (path != null)
                    {
                        ProcessPath(path);
                    }
                    else
                    {
                        AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Thicker Path is null", this);
                        MoveToNode(targetPosition);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        private void ProcessNavmeshPath(List<Vector3> navmeshPath)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Found Navmesh Path...", this);
            foreach (Vector3 node in navmeshPath)
            {
                WaypointQueue.Enqueue(node);
            }
        }

        /// <summary>
        /// Process a path by adding all nodes to the WaypointQueue
        ///
        /// will set PathCalculated to true
        /// </summary>
        /// <param name="path">path to process</param>
        private void ProcessPath(List<Node> path)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Found Database Path...", this);
            foreach (Node node in path)
            {
                WaypointQueue.Enqueue(new Vector3(node.Position.X, node.Position.Y, node.Position.Z));
            }
        }

        /// <summary>
        /// Move to a node with a max of 10 tries, if we aren't in the node range
        /// after this, there is something fishy going on
        /// </summary>
        /// <param name="targetPosition">position to move to</param>
        private void MoveToNode(Vector3 targetPosition)
        {
            double followOffset = 0;

            if (AmeisenCore.IsMounted)
            {
                followOffset = 5;
            }

            while (Utils.GetDistance(Me.pos, targetPosition) > 3 + followOffset)
            {
                CheckIfWeAreStuckIfYesJump(Me.pos, LastPosition);

                if (targetPosition.Z == 0)
                {
                    targetPosition.Z = Me.pos.Z;
                }

                AmeisenCore.MovePlayerToXYZ(targetPosition, InteractionType.MOVE);
                Thread.Sleep(100);

                if (Utils.GetDistance(Me.pos, targetPosition) < 3 + followOffset)
                {
                    WaypointQueue.Dequeue();
                }

                Me.Update();
                LastPosition = Me.pos;
            }
        }

        /// <summary>
        /// Remove all nodes that are in a straight line to reduce node count
        /// </summary>
        /// <param name="path">path to simplify</param>
        /// <returns>simplified path</returns>
        private List<Node> SimplifyPath(List<Node> path)
        {
            if (path == null)
            {
                return null;
            }

            List<Node> simplePath = new List<Node>();
            double oldX = 0.0;
            double oldY = 0.0;

            for (int x = 1; x < path.Count; x++)
            {
                double newX = path[x - 1].Position.X - path[x].Position.X;
                double newY = path[x - 1].Position.Y - path[x].Position.Y;

                if (newX != oldX && newY != oldY)
                {
                    simplePath.Add(path[x]);
                }

                oldX = newX;
                oldY = newY;
            }
            return simplePath;
        }

        /// <summary>
        /// Find a way to the given node and maybe thinken our path if wanted
        /// </summary>
        /// <param name="initialPosition">starting pos, you</param>
        /// <param name="targetPosition">target pos, the position to go to</param>
        /// <param name="thickenPath">if we should thinken our path, makes nodes connect better, but may be unsecure</param>
        /// <returns>path that was found or not</returns>
        private List<Node> FindWayToNode(Vector3 initialPosition, Vector3 targetPosition, bool thickenPath = false)
        {
            int distance = (int)Utils.GetDistance(initialPosition, targetPosition);

            // Get the map boundaries... limit nodes that we get from database... X
            int maxX = targetPosition.X >= initialPosition.X ? (int)targetPosition.X : (int)initialPosition.X;
            int minX = initialPosition.X <= targetPosition.X ? (int)initialPosition.X : (int)targetPosition.X;
            // Y
            int maxY = targetPosition.Y >= initialPosition.Y ? (int)targetPosition.Y : (int)initialPosition.Y;
            int minY = initialPosition.Y <= targetPosition.Y ? (int)initialPosition.Y : (int)targetPosition.Y;

            AmeisenLogger.Instance.Log(LogLevel.DEBUG, $"Trying to find path from {initialPosition.X},{initialPosition.Y},{initialPosition.Z} to: {targetPosition.X},{targetPosition.Y},{targetPosition.Z} Distance: {distance}", this);

            maxX += 10 * AmeisenDataHolder.Settings.PathfindingSearchRadius;
            minX -= 10 * AmeisenDataHolder.Settings.PathfindingSearchRadius;
            maxY += 10 * AmeisenDataHolder.Settings.PathfindingSearchRadius;
            minY -= 10 * AmeisenDataHolder.Settings.PathfindingSearchRadius;

            // Offsets to rebase nodes from negative values to positive X
            int offsetX = minX * -1;
            minX += offsetX;
            maxX += offsetX;
            // Y
            int offsetY = minY * -1;
            minY += offsetY;
            maxY += offsetY;

            initialPosition.X = (int)initialPosition.X + offsetX;
            initialPosition.Y = (int)initialPosition.Y + offsetY;
            targetPosition.X = (int)targetPosition.X + offsetX;
            targetPosition.Y = (int)targetPosition.Y + offsetY;

            // Get our nodes from the batabase
            List<MapNode> nodes = AmeisenDBManager.GetNodes(
                Me.ZoneID,
                Me.MapID,
                maxX - offsetX,
                minX - offsetX,
                maxY - offsetY,
                minY - offsetY);

            // We cant find ay path if there are no known nodes
            if (nodes.Count < 1)
            {
                return null;
            }

            Node[,] map = new Node[maxX + 1, maxY + 1];

            // Init map with all things blocked and Rebase negative nodes to be positive
            InitMap(ref map, maxX, minX, maxY, minY);
            RebaseNodes(ref map, nodes, offsetX, offsetY);

            // Fill path-gaps
            if (thickenPath)
            {
                map = ThinkenPathsOnMap(map, maxX, maxY, 5);
            }
            else
            {
                map = ThinkenPathsOnMap(map, maxX, maxY, 1);
            }

            // Find the path
            List<Node> path = AmeisenPath.FindPathAStar(map,
                                             new NodePosition((int)initialPosition.X, (int)initialPosition.Y, (int)initialPosition.Z),
                                             new NodePosition((int)targetPosition.X, (int)targetPosition.Y, (int)targetPosition.Z),
                                             true,
                                             true,
                                             5);

            //WriteMapToFile(map, initialPosition, targetPosition);

            if (path == null)
            {
                return null;
            }
            else
            {
                return RebasePath(path, offsetX, offsetY);
            }
        }

        /// <summary>
        /// Init a map with all fields blocked
        /// </summary>
        /// <param name="map">map to init</param>
        /// <param name="maxX">max X</param>
        /// <param name="minX">min X</param>
        /// <param name="maxY">max Y</param>
        /// <param name="minY">min Y</param>
        private void InitMap(ref Node[,] map, int maxX, int minX, int maxY, int minY)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    map[x, y] = new Node(new NodePosition(x, y, 0), true);
                }
            }
        }

        /// <summary>
        /// Add the offset to our map to eliminate negative coordinates
        /// </summary>
        /// <param name="map">the map to update</param>
        /// <param name="nodes">nodes to add to our map</param>
        /// <param name="offsetX">X offset</param>
        /// <param name="offsetY">Y offset</param>
        private void RebaseNodes(ref Node[,] map, List<MapNode> nodes, int offsetX, int offsetY)
        {
            foreach (MapNode node in nodes)
            {
                map[node.X + offsetX, node.Y + offsetY] = new Node(new NodePosition(node.X + offsetX, node.Y + offsetY, node.Z), false);
            }
        }

        /// <summary>
        /// Remove the offset from our Path that we used to calculate the path to
        /// reveal the wanted original coordinates
        /// </summary>
        /// <param name="path">path to rebase</param>
        /// <param name="offsetX">rebasing X offset</param>
        /// <param name="offsetY">rebasing Y offset</param>
        /// <returns>rebased Path</returns>
        private List<Node> RebasePath(List<Node> path, int offsetX, int offsetY)
        {
            List<Node> rebasedPath = new List<Node>();
            foreach (Node node in path)
            {
                rebasedPath.Add(
                    new Node(
                        new NodePosition(
                            node.Position.X - offsetX,
                            node.Position.Y - offsetY,
                            node.Position.Z),
                        false));
            }

            return rebasedPath;
        }

        /// <summary>
        /// Make our path thicker to fill small gaps
        /// </summary>
        /// <param name="map">map to thicken</param>
        /// <param name="maxX">map max X</param>
        /// <param name="maxY">map max Y</param>
        /// <returns></returns>
        private Node[,] ThinkenPathsOnMap(Node[,] map, int maxX, int maxY, int factor)
        {
            Node[,] newMap = new Node[map.GetLength(0), map.GetLength(1)];

            for (int y = 0; y <= maxY; y++)
            {
                for (int x = 0; x <= maxX; x++)
                {
                    if (!map[x, y].IsBlocked)
                    {
                        foreach (Node node in AmeisenPath.GetNeighbours(map, new NodePosition(x, y)))
                        {
                            newMap[node.Position.X, node.Position.Y] = new Node(
                                new NodePosition(
                                    map[node.Position.X, node.Position.Y].Position.X,
                                    map[node.Position.X, node.Position.Y].Position.Y,
                                    map[node.Position.X, node.Position.Y].Position.Z),
                                    false);
                        }
                    }
                    else
                    {
                        newMap[x, y] = map[x, y];
                    }
                }
            }
            factor--;

            if (factor > 1)
            {
                return ThinkenPathsOnMap(newMap, maxX, maxY, factor);
            }
            else
            {
                return newMap;
            }
        }

        /// <summary>
        /// Modify our go-to-position by a small random factor to provide "naturality"
        /// </summary>
        /// <param name="targetPos">pos you want to go to/param>
        /// <param name="offset">distance to keep to the target</param>
        /// <returns>modified position</returns>
        private Vector3 RebaseVector(Vector3 targetPos, int offset)
        {
            Random rnd = new Random();
            float factorX = rnd.Next((offset / 4) * -1, offset / 2);
            float factorY = rnd.Next((offset / 4) * -1, offset / 2);
            return new Vector3(targetPos.X + factorX, targetPos.Y + factorY, targetPos.Z);
        }
    }
}