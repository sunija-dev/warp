// extremely fast spatial hashing interest management based on uMMORPG GridChecker.
// => 30x faster in initial tests
// => scales way higher
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
    /// <summary>
    /// Copy from SpatialHashingInterestManagement, adapted for WARP.
    /// </summary>
    public class WarpInterestManagment : InterestManagement
    {
        [Tooltip("The maximum range that objects will be visible at.")]
        public int visRange = 30;

        // if we see 8 neighbors then 1 entry is visRange/3
        public int resolution => visRange / 3;

        [Tooltip("Rebuild all every 'rebuildInterval' seconds.")]
        public float rebuildInterval = 1;
        double lastRebuildTime;

        public enum CheckMethod
        {
            XZ_FOR_3D,
            XY_FOR_2D
        }
        [Tooltip("Spatial Hashing supports 3D (XZ) and 2D (XY) games.")]
        public CheckMethod checkMethod = CheckMethod.XZ_FOR_3D;

        // debugging
        public bool showSlider;

        // the grid
        Grid2D<NetworkConnection> grid = new Grid2D<NetworkConnection>();

        // project 3d world position to grid position
        Vector2Int ProjectToGrid(Vector3 position) =>
            checkMethod == CheckMethod.XZ_FOR_3D
            ? Vector2Int.RoundToInt(new Vector2(position.x, position.z) / resolution)
            : Vector2Int.RoundToInt(new Vector2(position.x, position.y) / resolution);

        public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
        {
            WarpNetworkPosition networkPosObject = identity.gameObject.GetComponent<WarpNetworkPosition>();

            if (!ServerManager.s_dictPlayers.ContainsKey(newObserver))
            {
                //Debug.Log("Didn't find the network connection on server.");
                return false;
            }
            WarpNetworkPosition networkPosPlayer = ServerManager.s_dictPlayers[newObserver].warpNetworkPosition;
            return bCanSeeEachOther(networkPosPlayer, networkPosObject, newObserver.identity.transform.position, identity.transform.position);
        }

        public override void OnRebuildObservers(NetworkIdentity _identity, HashSet<NetworkConnection> o_netConnObservers, bool initialize)
        {
            WarpNetworkPosition networkPosObject = _identity.gameObject.GetComponent<WarpNetworkPosition>();

            if (networkPosObject == null)
            {
                Debug.Log("No WarpNetworkPosition " + _identity.name);
                return;
            }

            foreach (NetworkConnectionToClient connClient in NetworkServer.connections.Values)
            {
                if (!ServerManager.s_dictPlayers.TryGetValue(connClient, out Player player)) 
                    continue;

                WarpNetworkPosition networkPosPlayer = player.warpNetworkPosition;
                if (bCanSeeEachOther(networkPosPlayer, networkPosObject, connClient.identity.transform.position, _identity.transform.position))
                    o_netConnObservers.Add(connClient);
            }
            

            /*
            // global? add to all players with same language/region
            if (networkPosObject.bIsGlobal)
            {
                foreach (NetworkConnectionToClient connClient in NetworkServer.connections.Values)
                {
                    if (connClient != null && connClient.isAuthenticated && connClient.identity != null)
                    {
                        if (!ServerManager.s_dictPlayers.TryGetValue(connClient, out Player player))
                            continue;

                        WarpNetworkPosition networkPosPlayer = player.warpNetworkPosition;

                        if (bSamePlayGroup(networkPosPlayer, networkPosObject))
                            o_netConnObservers.Add(connClient);
                    } 
                }
                return;
            }
            */
        }

        // update everyone's position in the grid
        // (internal so we can update from tests)
        internal void Update()
        {
            // only on server
            if (!NetworkServer.active) return;

            // IMPORTANT: refresh grid every update!
            // => newly spawned entities get observers assigned via
            //    OnCheckObservers. this can happen any time and we don't want
            //    them broadcast to old (moved or destroyed) connections.
            // => players do move all the time. we want them to always be in the
            //    correct grid position.
            // => note that the actual 'rebuildall' doesn't need to happen all
            //    the time.
            // NOTE: consider refreshing grid only every 'interval' too. but not
            //       for now. stability & correctness matter.

            // clear old grid results before we update everyone's position.
            // (this way we get rid of destroyed connections automatically)
            //
            // NOTE: keeps allocated HashSets internally.
            //       clearing & populating every frame works without allocations
            grid.ClearNonAlloc();

            // put every connection into the grid at it's main player's position
            // NOTE: player sees in a radius around him. NOT around his pet too.
            foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
            {
                // authenticated and joined world with a player?
                if (connection.isAuthenticated && connection.identity != null)
                {
                    // calculate current grid position
                    Vector2Int position = ProjectToGrid(connection.identity.transform.position);

                    // put into grid
                    grid.Add(position, connection);
                }
            }

            // rebuild all spawned entities' observers every 'interval'
            // this will call OnRebuildObservers which then returns the
            // observers at grid[position] for each entity.
            if (NetworkTime.time >= lastRebuildTime + rebuildInterval)
            {
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                RebuildAll();
                lastRebuildTime = NetworkTime.time;

                sw.Stop();
                DatabaseDebug.AddAction("Rebuild Observers", sw.ElapsedMilliseconds);
            }
        }

        // slider from dotsnet. it's nice to play around with in the benchmark demo.
        void OnGUI()
        {
            if (!showSlider) return;

            // only show while server is running. not on client, etc.
            if (!NetworkServer.active) return;

            int height = 30;
            int width = 250;
            GUILayout.BeginArea(new Rect(Screen.width / 2 - width / 2, Screen.height - height, width, height));
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("Radius:");
            visRange = Mathf.RoundToInt(GUILayout.HorizontalSlider(visRange, 0, 200, GUILayout.Width(150)));
            GUILayout.Label(visRange.ToString());
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private bool bCanSeeEachOther(WarpNetworkPosition _warpPosPlayer, WarpNetworkPosition _warpPosObject, Vector3 _v3Player, Vector3 _v3Object)
        {
            bool bSamePlayGroup = _warpPosObject.bAllRegionsLanguages
                    || (_warpPosPlayer.region == _warpPosObject.region 
                        && _warpPosPlayer.language == _warpPosObject.language);

            if (_warpPosObject.bIsGlobal)
                return bSamePlayGroup;

            bool bCouldSeeEachOther = _warpPosObject.iMapId == _warpPosPlayer.iMapId
                                        && bSamePlayGroup;

            if (!bCouldSeeEachOther)
                return false;

            Vector2Int v2iObjectPos = ProjectToGrid(_v3Object);
            Vector2Int v2iPlayerPos = ProjectToGrid(_v3Player);

            // distance needs to be at max one of the 8 neighbors, which is
            //   1 for the direct neighbors
            //   1.41 for the diagonal neighbors (= sqrt(2))
            // => use sqrMagnitude and '2' to avoid computations. same result.
            if ((v2iObjectPos - v2iPlayerPos).sqrMagnitude <= 2)
                return true;

            return false;
        }
    }
}