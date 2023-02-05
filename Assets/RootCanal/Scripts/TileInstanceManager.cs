#nullable enable

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RootCanal
{
    public class TileInstanceManager : MonoBehaviour
    {
        private readonly Dictionary<Vector3Int, TileInstance> _tiles = new();

        [Required] public Tilemap? Tilemap;
        [Required] public TileSelector? TileSelector;
        [Required] public BacteriaManager? BacteriaManager;
        public Transform? TileParent;
        [AssetsOnly] public GameObject? TileInstancePrefab;

        public event EventHandler<(TileInstance, Vector3Int)>? TileInstanceCreated;
        public event EventHandler<(TileInstance, Vector3Int)>? TileInstanceDestroyed;

        private void Awake() =>
            BacteriaManager!.BacteriumAdded.AddListener(bacterium =>
                bacterium.DestinationReached.AddListener(onDestinationReached)
            );

        private void onDestinationReached(Vector3Int position)
        {
            if (_tiles.TryGetValue(position, out TileInstance tile))
                return;

            Debug.Log($"Instantiating tile instance at position {position}...");
            TileBase tileBase = Tilemap!.GetTile(position);
            GameObject tileObj = Instantiate(TileInstancePrefab, Tilemap.CellToWorld(position), Quaternion.identity, TileParent != null ? TileParent : transform)!;
            tile = tileObj.GetComponent<TileInstance>();
            if (tile == null)
                throw new Exception($"{nameof(TileInstancePrefab)} must have a {nameof(TileInstance)} component somewhere in its hierarchy");

            _tiles[position] = tile;

            TileInstanceCreated?.Invoke(this, (tile, position));
        }

        public void BreakTileAt(Vector3Int position)
        {
            if (!_tiles.TryGetValue(position, out TileInstance tile)) {
                Debug.LogWarning($"No tile instance to break at position {position}...");
                return;
            }

            Debug.Log($"Breaking tile instance at position {position}...");
            _tiles.Remove(position);
            Destroy(tile);
            TileInstanceDestroyed?.Invoke(this, (tile, position));
        }

        public TileInstance? GetTileAtPosition(Vector3Int position) =>
            _tiles.TryGetValue(position, out TileInstance tileInstance) ? tileInstance : null;
    }
}
