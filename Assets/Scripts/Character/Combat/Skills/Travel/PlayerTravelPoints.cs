using System.Collections.Generic;
using UnityEngine;
using Project.Persistence;

namespace Project.Character.Combat
{
    /// <summary>
    /// Holds the places the player can return or warp to: the save point and up to
    /// <see cref="MaxMemos"/> memorized locations. Both are saved with the character.
    /// </summary>
    public class PlayerTravelPoints : MonoBehaviour, ISaveParticipant
    {
        /// <summary>The most memorized locations the player can hold.</summary>
        public const int MaxMemos = 3;

        private readonly List<WarpDestination> memos = new List<WarpDestination>();

        private bool hasSavePoint;
        private string savePointScene;
        private Vector3 savePointPosition;

        /// <summary>Gets whether a save point has been set.</summary>
        public bool HasSavePoint => hasSavePoint;

        /// <summary>Gets how many locations are memorized.</summary>
        public int MemoCount => memos.Count;

        /// <summary>
        /// Gets the save point.
        /// </summary>
        /// <returns>The save point, or null when none has been set.</returns>
        public WarpDestination? GetSavePoint()
        {
            return hasSavePoint ? new WarpDestination($"Save point ({savePointScene})", savePointScene, savePointPosition) : (WarpDestination?)null;
        }

        /// <summary>
        /// Sets the save point, replacing the previous one.
        /// </summary>
        /// <param name="sceneName">The scene of the save point.</param>
        /// <param name="position">The world position of the save point.</param>
        public void SetSavePoint(string sceneName, Vector3 position)
        {
            hasSavePoint = true;
            savePointScene = sceneName;
            savePointPosition = position;
        }

        /// <summary>
        /// Memorizes a location.
        /// </summary>
        /// <param name="sceneName">The scene of the location.</param>
        /// <param name="position">The world position of the location.</param>
        /// <returns>True if the location was memorized; false when all slots are used.</returns>
        public bool TryAddMemo(string sceneName, Vector3 position)
        {
            if (memos.Count >= MaxMemos)
            {
                return false;
            }

            memos.Add(new WarpDestination($"Memo {memos.Count + 1} ({sceneName})", sceneName, position));
            return true;
        }

        /// <summary>
        /// Lists the destinations a skill of the given level can open, the save point first.
        /// </summary>
        /// <param name="limit">The most destinations the skill level allows.</param>
        /// <returns>The available destinations.</returns>
        public IReadOnlyList<WarpDestination> GetDestinations(int limit)
        {
            var destinations = new List<WarpDestination>();
            var savePoint = GetSavePoint();

            if (savePoint.HasValue)
            {
                destinations.Add(savePoint.Value);
            }

            foreach (var memo in memos)
            {
                destinations.Add(memo);
            }

            if (destinations.Count > limit)
            {
                destinations.RemoveRange(limit, destinations.Count - limit);
            }

            return destinations;
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.hasSavePoint = hasSavePoint;
            data.savePointMapId = savePointScene ?? string.Empty;
            data.savePointX = savePointPosition.x;
            data.savePointY = savePointPosition.y;
            data.savePointZ = savePointPosition.z;
            data.memoMapIds.Clear();
            data.memoX.Clear();
            data.memoY.Clear();
            data.memoZ.Clear();

            foreach (var memo in memos)
            {
                data.memoMapIds.Add(memo.SceneName);
                data.memoX.Add(memo.Position.x);
                data.memoY.Add(memo.Position.y);
                data.memoZ.Add(memo.Position.z);
            }
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            if (data.hasSavePoint)
            {
                SetSavePoint(data.savePointMapId, new Vector3(data.savePointX, data.savePointY, data.savePointZ));
            }

            memos.Clear();

            for (var i = 0; i < data.memoMapIds.Count && i < MaxMemos; i++)
            {
                TryAddMemo(data.memoMapIds[i], new Vector3(data.memoX[i], data.memoY[i], data.memoZ[i]));
            }
        }
    }
}
