using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ANU.GoogleWrap
{
    public class GPSSavedGames : AGPSWrap, ISaveLoader
    {
        public GPSSavedGames(Action<bool> onAuthenticate) : base(onAuthenticate)
        {
        }

        public void Save(string saveId, string gameData, Action<bool> onGameSaved)
        {
            try
            {
                OpenSavedGame(saveId, (status, game) =>
                {
                    if (status != SavedGameRequestStatus.Success)
                    {
                        Debug.Log("===================== OpenSavedGame failed with status: " + status.ToString());
                        onGameSaved?.Invoke(false);
                    }
                    else
                    {
                        try
                        {
                            var data = Encoding.ASCII.GetBytes(gameData);
                            WriteGame(game, data, game.TotalTimePlayed + DateTime.Now.Subtract(game.LastModifiedTimestamp), (saveStatus, savedGame) =>
                            {
                                if(saveStatus != SavedGameRequestStatus.Success)
                                    Debug.Log("===================== WriteGame failed with status: " + status.ToString());

                                onGameSaved?.Invoke(saveStatus == SavedGameRequestStatus.Success);
                            });
                        }
                        catch
                        {
                            onGameSaved?.Invoke(false);
                        }
                    }
                });
            }
            catch
            {
                onGameSaved?.Invoke(false);
            }
        }

        public void Load(string saveId, Action<bool, string> onGameLoaded)
        {
            try
            {
                OpenSavedGame(saveId, (status, game) =>
            {
                if (status != SavedGameRequestStatus.Success)
                {
                    Debug.Log("===================== OpenSavedGame failed with status: " + status.ToString());
                    onGameLoaded?.Invoke(false, "");
                }
                else
                {
                    try
                    {
                        ReadGameData(game, (loadStatus, loadData) =>
                        {
                            if (loadStatus != SavedGameRequestStatus.Success)
                                Debug.Log("===================== ReadGameData failed with status: " + status.ToString());

                            var data = "";
                            if (loadData != null)
                                data = Encoding.ASCII.GetString(loadData);
                            onGameLoaded?.Invoke(loadStatus == SavedGameRequestStatus.Success, data);
                        });
                    }
                    catch
                    {
                        onGameLoaded?.Invoke(false, "");
                    }
                }
            });
            }
            catch
            {
                onGameLoaded?.Invoke(false, "");
            }
        }

        private void OpenSavedGame(string saveId, Action<SavedGameRequestStatus, ISavedGameMetadata> onSavedGameOpened)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(
                saveId,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                onSavedGameOpened
            );
        }

        private void WriteGame(ISavedGameMetadata game, byte[] savedData, TimeSpan totalPlaytime, Action<SavedGameRequestStatus, ISavedGameMetadata> onSavedGameWritten)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

            SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
            builder = builder
                .WithUpdatedPlayedTime(totalPlaytime)
                .WithUpdatedDescription("Saved game at " + DateTime.Now);
            SavedGameMetadataUpdate updatedMetadata = builder.Build();

            savedGameClient.CommitUpdate(game, updatedMetadata, savedData, onSavedGameWritten);
        }

        private void ReadGameData(ISavedGameMetadata game, Action<SavedGameRequestStatus, byte[]> onSavedGameDataRead)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.ReadBinaryData(game, onSavedGameDataRead);
        }
    }
}
