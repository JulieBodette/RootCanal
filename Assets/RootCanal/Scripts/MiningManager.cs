#nullable enable

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RootCanal
{
    public class MiningManager : MonoBehaviour
    {
        [Required] public Tilemap? Tilemap;
        [Required] public TileInstanceManager? TileInstanceManager;
        [Required] public BacteriaManager? BacteriaManager;
        public int DamagePerHit = 10;

        private void Awake()
        {
            BacteriaManager!.BacteriumSpawned.AddListener(bacterium => {
                Timer miningTimer = bacterium.GetComponent<Timer>();
                miningTimer.Triggered.AddListener(() => mine(bacterium));
            });

            TileInstanceManager!.TileInstanceCreated += onTileInstanceCreated;
        }

        private void onTileInstanceCreated(object sender, (TileInstance tileInstance, Vector3Int position) e) =>
            e.tileInstance.Durability!.AmountChanged.AddListener(delta => {
                if (e.tileInstance.Durability.CurrentAmount == 0)
                    TileInstanceManager!.BreakTileAt(e.position);
            });

        private void mine(Bacteria bacterium)
        {
            TileInstance? tile = TileInstanceManager!.GetTileAtPosition(bacterium.goalTilePos);
            if (tile == null) {
                Debug.LogError($"Bacterium {bacterium.name} is mining a tile with no {nameof(TileInstance)}", context: bacterium);
                return;
            }

            tile.Durability!.AddToAmount(-DamagePerHit);
        }
    }
}