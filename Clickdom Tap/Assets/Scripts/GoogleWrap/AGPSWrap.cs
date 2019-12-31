using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ANU.GoogleWrap
{
    public abstract class AGPSWrap
    {
        public AGPSWrap(Action<bool> onAuthenticate)
        {
            Init(onAuthenticate);
        }

        public void Init(Action<bool> onAuthenticate)
        {
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                // enables saving game progress.
                .EnableSavedGames()
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            // recommended for debugging:
            PlayGamesPlatform.DebugLogEnabled = true;
            // Activate the Google Play Games platform
            PlayGamesPlatform.Activate();

            Social.localUser.Authenticate(onAuthenticate);
        }
    }
}
